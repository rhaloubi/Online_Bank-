using System.Net.Http.Json;
using OnlineBank_FE.Models;
using System;
using System.Net.Http;

namespace OnlineBank_FE.Services
{
    public class AccountService
    {
        private readonly HttpClient _httpClient; 

        public AccountService(HttpClient httpClient) 
        {
            _httpClient = httpClient; 
        }

        // Get Balance
        public async Task<Account?> GetBalanceAsync()
        {
            var response = await _httpClient.GetAsync("Account/balance");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Account>();
            }

            return null; // Return null if the balance cannot be retrieved
        }

    }
}
