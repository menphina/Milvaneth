using MessagePack.AspNetCoreMvcFormatter;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvaneth.Server.Models;
using Milvaneth.Server.Service;
using Milvaneth.Server.Service.Implement;
using Milvaneth.Server.Statics;
using System;

namespace Milvaneth.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var config = GetConfig();
            var connection = GetConnectionString(config);

            GlobalConfig.Section = config.GetSection("ServerConfig");

            services.AddDbContextPool<MilvanethDbContext>(options => options.UseNpgsql(connection));
            services.AddHttpContextAccessor();

            services.TryAddSingleton(x => config);
            services.TryAddSingleton<ITimeService, CachedTimeService>();
            services.TryAddSingleton<ISrp6Service, Srp6MultiStepService>();
            services.TryAddSingleton<IPowService, ProofOfWorkService>();
            services.TryAddSingleton<IVerifyMailService, SmtpVerifyMailService>();

            services.TryAddScoped<IAuthentication, TokenKeyAuthenticationService>();
            services.TryAddScoped<IApiKeySignService, AuditedApiKeySignService>();
            services.TryAddScoped<ITokenSignService, AuditedTokenSignService>();
            services.TryAddScoped<IRepository, DataRepository>();

            services.AddMvc()
                .AddMvcOptions(option =>
                {
                    option.OutputFormatters.Add(new MessagePackOutputFormatter(ContractlessStandardResolver.Instance));
                    option.InputFormatters.Add(new MessagePackInputFormatter(ContractlessStandardResolver.Instance));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private IConfigurationRoot GetConfig()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var basePath = AppDomain.CurrentDomain.BaseDirectory;

            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .AddEnvironmentVariables();

            return builder.Build();
        }

        private string GetConnectionString(IConfigurationRoot config)
        {
            var connectionString = config.GetConnectionString("DbConnect");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Could not find a connection string named 'Default'.");
            }

            return connectionString;
        }
    }
}
