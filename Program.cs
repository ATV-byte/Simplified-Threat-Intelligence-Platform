using MongoDB.Driver;
using Simplified_Threat_Intelligence_Platform.Data; // namespace-ul tău cu IndexInitializer
using Simplified_Threat_Intelligence_Platform.Repositories;
using Simplified_Threat_Intelligence_Platform.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var cfg = builder.Configuration.GetSection("Mongo");
    var client = new MongoClient(cfg["ConnectionString"]);
    var db = client.GetDatabase(cfg["Database"]);

    IndexInitializer.EnsureAsync(db).GetAwaiter().GetResult();

    return db;
});

// DI pentru repo și service
builder.Services.AddScoped<MalwareRepository>();
builder.Services.AddScoped<IndicatorRepository>();
builder.Services.AddScoped<MalwareService>();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
