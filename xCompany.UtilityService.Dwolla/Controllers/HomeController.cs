using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using xCompany.UtilityService.Dwolla.Models;
using xCompany.UtilityService.Dwolla.Repository;
using Newtonsoft.Json;
using xCompany.UtilityService.Dwolla.Service;
using System.Net;

namespace xCompany.UtilityService.Dwolla.Controllers
{
    public class HomeController : Controller
    {
        private IxCompanyRepository _xCompanyRepository;
        private IDwollaService _dwollaService;
        public HomeController(IxCompanyRepository xCompanyRepository, IDwollaService dwollaService)
        {
            _xCompanyRepository = xCompanyRepository;
            _dwollaService = dwollaService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetCustomer(string CustomerIdOrEmail)
        {
            //get user from xCompany database
            xCompanyUser user = null;
            long customerId = 0;
            if (Int64.TryParse(CustomerIdOrEmail, out customerId))
            {
                user = await _xCompanyRepository.GetUserDetailsByUserId(customerId);
            }
            else
            {
                user = await _xCompanyRepository.GetUserDetailsByEmail(CustomerIdOrEmail);
            }

            ValidateUser(CustomerIdOrEmail, user);

            if (ViewBag.Error == null)
            {
                await Log("User validation success", user, user);

                //create customer in dwolla
                var customerLocation = await _dwollaService.CreateCustomerAsync(user.FirstName, user.LastName, user.Email);

                await Log("Create customer completed", user, customerLocation);

                var customer = await _dwollaService.GetCustomerAsync(customerLocation);

                await Log("Get customer completed", user, customer.Id);

                //add funcding source for the customer
                var fundingSource = await _dwollaService.CreateFundingSourceAsync(customerLocation,
                    user.RoutingNumber, user.AccountNumber, user.AccountType, user.AccountName);

                await Log("Create Funding source completed", user, fundingSource);
                //get fundingsource id
                var fundingSourceId = fundingSource?.AbsolutePath.Substring(fundingSource.AbsolutePath.LastIndexOf("/") + 1);

                //initiate micro depolist
                var statusCode = await _dwollaService.InitiateMicroDepositAsync(fundingSource);

                await Log("Initiate Micro deposit completed", user, $"Status code: {statusCode}");
                //verify micro deposit
                var microDepositStatusCode = await _dwollaService.VerifyMicroDepositsAsync(fundingSource, 0.01M, 0.02M);

                await Log("Verify micro deposit completed", user, $"Status code: {microDepositStatusCode}");

                if (microDepositStatusCode == HttpStatusCode.OK)
                {
                    var userProvider = await _xCompanyRepository.GetUserProviderByUserId(user.Id);

                    if (userProvider == null)
                    {
                        userProvider = new UserProvider();
                        userProvider.ProviderId = 8;
                        userProvider.ProviderName = "dwolla";
                        userProvider.AccessToken = fundingSourceId;
                        userProvider.TokenType = "Student-Repayment";
                        userProvider.MerchantId = customer.Id;
                        userProvider.Scope = null;
                        userProvider.xCompanyUserId = user.Id;
                        userProvider.GoCardlessId = null;
                        userProvider.MandateResourceId = null;
                        userProvider.MandateScheme = null;
                        userProvider.WebhookAction = null;
                        userProvider.WebhookCause = null;
                        userProvider.RedirectFlowId = null;
                        //userProvider.MicroDepositBankVerificationId = null;

                    }

                    userProvider.UpdatedOnUtc = DateTime.UtcNow;
                    await _xCompanyRepository.SaveUserProvider(userProvider);

                    await Log("User provider save completed", user, $"User provider id: {userProvider.Id}");
                }

                var logs = await _xCompanyRepository.GetDwollaLogByUserId(user.Id);

                return View("Index", logs);
            }

            return View("Index");
        }

        private async Task Log(string eventName, xCompanyUser user, Object obj)
        {
            var data = obj != null ? JsonConvert.SerializeObject(obj) : null;
            await _xCompanyRepository.SaveDwollaLog(new UtilityDwollaLog { EventName= eventName, UserId = user.Id, LogData = data, CreatedByUser = "get it from principle object" });
        }

        private void ValidateUser(string CustomerIdOrEmail, xCompanyUser user)
        {
            if (user == null)
            {
                ViewBag.Error = $"No User Exist for entered Email or Id - {CustomerIdOrEmail}";
            }
            else if (user.Id <= 0)
            {
                ViewBag.Error = $"User Id is 0 - {CustomerIdOrEmail}";
            }
            else if (user.Email == null)
            {
                ViewBag.Error = $"User Email is null - {CustomerIdOrEmail}";
            }
            else if (user.FirstName == null)
            {
                ViewBag.Error = $"User First name is null - {CustomerIdOrEmail}";
            }
            else if (user.LastName == null)
            {
                ViewBag.Error = $"User Last name is null - {CustomerIdOrEmail}";
            }
            else if (user.BankName == null)
            {
                ViewBag.Error = $"User Bank name is null - {CustomerIdOrEmail}";
            }
            else if (user.AccountName == null)
            {
                ViewBag.Error = $"User Account name is null - {CustomerIdOrEmail}";
            }
            else if (user.AccountType == null)
            {
                ViewBag.Error = $"User Account type is null - {CustomerIdOrEmail}";
            }
            else if (user.AccountNumber == null)
            {
                ViewBag.Error = $"User Account number is null - {CustomerIdOrEmail}";
            }
            else if (user.RoutingNumber == null)
            {
                ViewBag.Error = $"User Routing number is null - {CustomerIdOrEmail}";
            }
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
