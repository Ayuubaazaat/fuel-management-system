using FuelManagement.Models;
using FuelManagement.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;


namespace FuelManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }

        public DbSet<FuelSale> FuelSales { get; set; }

        public DbSet<Inventory> Inventories { get; set; }

        public DbSet<Fleet> Fleet { get; set; }

        public DbSet<Pump> Pumps { get; set; }

        public DbSet<Trip> Trips { get; set; }

        public DbSet<Compensation> Compensations { get; set; }

        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<Receipt> Receipts { get; set; }

        public DbSet<FuelClient> FuelClients { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Shift> Shifts { get; set; }

        // Future DbSets for other modules
        // public DbSet<Vehicle> Vehicles { get; set; }
        // public DbSet<Compensation> Compensations { get; set; }
        // public DbSet<Invoice> Invoices { get; set; }
        // public DbSet<Receipt> Receipts { get; set; }
        // public DbSet<Report> Reports { get; set; }
        // public DbSet<Notification> Notifications { get; set; }
        // public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());

            // NO CONFIGURATION FOR INVENTORY - Let EF Core use conventions
            // This will map to your Inventories table automatically

            // Apply configurations for other entities as they are created
            // modelBuilder.ApplyConfiguration(new FuelSaleConfiguration());
            // modelBuilder.ApplyConfiguration(new PumpConfiguration());
            // modelBuilder.ApplyConfiguration(new VehicleConfiguration());
            // modelBuilder.ApplyConfiguration(new CompensationConfiguration());
            // modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
            // modelBuilder.ApplyConfiguration(new ReceiptConfiguration());
            // modelBuilder.ApplyConfiguration(new ReportConfiguration());
            // modelBuilder.ApplyConfiguration(new NotificationConfiguration());
            // modelBuilder.ApplyConfiguration(new SettingConfiguration());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // This is for design-time migrations
            // Connection string should be in appsettings.json for runtime
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");
            }
        }
    }
}