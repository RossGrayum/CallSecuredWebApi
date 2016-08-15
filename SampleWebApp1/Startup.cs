using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SampleWebApp1
{
    public class Startup
    {
        internal static string AadInstance { get; private set; }
        internal static string TenantId { get; private set; }
        internal static string Tenant { get; private set; }
        internal static string Authority { get; private set; }
        internal static string CallbackPath { get; private set; }
        internal static string ClientId { get; private set; }
        internal static string ClientSecret { get; private set; }
        internal static string ServiceAppIdUri { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();

                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc();

            services.AddAuthentication(
                SharedOptions => SharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseCookieAuthentication();

            Startup.AadInstance = Configuration["Authentication:AzureAd:AADInstance"];
            Startup.TenantId = Configuration["Authentication:AzureAd:TenantId"];
            Startup.Tenant = Configuration["Authentication:AzureAd:Domain"];
            Startup.Authority = Startup.AadInstance + Startup.TenantId;
            Startup.CallbackPath = Configuration["Authentication:AzureAd:CallbackPath"];
            Startup.ClientId = Configuration["Authentication:AzureAd:ClientId"];
            Startup.ClientSecret = Configuration["Authentication:AzureAd:ClientSecret"];
            Startup.ServiceAppIdUri = Configuration["Authentication:AzureAd:ServiceAppIdUri"];

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                ClientId = Startup.ClientId,
                Authority = Startup.Authority,
                CallbackPath = Startup.CallbackPath
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
