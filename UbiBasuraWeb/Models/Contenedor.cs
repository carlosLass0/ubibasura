namespace UbiBasuraWeb.Models
{
    public class Contenedor
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string Tipo { get; set; }
        public string Estado { get; set; }
    }
}