using ConstructionManagementAPI.Repositories;
using ConstructionManagementSaaS;
using ConstructionManagementSaaS.Data;
using ConstructionManagementSaaS.Repositories;
using ConstructionManagementSaaS.Services;
using ConstructionManagementService.Services;
using ConstructionManagementService.Services.Contracts;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddSingleton<IMongoClient>(sp =>
//{
//    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
//    return new MongoClient(settings.ConnectionString);
//});
// Add MongoDB client configuration
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    // Fetch the connection string from the environment variable
    var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_URL")
                                ?? throw new Exception("MONGO_URL environment variable not set.");
    return new MongoClient(mongoConnectionString);
});

// Add services to the container.
builder.Services.Configure<MongoDBSettings>(options =>
{
    options.ConnectionString = Environment.GetEnvironmentVariable("MONGO_URL");
    options.DatabaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME")
                           ?? "defaultDatabaseName"; // Replace with a fallback name or remove as needed
});

//builder.Services.Configure<MongoDBSettings>(options =>
//{
//    options.ConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
//    options.DatabaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME");
//}); 


builder.Services.AddSingleton<IMongoDBSettings>(sp =>
    sp.GetRequiredService<IOptions<MongoDBSettings>>().Value);
builder.Services.AddSingleton<RoleSeeder>();


builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed roles when the app starts
using (var scope = app.Services.CreateScope())
{
    var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
    await roleSeeder.SeedRolesAsync(); // This will seed the roles into the MongoDB database
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
