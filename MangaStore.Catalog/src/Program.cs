using Microsoft.AspNetCore.Mvc;
using MangaStore.Catalog.Config;
using MangaStore.Catalog.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<CatalogDatabaseSettings>(
    builder.Configuration.GetSection("CatalogDatabase"));

builder.Services.AddSingleton<CatalogService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "Default",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod();
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Default");

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
