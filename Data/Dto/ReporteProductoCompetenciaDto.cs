namespace raw_ws.Data.Dto
{
    public  class ReporteProductoCompetenciaDto
    {
        public int? IdReporte { get; set; }
        public int? IdVisita { get; set; }
        public string Ean { get; set; }
        public bool? Surtido { get; set; }
        public float? Pvp { get; set; }
        public int? FacingIni { get; set; }
        public int? FacingFin { get; set; }
        public int? Rotura { get; set; }
        public int? SinBalizaje { get; set; }
        public int? EnPromocion { get; set; }
        public float? PrecioPromocion { get; set; }
        public int? AlturaBalda { get; set; }
        public int? Visibilidad { get; set; }
        public bool? Deleted { get; set; }
    }
}