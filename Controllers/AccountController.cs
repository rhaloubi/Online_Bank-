using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBank.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly OnlineBankContext _context;

    public AccountController(OnlineBankContext context)
    {
        _context = context;
    }

    // Get the current client's ID from the JWT
    private int GetClientIdFromToken()
    {
        var clientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(clientIdClaim);
    }

    // Get Balance
    [HttpGet("balance")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetBalance()
    {
        var clientId = GetClientIdFromToken();
        var account = await _context.Accounts.FirstOrDefaultAsync( a=> a.ClientID == clientId);

        if (account == null)
            return NotFound("Account not found or not associated with the current client.");

        return Ok(account);
    }
}
