using System.Net.Http.Json;
using OnlineBank_FE.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OnlineBank_FE.Services
{
    public class TransactionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public TransactionService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        // Get Transactions
        public async Task<List<Transaction>?> GetTransactionsOutAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync("Transaction/TransactionOut");

                if (response.IsSuccessStatusCode)
                {
                    // Read the response as a JSON string
                    var json = await response.Content.ReadAsStringAsync();

                    // Deserialize into a wrapper object
                    var wrapper = JsonSerializer.Deserialize<Wrapper<List<Transaction>>>(json);

                    // Return the transactions inside the $values property
                    return wrapper?.Values;
                }
                else
                {
                    Console.WriteLine($"Failed to fetch transactions: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching transactions: {ex.Message}");
            }

            return null;
        }

        public async Task<List<Transaction>?> GetTransactionsInAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync("Transaction/TransactionIn");

                if (response.IsSuccessStatusCode)
                {
                    // Read the response as a JSON string
                    var json = await response.Content.ReadAsStringAsync();

                    // Deserialize into a wrapper object
                    var wrapper = JsonSerializer.Deserialize<Wrapper<List<Transaction>>>(json);

                    // Return the transactions inside the $values property
                    return wrapper?.Values;
                }
                else
                {
                    Console.WriteLine($"Failed to fetch transactions: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching transactions: {ex.Message}");
            }

            return null;
        }


        public async Task<bool> TransferAsync(int targetClientId, decimal amount)
    {
        try
        {
            // Retrieve the token from local storage
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (!string.IsNullOrEmpty(token))
            {
                // Add the token to the Authorization header
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Prepare the request body
            var requestBody = new Dictionary<string, object>
        {
            { "targetClientId", targetClientId },
            { "amount", amount }
        };

            // Make the POST request
            var response = await _httpClient.PostAsJsonAsync("Transaction/transfer", requestBody);

            if (response.IsSuccessStatusCode)
            {
                return true; // Transfer successful
            }
            else
            {
                Console.WriteLine($"Transfer failed: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions
            Console.WriteLine($"An error occurred while transferring funds: {ex.Message}");
        }

        return false; // Transfer failed
    }

    }

    // Wrapper class to match API response structure
    public class Wrapper<T>
    {
        [JsonPropertyName("$id")]
        public string Id { get; set; }

        [JsonPropertyName("$values")]
        public T Values { get; set; }
    }
}
