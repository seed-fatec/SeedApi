using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SeedApi.API.Middlewares;
using SeedApi.Application.Interfaces;
using SeedApi.Application.Services;
using SeedApi.Domain.Configuration;
using SeedApi.Infrastructure.Config;
using SeedApi.Infrastructure.OpenApi;
using SeedApi.Infrastructure.Persistence;
using SeedApi.Infrastructure.Seeders;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseSettings>(
  builder.Configuration.GetSection("DatabaseSettings")
);

builder.Services.Configure<JwtSettings>(
  builder.Configuration.GetSection("JwtSettings")
);

builder.Services.AddSingleton(sp => new Configuration(builder.Configuration));

var configuration = new Configuration(builder.Configuration);
var jwtSecretKey = Encoding.ASCII.GetBytes(configuration.JwtSettings.Secret);

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
      ValidIssuer = configuration.JwtSettings.Issuer,
      ValidateAudience = true,
      ValidAudience = configuration.JwtSettings.Audience,
      ValidateLifetime = true
    };
  });

builder.Services.AddDbContext<IPersistenceContext, ApplicationDbContext>(options =>
  options.UseMySql(configuration.DatabaseSettings.ConnectionString, ServerVersion.AutoDetect(configuration.DatabaseSettings.ConnectionString))
);

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TeacherService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<ClassService>();
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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<AdminRouteRestrictionMiddleware>();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
  var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
  await seeder.SeedAsync();
}

app.Run();
