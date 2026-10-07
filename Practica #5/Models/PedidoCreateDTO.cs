namespace Practica__5.Models
{
    public class PedidoCreateDTO
    {
        public int Id { get; set; }
        public double Subtotal { get; set; }
        public double Impuestos { get; set; }
        public int Total { get; set; }
        public string Estado { get; set; }
    }
}
