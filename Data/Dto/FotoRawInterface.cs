namespace raw_ws.Data.Dto
{
    public class FotoRawInterface
    {
        public long? IdFoto { get; set; }
        public long? IdVisita { get; set; }
        public string Nombre { get; set; }
        public long? Categoria { get; set; }
        public string Observaciones { get; set; }
        public string Ubicacion { get; set; }
        public string UrlFoto { get; set; }
        public string IdAux { get; set; }
        public bool? Deleted { get; set; }
    }
}