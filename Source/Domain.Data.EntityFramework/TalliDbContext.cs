namespace My.Talli.Domain.Data.EntityFramework;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

    public DbSet<Expense> Expenses { get; set; } = null!;

    public DbSet<ExpenseEtsy> ExpenseEtsys { get; set; } = null!;

    public DbSet<ExpenseGumroad> ExpenseGumroads { get; set; } = null!;

    public DbSet<ExpenseManual> ExpenseManuals { get; set; } = null!;

    public DbSet<ExpenseStripe> ExpenseStripes { get; set; } = null!;

    public DbSet<Goal> Goals { get; set; } = null!;

    public DbSet<GoalType> GoalTypes { get; set; } = null!;

    public DbSet<Heartbeat> Heartbeats { get; set; } = null!;

    public DbSet<Order> Orders { get; set; } = null!;

    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    public DbSet<Payout> Payouts { get; set; } = null!;

    public DbSet<PayoutEtsy> PayoutEtsys { get; set; } = null!;

    public DbSet<PayoutGumroad> PayoutGumroads { get; set; } = null!;

    public DbSet<PayoutManual> PayoutManuals { get; set; } = null!;

    public DbSet<PayoutStripe> PayoutStripes { get; set; } = null!;

    public DbSet<Product> Products { get; set; } = null!;

    public DbSet<ProductType> ProductTypes { get; set; } = null!;

    public DbSet<PlatformConnection> PlatformConnections { get; set; } = null!;

    public DbSet<ProductVendor> ProductVendors { get; set; } = null!;

    public DbSet<Revenue> Revenues { get; set; } = null!;

    public DbSet<RevenueEtsy> RevenueEtsys { get; set; } = null!;

    public DbSet<RevenueGumroad> RevenueGumroads { get; set; } = null!;

    public DbSet<RevenueManual> RevenueManuals { get; set; } = null!;

    public DbSet<RevenueStripe> RevenueStripes { get; set; } = null!;

    public DbSet<ShopConnection> ShopConnections { get; set; } = null!;

    public DbSet<ShopConnectionEtsy> ShopConnectionEtsys { get; set; } = null!;

    public DbSet<Subscription> Subscriptions { get; set; } = null!;

    public DbSet<SubscriptionStripe> SubscriptionStripes { get; set; } = null!;

    public DbSet<Suggestion> Suggestions { get; set; } = null!;

    public DbSet<SuggestionVote> SuggestionVotes { get; set; } = null!;

    public DbSet<SystemSetting> SystemSettings { get; set; } = null!;

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

        ApplyUtcDateTimeConverter(modelBuilder);
    }

    // SQL Server datetime2 stores no timezone, and EF Core hydrates the value back as DateTimeKind.Unspecified.
    // Provider SDKs (Stripe.net's DateRangeOptions, anything calling .ToUniversalTime() or DateTimeOffset.ToUnixTimeSeconds())
    // treat Unspecified as Local and silently shift the value by the local timezone offset. By convention every DateTime we
    // store is UTC, so apply a project-wide value converter that stamps Kind=Utc on read and normalizes Local→Utc on write.
    private static void ApplyUtcDateTimeConverter(ModelBuilder modelBuilder)
    {
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Local ? v.ToUniversalTime() : v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableUtcConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue && v.Value.Kind == DateTimeKind.Local ? v.Value.ToUniversalTime() : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(utcConverter);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(nullableUtcConverter);
            }
        }
    }


    #endregion
}
