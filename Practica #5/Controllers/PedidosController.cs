using Microsoft.AspNetCore.Mvc;
using Practica__5.Models;
using Practica__5.Services;
using System;

namespace Practica__5.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly PedidoService _pedidoService;

        public PedidosController()
        {
            _pedidoService = new PedidoService();
        }

        [HttpPost]
        public IActionResult CrearPedido()
        {
            var pedido = _pedidoService.CrearPedido();
            return Ok(new { id = pedido.Id, mensaje = "Pedido creado" });
        }

        [HttpPost("{id}/productos")]
        public IActionResult AgregarProducto(int id, [FromBody] ProductoCreateDTO dto)
        {
            try
            {
                var mensaje = _pedidoService.AgregarProducto(id, dto);
                return Ok(new { mensaje });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult ObtenerPedido(int id)
        {
            try
            {
                var (pedido, subtotal, itbis, total, productosConTotal) = _pedidoService.ObtenerPedido(id);
                var response = new
                {
                    id = pedido.Id,
                    estado = pedido.Estado,
                    productos = productosConTotal,
                    subtotal,
                    itbis,
                    total
                };
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpDelete("{pedidoId}/productos/{productoId}")]
        public IActionResult EliminarProducto(int pedidoId, int productoId)
        {
            try
            {
                _pedidoService.EliminarProducto(pedidoId, productoId);
                return Ok(new { mensaje = "Producto eliminado" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("{id}/cancelar")]
        public IActionResult CancelarPedido(int id)
        {
            try
            {
                _pedidoService.CancelarPedido(id);
                return Ok(new { mensaje = "Pedido cancelado" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}