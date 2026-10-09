namespace UbiBasuraWeb.Models
{
    public class Reporte
    {
        public int Id { get; set; }
        public int ContenedorId { get; set; }
        public int UsuarioId { get; set; }
        public string TipoReporte { get; set; }
        public string Comentario { get; set; }
        public string Estado { get; set; }
        public DateTime Fecha { get; set; }
    }
}