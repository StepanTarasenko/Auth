using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.DateBase
{
    public static class DatabaseInitializer
    {
        public static async void Seed(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    context.Database.Migrate();

                    var roles = AppData.Roles.ToArray();

                    foreach (var role in roles)
                    {
                        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                        if (!context.Roles.Any(r => r.Name == role))
                        {
                            await roleManager.CreateAsync(new ApplicationRole { Name = role, NormalizedName = role.ToUpper() });
                        }
                    }

                    await Register(context, scope, new ApplicationUser
                    {
                        Email = "tarasenko@inexika.com",
                        FirstName = "Stepan",
                        LastName = "Tarasenko",
                    }, "#4>2tKHz~x?C");

                    await context.SaveChangesAsync();
                }
            }
        }
        private static async Task Register(ApplicationDbContext context, IServiceScope scope, ApplicationUser user, string passwordString)
        {
            user.UserName = user.Email;
            user.NormalizedEmail = user.Email.ToUpper();
            user.NormalizedUserName = user.NormalizedEmail;
            user.PhoneNumber = "+00000000000";
            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = true;
            user.SecurityStamp = Guid.NewGuid().ToString("D");


            if (!context.Users.Any(u => u.UserName == user.UserName))
            {
                var password = new PasswordHasher<ApplicationUser>();
                var hashed = password.HashPassword(user, passwordString);
                user.PasswordHash = hashed;
                var userStore = scope.ServiceProvider.GetService<ApplicationUserStore>();
                var result = await userStore.CreateAsync(user);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException("Cannot create account");
                }

                var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                var roles = AppData.Roles.ToArray();
                foreach (var role in roles)
                {
                    var roleAdded = await userManager.AddToRoleAsync(user, role);
                    if (roleAdded.Succeeded)
                    {
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
