using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.Security.CAS;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookieSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Setup based on https://github.com/aspnet/Security/tree/rel/2.0.0/samples/SocialSample
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o =>
                {
                    o.LoginPath = new PathString("/login");

                    o.AccessDeniedPath = new PathString("/access-denied");

                    o.Cookie = new CookieBuilder
                    {
                        Name = ".AspNetCore.CasSample"
                    };

                    o.Events = new CookieAuthenticationEvents
                    {
                        // Add user roles to the existing identity.  
                        // This example is giving every user "User" and "Admin" roles.
                        // You can use services or other logic here to determine actual roles for your users.
                        OnSigningIn = context =>
                        {
                            // Use `GetRequiredService` if you have a service that is using DI or an EF Context.
                            // var username = context.Principal.Identity.Name;
                            // var userSvc = context.HttpContext.RequestServices.GetRequiredService<UserService>();
                            // var roles = userSvc.GetRoles(username);
                            
                            // Hard coded roles.
                            var roles = new[] { "User", "Admin" };

                            // `AddClaim` is not available directly from `context.Principal.Identity`.
                            // We can add a new empty identity with the roles we want to the principal. 
                            var identity = new ClaimsIdentity();
                            
                            foreach (var role in roles)
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, role));
                            }

                            context.Principal.AddIdentity(identity);

                            return Task.FromResult(0);
                        }
                    };
                })
                .AddCAS(o =>
                {
                    o.CasServerUrlBase = Configuration["CasBaseUrl"];   // Set in `appsettings.json` file.
                    o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                });

            services.AddMvc();
            
            //// You can make the site require Authorization on all endpoints by default:
            //var globalAuthPolicy = new AuthorizationPolicyBuilder()
            //    .RequireAuthenticatedUser()
            //    .Build();

            //services.AddMvc(options =>
            //{
            //    options.Filters.Add(new AuthorizeFilter(globalAuthPolicy));
            //});
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvcWithDefaultRoute();
        }
    }
}
