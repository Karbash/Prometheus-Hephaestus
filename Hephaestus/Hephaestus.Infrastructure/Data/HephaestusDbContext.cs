using Hephaestus.Domain.Entities;
using Hephaestus.Infrastructure.Configuration;
using Hephaestus.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Hephaestus.Infrastructure.Data;

public class HephaestusDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Additional> Additionals { get; set; }
    public DbSet<ProductionQueue> ProductionQueues { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<SalesLog> SalesLogs { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<MenuItemTag> MenuItemTags { get; set; }
    public DbSet<CompanyImage> CompanyImages { get; set; }
    public DbSet<CompanyOperatingHour> CompanyOperatingHours { get; set; }
    public DbSet<CompanySocialMedia> CompanySocialMedia { get; set; }
    public DbSet<MenuItemImage> MenuItemImages { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<CouponUsage> CouponUsages { get; set; }
    public DbSet<PromotionUsage> PromotionUsages { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Customization> Customizations { get; set; }
    public DbSet<MenuItemAdditional> MenuItemAdditionals { get; set; }
    public DbSet<OrderItemAdditional> OrderItemAdditionals { get; set; }
    public DbSet<OrderItemTag> OrderItemTags { get; set; }
    public DbSet<ConversationSession> ConversationSessions { get; set; }
    public DbSet<ConversationMessage> ConversationMessages { get; set; }

    public HephaestusDbContext(DbContextOptions<HephaestusDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new MenuItemConfiguration());
        modelBuilder.ApplyConfiguration(new AdditionalConfiguration());
        modelBuilder.ApplyConfiguration(new ProductionQueueConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new SalesLogConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewConfiguration());
        modelBuilder.ApplyConfiguration(new PromotionConfiguration());
        modelBuilder.ApplyConfiguration(new CouponConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new PasswordResetTokenConfiguration());
        modelBuilder.ApplyConfiguration(new TagConfiguration());
        modelBuilder.ApplyConfiguration(new MenuItemTagConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyImageConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyOperatingHourConfiguration());
        modelBuilder.ApplyConfiguration(new CompanySocialMediaConfiguration());
        modelBuilder.ApplyConfiguration(new MenuItemImageConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new CouponUsageConfiguration());
        modelBuilder.ApplyConfiguration(new PromotionUsageConfiguration());
        modelBuilder.ApplyConfiguration(new AddressConfiguration());
        modelBuilder.ApplyConfiguration(new CustomizationConfiguration());
        modelBuilder.ApplyConfiguration(new MenuItemAdditionalConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemAdditionalConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemTagConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationSessionConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationMessageConfiguration());
    }
}
