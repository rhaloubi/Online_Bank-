using System.Net.Http.Json;
using OnlineBank_FE.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using System.Threading.Tasks;

namespace OnlineBank_FE.Services
{
    public class ClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public ClientService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        // Get client ID from token
        private async Task<int?> GetClientIdFromTokenAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Extract ClientID from JWT claim (which corresponds to ClaimTypes.NameIdentifier)
                var clientIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (clientIdClaim != null && int.TryParse(clientIdClaim.Value, out var clientId))
                {
                    return clientId; // Return the extracted client ID
                }
            }

            return null; // Return null if token is invalid or claim not found
        }

        // Get Client Details
        public async Task<Client?> GetClientDetailsAsync()
        {
            var clientId = await GetClientIdFromTokenAsync();
            if (clientId == null)
            {
                return null; // No client ID available
            }

            var response = await _httpClient.GetAsync($"Clients/{clientId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Client>();
            }

            return null; // Return null if client details cannot be retrieved
        }

        // Update Client Details
        public async Task<bool> UpdateClientDetailsAsync(Client client)
        {
            var clientId = await GetClientIdFromTokenAsync();
            if (clientId == null)
            {
                return false; // No client ID available
            }

            // Ensure the client ID matches the logged-in user's client ID
            client.ClientID = clientId.Value;

            var response = await _httpClient.PutAsJsonAsync($"Clients/{clientId}", client);

            return response.IsSuccessStatusCode;
        }


        // Delete Client
        public async Task<bool> DeleteClientAsync()
        {
            var clientId = await GetClientIdFromTokenAsync();
            if (clientId == null)
            {
                return false; // No client ID available
            }

            var response = await _httpClient.DeleteAsync($"Clients/{clientId}");

            return response.IsSuccessStatusCode;
        }
    }
}
