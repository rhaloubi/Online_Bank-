using Microsoft.EntityFrameworkCore;
using OnlineBank.Models;

public class OnlineBankContext : DbContext
{
    public OnlineBankContext(DbContextOptions<OnlineBankContext> options)
       : base(options)
    {
    }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<SocialLoan> SocialLoans { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<Notification> Notifications { get; set; }

  
}