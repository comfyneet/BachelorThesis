﻿using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RiceDoctor.OntologyManager;
using RiceDoctor.QueryManager;
using RiceDoctor.RuleManager;
using Manager = RiceDoctor.OntologyManager.Manager;

namespace RiceDoctor.WebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddSingleton<IOntologyManager>(Manager.Instance);

            var problemData = File.ReadAllText(Path.Combine(AppContext.BaseDirectory,
                @"..\..\..\..\Resources\problem-types.json"));
            var logicData = File.ReadAllText(Path.Combine(AppContext.BaseDirectory,
                @"..\..\..\..\Resources\logic-rules.txt"));
            var relationData = File.ReadAllText(Path.Combine(AppContext.BaseDirectory,
                @"..\..\..\..\Resources\relation-rules.txt"));
            services.AddSingleton<IRuleManager>(new RuleManager.Manager(problemData, logicData, relationData));

            var queryData = File.ReadAllText(Path.Combine(AppContext.BaseDirectory,
                @"..\..\..\..\Resources\query-rules.txt"));
            services.AddSingleton<IQueryManager>(new QueryManager.Manager(queryData));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseSession();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}