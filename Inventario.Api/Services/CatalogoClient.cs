using System.Net.Http.Json;

namespace Inventario.Api.Services;

public class CatalogoClient(HttpClient http)
{
    public async Task<bool> ValidarProductoEnCatalogoAsync(int idProducto)
    {
        try
        {
            using var respuesta = await http.GetAsync($"api/Productos/{idProducto}");
            if (!respuesta.IsSuccessStatusCode)
            {
                return false;
            }

            var datos = await respuesta.Content.ReadFromJsonAsync<ResumenProductoCatalogo>();
            return datos is { Estado: true };
        }
        catch
        {
            return false;
        }
    }

    private sealed record ResumenProductoCatalogo(int IdProducto, string Nombre, decimal Precio, bool Estado);
}
