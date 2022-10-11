namespace raw_ws.Data.Dto
{
    public partial class ReporteIncidenciasDto
    {
        public long? IdIncidencia { get; set; }
        public long? IdVisita { get; set; }
        public long? IdTipoIncidencia { get; set; }
        public long? IdAccion { get; set; }
        public string Observaciones { get; set; }
        public bool? Deleted { get; set; }
        public long? Estado { get; set; }
        public long? IdVisitaModificacion { get; set; }
        public string UrlFoto { get; set; }
    }
}