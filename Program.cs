using CarvedRockFitness.Components;
using CarvedRockFitness.Services;
using CarvedRockFitness.Repositories;
using CarvedRockFitness.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.AzureAppServices;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault configuration (only for non-development environments)
if (!builder.Environment.IsDevelopment())
{
    var keyVaultUrl = "https://pertan4711-carvedrock-kv.vault.azure.net/";
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "CarvedRockFitness.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Logging.AddAzureWebAppDiagnostics();

// Add Entity Framework DbContext
string connectionString = "";

if (builder.Environment.IsDevelopment())
{
    // Use LocalDB for development
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
}
else
{
    // Build connection string from Key Vault secrets for production
    var server = builder.Configuration["DatabaseServer"];
    var database = builder.Configuration["DatabaseName"];
    var username = builder.Configuration["DatabaseUsername"];
    var password = builder.Configuration["DatabasePassword"];
    
    if (!string.IsNullOrEmpty(server) && !string.IsNullOrEmpty(database) && 
        !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
    {
        connectionString = $"Server={server};Database={database};User Id={username};Password={password};Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;";
    }
}

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Use SQL Server LocalDB for development
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure();
            });
        }
        else
        {
            // Use Azure SQL Database for production
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        }
    });
    builder.Services.AddScoped<ICartRepository, EFSqlCartRepository>();
    builder.Services.AddScoped<IProductRepository, EFProductRepository>();
}
else
{
    builder.Services.AddScoped<ICartRepository, InMemoryCartRepository>();
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
}

builder.Services.AddScoped<ShoppingCartService>();
builder.Services.AddSingleton<CartEventService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAntiforgery();


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();