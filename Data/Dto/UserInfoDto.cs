
 namespace raw_ws.Data.Dto
{
    public class UserInfoDto
    {
        public string IdPersona { get; set; }
        public string Login { get; set; }

        public UserInfoDto(string idPersona, string login)
        {
            IdPersona = idPersona;
            Login = login;
        }

    }
}
