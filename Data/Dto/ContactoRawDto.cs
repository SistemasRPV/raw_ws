namespace raw_ws.Data.Dto
{
    public partial class ContactoRawDto
    {
        public long IdContacto { get; set; }
        public long? IdVisita { get; set; }
        public string Nombre { get; set; }
        public string Cargo { get; set; }
        public string Observaciones { get; set; }
        public long? Telefono { get; set; }
        public string Correo { get; set; }
        public bool? Deleted { get; set; }
    }
}