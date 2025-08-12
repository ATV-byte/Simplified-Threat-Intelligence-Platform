using MongoDB.Driver;
using Simplified_Threat_Intelligence_Platform.Data;
using Simplified_Threat_Intelligence_Platform.Repositories;
using Simplified_Threat_Intelligence_Platform.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB (Cosmos for Mongo) via env var
builder.Services.AddSingleton<IMongoDatabase>(_ =>
{
    // Prefer Azure app setting (env var)
    var conn = builder.Configuration["AZURE_COSMOS_CONNECTIONSTRING"]
               ?? Environment.GetEnvironmentVariable("AZURE_COSMOS_CONNECTIONSTRING");

    if (string.IsNullOrWhiteSpace(conn))
        throw new InvalidOperationException("AZURE_COSMOS_CONNECTIONSTRING is not set.");

    var url = new MongoUrl(conn);
    var client = new MongoClient(url);

    // If the database name isn’t in the connection string, fall back to config, else default
    var dbName = !string.IsNullOrWhiteSpace(url.DatabaseName)
        ? url.DatabaseName
        : (builder.Configuration["Mongo:Database"] ?? "ti_db");

    var db = client.GetDatabase(dbName);

    // Ensure indexes once at startup
    IndexInitializer.EnsureAsync(db).GetAwaiter().GetResult();
    return db;
});

// DI
builder.Services.AddScoped<MalwareRepository>();
builder.Services.AddScoped<IndicatorRepository>();
builder.Services.AddScoped<MalwareService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
