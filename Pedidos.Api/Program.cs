using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var cadenaConexion = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PedidosDbContext>(opts =>
    opts.UseSqlServer(cadenaConexion));

builder.Services.AddHttpClient("ClienteCatalogo", http =>
{
    var url = builder.Configuration["ServiciosRemotos:CatalogoBaseUrl"] ?? "http://localhost:5001/";
    http.BaseAddress = new Uri(url);
});

builder.Services.AddHttpClient("ClienteInventario", http =>
{
    var url = builder.Configuration["ServiciosRemotos:InventarioBaseUrl"] ?? "http://localhost:5002/";
    http.BaseAddress = new Uri(url);
});

builder.Services.AddScoped<MicroserviciosClient>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
