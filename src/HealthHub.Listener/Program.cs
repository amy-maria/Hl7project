using HealthHub.Data;
using HealthHub.Listener;

var builder = Host.CreateApplicationBuilder(args);

// Bind the "Mllp" section of appsettings.json to the MllpOptions class
builder.Services.Configure<MllpOptions>(builder.Configuration.GetSection("Mllp"));

string connectionString = builder.Configuration.GetConnectionString("HealthHub")
    ?? throw new InvalidOperationException("Connection string 'HealthHub' is not set. See Lesson 2B, Step 8a.");
builder.Services.AddSingleton(new MessageRepository(connectionString));

builder.Services.AddHostedService<MllpListenerService>();

builder.Build().Run();