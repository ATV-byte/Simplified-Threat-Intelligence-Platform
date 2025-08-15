using MongoDB.Driver;
using Simplified_Threat_Intelligence_Platform.Data;
using Simplified_Threat_Intelligence_Platform.Repositories;
using Simplified_Threat_Intelligence_Platform.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Mongo (Cosmos for Mongo)
builder.Services.AddSingleton<IMongoDatabase>(_ =>
{
    var conn =
        builder.Configuration["AZURE_COSMOS_CONNECTIONSTRING"] 
        ?? Environment.GetEnvironmentVariable("AZURE_COSMOS_CONNECTIONSTRING");

    if (string.IsNullOrWhiteSpace(conn))
        throw new InvalidOperationException("AZURE_COSMOS_CONNECTIONSTRING is not set in either configuration or environment.");

    var url = new MongoUrl(conn);
    var client = new MongoClient(url);

    var dbName = !string.IsNullOrWhiteSpace(url.DatabaseName)
        ? url.DatabaseName
        : (builder.Configuration["Mongo:Database"] ?? "ti_db");

    var db = client.GetDatabase(dbName);

    // ensure indexes at startup
    IndexInitializer.EnsureAsync(db).GetAwaiter().GetResult();
    return db;
});


// DI
builder.Services.AddScoped<MalwareRepository>();
builder.Services.AddScoped<IndicatorRepository>();
builder.Services.AddScoped<MalwareService>();

var app = builder.Build();

// Swagger ON in Prod
app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.RoutePrefix = "swagger"; // /swagger
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "Threat Intel API v1");
});

// Root → Swagger (or health if you prefer)
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
