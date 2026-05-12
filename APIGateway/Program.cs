using Shared.Extensions;
using Unadesk_Test.Data;
using Unadesk_Test.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabase<ApiDbContext>(builder.Configuration, "ApiDb");
builder.Services.AddRabbitMqConnectionManager();
builder.Services.AddFileStorage();
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

await app.MigrateDatabaseAsync<ApiDbContext>();

app.ConfigureSwagger();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();