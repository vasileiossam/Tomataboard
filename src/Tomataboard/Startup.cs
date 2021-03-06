﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tomataboard.Data;
using Tomataboard.Logger;
using Tomataboard.Models;
using Tomataboard.Services;
using Tomataboard.Services.AccessTokens;
using Tomataboard.Services.Cache;
using Tomataboard.Services.Encryption;
using Tomataboard.Services.Greetings;
using Tomataboard.Services.Locations;
using Tomataboard.Services.Locations.Freegeoip;
using Tomataboard.Services.Locations.GeoLite;
using Tomataboard.Services.Locations.IpGeolocation;
using Tomataboard.Services.Photos;
using Tomataboard.Services.Photos.Api500px;
using Tomataboard.Services.Photos.Flickr;
using Tomataboard.Services.Photos.Tirolography;
using Tomataboard.Services.Quotes;
using Tomataboard.Services.Weather;
using Tomataboard.Services.Weather.Forecast;
using Tomataboard.Services.Weather.OpenWeatherMap;
using Tomataboard.Services.Weather.Yahoo;

namespace Tomataboard
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("keys.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                // will cascade over appsettings.json
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TomataboardContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("TomataboardConnection")));
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("TomataboardConnection")));

            services.AddIdentity<ApplicationUser, ApplicationUserRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 6;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.SignIn.RequireConfirmedEmail = true;
                    options.Cookies.ApplicationCookie.LoginPath = "/Account/Login";
                    options.Cookies.ApplicationCookie.Events = new CookieAuthenticationEvents()
                    {
                        OnRedirectToLogin = async ctx =>
                        {
                            // return 401 unauthorized when accessing the api and not logged in
                            // instead of redirecting and returning the html for the login view
                            if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                            {
                                ctx.Response.StatusCode = 401;
                            }
                            else
                            {
                                ctx.Response.Redirect(ctx.RedirectUri);
                            }
                            await Task.Yield();
                        }
                    };
                }
            ).AddEntityFrameworkStores<ApplicationDbContext>()
             .AddDefaultTokenProviders();

            services.AddMvc()
                .AddJsonOptions(
                    // will camel case all web api responses
                    opt => opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver());

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            // location services
            services.AddTransient<ILocationProvider, LocationProvider>();
            services.AddTransient<IFreegeoipService, FreegeoipService>();
            services.AddTransient<IGeoLiteService, GeoLiteService>();
            services.AddTransient<IIpGeolocationService, IpGeolocationService>();
            services.AddTransient<ICacheRepository<Location>, CacheRepository<Location>>();

            // photo services
            services.AddTransient<IPhotoProvider, PhotoProvider>();
            services.AddTransient<IApi500px, Api500px>();
            services.AddTransient<IFlickrService, FlickrService>();
            services.AddTransient<ITirolographyService, TirolographyService>();
            services.AddTransient<ICacheRepository<List<Photo>>, CacheRepository<List<Photo>>>();

            // weather services
            services.AddTransient<IWeatherProvider, WeatherProvider>();
            services.AddTransient<IYahooWeatherService, YahooWeatherService>();
            services.AddTransient<IOpenWeatherMapService, OpenWeatherMapService>();
            services.AddTransient<IForecastService, ForecastService>();
            services.AddTransient<ICacheRepository<WeatherConditions>, CacheRepository<WeatherConditions>>();

            services.AddTransient<IEncryptionService, EncryptionService>();
            services.AddTransient<ICookiesService<OauthToken>, CookiesService<OauthToken>>();
            services.AddTransient<IAccessTokensRepository, AccessTokensRepository>();
            services.AddTransient<IQuoteRepository, QuoteRepository>();

            services.AddLogging();

            services.Configure<DataSettings>(Configuration.GetSection("Data:DefaultConnection"));
            services.Configure<Api500pxKeys>(Configuration.GetSection("ApiSettings:Api500pxKeys"));
            services.Configure<FlickrServiceKeys>(Configuration.GetSection("ApiSettings:FlickrKeys"));
            services.Configure<YahooWeatherServiceKeys>(Configuration.GetSection("ApiSettings:YahooWeatherKeys"));
            services.Configure<OpenWeatherMapKeys>(Configuration.GetSection("ApiSettings:OpenWeatherMapKeys"));
            services.Configure<ForecastKeys>(Configuration.GetSection("ApiSettings:ForecastKeys"));
            services.Configure<EncryptionServiceKeys>(Configuration.GetSection("ApiSettings:EncryptionServiceKeys"));
            services.Configure<SendGridOptions>(Configuration.GetSection("ApiSettings:SendGridOptions"));
            services.Configure<GmailOptions>(Configuration.GetSection("ApiSettings:GmailOptions"));
            services.Configure<EmailLoggerOptions>(Configuration.GetSection("ApiSettings:EmailLoggerOptions"));

            services.AddScoped<IViewRenderService, ViewRenderService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
            ILoggerFactory loggerFactory, 
            IEmailSender emailSender,
            IOptions<EmailLoggerOptions> emailLoggerOptions)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddConsole(LogLevel.Debug);
            loggerFactory.AddDebug(LogLevel.Debug);
            loggerFactory.AddEmail(emailSender, emailLoggerOptions, LogLevel.Critical);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                //app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // For more details on creating database during deployment see http://go.microsoft.com/fwlink/?LinkID=615859
                //try
                //{
                //    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                //        .CreateScope())
                //    {
                //        serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
                //             .Database.Migrate();
                //    }
                //}
                //catch { }
            }

            app.UseStaticFiles();

            app.UseIdentity();

            //app.Use(next =>
            //{
            //    return ctx =>
            //    {
            //        ctx.Response.Headers.Remove("Server");
            //        return next(ctx);
            //    };
            //});

            // To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715
            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                       name: "dashboard",
                       template: "focus/{controller=Dashboard}/{action=Index}");
            });
        }
    }
}