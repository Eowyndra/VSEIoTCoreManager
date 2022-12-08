// ----------------------------------------------------------------------------
// Filename: Startup.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp
{
    using System.Text.Json.Serialization;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using VSEIoTCoreServer.DAL;
    using VSEIoTCoreServer.WebApp.Models;
    using VSEIoTCoreServer.WebApp.Services;
    using VSEIoTCoreServer.WebApp.ViewModels;

    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Env = env;
            Configuration = configuration;
        }

        private IWebHostEnvironment Env { get; }
        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<IoTCoreOptions>(Configuration.GetSection(IoTCoreOptions.IoTCoreSettings));
            services.AddAutoMapper(typeof(AutoMapperProfile));

            var conStr = Configuration.GetConnectionString("SQLiteConnection");
            services.AddDbContext<SQLiteDbContext>(options => options.UseSqlite(conStr));

            services.AddScoped<IDeviceConfigurationService, DeviceConfigurationService>();
            services.AddScoped<IGlobalConfigurationService, GlobalConfigurationService>();

            // Main IoT Core must be singleton otherwise you would create multiple different iotcores - this might be the issue of not being able to access remote/1/ ...
            services.AddSingleton<IIoTCoreServer, IoTCoreServer>();

            services.AddControllersWithViews()
                .AddJsonOptions(p => p.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
                .AddNewtonsoftJson(p =>
                {
                    p.SerializerSettings.Converters.Add(new StringEnumConverter());
                    p.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    p.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });
            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IDeviceConfigurationService deviceConfigurationService, IIoTCoreServer iotCoreServer)
        {
            // Configure the HTTP request pipeline.
            if (!Env.IsDevelopment())
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            else
            {
                // Only in Dev mode
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller}/{action=Index}/{id?}");
                    endpoints.MapFallbackToFile("index.html");
                });

            // Populate in-memory cache
            if (deviceConfigurationService != null)
            {
                var dbDevices = deviceConfigurationService.GetAll().GetAwaiter().GetResult();
                foreach (var dbDevice in dbDevices)
                {
                    iotCoreServer?.Add(dbDevice).GetAwaiter().GetResult();
                }
            }
        }
    }
}
