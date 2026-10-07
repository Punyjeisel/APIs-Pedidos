using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using Practica__5.Models;
using Microsoft.AspNetCore.Mvc;

namespace Practica__5.Services
{
    public class PedidoService
    {
        private string rutaArchivo = "pedidos.json";

        // Guarda la lista completa de pedidos en el archivo JSON
        public void GuardarPedidos(List<Pedido> pedidos)
        {
            var json = JsonSerializer.Serialize(pedidos, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(rutaArchivo, json);
        }

        // Obtiene la lista de pedidos desde el archivo JSON (si no existe, retorna lista vacía)
        public List<Pedido> ConseguirPedidos()
        {
            if (!File.Exists(rutaArchivo))
                return new List<Pedido>();

            var json = File.ReadAllText(rutaArchivo);
            var pedidos = JsonSerializer.Deserialize<List<Pedido>>(json);
            return pedidos ?? new List<Pedido>();
        }

        // Crea un nuevo pedido vacío con estado "Activo"
        public Pedido CrearPedido()
        {
            var pedidos = ConseguirPedidos();
            int nuevoId = pedidos.Any() ? pedidos.Max(p => p.Id) + 1 : 1;

            var nuevoPedido = new Pedido
            {
                Id = nuevoId,
                Estado = "Activo",
                Productos = new List<Producto>()
            };

            pedidos.Add(nuevoPedido);
            GuardarPedidos(pedidos);
            return nuevoPedido;
        }

        // Agrega un producto a un pedido. El Id del producto se genera automáticamente.
        public string AgregarProducto(int pedidoId, ProductoCreateDTO dto)
        {
            // Validaciones de entrada
            if (dto.Cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero.");
            if (dto.precio < 0)
                throw new ArgumentException("El precio no puede ser negativo.");

            var pedidos = ConseguirPedidos();
            var pedido = pedidos.FirstOrDefault(p => p.Id == pedidoId);

            if (pedido == null)
                throw new KeyNotFoundException($"Pedido con ID {pedidoId} no encontrado.");

            if (pedido.Estado == "Cancelado")
                throw new InvalidOperationException("No se puede modificar un pedido cancelado.");

            // Buscar si ya existe un producto con el mismo nombre (y mismo precio, opcional)
            // En este caso, usamos nombre como identificador único para evitar duplicados.
            var productoExistente = pedido.Productos.FirstOrDefault(p => p.Nombre == dto.Nombre && p.precio == dto.precio);
            if (productoExistente != null)
            {
                // Incrementar cantidad
                productoExistente.Cantidad += dto.Cantidad;
                GuardarPedidos(pedidos);
                return "Cantidad actualizada";
            }
            else
            {
                // Generar nuevo Id para el producto (máximo actual + 1, o 1 si no hay productos)
                int nuevoId = pedido.Productos.Any() ? pedido.Productos.Max(p => p.Id) + 1 : 1;

                var nuevoProducto = new Producto
                {
                    Id = nuevoId,
                    Nombre = dto.Nombre,
                    precio = dto.precio,
                    Cantidad = dto.Cantidad
                };
                pedido.Productos.Add(nuevoProducto);
                GuardarPedidos(pedidos);
                return "Producto agregado";
            }
        }

        // Obtiene un pedido con sus cálculos: subtotal, ITBIS (18%) y total.
        public (Pedido pedido, decimal subtotal, decimal itbis, decimal total, List<dynamic> productosConTotal) ObtenerPedido(int pedidoId)
        {
            var pedidos = ConseguirPedidos();
            var pedido = pedidos.FirstOrDefault(p => p.Id == pedidoId);

            if (pedido == null)
                throw new KeyNotFoundException($"Pedido con ID {pedidoId} no encontrado.");

            var productosConTotal = new List<dynamic>();
            decimal subtotal = 0;

            foreach (var prod in pedido.Productos)
            {
                decimal totalProducto = prod.precio * prod.Cantidad;
                productosConTotal.Add(new
                {
                    id = prod.Id,
                    nombre = prod.Nombre,
                    precio = prod.precio,
                    cantidad = prod.Cantidad,
                    total = totalProducto
                });
                subtotal += totalProducto;
            }

            decimal itbis = subtotal * 0.18m;
            decimal total = subtotal + itbis;

            return (pedido, subtotal, itbis, total, productosConTotal);
        }

        // Elimina un producto de un pedido por su Id.
        public void EliminarProducto(int pedidoId, int productId)
        {
            var pedidos = ConseguirPedidos();
            var pedido = pedidos.FirstOrDefault(p => p.Id == pedidoId);

            if (pedido == null)
                throw new KeyNotFoundException($"Pedido con ID {pedidoId} no encontrado.");

            if (pedido.Estado == "Cancelado")
                throw new InvalidOperationException("No se puede modificar un pedido cancelado.");

            var producto = pedido.Productos.FirstOrDefault(p => p.Id == productId);
            if (producto == null)
                throw new KeyNotFoundException($"Producto con ID {productId} no encontrado en el pedido.");

            pedido.Productos.Remove(producto);
            GuardarPedidos(pedidos);
        }

        // Cancela un pedido (cambia su estado a "Cancelado").
        public void CancelarPedido(int pedidoId)
        {
            var pedidos = ConseguirPedidos();
            var pedido = pedidos.FirstOrDefault(p => p.Id == pedidoId);

            if (pedido == null)
                throw new KeyNotFoundException($"Pedido con ID {pedidoId} no encontrado.");

            if (pedido.Estado == "Cancelado")
                throw new InvalidOperationException("El pedido ya está cancelado.");

            pedido.Estado = "Cancelado";
            GuardarPedidos(pedidos);
        }
    }
}