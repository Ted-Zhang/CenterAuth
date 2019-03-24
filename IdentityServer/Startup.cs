// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Reflection;
using Autofac;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        private readonly IConfiguration _configuration;

        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // uncomment, if you wan to add an MVC-based UI
            //services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            var dbConnectionString = _configuration["DBConnectionString"];

            var builder = services.AddIdentityServer()                
                .AddConfigurationStore(options =>
                    {
                        options.ConfigureDbContext = b =>
                            b.UseSqlServer(dbConnectionString,
                                sql => sql.MigrationsAssembly(migrationsAssembly));
                    })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(dbConnectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                });

            builder.AddDeveloperSigningCredential();

            /*
            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }
            */
        }

        public void Configure(IApplicationBuilder app)
        {
            //if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // uncomment if you want to support static files
            //app.UseStaticFiles();

            app.UseIdentityServer();

            // uncomment, if you wan to add an MVC-based UI
            //app.UseMvcWithDefaultRoute();

            InitializeDatabase(app);
        }

        // Register service here.
        public void ConfigureContainer(ContainerBuilder builder)
        {
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    //
                    var client = new Client
                    {
                        ClientId = "TestClientForPhotoLake",
                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        ClientSecrets =
                        {
                            // This needs to comes from azure vault.

                            new Secret("testclientphotolake".Sha256())
                        },
                        AllowedScopes = {"PhotoLakeAPI"}
                    };

                    context.Clients.Add(client.ToEntity());
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    var resource = new IdentityResources.OpenId();
                    context.IdentityResources.Add(resource.ToEntity());
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    var apiResource = new ApiResource("PhotoLakeAPI", "The API for upload photo to Photo Lake APP");
                    context.ApiResources.Add(apiResource.ToEntity());
                    context.SaveChanges();
                }
            }
        }
    }
}