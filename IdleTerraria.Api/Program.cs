using System.Text;
using MediatR;
using IdleTerraria.Api.Data;
using IdleTerraria.Api.Entities;
using IdleTerraria.Api.Security;
using IdleTerraria.Api.Services;
using IdleTerraria.Api.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.Configure<GameRulesOptions>(
    builder.Configuration.GetSection(GameRulesOptions.SectionName));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddScoped<IPlayerProgressionService, PlayerProgressionService>();
builder.Services.AddScoped<IHuntingService, HuntingService>();

builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();


builder.Services.AddScoped<
    IPasswordHasher<Account>,
    PasswordHasher<Account>>();

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "Brak sekcji Jwt w konfiguracji.");

if (string.IsNullOrWhiteSpace(jwtOptions.Key))
{
    throw new InvalidOperationException(
        "Brak Jwt:Key w konfiguracji.");
}

var signingKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtOptions.Key));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();