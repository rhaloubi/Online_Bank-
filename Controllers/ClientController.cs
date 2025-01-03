using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OnlineBank.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly OnlineBankContext _context;
    private readonly IConfiguration _configuration;

    public ClientsController(OnlineBankContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // Register (Sign Up)
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Client client)
    {
        if (client == null || string.IsNullOrWhiteSpace(client.Email) || string.IsNullOrWhiteSpace(client.Password))
        {
            return BadRequest("Invalid client data.");
        }

        // Check if the email already exists
        if (_context.Clients.Any(c => c.Email == client.Email))
        {
            return BadRequest("Email already exists.");
        }
        if (_context.Admins.Any(a => a.Email == client.Email ))
        {
            return BadRequest("Email already exists.");
        }

        // Hash the password
        client.Password = BCrypt.Net.BCrypt.HashPassword(client.Password);

        // Add the client
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Automatically create an account for the client
        var account = new Account
        {
            Balance = 0, // Initial balance
            AccountType = "Default",
            ClientID = client.ClientID
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return Ok("Client registered successfully!");
    }

    // Login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Client client)
    {
        if (client == null || string.IsNullOrWhiteSpace(client.Email) || string.IsNullOrWhiteSpace(client.Password))
        {
            return BadRequest("Invalid login data.");
        }

        var existingClient = await _context.Clients.FirstOrDefaultAsync(c => c.Email == client.Email);

        if (existingClient == null || !BCrypt.Net.BCrypt.Verify(client.Password, existingClient.Password))
        {
            return Unauthorized("Invalid email or password.");
        }

        var token = GenerateJwtToken(existingClient); // Generate JWT token
        return Ok(new { token });
    }

    // Logout
    [HttpPost("logout")]
    [Authorize(Roles = "Client")]
    public IActionResult Logout()
    {
        // Logout is typically handled on the client side by clearing the token
        return Ok(new { message = "Logged out successfully." });
    }

    // Get All Clients
    [HttpGet("index")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetAllClients()
    {
        var clients = await _context.Clients.ToListAsync();
        return Ok(clients);
    }

    // Get Client by ID
    [HttpGet("{id}")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetClientById(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null)
        {
            return NotFound("Client not found.");
        }
        return Ok(client);
    }

    // Update Client
    [HttpPut("{id}")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> UpdateClient(int id, [FromBody] Client updatedClient)
    {
        if (updatedClient == null)
        {
            return BadRequest("Invalid client data.");
        }

        var existingClient = await _context.Clients.FindAsync(id);
        if (existingClient == null)
        {
            return NotFound("Client not found.");
        }

        // Ensure the client is updating their own profile
        var loggedInUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (existingClient.ClientID != loggedInUserId)
        {
            return Unauthorized("You can only update your own profile.");
        }

        // Update fields only if they are explicitly sent in the request (not null)
        if (updatedClient.Name != null)
            existingClient.Name = updatedClient.Name;

        if (updatedClient.PhoneNumber != null)
            existingClient.PhoneNumber = updatedClient.PhoneNumber;

        if (updatedClient.Address != null)
            existingClient.Address = updatedClient.Address;

        await _context.SaveChangesAsync();
        return Ok(existingClient);
    }


    // Delete Client
    [HttpDelete("{id}")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null)
        {
            return NotFound("Client not found.");
        }

        // Ensure the client is deleting their own profile
        var loggedInUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (client.ClientID != loggedInUserId)
        {
            return Unauthorized("You can only delete your own profile.");
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Client deleted successfully." });
    }

    // Private: Generate JWT Token
    private string GenerateJwtToken(Client client)
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, client.ClientID.ToString()), // The unique client ID as the identifier
        new Claim(ClaimTypes.Name, client.Name),                          // The name of the client
        new Claim(ClaimTypes.Email, client.Email),                        // The email of the client
        new Claim(ClaimTypes.Role, "Client")                               // The role of the client
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])); // Secret key to sign the token
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // Signing algorithm

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(1), // Token expiration time
            signingCredentials: creds); // Signing the token

        return new JwtSecurityTokenHandler().WriteToken(token); // Convert the token to string
    }

}
