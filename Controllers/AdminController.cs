using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBank.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly OnlineBankContext _context;
    private readonly IConfiguration _configuration;


    public AdminController(OnlineBankContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // Register (Sign Up)
    [HttpPost("register")]
    [AllowAnonymous] // Allow anonymous access to register
    public async Task<IActionResult> Register([FromBody] Admin admin)
    {
        // Hash the password
        admin.Password = BCrypt.Net.BCrypt.HashPassword(admin.Password);

        // Check if the email already exists
        if (_context.Admins.Any(a => a.Email == admin.Email ))
        {
            return BadRequest("Email already exists.");
        }
        // Check if the email already exists
        if (_context.Clients.Any(c => c.Email == admin.Email))
        {
            return BadRequest("Email already exists.");
        }

        _context.Admins.Add(admin);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Admin registered successfully!" });
    }

    // Login
    [HttpPost("login")]
    [AllowAnonymous] // Allow anonymous access to login
    public async Task<IActionResult> Login([FromBody] Admin admin)
    {
        var existingAdmin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Email == admin.Email);

        if (existingAdmin == null || !BCrypt.Net.BCrypt.Verify(admin.Password, existingAdmin.Password))
        {
            return Unauthorized("Invalid email or password.");
        }

        var token = GenerateJwtToken(existingAdmin); // Generate JWT token
        return Ok(new { token });
    }

    // Logout
    [HttpPost("logout")]
    [Authorize(Roles = "Admin")]
    public IActionResult Logout()
    {
        // In this case, the client is responsible for clearing the token on the frontend (e.g., localStorage)
        return Ok(new { message = "Logged out successfully." });
    }

    // Get All Admins (Show index - Admins only)
    [HttpGet("index/admins")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllAdmins()
    {
        // Ensure that the logged-in user is an admin
        if (!User.IsInRole("Admin"))
        {
            return Unauthorized("Only admins can view the admin list.");
        }

        var admins = await _context.Admins.ToListAsync();
        return Ok(admins);
    }

    // Get All Clients (Show index - Admins only)
    [HttpGet("index/clients")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllClients()
    {
        // Ensure that the logged-in user is an admin
        if (!User.IsInRole("Admin"))
        {
            return Unauthorized("Only admins can view the client list.");
        }

        var clients = await _context.Clients.ToListAsync();
        return Ok(clients);
    }

    // Get Admin by ID (Show - Admins can view their own and other admins)
    [HttpGet("show/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminById(int id)
    {
        var admin = await _context.Admins.FindAsync(id);
        if (admin == null)
        {
            return NotFound();
        }

        return Ok(admin);
    }

    // Update Admin
    [HttpPut("update/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAdmin(int id, [FromBody] Admin updatedAdmin)
    {
        // Ensure that the logged-in user is an admin
        if (!User.IsInRole("Admin"))
        {
            return Unauthorized("Only admins can update admin details.");
        }

        var admin = await _context.Admins.FindAsync(id);
        if (admin == null)
        {
            return NotFound();
        }

        // Check that the admin cannot update their own details
        var loggedInAdminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (admin.AdminID == loggedInAdminId)
        {
            return Unauthorized("You cannot update your own details.");
        }

        // Update the admin data
        admin.Name = updatedAdmin.Name;
        admin.Email = updatedAdmin.Email;


        await _context.SaveChangesAsync();
        return Ok(admin);
    }

    // Delete Admin
    [HttpDelete("delete/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAdmin(int id)
    {
        // Ensure that the logged-in user is an admin
        if (!User.IsInRole("Admin"))
        {
            return Unauthorized("Only admins can delete admin details.");
        }

        var admin = await _context.Admins.FindAsync(id);
        if (admin == null)
        {
            return NotFound();
        }

        // Check that the admin cannot delete their own account
        var loggedInAdminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (admin.AdminID == loggedInAdminId)
        {
            return Unauthorized("You cannot delete your own account.");
        }

        _context.Admins.Remove(admin);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Helper method to generate JWT token
    private string GenerateJwtToken(Admin admin)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, admin.AdminID.ToString()),
            new Claim(ClaimTypes.Name, admin.Name),
            new Claim(ClaimTypes.Email, admin.Email),
            new Claim(ClaimTypes.Role, "Admin") // Assign the Admin role to the claims
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
