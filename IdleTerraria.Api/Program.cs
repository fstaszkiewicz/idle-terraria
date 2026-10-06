using IdleTerraria.Api.Data;
using IdleTerraria.Api.Entities;
using IdleTerraria.Api.Infrastructure.Errors;
using IdleTerraria.Api.Options;
using IdleTerraria.Api.Security;
using IdleTerraria.Api.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
    {
        options.UseNpgsql(
            builder.Configuration.GetConnectionString(
                "DefaultConnection"));
    });

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(
        JwtOptions.SectionName));

builder.Services.Configure<GameRulesOptions>(
    builder.Configuration.GetSection(
        GameRulesOptions.SectionName));

builder.Services.AddMediatR(
    configuration =>
        configuration.RegisterServicesFromAssembly(
            typeof(Program).Assembly));

builder.Services.AddScoped<
    IPlayerProgressionService,
    PlayerProgressionService>();

builder.Services.AddSingleton<IRandomSource, GameRandomSource>();

builder.Services.AddScoped<
    IHuntingRewardCalculator,
    HuntingRewardCalculator>();

builder.Services.AddScoped<
    IHuntingService,
    HuntingService>();

builder.Services.AddScoped<
    IPlayerService,
    PlayerService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IJwtTokenService,
    JwtTokenService>();

builder.Services.AddScoped<
    IPasswordHasher<Account>,
    PasswordHasher<Account>>();

builder.Services.AddScoped<IGameDataSeeder, GameDataSeeder>();

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

if (Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key musi mieć co najmniej 32 bajty.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
{
    throw new InvalidOperationException(
        "Brak Jwt:Issuer w konfiguracji.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
{
    throw new InvalidOperationException(
        "Brak Jwt:Audience w konfiguracji.");
}

var signingKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtOptions.Key));

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
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

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                context.Response.ContentType =
                    "application/problem+json";

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Type = "https://httpstatuses.com/401",
                    Detail = "Wymagany jest poprawny token dostępu.",
                    Instance = context.Request.Path
                };

                problemDetails.Extensions["traceId"] =
                    context.HttpContext.TraceIdentifier;

                await context.Response.WriteAsJsonAsync(
                    problemDetails,
                    cancellationToken: context.HttpContext.RequestAborted);
            },

            OnForbidden = async context =>
            {
                context.Response.StatusCode =
                    StatusCodes.Status403Forbidden;

                context.Response.ContentType =
                    "application/problem+json";

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Forbidden",
                    Type = "https://httpstatuses.com/403",
                    Detail = "Nie masz uprawnień do wykonania tej operacji.",
                    Instance = context.Request.Path
                };

                problemDetails.Extensions["traceId"] =
                    context.HttpContext.TraceIdentifier;

                await context.Response.WriteAsJsonAsync(
                    problemDetails,
                    cancellationToken: context.HttpContext.RequestAborted);
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var cancellationToken = app.Lifetime.ApplicationStopping;

    var dbContext = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync(cancellationToken);

    var seeder = scope.ServiceProvider
        .GetRequiredService<IGameDataSeeder>();

    await seeder.SeedAsync(cancellationToken);
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();