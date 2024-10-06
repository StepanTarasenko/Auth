using Auth.Infrastructure.DateBase;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using PricePoint.API.UnitOfWork;

namespace Auth.Infrastructure.Openiddict
{
    public static class OpenIdDictScopeConfig
    {
        public static async Task SeedScopes(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<ApplicationDbContext, ApplicationUser, ApplicationRole>>();

            var scopes = unitOfWork.GetRepository<OpenIddictEntityFrameworkCoreScope>().GetAll();
            var scopeManager = scope.ServiceProvider.GetRequiredService<OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope>>();
            // Проверка и создание скоупа "api1"
            if (scopes.FirstOrDefault(x => x.Name == "api1") == null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "api1",
                    DisplayName = "My API",
                    Resources = { "resource_server_1" } // Название ресурса, который защищает этот скоуп
                });
            }

            // Проверка и создание скоупа "api2"
            if (scopes.FirstOrDefault(x => x.Name == "api2") == null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "api2",
                    DisplayName = "Another API",
                    Resources = { "resource_server_2" }
                });
            }

            // Проверка и создание скоупа "offline_access"
            if (scopes.FirstOrDefault(x => x.Name == "offline_access") == null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "offline_access",
                    DisplayName = "Offline Access",
                    Resources = { "resource_server_1" } // Ресурс, который поддерживает offline access
                });
            }
        }
    }
}
