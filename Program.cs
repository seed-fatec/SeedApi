using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SeedApi.Infrastructure.OpenApi;
using SeedApi.Middlewares;
using SeedApi.Models;
using SeedApi.Models.Config;
using SeedApi.Seeders;
using SeedApi.Services;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Load database settings from environment
builder.Services.Configure<DatabaseSettings>(
  builder.Configuration.GetSection("DatabaseSettings")
);

// Load JWT settings from environment
builder.Services.Configure<JwtSettings>(
  builder.Configuration.GetSection("JwtSettings")
);

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
  ?? throw new Exception("Failed to load JwtSettings from environment.");

var databaseSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>()
  ?? throw new Exception("Failed to load DatabaseSettings from environment.");

var jwtSecretKey = Encoding.ASCII.GetBytes(jwtSettings.Secret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(jwtSecretKey),
      ValidateIssuer = true,
      ValidIssuer = jwtSettings.Issuer,
      ValidateAudience = true,
      ValidAudience = jwtSettings.Audience,
      ValidateLifetime = true
    };
  });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
  options.UseMySql(databaseSettings.ConnectionString, ServerVersion.AutoDetect(databaseSettings.ConnectionString))
);

// Add services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TeacherService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<AdminSeeder>();

builder.Services.AddControllers()
  .AddJsonOptions(options =>
  {
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.JsonSerializerOptions.WriteIndented = true;
  });

builder.Services.AddOpenApi(options =>
{
  options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
  options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowSeedApp", policy =>
  {
    policy.WithOrigins("http://localhost:5173")
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials();
  });
});

var app = builder.Build();

app.UseCors("AllowSeedApp");

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.MapScalarApiReference(options =>
  {
    options.Title = "Seed API Docs";
    options.Theme = ScalarTheme.BluePlanet;
    options.WithHttpBearerAuthentication(bearer =>
    {
      bearer.Token = "your-bearer-token";
    });
  });
}

app.UseMiddleware<AdminRouteRestrictionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
  var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
  await seeder.SeedAsync();
}

app.Run();
