using ProductManagement.Infrastructure;
using ProductManagement.MigrationService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddInfrastructure();
builder.Services.AddHostedService<MigrationWorker>();

var host = builder.Build();

host.Run();

return Environment.ExitCode;
