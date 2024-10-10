using Auth.Infrastructure.DateBase;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using PricePoint.API.UnitOfWork;

namespace Auth.Infrastructure.Openiddict
{
    public static class OpenIdDictClientConfig
    {
        public static async Task SeedClients(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<ApplicationDbContext, ApplicationUser, ApplicationRole>>();

            var clients = unitOfWork.GetRepository<OpenIddictEntityFrameworkCoreApplication>().GetAll();
            var clientManager = scope.ServiceProvider.GetRequiredService<OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication>>();

            // Проверка и создание клиента "price_point"
            if (clients.FirstOrDefault(x => x.ClientId == "price_point") == null)
            {
                await clientManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "price_point",
                    ClientSecret = "secret",
                    Permissions =
                {
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    // Используем наш скоуп offline_access
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Address,
                    "api1", // Используем пользовательский скоуп
                    "offline_access" // Скоуп для offline access
                },
                    DisplayName = "Price Point Client",
                });
            }

            // Проверка и создание клиента "api_access_code"
            if (clients.FirstOrDefault(x => x.ClientId == "api_access_code") == null)
            {
                await clientManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "api_access_code",
                    ClientSecret = "secret",
                    Permissions =
                {
                    "delegation", // Собственный грант
                    "api1",
                    "api2" // Пользовательские скоупы
                },
                    DisplayName = "API Access Code Client",
                });
            }

            // Проверка и создание клиента "scheduler"
            if (clients.FirstOrDefault(x => x.ClientId == "scheduler") == null)
            {
                await clientManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "scheduler",
                    ClientSecret = "secret",
                    Permissions =
                {
                    "scheduler", // Собственный грант
                    "api1",
                    "api2"
                },
                    DisplayName = "Scheduler Client"
                });
            }
        }
    }
}
