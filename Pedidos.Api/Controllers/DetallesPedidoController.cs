using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;
using Pedidos.Api.Services;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DetallesPedidoController(PedidosDbContext db, MicroserviciosClient integracion) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DetallePedido>>> GetDetalles()
    {
        return Ok(await db.DetallesPedido.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DetallePedido>> GetDetalle(int id)
    {
        var item = await db.DetallesPedido.FindAsync(id);
        if (item is null)
        {
            return NotFound(new { error = $"No existe el detalle de pedido con ID {id}." });
        }

        return Ok(item);
    }

    [HttpGet("pedido/{idPedido:int}")]
    public async Task<ActionResult<IEnumerable<DetallePedido>>> GetDetallesPorPedido(int idPedido)
    {
        if (!await db.Pedidos.AnyAsync(p => p.IdPedido == idPedido))
        {
            return NotFound(new { error = $"No se encontró el pedido con ID {idPedido}." });
        }

        var lineas = await db.DetallesPedido
            .Where(d => d.IdPedido == idPedido)
            .AsNoTracking()
            .ToListAsync();

        return Ok(lineas);
    }

    [HttpPost]
    public async Task<ActionResult<DetallePedido>> PostDetalle([FromBody] DetallePedido modelo)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var orden = await db.Pedidos.Include(p => p.Detalles).FirstOrDefaultAsync(p => p.IdPedido == modelo.IdPedido);
        if (orden is null)
        {
            return BadRequest(new { error = $"FK Física inválida: el pedido con ID {modelo.IdPedido} no existe." });
        }

        var infoProducto = await integracion.ConsultarProductoAsync(modelo.IdProducto);
        if (infoProducto is null || !infoProducto.Estado)
        {
            return BadRequest(new { error = $"FK Lógica inválida: el producto con ID {modelo.IdProducto} no existe o está inactivo en Catalogo.Api." });
        }

        modelo.PrecioUnitario = infoProducto.Precio;
        db.DetallesPedido.Add(modelo);

        orden.Total += modelo.Cantidad * modelo.PrecioUnitario;
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDetalle), new { id = modelo.IdDetalle }, modelo);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutDetalle(int id, [FromBody] DetallePedido modelo)
    {
        if (id != modelo.IdDetalle)
        {
            return BadRequest(new { error = "El ID de la URL no coincide con el ID del detalle." });
        }

        var actual = await db.DetallesPedido.FindAsync(id);
        if (actual is null)
        {
            return NotFound(new { error = $"No existe el detalle de pedido con ID {id}." });
        }

        var orden = await db.Pedidos.FindAsync(modelo.IdPedido);
        if (orden is null)
        {
            return BadRequest(new { error = $"FK Física inválida: el pedido con ID {modelo.IdPedido} no existe." });
        }

        var infoProducto = await integracion.ConsultarProductoAsync(modelo.IdProducto);
        if (infoProducto is null || !infoProducto.Estado)
        {
            return BadRequest(new { error = $"FK Lógica inválida: el producto con ID {modelo.IdProducto} no existe en Catalogo.Api." });
        }

        actual.IdPedido = modelo.IdPedido;
        actual.IdProducto = modelo.IdProducto;
        actual.Cantidad = modelo.Cantidad;
        actual.PrecioUnitario = modelo.PrecioUnitario;

        await db.SaveChangesAsync();

        orden.Total = await db.DetallesPedido
            .Where(d => d.IdPedido == orden.IdPedido)
            .SumAsync(d => d.Cantidad * d.PrecioUnitario);

        await db.SaveChangesAsync();

        return Ok(actual);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDetalle(int id)
    {
        var actual = await db.DetallesPedido.FindAsync(id);
        if (actual is null)
        {
            return NotFound(new { error = $"No existe el detalle de pedido con ID {id}." });
        }

        var idPedidoAsociado = actual.IdPedido;
        db.DetallesPedido.Remove(actual);
        await db.SaveChangesAsync();

        var orden = await db.Pedidos.FindAsync(idPedidoAsociado);
        if (orden is not null)
        {
            orden.Total = await db.DetallesPedido
                .Where(d => d.IdPedido == idPedidoAsociado)
                .SumAsync(d => d.Cantidad * d.PrecioUnitario);

            await db.SaveChangesAsync();
        }

        return Ok(new { resultado = "Detalle de pedido eliminado exitosamente." });
    }
}
