using MongoDB.Driver;
using Simplified_Threat_Intelligence_Platform.Data;
using Simplified_Threat_Intelligence_Platform.Repositories;
using Simplified_Threat_Intelligence_Platform.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---- Mongo (Cosmos for Mongo) ----
builder.Services.AddSingleton<IMongoDatabase>(_ =>
{
    // 1) Azure App Service: prefer ENV var
    string? conn = Environment.GetEnvironmentVariable("AZURE_COSMOS_CONNECTIONSTRING");

    // 2) Azure App Service (opțiune recomandată): ConnectionStrings:CosmosMongo
    if (string.IsNullOrWhiteSpace(conn))
        conn = builder.Configuration.GetConnectionString("CosmosMongo");

    // 3) Local dev fallback din appsettings.json
    if (string.IsNullOrWhiteSpace(conn))
        conn = builder.Configuration["AZURE_COSMOS_CONNECTIONSTRING_local"];

    if (string.IsNullOrWhiteSpace(conn))
        throw new InvalidOperationException("No Mongo/Cosmos connection string configured. Set AZURE_COSMOS_CONNECTIONSTRING (env) or ConnectionStrings:CosmosMongo or AZURE_COSMOS_CONNECTIONSTRING_local.");

    var url = new MongoUrl(conn);
    var client = new MongoClient(url);

    var dbName = !string.IsNullOrWhiteSpace(url.DatabaseName)
        ? url.DatabaseName
        : (builder.Configuration["Mongo:Database"] ?? "ti_db");

    var db = client.GetDatabase(dbName);

    // Dacă indexarea ar putea pica la first run, poți prinde excepția:
    // try { await IndexInitializer.EnsureAsync(db); } catch (Exception ex) { Console.WriteLine(ex); }
    IndexInitializer.EnsureAsync(db).GetAwaiter().GetResult();

    return db;
});

// DI
builder.Services.AddScoped<IMalwareRepository, MalwareRepository>();
builder.Services.AddScoped<IIndicatorRepository, IndicatorRepository>();
builder.Services.AddScoped<IMalwareService, MalwareService>();

var app = builder.Build();

// Swagger ON în Prod
app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.RoutePrefix = "swagger"; // /swagger
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "Threat Intel API v1");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
