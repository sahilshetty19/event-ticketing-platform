var builder = WebApplication.CreateBuilder(args);

const string SpaCorsPolicy = "spa";

// Allow the browser SPA (served from a different origin) to call the gateway.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
    options.AddPolicy(SpaCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

// YARP reverse proxy: routes/clusters are loaded from the "ReverseProxy" config section.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();

var app = builder.Build();

// Applies the CORS policy to all requests (incl. proxied routes) and handles preflight.
app.UseCors(SpaCorsPolicy);

app.MapHealthChecks("/health");
app.MapReverseProxy();

app.Run();
