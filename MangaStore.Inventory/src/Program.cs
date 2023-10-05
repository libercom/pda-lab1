using MangaStore.Inventory.Context;
using MangaStore.Inventory.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<InventoryContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<MangaStockService>();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/status", () =>
{
    return Results.Ok();
});
app.MapControllers();

var client = new HttpClient();
var url = app.Configuration["ApiGatewayUrl"];
var body = new
{
    Url = app.Configuration["BaseUrl"]
};

await client.PostAsJsonAsync(url, body);

app.Run();
