using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Auth.Controllers
{
    public static class AuthorizationEndpoints
    {
        public static WebApplication MapAuthorizationEndpoints(this WebApplication app)
        {
            app.MapPost("/auth/connect/token", Exchange);
            return app;
        }

        public static async Task<IResult> Exchange(HttpContext httpContext, IOpenIddictApplicationManager applicationManager, IOpenIddictScopeManager scopeManager)
        {
            OpenIddictRequest? request = httpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (request.IsPasswordGrantType())
            {
                object? application = await applicationManager.FindByClientIdAsync(request.ClientId!)
                    ?? throw new InvalidOperationException("The application details cannot be found in the database.");

                var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);
                identity.SetClaim(Claims.Subject, "F62M6T5SCH4ZCCGXJLA534633HPYQJYF")
                    .SetClaim(Claims.Email, "tarasenko@inexika.com")
                    .SetClaim(Claims.Name, "Tarasenko")
                    .SetClaim(Claims.PreferredUsername, "Tarasenko")
                    .SetClaim("TwoFactorEnabled", "false")
                    .SetClaims(Claims.Role, ["SystemAdministrator", "BusinessOwner"]);

                // Set the list of scopes granted to the client application.
                identity.SetScopes(new[]
                {
                Scopes.OpenId,
                Scopes.Email,
                Scopes.Profile,
                Scopes.Roles
            }.Intersect(request.GetScopes()));

                identity.SetDestinations(GetDestinations);

                //identity.AddClaim(OpenIddictConstants.Claims.Subject, "18b70468-478e-4f01-a6a7-05007cc454ef");
                //identity.AddClaim("client_id", "price_point");

                //// Добавляем кастомные claims.
                //identity.AddClaim("name", "tarasenko@inexika.com"); //
                //identity.AddClaim(ClaimTypes.Email, "tarasenko@inexika.com"); //
                //identity.AddClaim("given_name", "Stepan"); //
                //identity.AddClaim(ClaimTypes.Surname, "Tarasenko"); //
                //identity.AddClaim(ClaimTypes.Role, "SystemAdministrator"); //
                //identity.AddClaim(ClaimTypes.Role, "BusinessOwner"); //
                //identity.AddClaim(ClaimTypes.Role, "Manager"); //
                //identity.AddClaim(ClaimTypes.Role, "SuperSystemAdministrator"); //
                //identity.AddClaim("email_verified", "True"); //
                //identity.AddClaim("TwoFactorEnabled", "false"); //
                //identity.AddClaim(ClaimTypes.Actor, "Director"); //
                //identity.AddClaim("AspNet.Identity.SecurityStamp", "F62M6T5SCH4ZCCGXJLA534633HPYQJYF"); //

                ////// Добавляем scope и другие параметры.
                //var scopes = new List<string> { "api1", "openid" };
                //identity.AddClaim(OpenIddictConstants.Claims.Scope, string.Join(" ", scopes)); //

                //// Добавляем данные по времени.
                //identity.AddClaim(OpenIddictConstants.Claims.IssuedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()); 
                //identity.AddClaim(new Claim("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)); //
                //identity.AddClaim(new Claim("exp", DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
                //identity.AddClaim(OpenIddictConstants.Claims.JwtId, Guid.NewGuid().ToString());


                ClaimsPrincipal principal = new(identity);

                return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new NotImplementedException("The specified grant type is not implemented.");
        }

        static IEnumerable<string> GetDestinations(Claim claim)
        {
            switch (claim.Type)
            {
                case Claims.Name or Claims.PreferredUsername:
                    yield return Destinations.AccessToken;

                    if (claim.Subject is not null && claim.Subject.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

                    if (claim.Subject is not null && claim.Subject.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if (claim.Subject is not null && claim.Subject.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;

                    yield break;

                case "AspNet.Identity.SecurityStamp": yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }
    }
}
