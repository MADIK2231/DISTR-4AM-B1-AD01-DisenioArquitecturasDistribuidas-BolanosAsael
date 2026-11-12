using System.Net.Http.Json;

namespace Pedidos.Api.Services;

public class MicroserviciosClient(IHttpClientFactory httpFactory)
{
    private readonly HttpClient _httpCatalogo = httpFactory.CreateClient("ClienteCatalogo");
    private readonly HttpClient _httpInventario = httpFactory.CreateClient("ClienteInventario");

    public async Task<DatosProductoRemoto?> ConsultarProductoAsync(int idProducto)
    {
        try
        {
            using var respuesta = await _httpCatalogo.GetAsync($"api/Productos/{idProducto}");
            if (!respuesta.IsSuccessStatusCode)
            {
                return null;
            }

            return await respuesta.Content.ReadFromJsonAsync<DatosProductoRemoto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<DatosExistenciaRemota>> ConsultarExistenciasProductoAsync(int idProducto)
    {
        try
        {
            using var respuesta = await _httpInventario.GetAsync($"api/Existencias/producto/{idProducto}");
            if (!respuesta.IsSuccessStatusCode)
            {
                return [];
            }

            return await respuesta.Content.ReadFromJsonAsync<List<DatosExistenciaRemota>>() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<bool> SolicitarReservaBodegaAsync(int idProducto, int idBodega, int cantidad)
    {
        try
        {
            var solicitud = new
            {
                IdProducto = idProducto,
                IdBodega = idBodega,
                Cantidad = cantidad,
                Estado = "CONFIRMADA"
            };

            using var respuesta = await _httpInventario.PostAsJsonAsync("api/ReservasStock", solicitud);
            return respuesta.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

public sealed record DatosProductoRemoto(int IdProducto, string Nombre, string SKU, decimal Precio, bool Estado);

public sealed class DatosExistenciaRemota
{
    public int IdExistencia { get; set; }
    public int IdProducto { get; set; }
    public int IdBodega { get; set; }
    public int Stock { get; set; }
}
