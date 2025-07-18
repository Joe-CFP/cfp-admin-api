using AdminApi.Lib;
using AdminApi.Repositories;
using AdminApi.Routing;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(
    options => options.AddPolicy("AllowFrontend", 
        policy => policy.WithOrigins("http://localhost:7777").AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(SlugifyParameterTransformer.SlugifyControllerNames);

builder.Services.AddSingleton<ISecretStore>(_ => {
    SecretName[] secrets = [SecretName.ProdDatabase, SecretName.ProdMachineKey];
    const string configPath = @"C:\release\secrets.config", region = "eu-west-2";
    return SecretStore.CreateAsync(secrets, configPath, region).GetAwaiter().GetResult();
});
builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseSwagger().UseSwaggerUI();

app.UseCors("AllowFrontend");
app.MapGet("/ping", () => "pong");
app.MapControllers();
app.Run();
