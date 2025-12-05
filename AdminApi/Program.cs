using AdminApi.Cache;
using AdminApi.Lib;
using AdminApi.Repositories;
using AdminApi.Services;
using static System.Text.Json.Serialization.JsonIgnoreCondition;
using static AdminApi.Lib.SecretName;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(
    options => options.AddPolicy("AllowFrontend", 
        policy => policy.WithOrigins("http://localhost:7777").AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .AddJsonOptions(o => { o.JsonSerializerOptions.DefaultIgnoreCondition = WhenWritingNull; });

builder.Services.AddSingleton<ISecretStore>(_ => {
    SecretName[] secrets = [ProdDatabase, ProdMachineKey, ProdCoreOpensearch];
    const string configPath = @"C:\release\secrets.config", region = "eu-west-2";
    return SecretStore.CreateAsync(secrets, configPath, region).GetAwaiter().GetResult();
});

builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();
builder.Services.AddSingleton<IOrganisationCache, OrganisationCache>();
builder.Services.AddScoped<IDatabaseCommands, DatabaseCommands>();
builder.Services.AddSingleton<IOpenSearchRepository, OpenSearchRepository>();
builder.Services.AddScoped<ISavedSearchService, SavedSearchService>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseSwagger().UseSwaggerUI(options => { options.EnableTryItOutByDefault(); });

app.UseCors("AllowFrontend");
app.MapGet("/ping", () => "pong");
app.MapControllers();
app.Run();
