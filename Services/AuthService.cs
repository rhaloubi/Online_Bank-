using System.Net.Http.Json;
using OnlineBank_FE.Models;
using System.Text.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;

namespace OnlineBank_FE.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task<bool> LoginAsync(Client request)
        {
            var response = await _httpClient.PostAsJsonAsync("Clients/login", request);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (responseData?.Token != null)
                {
                    // Save token in local storage
                    await _localStorage.SetItemAsync("authToken", responseData.Token);

                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", responseData.Token);

                    return true;
                }
            }

            return false;
        }

        public async Task<bool> RegisterAsync(Client request)
        {
            var response = await _httpClient.PostAsJsonAsync("Clients/register", request);

            return response.IsSuccessStatusCode;
        }

        public async Task InitializeAuth()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("authToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<bool> IsLoggedInAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            return !string.IsNullOrEmpty(token);
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty; // Adjust this to match your API's token response
    }
}