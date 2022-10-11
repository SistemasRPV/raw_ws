namespace raw_ws.Data.Dto
{
    public class ReporteDobleUbicacionRawDto
    {
        public int IdDobleUbicacion { get; set; }
        public int IdVisita { get; set; }
        public int IdTipoUbicacion { get; set; }
        public string Foto { get; set; }
        public bool HayProductos { get; set; }
        public bool Deleted { get; set; }
    }
}