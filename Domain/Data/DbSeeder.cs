using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductManagementSystem.Domain.Data;
using ProductManagementSystem.Domain.Entities;

namespace ProductManagementSystem.Domain.Data
{
    public static class DbSeeder
    {
        public static async Task SeedData(
    DataContext context, 
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager)
        {
            // Seed Roles
            if (!await roleManager.Roles.AnyAsync())
            {
                var roles = new[] { "Admin", "User" };
                foreach (var role in roles)
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }

            // Seed Users
            if (!await userManager.Users.AnyAsync())
            {
                var users = new List<AppUser>
                {
                    new AppUser
                    {
                        UserName = "admin@example.com",
                        Email = "admin@example.com",
                        FirstName = "Admin",
                        LastName = "User",
                        EmailConfirmed = true,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        LastLoginDate = null
                    },
                    new AppUser
                    {
                        UserName = "user@example.com",
                        Email = "user@example.com",
                        FirstName = "Regular",
                        LastName = "User",
                        EmailConfirmed = true,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        LastLoginDate = null
                    }
                };

                foreach (var user in users)
                {
                    var result = await userManager.CreateAsync(user, "Password123!"); // Default password for testing
                    if (result.Succeeded)
                    {
                        // Assign roles based on email
                        if (user.Email == "admin@example.com")
                        {
                            await userManager.AddToRoleAsync(user, "Admin");
                        }
                        else
                        {
                            await userManager.AddToRoleAsync(user, "User");
                        }
                    }
                }
            }

            // Get admin user for product ownership
            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            if (adminUser != null && !await context.Products.AnyAsync())
            {
                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Sample Product 1",
                        Description = "This is a sample product description for product 1",
                        Price = 99.99m,
                        StockQuantity = 100,
                        Category = "Electronics",
                        SKU = "PROD-001",
                        ImageUrl = null,
                        Weight = 1.5m,
                        Tags = "electronics,gadget,new",
                        IsAvailable = true,
                        UserId = adminUser.Id.ToString(),
                        CreatedDate = DateTime.Now,
                        CreatedBy = adminUser.Id.ToString(),
                        IsActive = true,
                        IsDeleted = false
                    },
                    new Product
                    {
                        Name = "Sample Product 2",
                        Description = "This is a sample product description for product 2",
                        Price = 149.99m,
                        StockQuantity = 50,
                        Category = "Home & Living",
                        SKU = "PROD-002",
                        ImageUrl = null,
                        Weight = 2.5m,
                        Tags = "home,living,furniture",
                        IsAvailable = true,
                        UserId = adminUser.Id.ToString(),
                        CreatedDate = DateTime.Now,
                        CreatedBy =  adminUser.Id.ToString(),
                        IsActive = true,
                        IsDeleted = false
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }
    }
}