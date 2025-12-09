using System.Security.Claims;
using AdminApi.Cache;
using AdminApi.Lib;
using AdminApi.Repositories;
using AdminApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using static System.Text.Json.Serialization.JsonIgnoreCondition;
using static AdminApi.Lib.SecretName;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => options.AddPolicy("AllowFrontend",
    policy => policy.WithOrigins("http://localhost:7777").AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(o => {
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement { {
        new() { Reference = new() { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
        Array.Empty<string>()
    } });
});

builder.Services.AddControllers()
    .AddJsonOptions(o => { o.JsonSerializerOptions.DefaultIgnoreCondition = WhenWritingNull; });

builder.Services.AddSingleton<ISecretStore>(_ => {
    SecretName[] secrets = [ProdDatabase, ProdMachineKey, ProdCoreOpensearch];
    const string configPath = @"C:\release\secrets.config", region = "eu-west-2";
    return SecretStore.CreateAsync(secrets, configPath, region).GetAwaiter().GetResult();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<ISecretStore>((options, secrets) => {
        byte[] keyBytes = JwtTokenService.GetSigningKeyBytes(secrets);

        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimTypes.Email,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(o => {
    o.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireRole("admin")
        .Build();
});

builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<IOpenSearchRepository, OpenSearchRepository>();
builder.Services.AddSingleton<IOrganisationCache, OrganisationCache>();
builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();
builder.Services.AddScoped<IDatabaseCommands, DatabaseCommands>();
builder.Services.AddScoped<ISavedSearchService, SavedSearchService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseSwagger().UseSwaggerUI(options => { options.EnableTryItOutByDefault(); });

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/ping", () => "pong").AllowAnonymous();
app.MapControllers();
app.Run();