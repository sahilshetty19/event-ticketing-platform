var builder = WebApplication.CreateBuilder(args);

// YARP reverse proxy: routes/clusters are loaded from the "ReverseProxy" config section.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapReverseProxy();

app.Run();
