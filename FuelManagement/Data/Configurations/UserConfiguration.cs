using FuelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace FuelManagement.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Table name
            builder.ToTable("Users");

            // Primary key
            builder.HasKey(u => u.Id);

            // Identity column
            builder.Property(u => u.Id)
                   .UseIdentityColumn();

            // FullName
            builder.Property(u => u.FullName)
                   .IsRequired()
                   .HasMaxLength(200);

            // Email
            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(256);

            // PasswordHash
            builder.Property(u => u.PasswordHash)
                   .IsRequired()
                   .HasMaxLength(500);

            // Role (stored as string)
            builder.Property(u => u.Role)
                   .IsRequired()
                   .HasMaxLength(20);

            // IsActive
            builder.Property(u => u.IsActive)
                   .IsRequired()
                   .HasDefaultValue(true);

            // CreatedAt
            builder.Property(u => u.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            // Unique Email index
            builder.HasIndex(u => u.Email)
                   .IsUnique()
                   .HasDatabaseName("IX_Users_Email");

            // Performance indexes
            builder.HasIndex(u => u.Role)
                   .HasDatabaseName("IX_Users_Role");

            builder.HasIndex(u => u.IsActive)
                   .HasDatabaseName("IX_Users_IsActive");

            builder.HasIndex(u => u.CreatedAt)
                   .HasDatabaseName("IX_Users_CreatedAt");

            // Seed admin user - Use string "Admin" not enum
            builder.HasData(new User
            {
                Id = 1,
                FullName = "System Administrator",
                Email = "admin@fuelms.com",
                PasswordHash = "AQAAAAIAAYagAAAAEI5X8VK7Qq0Qq0Qq0Qq0Qq0Qq0Qq0Qq0Qq0Qq0Qq0Qq0Qq0Qq0Qq0Q",
                Role = "Admin", // MUST BE STRING
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}