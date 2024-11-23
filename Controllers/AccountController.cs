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

    [HttpPost("deposit")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Deposit([FromBody] Dictionary<string, object> requestBody)
    {
        if (!requestBody.TryGetValue("accountId", out var accountIdObj) ||
            !requestBody.TryGetValue("amount", out var amountObj))
        {
            return BadRequest("Both 'accountId' and 'amount' are required.");
        }

        if (!int.TryParse(accountIdObj?.ToString(), out int accountId) ||
            !decimal.TryParse(amountObj?.ToString(), out decimal amount))
        {
            return BadRequest("Invalid 'accountId' or 'amount' format.");
        }

        if (amount <= 0)
        {
            return BadRequest("Deposit amount must be greater than zero.");
        }

        var clientId = GetClientIdFromToken();
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountID == accountId && a.ClientID == clientId);

        if (account == null)
        {
            return NotFound("Account not found or not associated with the current client.");
        }

        account.Balance += amount;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Deposit successful", balance = account.Balance });
    }


    // Withdraw
    [HttpPost("withdraw")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Withdraw(int accountId, decimal amount)
    {
        if (amount <= 0)
            return BadRequest("Withdrawal amount must be greater than zero.");

        var clientId = GetClientIdFromToken();
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountID == accountId && a.ClientID == clientId);

        if (account == null)
            return NotFound("Account not found or not associated with the current client.");

        if (account.Balance < amount)
            return BadRequest("Insufficient funds.");

        account.Balance -= amount;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Withdrawal successful", balance = account.Balance });
    }

    // Transfer
    [HttpPost("transfer")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Transfer(int sourceAccountId, int targetAccountId, decimal amount)
    {
        if (amount <= 0)
            return BadRequest("Transfer amount must be greater than zero.");

        var clientId = GetClientIdFromToken();

        var sourceAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountID == sourceAccountId && a.ClientID == clientId);
        if (sourceAccount == null)
            return NotFound("Source account not found or not associated with the current client.");

        if (sourceAccount.Balance < amount)
            return BadRequest("Insufficient funds.");

        var targetAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountID == targetAccountId);
        if (targetAccount == null)
            return NotFound("Target account not found.");

        // Perform the transfer
        sourceAccount.Balance -= amount;
        targetAccount.Balance += amount;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Transfer successful",
            sourceBalance = sourceAccount.Balance,
            targetBalance = targetAccount.Balance
        });
    }

    // Get Balance
    [HttpGet("balance")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetBalance(int accountId)
    {
        var clientId = GetClientIdFromToken();
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountID == accountId && a.ClientID == clientId);

        if (account == null)
            return NotFound("Account not found or not associated with the current client.");

        return Ok(new { balance = account.Balance });
    }
}
