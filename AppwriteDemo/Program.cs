using AppwriteDemo.Components;
using AppwriteDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HTTP context accessor for accessing cookies
builder.Services.AddHttpContextAccessor();

// Register services
builder.Services.AddScoped<AppwriteService>();
builder.Services.AddSingleton<ConfigurationValidationService>();

// Add logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddDebug();
}

var app = builder.Build();

// Validate configuration on startup
using (var scope = app.Services.CreateScope())
{
    var configValidator = scope.ServiceProvider.GetRequiredService<ConfigurationValidationService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    if (!configValidator.ValidateAppwriteConfiguration())
    {
        logger.LogCritical("Application cannot start due to invalid Appwrite configuration. Please check your appsettings.json file.");
        
        if (app.Environment.IsProduction())
        {
            // In production, we want to fail fast with configuration issues
            throw new InvalidOperationException("Invalid Appwrite configuration detected. Please check your configuration settings.");
        }
        else
        {
            logger.LogWarning("Running in development mode with invalid configuration. Some features may not work correctly.");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
