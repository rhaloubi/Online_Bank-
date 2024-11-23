using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBank.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Client")]
public class FriendshipController : ControllerBase
{
    private readonly OnlineBankContext _context;

    public FriendshipController(OnlineBankContext context)
    {
        _context = context;
    }

    // Send Friend Request
    [HttpPost("send")]
    public async Task<IActionResult> SendFriendRequest(Friendship friendship)
    {
        var loggedInUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var targetUserExists = await _context.Clients.AnyAsync(c => c.ClientID == friendship.Friend2ID);
        if (!targetUserExists)
        {
            return BadRequest("No user found with the provided ID.");
        }

        if (loggedInUserId == friendship.Friend2ID)
        {
            return BadRequest("You cannot send a friend request to yourself.");
        }

        var existingFriendship = await _context.Friendships
            .FirstOrDefaultAsync(f =>
                (f.Friend1ID == loggedInUserId && f.Friend2ID == friendship.Friend2ID) ||
                (f.Friend1ID == friendship.Friend2ID && f.Friend2ID == loggedInUserId));

        if (existingFriendship != null)
        {
            return BadRequest("A friendship or request already exists.");
        }

        var newFriendship = new Friendship
        {
            Friend1ID = loggedInUserId,
            Friend2ID = friendship.Friend2ID,
            Status = "Pending"
        };

        _context.Friendships.Add(newFriendship);
        await _context.SaveChangesAsync();

        return Ok("Friend request sent successfully.");
    }

    // Accept Friend Request
    [HttpPost("accept")]
    public async Task<IActionResult> AcceptFriendRequest(Friendship friendship)
    {
        var loggedInUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var existingFriendship = await _context.Friendships
            .FirstOrDefaultAsync(f =>
                f.FriendshipID == friendship.FriendshipID &&
                f.Friend2ID == loggedInUserId &&
                f.Status == "Pending");

        if (existingFriendship == null)
        {
            return NotFound("Friend request not found or already processed.");
        }

        existingFriendship.Status = "Accepted";
        await _context.SaveChangesAsync();

        return Ok("Friend request accepted.");
    }

    // Remove Friend
    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveFriend(Friendship friendship)
    {
        var loggedInUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var existingFriendship = await _context.Friendships
            .FirstOrDefaultAsync(f =>
                f.FriendshipID == friendship.FriendshipID &&
                (f.Friend1ID == loggedInUserId || f.Friend2ID == loggedInUserId));

        if (existingFriendship == null)
        {
            return NotFound("Friendship not found.");
        }

        _context.Friendships.Remove(existingFriendship);
        await _context.SaveChangesAsync();

        return Ok("Friendship removed.");
    }

    // Show Friendships
    [HttpGet("list")]
    public async Task<IActionResult> GetFriendships()
    {
        var loggedInUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var friendships = await _context.Friendships
            .Include(f => f.Client1)
            .Include(f => f.Client2)
            .Where(f =>
                f.Friend1ID == loggedInUserId || f.Friend2ID == loggedInUserId)
            .Select(f => new
            {
                FriendshipID = f.FriendshipID,
                FriendName = f.Friend1ID == loggedInUserId ? f.Client2.Name : f.Client1.Name,
                FriendEmail = f.Friend1ID == loggedInUserId ? f.Client2.Email : f.Client1.Email,
                Status = f.Status
            })
            .ToListAsync();

        return Ok(friendships);
    }
}
