using MyApiWeb.Models.Entities;
using MyApiWeb.Services.Interfaces;

namespace MyApiWeb.Api.Data
{
    public static class DataSeeder
    {
        public static void Seed(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var userService = serviceScope.ServiceProvider.GetService<IUserService>();
                if (userService != null)
                {
                    SeedAdminUser(userService).Wait();
                }
            }
        }

        private static async Task SeedAdminUser(IUserService userService)
        {
            if (!await userService.UsernameExistsAsync("admin"))
            {
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = userService.HashPassword("123456"),
                    RealName = "Administrator",
                    IsActive = true
                };
                // In a real application, you would also assign roles here.
                // For now, we just create the user.
                await userService.RegisterAsync(new Models.DTOs.UserRegisterDto
                {
                    Username = adminUser.Username,
                    Email = adminUser.Email,
                    Password = "123456", // The service will hash this
                    RealName = adminUser.RealName
                });
            }
        }
    }
}