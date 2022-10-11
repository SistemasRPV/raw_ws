namespace raw_ws.Data.Dto
{
    public class ReportePromocionRawDto
    {
        public int? IdRPromocion { get; set; }
        public int? IdVisita { get; set; }
        public int? IdPromocion { get; set; }
        public int? Estado { get; set; }
        public int? IdMotivo { get; set; }
        public string Observaciones { get; set; }
        public bool? Deleted { get; set; }
    }
}