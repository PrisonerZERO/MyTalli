namespace My.Talli.Domain.Data.EntityFramework;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>Database Context</summary>
public class TalliDbContext : DbContext
{
    #region <Constructors>

    public TalliDbContext(DbContextOptions<TalliDbContext> options) : base(options) { }


    #endregion

    #region <Properties>

    public SemaphoreSlim ConcurrencyLock { get; } = new(1, 1);

    public DbSet<AuthenticatedUser> AuthenticatedUsers { get; set; } = null!;

    public DbSet<Billing> Billings { get; set; } = null!;

    public DbSet<BillingStripe> BillingStripes { get; set; } = null!;

    public DbSet<Order> Orders { get; set; } = null!;

    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    public DbSet<Product> Products { get; set; } = null!;

    public DbSet<ProductType> ProductTypes { get; set; } = null!;

    public DbSet<ProductVendor> ProductVendors { get; set; } = null!;

    public DbSet<Revenue> Revenues { get; set; } = null!;

    public DbSet<RevenueManual> RevenueManuals { get; set; } = null!;

    public DbSet<Subscription> Subscriptions { get; set; } = null!;

    public DbSet<SubscriptionStripe> SubscriptionStripes { get; set; } = null!;

    public DbSet<Suggestion> Suggestions { get; set; } = null!;

    public DbSet<SyncQueue> SyncQueues { get; set; } = null!;

    public DbSet<SuggestionVote> SuggestionVotes { get; set; } = null!;

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<UserAuthenticationApple> UserAuthenticationApples { get; set; } = null!;

    public DbSet<UserAuthenticationGoogle> UserAuthenticationGoogles { get; set; } = null!;

    public DbSet<UserAuthenticationMicrosoft> UserAuthenticationMicrosofts { get; set; } = null!;

    public DbSet<UserRole> UserRoles { get; set; } = null!;


    #endregion

    #region <Methods>

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TalliDbContext).Assembly);
    }


    #endregion
}
