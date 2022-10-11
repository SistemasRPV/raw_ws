namespace raw_ws.Data.Dto
{    
    public class UserDto
    {
        public int    IdUsuario { get; set; }
        public string IdPersona { get; set; }
        public int    IdPerfil { get; set; }
        public string Nombre { get; set; }
        public string Detalle { get; set; }
        public string Funciones { get; set; }
        public string Topics { get; set; }
        public string IdPersonaResponsable { get; set; }
        public bool   Titular { get; set; }
        public string Login { get; set; }
        //public string Password { get; set; }

        public UserDto()
        {
            IdUsuario  = 0;
            IdPersona = "";
            IdPerfil = 0;
            Nombre = "";
            Detalle = "";
            Funciones = "";
            Topics = "";
            IdPersonaResponsable = "";
            Titular = false;
            Login = "";
            //Password = "";
        }
    }
}
