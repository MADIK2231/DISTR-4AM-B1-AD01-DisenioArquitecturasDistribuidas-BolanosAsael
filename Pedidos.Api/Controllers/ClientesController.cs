using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController(PedidosDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
    {
        return Ok(await db.Clientes.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cliente>> GetCliente(int id)
    {
        var cliente = await db.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return NotFound(new { error = $"No se encontró el cliente con ID {id}." });
        }

        return Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<Cliente>> PostCliente([FromBody] Cliente modelo)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (await db.Clientes.AnyAsync(c => c.Cedula == modelo.Cedula))
        {
            return BadRequest(new { error = $"La cédula '{modelo.Cedula}' ya está registrada en PedidosDB." });
        }

        db.Clientes.Add(modelo);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCliente), new { id = modelo.IdCliente }, modelo);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutCliente(int id, [FromBody] Cliente modelo)
    {
        if (id != modelo.IdCliente)
        {
            return BadRequest(new { error = "El ID de la URL no coincide con el ID del cliente." });
        }

        if (!await db.Clientes.AnyAsync(c => c.IdCliente == id))
        {
            return NotFound(new { error = $"No se encontró el cliente con ID {id}." });
        }

        if (await db.Clientes.AnyAsync(c => c.Cedula == modelo.Cedula && c.IdCliente != id))
        {
            return BadRequest(new { error = $"La cédula '{modelo.Cedula}' ya está asignada a otro cliente." });
        }

        db.Entry(modelo).State = EntityState.Modified;
        await db.SaveChangesAsync();

        return Ok(modelo);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCliente(int id)
    {
        var cliente = await db.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return NotFound(new { error = $"No se encontró el cliente con ID {id}." });
        }

        if (await db.Pedidos.AnyAsync(p => p.IdCliente == id))
        {
            return BadRequest(new { error = "Restricción de llave foránea física: el cliente posee órdenes de compra asociadas." });
        }

        db.Clientes.Remove(cliente);
        await db.SaveChangesAsync();

        return Ok(new { resultado = "Cliente eliminado exitosamente." });
    }
}
