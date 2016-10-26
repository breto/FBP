using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FBP.Data;
using FBP.Models;
using FBP.Services;
using FBP.Services.Impl;
using FBP.Dao;
using FBP.Dao.Sql;
using FBP.Service;
using FBP.Service.Impl;
using Hangfire;
using System.Diagnostics;
using FBP.Logging;
using Microsoft.AspNetCore.Identity;

namespace FBP
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            System.Diagnostics.Trace.TraceError("Starting FBP");
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            System.Diagnostics.Trace.TraceError("Finished starting FBP");
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ConnectionStringFBP")));

            // Add Identity services to the services container.

            services.AddIdentity<ApplicationUser, IdentityRole>(o => {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 5; })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<AppSettings>(options => Configuration.GetSection("ConnectionStrings").Bind(options));
            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddTransient<IMatchupService, MatchupServiceImpl>();

            services.AddHangfire(config =>
            {
                config.UseSqlServerStorage(Configuration.GetConnectionString("ConnectionStringFBP"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            //loggerFactory.AddFbpLogger(LogLevel.Information, @"c:\temp\logtest\", "fbp.log", 2, 100000);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseDeveloperExceptionPage();
                //app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            app.UseHangfireDashboard();

            app.UseHangfireServer();

            RecurringJob.AddOrUpdate<IMatchupService>(r => r.updateMatchups(), Cron.Minutely);

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            //configureRoles(app.ApplicationServices.GetService<ApplicationDbContext>());
        }
        
    }
}
