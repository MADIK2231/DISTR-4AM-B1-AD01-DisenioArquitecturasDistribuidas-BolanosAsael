using Inventario.Api.Data;
using Inventario.Api.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var cadenaConexion = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<InventarioDbContext>(opts =>
    opts.UseSqlServer(cadenaConexion));

builder.Services.AddHttpClient<CatalogoClient>(http =>
{
    var destino = builder.Configuration["ServiciosRemotos:CatalogoBaseUrl"] ?? "http://localhost:5001/";
    http.BaseAddress = new Uri(destino);
});

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
