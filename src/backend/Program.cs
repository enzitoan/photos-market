using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PhotosMarket.API.Services;
using PhotosMarket.API.Repositories;
using PhotosMarket.API.Repositories.InMemory;
using PhotosMarket.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClient factory
builder.Services.AddHttpClient();

// Configure settings
builder.Services.Configure<CosmosDbSettings>(builder.Configuration.GetSection("CosmosDb"));
var googleDriveSettings = builder.Configuration.GetSection("GoogleDrive").Get<GoogleDriveSettings>();
builder.Services.AddSingleton(googleDriveSettings!);
builder.Services.Configure<GooglePhotosSettings>(builder.Configuration.GetSection("GoogleOAuth"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("Application"));

// Try to connect to Cosmos DB, fallback to in-memory if not available
var cosmosConnectionString = builder.Configuration.GetConnectionString("CosmosDb");

try
{
    // Try to initialize Cosmos DB
    var settings = builder.Configuration.GetSection("CosmosDb").Get<CosmosDbSettings>();
    var cosmosDbService = new CosmosDbService(cosmosConnectionString!, settings!);
    
    // Test connection - this will throw if Cosmos DB is not available
    var testTask = Task.Run(async () => await cosmosDbService.InitializeDatabaseAsync());
    testTask.Wait(TimeSpan.FromSeconds(5)); // 5 second timeout
    
    // If we get here, Cosmos DB is available
    builder.Services.AddSingleton<ICosmosDbService>(cosmosDbService);
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IDownloadLinkRepository, DownloadLinkRepository>();
    builder.Services.AddSingleton<IAlbumRepository, InMemoryAlbumRepository>();
    builder.Services.AddScoped<IPhotographerSettingsRepository, PhotographerSettingsCosmosRepository>();
    
    Console.WriteLine("✅ Using Cosmos DB for data storage");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  Warning: Could not initialize Cosmos DB: {ex.Message}");
    Console.WriteLine("📦 Using IN-MEMORY storage (data will be lost on restart)");
    
    // Use in-memory repositories
    builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
    builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
    builder.Services.AddSingleton<IDownloadLinkRepository, InMemoryDownloadLinkRepository>();
    builder.Services.AddSingleton<IAlbumRepository, InMemoryAlbumRepository>();
    builder.Services.AddSingleton<IPhotographerSettingsRepository, InMemoryPhotographerSettingsRepository>();
}

// Register services
builder.Services.AddSingleton<GoogleDriveService>();
builder.Services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IWatermarkService, WatermarkService>();

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings!.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:5173", "http://localhost:3000", "http://localhost:3001", "http://localhost:3002" };

// Add frontend URL from environment if present
var frontendUrl = builder.Configuration["FRONTEND_URL"];
if (!string.IsNullOrEmpty(frontendUrl))
{
    var originsList = allowedOrigins.ToList();
    if (!originsList.Contains(frontendUrl))
    {
        originsList.Add(frontendUrl);
    }
    allowedOrigins = originsList.ToArray();
    Console.WriteLine($"✅ Added CORS origin: {frontendUrl}");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Cosmos DB already initialized above if available
// No need to initialize again here

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
