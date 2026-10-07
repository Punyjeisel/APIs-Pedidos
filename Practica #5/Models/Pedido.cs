namespace Practica__5.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public List<Producto> Productos { get; set; }
        public double Subtotal { get; set; }
        public double Impuestos { get; set; }
        public int Total { get; set; }
        public string Estado { get; set; }
    }
}
