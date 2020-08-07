using Dwolla.Client;
using Dwolla.Client.Models;
using Dwolla.Client.Models.Requests;
using Dwolla.Client.Models.Responses;
using Dwolla.Client.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace xCompany.UtilityService.Dwolla.Service
{
    public class DwollaService : IDwollaService
    {
        private readonly Headers _headers;
        private readonly IDwollaClient _client;
        private readonly HttpClient _httpClient;
        private readonly RootResponse rootRes;
        private const string key = "";
        private const string secret = "";
        public DwollaService(IDwollaClient client)
        {
            _headers = new Headers();
            _client = client;
            rootRes = GetRootAsync().GetAwaiter().GetResult();
            _httpClient = HttpClientFactory.Create();
        }
        private async Task<TokenResponse> SetAuthorizationHeader()
        {
            var response = await _client.PostAuthAsync<TokenResponse>(
                new Uri($"{_client.AuthBaseAddress}/token"), new AppTokenRequest { Key = key, Secret = secret });

            // TODO: Securely store token in your database for reuse
            if (!_headers.ContainsKey("Authorization"))
                _headers.Add("Authorization", $"Bearer {response.Content.Token}");
            else
                _headers["Authorization"] = $"Bearer {response.Content.Token}";

            if (_httpClient != null && !_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.dwolla.v1.hal+json"));
                _httpClient.DefaultRequestHeaders.Add("Authorization", _headers["Authorization"]);
            }

            return response.Content;
        }
        public async Task<Uri> CreateCustomerAsync(string firstName, string lastName, string email)
        {
            var request = new CreateCustomerRequest
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Type = "unverified"
            };

            _headers["Idempotency-Key"] = email;
            var r = await PostAsync<CreateCustomerRequest, EmptyResponse>(rootRes.Links["customers"].Href, request);
            return r.Response.Headers.Location;
        }
        private async Task<RootResponse> GetRootAsync() =>
            (await GetAsync<RootResponse>(new Uri(_client.ApiBaseAddress))).Content;
        private async Task<RestResponse<TRes>> GetAsync<TRes>(Uri uri) where TRes : IDwollaResponse =>
            await ExecAsync(() => _client.GetAsync<TRes>(uri, _headers));

        public async Task<Uri> CreateFundingSourceAsync(Uri uri, string routingNumber, string accountNumber, string bankAccountType, string name)
        {
            var requestBody = new {
                          routingNumber = routingNumber,
                          accountNumber = accountNumber,
                          bankAccountType = bankAccountType,
                          name = name};
            var content = new StringContent(JsonConvert.SerializeObject(requestBody));
            var request = new HttpRequestMessage(HttpMethod.Post, $@"{uri.AbsoluteUri}\funding-sources");

            request.Content = content;
            request.Content.Headers.Remove("Content-Type");
            request.Content.Headers.Add("Content-Type", "application/vnd.dwolla.v1.hal+json");
            //request.Content.Headers.Add("Idempotency-Key", uri.AbsoluteUri);
            var response = await _httpClient.SendAsync(request);
            return response.Headers.Location;
        }

        public async Task<HttpStatusCode> InitiateMicroDepositAsync(Uri uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $@"{uri.AbsoluteUri}\micro-deposits");

            request.Content = new StringContent("");
            request.Content.Headers.Remove("Content-Type");
            request.Content.Headers.Add("Content-Type", "application/vnd.dwolla.v1.hal+json");
            //request.Content.Headers.Add("Idempotency-Key", uri.AbsoluteUri);
            var response = await _httpClient.SendAsync(request);
            return response.StatusCode;
        }
        public Task<Uri> CreateTransferAsync(string sourceFundingSourceId, string destinationFundingSourceId, decimal amount, decimal? fee, Uri chargeTo, string sourceAddenda, string destinationAddenda)
        {
            throw new NotImplementedException();
        }

        public async Task<Customer> GetCustomerAsync(Uri uri)
        {
            return (await GetAsync<Customer>(uri)).Content;
        }

        public Task<Customer> UpdateCustomerAsync(UpdateCustomerRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<HttpStatusCode> VerifyMicroDepositsAsync(Uri uri, decimal amount1, decimal amount2)
        {
            var response = await PostAsync(new Uri($"{uri.AbsoluteUri}/micro-deposits"),
                new MicroDepositsRequest
                {
                    Amount1 = new Money { Value = amount1, Currency = "USD" },
                    Amount2 = new Money { Value = amount2, Currency = "USD" }
                });
                return response.Response.StatusCode;
        }
        private async Task<RestResponse<EmptyResponse>> PostAsync<TReq>(Uri uri, TReq request) =>
            await ExecAsync(() => _client.PostAsync<TReq, EmptyResponse>(uri, request, _headers));

        private async Task<RestResponse<TRes>> PostAsync<TReq, TRes>(Uri uri, TReq request) where TRes : IDwollaResponse =>
           await ExecAsync(() => _client.PostAsync<TReq, TRes>(uri, request, _headers));

        private async Task<RestResponse<TRes>> ExecAsync<TRes>(Func<Task<RestResponse<TRes>>> func) where TRes : IDwollaResponse
        {
            await SetAuthorizationHeader();
            var r = await func();
            if (r.Error == null) return r;

            // TODO: Handle error specific to your application
            var e = r.Error;
            Console.WriteLine($"{e.Code}: {e.Message}");

            // To print out the full error response, uncomment the line below
            // Console.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));

            // Example error handling. More info: https://docsv2.dwolla.com/#errors
            if (e.Code == "ExpiredAccessToken")
            {
                await SetAuthorizationHeader();
            }

            return r;
        }
    }
}
