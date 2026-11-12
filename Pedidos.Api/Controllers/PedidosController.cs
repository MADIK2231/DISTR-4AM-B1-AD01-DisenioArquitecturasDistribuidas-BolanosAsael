using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;
using Pedidos.Api.Services;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController(PedidosDbContext db, MicroserviciosClient integracion) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
    {
        var ordenes = await db.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
            .OrderByDescending(p => p.Fecha)
            .AsNoTracking()
            .ToListAsync();

        return Ok(ordenes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Pedido>> GetPedido(int id)
    {
        var orden = await db.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.IdPedido == id);

        if (orden is null)
        {
            return NotFound(new { error = $"No se encontró el pedido con ID {id}." });
        }

        return Ok(orden);
    }

    [HttpPost]
    public async Task<ActionResult<Pedido>> PostPedido([FromBody] Pedido modelo)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await db.Clientes.AnyAsync(c => c.IdCliente == modelo.IdCliente))
        {
            return BadRequest(new { error = $"FK Física inválida: el cliente con ID {modelo.IdCliente} no existe en PedidosDB." });
        }

        if (modelo.Detalles is null || modelo.Detalles.Count == 0)
        {
            return BadRequest(new { error = "La orden de compra requiere al menos un ítem en Detalles." });
        }

        decimal sumaTotal = 0m;
        var colaReservas = new List<(int IdProducto, int IdBodega, int Cantidad)>();

        foreach (var linea in modelo.Detalles)
        {
            var infoProducto = await integracion.ConsultarProductoAsync(linea.IdProducto);
            if (infoProducto is null || !infoProducto.Estado)
            {
                return BadRequest(new
                {
                    error = $"FK Lógica inválida: el producto con ID {linea.IdProducto} no existe o se encuentra inactivo en Catalogo.Api."
                });
            }

            var stockBodegas = await integracion.ConsultarExistenciasProductoAsync(linea.IdProducto);
            var bodegaSeleccionada = stockBodegas.FirstOrDefault(b => b.Stock >= linea.Cantidad);

            if (bodegaSeleccionada is null)
            {
                var disponibleTotal = stockBodegas.Sum(x => x.Stock);
                return BadRequest(new
                {
                    error = $"Stock insuficiente para '{infoProducto.Nombre}' (ID {linea.IdProducto}). Requerido: {linea.Cantidad}, Disponible en bodegas: {disponibleTotal}."
                });
            }

            linea.PrecioUnitario = infoProducto.Precio;
            sumaTotal += linea.Cantidad * linea.PrecioUnitario;

            colaReservas.Add((linea.IdProducto, bodegaSeleccionada.IdBodega, linea.Cantidad));
        }

        foreach (var reserva in colaReservas)
        {
            var reservado = await integracion.SolicitarReservaBodegaAsync(reserva.IdProducto, reserva.IdBodega, reserva.Cantidad);
            if (!reservado)
            {
                return BadRequest(new
                {
                    error = $"Error al confirmar la reserva en Inventario.Api para el producto {reserva.IdProducto} en la bodega {reserva.IdBodega}."
                });
            }
        }

        modelo.Fecha = DateTime.Now;
        modelo.Total = sumaTotal;

        db.Pedidos.Add(modelo);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPedido), new { id = modelo.IdPedido }, modelo);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutPedido(int id, [FromBody] Pedido modelo)
    {
        if (id != modelo.IdPedido)
        {
            return BadRequest(new { error = "El ID de la URL no coincide con el ID del pedido." });
        }

        var existente = await db.Pedidos.FindAsync(id);
        if (existente is null)
        {
            return NotFound(new { error = $"No se encontró el pedido con ID {id}." });
        }

        existente.Estado = modelo.Estado;
        await db.SaveChangesAsync();

        return Ok(existente);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePedido(int id)
    {
        var existente = await db.Pedidos
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.IdPedido == id);

        if (existente is null)
        {
            return NotFound(new { error = $"No se encontró el pedido con ID {id}." });
        }

        db.Pedidos.Remove(existente);
        await db.SaveChangesAsync();

        return Ok(new { resultado = "Pedido eliminado exitosamente." });
    }
}
