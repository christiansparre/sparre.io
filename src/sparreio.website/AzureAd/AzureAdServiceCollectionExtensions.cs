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
        public static IServiceCollection AddAzureAdAuthentication(this IServiceCollection services)
        {
            // Move to config binding
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            });

            services.AddSingleton<IConfigureOptions<AzureAdOptions>, BindAzureAdOptions>();
            services.AddSingleton<IPostConfigureOptions<OpenIdConnectOptions>, PostConfigureAzureOptions>();
            services.AddOpenIdConnectAuthentication();
            services.AddCookieAuthentication();
            return services;
        }

        private class BindAzureAdOptions : ConfigureOptions<AzureAdOptions>
        {
            public BindAzureAdOptions(IConfiguration config) :
                base(options => config.GetSection("AzureAd").Bind(options))
            { }
        }

        private class PostConfigureAzureOptions: IPostConfigureOptions<OpenIdConnectOptions>
        {
            private readonly AzureAdOptions _azureOptions;

            public PostConfigureAzureOptions(IOptions<AzureAdOptions> azureOptions)
            {
                _azureOptions = azureOptions.Value;
            }

            public void PostConfigure(string name, OpenIdConnectOptions options)
            {
                options.ClientId = _azureOptions.ClientId;
                options.Authority = $"{_azureOptions.Instance}{_azureOptions.TenantId}";
                options.UseTokenLifetime = true;
                options.CallbackPath = _azureOptions.CallbackPath;
                options.RequireHttpsMetadata = false;
            }
        }
    }
}
