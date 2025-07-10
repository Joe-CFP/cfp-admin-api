using AdminApi.Lib;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddSingleton<SecretStore>(_ => {
    SecretName[] secrets = [SecretName.ProdDatabase, SecretName.ProdMachineKey];
    const string configPath = @"C:\release\secrets.config", region = "eu-west-2";
    return SecretStore.CreateAsync(secrets, configPath, region).GetAwaiter().GetResult();
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseSwagger().UseSwaggerUI();
else
    app.UseHttpsRedirection();

app.MapGet("/ping", () => "pong");
app.MapControllers();
app.Run();
