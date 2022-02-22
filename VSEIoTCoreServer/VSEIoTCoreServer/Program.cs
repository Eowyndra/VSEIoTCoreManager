using System.Diagnostics;
using VSEIoTCoreServer.DAL;
using VSEIoTCoreServer.DAL.Models;
using VSEIoTCoreServer.Services;
using VSEIoTCoreServer.ViewModels;
using Serilog;
using VSEIoTCoreServer;
using Microsoft.EntityFrameworkCore;

// Setup logging
var loggingConfig = "serilogsettings.json";

Microsoft.Extensions.Logging.ILogger _logger;
LoggerFactory _loggerFactory;

_loggerFactory = new LoggerFactory();

var configuration = new ConfigurationBuilder()
    .AddJsonFile(loggingConfig)
    .Build();

var loggerConfiguration = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration);

_loggerFactory.AddSerilog(loggerConfiguration.CreateLogger());

_logger = _loggerFactory.CreateLogger<Program>();

var builder = WebApplication.CreateBuilder(args);


// Add services to the DI container.
var services = builder.Services;

services.Configure<IoTCoreOptions>(
    builder.Configuration.GetSection(IoTCoreOptions.IoTCoreSettings));


services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

// configure automapper with all automapper profiles from this assembly
services.AddAutoMapper(typeof(AutoMapperProfile));

string conStr = builder.Configuration.GetConnectionString("SQLiteConnection");
services.AddDbContext<SQLiteDbContext>(options => options.UseSqlite(conStr));

services.AddScoped<IDeviceConfigurationService, DeviceConfigurationService>();
services.AddScoped<IIoTCoreService, IoTCoreService>();

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// instanciate device configuration service directly here

//IDeviceConfigurationService deviceConfigurationService = new DeviceConfigurationService();

//IIoTCoreService iotCoreService = new IoTCoreService();

// Issue#51 - Read VSE connection data -> start IoT Cores

await using (var context = new SQLiteDbContext())
{
    // read VSE connection data from SQLite db and store them in a in-memory settings object
    var deviceConfigurations = context.DeviceConfigurations.ToList();

    // start IoTCore instances based on the connection settings
    deviceConfigurations.ForEach(x =>
    {
        ProcessStartInfo startInfo = new ProcessStartInfo("..\\..\\VSEIoTCore\\win-x64\\vse-iotcore-adapter-process.exe");
        startInfo.Arguments = "--vse-ip " + x.VseIpAddress + " --vse-port " + x.VsePort + " --iotcore-uri " + "http://127.0.0.1" + ":" + x.IoTCorePort;
        Process process = Process.Start(startInfo);
    });
}


app.Run();