using Dwolla.Client.Models.Requests;
using Dwolla.Client.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace xCompany.UtilityService.Dwolla.Service
{
    public interface IDwollaService
    {
        Task<Uri> CreateCustomerAsync(string firstName, string lastName, string email);
        Task<Uri> CreateFundingSourceAsync(Uri uri, string routingNumber, string accountNumber, string bankAccountType, string name);
        Task<Customer> GetCustomerAsync(Uri uri);
        Task<HttpStatusCode> InitiateMicroDepositAsync(Uri uri);
        Task<Customer> UpdateCustomerAsync(UpdateCustomerRequest request);
        Task<HttpStatusCode> VerifyMicroDepositsAsync(Uri uri, decimal amount1, decimal amount2);
        Task<Uri> CreateTransferAsync(string sourceFundingSourceId, string destinationFundingSourceId,
            decimal amount, decimal? fee, Uri chargeTo, string sourceAddenda, string destinationAddenda);
    }
}
