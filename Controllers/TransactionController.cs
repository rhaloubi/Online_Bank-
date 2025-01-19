using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBank.Models;
using System.Security.Claims;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class TransactionController : ControllerBase
{
    private readonly OnlineBankContext _context;

    public TransactionController(OnlineBankContext context)
    {
        _context = context;
    }

    // Get the current client's ID from the JWT
    private int GetClientIdFromToken()
    {
        var clientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(clientIdClaim);
    }

    // Deposit
    [HttpPost("deposit")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Deposit([FromBody] Dictionary<string, object> requestBody)
    {
        if (!requestBody.TryGetValue("amount", out var amountObj))
        {
            return BadRequest(" the 'amount' are required.");
        }

        if (!decimal.TryParse(amountObj?.ToString(), out decimal amount))
        {
            return BadRequest("Invalid  'amount' format.");
        }

        if (amount <= 0)
        {
            return BadRequest("Deposit amount must be greater than zero.");
        }

        var clientId = GetClientIdFromToken();
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.ClientID == clientId);

        if (account == null)
        {
            return NotFound("Account not found or not associated with the current client.");
        }

        account.Balance += amount;

        // Log transaction
        var transaction = new Transaction
        {
            Account1ID = account.AccountID,
            Amount = amount,
            Date = DateTime.UtcNow,
            Type = "Deposit"
        };

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Deposit successful", balance = account.Balance });
    }


    // Transfer
    [HttpPost("transfer")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Transfer([FromBody] Dictionary<string, object> requestBody)
    {
        if (!requestBody.TryGetValue("targetClientId", out var targetClientIdObj) ||
            !requestBody.TryGetValue("amount", out var amountObj))
        {
            return BadRequest("Target client ID and amount are required.");
        }

        if (!int.TryParse(targetClientIdObj?.ToString(), out int targetClientId) ||
            !decimal.TryParse(amountObj?.ToString(), out decimal amount))
        {
            return BadRequest("Invalid input format.");
        }

        if (amount <= 0)
        {
            return BadRequest("Transfer amount must be greater than zero.");
        }

        var clientId = GetClientIdFromToken();

        // Prevent transferring to self
        if (clientId == targetClientId)
        {
            return BadRequest("You cannot transfer money to yourself.");
        }

        // Get the source account associated with the current client
        var sourceAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.ClientID == clientId);
        if (sourceAccount == null)
        {
            return NotFound("Source account not found or not associated with the current client.");
        }

        if (sourceAccount.Balance < amount)
        {
            return BadRequest("Insufficient funds.");
        }

        // Get the target account associated with the recipient client ID
        var targetAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.ClientID == targetClientId);
        if (targetAccount == null)
        {
            return NotFound("Target account not found for the specified client.");
        }

        // Perform the transfer
        sourceAccount.Balance -= amount;
        targetAccount.Balance += amount;

        // Log transactions
        var sourceTransaction = new Transaction
        {
            Account1ID = sourceAccount.AccountID,
            Account2ID = targetAccount.AccountID,
            Amount = amount,
            Date = DateTime.UtcNow,
            Type = "Transfer"
        };

        await _context.Transactions.AddAsync(sourceTransaction);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Transfer successful",
            sourceBalance = sourceAccount.Balance,
            targetBalance = targetAccount.Balance
        });
    }

    // Transaction
    [HttpGet("TransactionOut")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetTransactionOut()
    {
        var clientId = GetClientIdFromToken();

        // Get the account associated with the current client
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.ClientID == clientId);

        if (account == null)
        {
            return NotFound("Account not found or not associated with the current client.");
        }

        // Get all transactions associated with the account, excluding those of type "Deposit"
        var transactionsOut = await _context.Transactions
                                              .Where(t => t.Account1ID == account.AccountID && t.Type != "Deposit")
                                              .ToListAsync();

        if (transactionsOut == null || !transactionsOut.Any())
        {
            return NotFound("The client has no transactions.");
        }

        return Ok(transactionsOut);
    }



    // Transaction
    [HttpGet("TransactionIn")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetTransactionIn()
    {
        var clientId = GetClientIdFromToken();

        // Get the account associated with the current client
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.ClientID == clientId);

        if (account == null)
        {
            return NotFound("Account not found or not associated with the current client.");
        }

        // Get all transactions associated with the account
        var transactionsIn = await _context.Transactions
                                          .Where(t => t.Account2ID == account.AccountID || t.Type == "Deposit")
                                          .ToListAsync();

        if (transactionsIn == null || !transactionsIn.Any())
        {
            return NotFound("The client has no transactions.");
        }

        return Ok(transactionsIn);
    }


}
