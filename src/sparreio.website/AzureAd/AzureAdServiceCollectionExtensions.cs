using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication.Extensions
{
    public static class AzureAdServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureAdAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var azureOptions = new AzureAdOptions();
            configuration.GetSection("AzureAd").Bind(azureOptions);

            // Move to config binding
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                options.ClientId = azureOptions.ClientId;
                options.Authority = $"{azureOptions.Instance}{azureOptions.TenantId}";
                options.UseTokenLifetime = true;
                options.CallbackPath = azureOptions.CallbackPath;
                options.RequireHttpsMetadata = false;
            });

            return services;
        }
    }
}
