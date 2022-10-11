using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using raw_ws.Data.DbContexts;
using raw_ws.Data.Dto;
using raw_ws.Helpers;
using SqlException = raw_ws.Helpers.SqlException;

namespace raw_ws.Repositories
{
    public class MainRepository
    {
        #region [Variables & Constructor]
        private readonly RpvDbContext _context;
        private readonly AppSettings _appSettings;
        //private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly int repositorio = 27;
        
        public MainRepository(IOptions<AppSettings> appSettings, RpvDbContext context)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }
        #endregion

        #region [Public]
        public async Task<AuthDto> Authenticate(LoginDto auth)
        {            
            // Comprobamos si viene informado el login y el password
            if (string.IsNullOrEmpty(auth.Login) || string.IsNullOrEmpty(auth.Password))
                throw new NotContentException("Usuario o password no válidos");

            // Obtenemos los usuarios del login solicitado
            var usersJson = "";
            var conn = new SqlConnection(_context.ConnIntranet);
            SqlCommand cmd = new SqlCommand("sp_auth", conn);
            cmd.CommandTimeout = 120;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@login", System.Data.SqlDbType.NVarChar).Value = auth.Login;
            cmd.Parameters.Add("@pass", System.Data.SqlDbType.NVarChar).Value = auth.Password;

            await conn.OpenAsync();
            try
            {
                var dataReader = await cmd.ExecuteReaderAsync();
                while (dataReader.Read())
                {
                    usersJson += dataReader.GetString(0);
                }
            }
            catch (Exception e)
            {
                usersJson = "";
            }
            finally
            {
                await conn.CloseAsync();
            }
            
            
            
            if(usersJson == "")
                throw new NotContentException("El usuario no es válido");

            var users = (List<UserDto>) null;
            try
            {
                users  = JsonSerializer.Deserialize<List<UserDto>>(usersJson);
            }
            catch (Exception ex)
            {
                throw new Helpers.SqlException("Error de deserialización: \n" + ex.Message);
            }
            
            // Comprobamos si existe el usuario
            if (users == null) throw new NotContentException("El usuario no es válido");

            // Comprobamos si coincide el password encriptándolo primero
            //if (EncryptPassword(auth.Password) != user.Password) throw new BadRequestException("El password no es válido");
            
            // Convertimos la key de appsettings en un array de bytes
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

            // Crea el descriptor del token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // 	Establece las notificaciones de salida que se van a incluir en el token emitido.
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, users[0].IdPersona),
                    new Claim(ClaimTypes.Email, users[0].Detalle)
                }),

                // Establece la fecha que expira el token
                Expires = DateTime.UtcNow.AddDays(2),

                // Establece las credenciales que se utilizan para firmar el token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)               
            };

            // Creamos un manejador para el token
            var tokenHandler = new JwtSecurityTokenHandler();

            // Creamos el token en base al descriptor del token especificado
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Serializa el token
            var tokenString = tokenHandler.WriteToken(token);

            // Creamos el AuthDto de salida
            AuthDto response = new AuthDto
            {
                Users = usersJson,
                Token = tokenString
            };          

            return response;
        }
        
        #endregion

        #region [Getters]
        public async Task<int> GetAppVersion()
        {
            var res = -1;
            int rowcount = 0;
           
            
            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_getAppVersion", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader reader = await cmd.ExecuteReaderAsync();
            
                while (reader.Read())
                {
                    res = reader.GetInt32(0);
                }
            
                await reader.CloseAsync();

            }
            
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }
            
            return res;
        }
        
        public async Task<object> GetTodosCentros(string authorization, int idUser)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;
            
            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_getTodosCentros", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@idUser", SqlDbType.NVarChar).Value = idUser;

                SqlDataReader reader = await cmd.ExecuteReaderAsync();
            
                while (reader.Read())
                {
                    res += reader.GetString(0);
                }
            
                await reader.CloseAsync();

            }
            // catch (NotContentException ex)
            // {   
            //     throw new NotContentException(ex.Message);
            // }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }
            
            //return JsonSerializer.Serialize<TareasOnDemand[]>(JsonSerializer.Deserialize<TareasOnDemand[]>(res));
            return res;
        }
        
        public async Task<object> GetTodosTipos(string authorization)
        {
            var res = "";
            int rowcount = 0;
            // var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            // var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            // var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;
            
            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getTodosTipos", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                // cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
              

                SqlDataReader reader = await cmd.ExecuteReaderAsync();
            
                while (reader.Read())
                {
                    res += reader.GetString(0);
                }
            
                await reader.CloseAsync();
                
                if(string.IsNullOrEmpty(res))
                    throw new NotContentException("Sin datos");

            }
            catch (NotContentException ex)
            {   
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }
            
            //return JsonSerializer.Serialize<TareasOnDemand[]>(JsonSerializer.Deserialize<TareasOnDemand[]>(res));
            return res;
        }
        
        public async Task<object> GetCentrosRuta(string authorization, int idUser)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;
            
            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_getCentrosRuta", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@idUser", SqlDbType.NVarChar).Value = idUser;

                SqlDataReader reader = await cmd.ExecuteReaderAsync();
            
                while (reader.Read())
                {
                    res += reader.GetString(0);
                }
            
                await reader.CloseAsync();
                
                if(string.IsNullOrEmpty(res))
                    throw new NotContentException("No hay centros");
                    
            }
            catch (NotContentException ex)
            {   
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }
            
            //return JsonSerializer.Serialize<TareasOnDemand[]>(JsonSerializer.Deserialize<TareasOnDemand[]>(res));
            return res;
        }
        
        public async Task<int> GetVisita(string authorization, int idUser, int idCentro)
        {
            var res = -1;
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;
            
            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_getVisita", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@idUser", SqlDbType.NVarChar).Value = idUser;
                cmd.Parameters.Add("@idCentro", SqlDbType.NVarChar).Value = idCentro;

                SqlDataReader reader = await cmd.ExecuteReaderAsync();
            
                while (reader.Read())
                {
                    res = reader.GetInt32(0);
                }
            
                await reader.CloseAsync();
                
                if (res == -1)
                    throw new NotContentException("Error: " + "No se ha isertado la visita correctamente");
                
            }
            catch (NotContentException ex)
            {   
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }
            
            //return JsonSerializer.Serialize<TareasOnDemand[]>(JsonSerializer.Deserialize<TareasOnDemand[]>(res));
            return res;
        }
        
        public async Task<int> CerrarVisita(string authorization, int idVisita)
        {
            var res = 0;
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;
            
            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_cerrarVisita", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@idVisita", SqlDbType.Int).Value = idVisita;

                await cmd.ExecuteNonQueryAsync();
                
                // SqlDataReader reader = await cmd.ExecuteReaderAsync();
                // while (reader.Read())
                // {
                //     res = reader.GetInt32(0);
                // }
                //
                // await reader.CloseAsync();
                //
                // if (res == -1)
                //     throw new NotContentException("Error: " + "No se ha isertado la visita correctamente");
                
            }
            catch (NotContentException ex)
            {   
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }
            
            //return JsonSerializer.Serialize<TareasOnDemand[]>(JsonSerializer.Deserialize<TareasOnDemand[]>(res));
            return res;
        }
        
        public async Task<string> GetContactos(string authorization, int idCentro)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var idPerson = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;
            
            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getContactos", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = idPerson;
                cmd.Parameters.Add("@idCentro", SqlDbType.NVarChar).Value = idCentro;

                SqlDataReader reader = await cmd.ExecuteReaderAsync();
            
                while (reader.Read())
                {
                    res += reader.GetString(0);
                }
            
                await reader.CloseAsync();
                
                if (res == "")
                    throw new NotContentException("Sin datos");
                
            }
            catch (NotContentException ex)
            {   
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }
            
            //return JsonSerializer.Serialize<TareasOnDemand[]>(JsonSerializer.Deserialize<TareasOnDemand[]>(res));
            return res;
        }
        
        public async Task<string> GetDocumentacion(string authorization, int idCentro, int idUsuario)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var idPerson = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;
            
            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getDocumentacion", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = idPerson;
                cmd.Parameters.Add("@idCentro", SqlDbType.NVarChar).Value = idCentro;
                cmd.Parameters.Add("@idUsuario", SqlDbType.NVarChar).Value = idUsuario;

                SqlDataReader reader = await cmd.ExecuteReaderAsync();
            
                while (reader.Read())
                {
                    res += reader.GetString(0);
                }
            
                await reader.CloseAsync();
                
                if (res == "")
                    throw new NotContentException("Sin datos");
                
            }
            catch (NotContentException ex)
            {   
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }
            
            //return JsonSerializer.Serialize<TareasOnDemand[]>(JsonSerializer.Deserialize<TareasOnDemand[]>(res));
            return res;
        }

        public async Task<string> GetProductosCompetencia(string authorization)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var idPerson = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getProductosCompetencia", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = idPerson;

                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }

                await reader.CloseAsync();

                if (res == "")
                    throw new NotContentException("Sin datos");

            }
            catch (NotContentException ex)
            {
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            //return JsonSerializer.Serialize<TareasOnDemand[]>(JsonSerializer.Deserialize<TareasOnDemand[]>(res));
            return res;
        }

        public async Task<string> GetProductos(string authorization)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var idPerson = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getProductos", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = idPerson;

                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }

                await reader.CloseAsync();

                if (res == "")
                    throw new NotContentException("Sin datos");

            }
            catch (NotContentException ex)
            {
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            //return JsonSerializer.Serialize<TareasOnDemand[]>(JsonSerializer.Deserialize<TareasOnDemand[]>(res));
            return res;
        }

        public async Task<string> GetReporteProductos(string authorization, int idCentro)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var idPerson = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getReporteProductos", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = idPerson;
                cmd.Parameters.Add("@idCentro", SqlDbType.Int).Value = idCentro;

                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }

                await reader.CloseAsync();

                if (res == "")
                    throw new NotContentException("Sin datos");

            }
            catch (NotContentException ex)
            {
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            //return JsonSerializer.Serialize<TareasOnDemand[]>(JsonSerializer.Deserialize<TareasOnDemand[]>(res));
            return res;
        }

        public async Task<string> GetReporteProductosCompetencia(string authorization, int idCentro)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var idPerson = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getReporteProductosCompetencia", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = idPerson;
                cmd.Parameters.Add("@idCentro", SqlDbType.Int).Value = idCentro;

                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }

                await reader.CloseAsync();

                if (res == "")
                    throw new NotContentException("Sin datos");

            }
            catch (NotContentException ex)
            {
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            //return JsonSerializer.Serialize<TareasOnDemand[]>(JsonSerializer.Deserialize<TareasOnDemand[]>(res));
            return res;
        }
        
        public async Task<string> GetFotos(string authorization, int idCentro, int categoria)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var idPerson = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getFotos", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = idPerson;
                cmd.Parameters.Add("@idCentro", SqlDbType.Int).Value = idCentro;
                cmd.Parameters.Add("@categoria", SqlDbType.Int).Value = categoria;

                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }

                await reader.CloseAsync();

                if (res == "")
                    throw new NotContentException("Sin datos");

            }
            catch (NotContentException ex)
            {
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            //return JsonSerializer.Serialize<TareasOnDemand[]>(JsonSerializer.Deserialize<TareasOnDemand[]>(res));
            return res;
        }
        
        public async Task<string> GetFotos2(string authorization, int idVisita, int idCentro, int categoria)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var idPerson = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getFotos2", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = idPerson;
                cmd.Parameters.Add("@idVisita", SqlDbType.Int).Value = idVisita;
                cmd.Parameters.Add("@idCentro", SqlDbType.Int).Value = idCentro;
                cmd.Parameters.Add("@categoria", SqlDbType.Int).Value = categoria;

                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }

                await reader.CloseAsync();

                if (res == "")
                    throw new NotContentException("Sin datos");

            }
            catch (NotContentException ex)
            {
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            //return JsonSerializer.Serialize<TareasOnDemand[]>(JsonSerializer.Deserialize<TareasOnDemand[]>(res));
            return res;
        }
        
        public async Task<string> GetReporteEa(string authorization, int idCentro)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getReporteEa", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@idCentro", SqlDbType.Int).Value = idCentro;
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }


                // if (res.Contains("null"))
                if (JsonSerializer.Deserialize<ReporteEaFullDto[]>(res)[0].Ea == null)
                    throw new NotContentException("Sin datos");
            }
            catch (JsonException jsEx)
            {
                throw new NotContentException("Sin datos");
            }
            catch (NotContentException ex)
            {
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(ea.Ea) + JsonSerializer.Serialize(ea.EaRef);
            return res;
        }
        
        public async Task<string> GetPromociones(string authorization, int idCentro)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getPromociones", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@idCentro", SqlDbType.Int).Value = idCentro;
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }


                // if (res.Contains("null"))
                if (string.IsNullOrEmpty(res))
                    throw new NotContentException("Sin datos");
            }
            catch (JsonException jsEx)
            {
                throw new NotContentException("Sin datos");
            }
            catch (NotContentException ex)
            {
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(ea.Ea) + JsonSerializer.Serialize(ea.EaRef);
            return res;
        }
        
        public async Task<string> GetIncidencias(string authorization, int idCentro)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getIncidencias", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@idCentro", SqlDbType.Int).Value = idCentro;
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }
                
                if (string.IsNullOrEmpty(res))
                    throw new NotContentException("Sin datos");
                
            }
            catch (JsonException jsEx)
            {
                throw new NotContentException("Sin datos");
            }
            catch (NotContentException ex)
            {
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(ea.Ea) + JsonSerializer.Serialize(ea.EaRef);
            return res;
        }
        
        public async Task<string> GetTipoIncidencias(string authorization)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getIncidenciasTipo", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }
                
                if (string.IsNullOrEmpty(res))
                    throw new NotContentException("Sin datos");
                
            }
            catch (JsonException jsEx)
            {
                throw new NotContentException("Sin datos");
            }
            catch (NotContentException ex)
            {
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(ea.Ea) + JsonSerializer.Serialize(ea.EaRef);
            return res;
        }
        
        public async Task<string> GetIncidenciasAcciones(string authorization)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getIncidenciasAcciones", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }
                
                if (string.IsNullOrEmpty(res))
                    throw new NotContentException("Sin datos");
                
            }
            catch (JsonException jsEx)
            {
                throw new NotContentException("Sin datos");
            }
            catch (NotContentException ex)
            {
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(ea.Ea) + JsonSerializer.Serialize(ea.EaRef);
            return res;
        }
        
        public async Task<object> GetReportePromociones(string authorization, int idCentro)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getReportePromociones", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@idCentro", SqlDbType.Int).Value = idCentro;
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }


                // if (res.Contains("null"))
                if (string.IsNullOrEmpty(res))
                    throw new NotContentException("Sin datos");
            }
            catch (JsonException jsEx)
            {
                throw new NotContentException("Sin datos");
            }
            catch (NotContentException ex)
            {
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(ea.Ea) + JsonSerializer.Serialize(ea.EaRef);
            return res;
        }
        
        public async Task<object> GetReporteDoblesUbicaciones(string authorization, int idVisita)
        {
            var res = "";
            int rowcount = 0;
            // var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            // var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            // var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getReporteDoblesUbicaciones", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                // cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@idVisita", SqlDbType.Int).Value = idVisita;
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }


                // if (res.Contains("null"))
                if (string.IsNullOrEmpty(res))
                    throw new NotContentException("Sin datos");
            }
            catch (JsonException jsEx)
            {
                throw new NotContentException("Sin datos");
            }
            catch (NotContentException ex)
            {
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(ea.Ea) + JsonSerializer.Serialize(ea.EaRef);
            return res;
        }
        
        public async Task<object> GetReportePlvMenudo(string authorization, int idVisita)
        {
            var res = "";
            int rowcount = 0;
            // var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            // var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            // var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_getReportePlvMenudo", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                // cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@idVisita", SqlDbType.Int).Value = idVisita;
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }


                // if (res.Contains("null"))
                if (string.IsNullOrEmpty(res))
                    throw new NotContentException("Sin datos");
            }
            catch (JsonException jsEx)
            {
                throw new NotContentException("Sin datos");
            }
            catch (NotContentException ex)
            {
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(ea.Ea) + JsonSerializer.Serialize(ea.EaRef);
            return res;
        }
        
        


        #endregion

        #region [Setters]
        public async Task<object> SetContacto(string authorization, ContactoRawDto contacto)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_setContacto", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@person", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@paramJson", SqlDbType.NVarChar).Value = JsonSerializer.Serialize(contacto);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetInt32(0);
                }

                await reader.CloseAsync();
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(grupo);
            return res;
        }

        public async Task<object> SetReporteProducto(string authorization, ReporteProductoDto[] reporte)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_setReporteProducto", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@paramJson", SqlDbType.NVarChar).Value = JsonSerializer.Serialize(reporte);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetInt32(0);
                }

                await reader.CloseAsync();
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(grupo);
            return res;
        }
        
        public async Task<object> SetReporteIncidencia(string authorization, ReporteIncidenciasDto incidencia)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_setReporteIncidencia", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@paramJson", SqlDbType.NVarChar).Value = JsonSerializer.Serialize(incidencia);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetInt32(0);
                }

                await reader.CloseAsync();
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(grupo);
            return res;
        }

        public async Task<object> SetReporteProductoCompetencia(string authorization, ReporteProductoCompetenciaDto[] reporte)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_setReporteProductoCompetencia", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@paramJson", SqlDbType.NVarChar).Value = JsonSerializer.Serialize(reporte);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetInt32(0);
                }

                await reader.CloseAsync();
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(grupo);
            return res;
        }
        
        public async Task<string> SetFoto(string authorization, FotoRawInterface t)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();

            try
            {
                if (t.UrlFoto.Length < 1)
                {
                    throw new Helpers.SqlException("Error");
                }
                if (t.IdFoto == 0)
                {
                    string pixName = F.Normalize(F.Normalize(t.Nombre)
                                                 + "_" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()
                                                 + ".jpg");

                    //Subir foto...
                    string anio = DateTime.Now.ToLocalTime().ToString("yyyy");
                    string mes = DateTime.Now.ToLocalTime().ToString("MM");
                    Directory.CreateDirectory(@"F:\inetpub\fotos.rpv.es\repositorio\" + repositorio + "\\" + anio + "\\" + mes + "\\");
                    Directory.CreateDirectory(@"F:\inetpub\fotos.rpv.es\repositorio\" + repositorio + "\\thumbs\\" + anio + "\\" + mes + "\\");
                    pixName = anio + "/" + mes + "/" + pixName;
                    MemoryStream msImage = new MemoryStream(Convert.FromBase64String(t.UrlFoto));
                    Bitmap bmpImage = new Bitmap(msImage);
                    Bitmap resizedImage = F.CreateThumbnail(bmpImage, 1200, 900);
                    Bitmap resizedThumb = F.CreateThumbnail(bmpImage, 267, 200);

                    resizedImage.Save(@"F:\inetpub\fotos.rpv.es\repositorio\"+ repositorio + "\\" + pixName, ImageFormat.Jpeg);
                    resizedThumb.Save(@"F:\inetpub\fotos.rpv.es\repositorio\" + repositorio + "\\thumbs\\" + pixName, ImageFormat.Jpeg);

                    resizedThumb.Dispose();
                    resizedImage.Dispose();
                    bmpImage.Dispose();
                    msImage.Close();

                    t.UrlFoto = @"fotos.rpv.es/repositorio/" + repositorio + "/" + pixName;
                    t.Nombre = pixName;

                }
                
                //Subir row...
                SqlCommand cmd = new SqlCommand("sp_app_setFoto", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@paramJson", SqlDbType.NVarChar).Value = JsonSerializer.Serialize(t);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }

                await reader.CloseAsync();
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(grupo);
            return res;
        }
        
        public async Task<string> SetReporteEa(string authorization, ReporteEaFullDto ea)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_setReporteEa", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@paramJsonEa", SqlDbType.NVarChar).Value = JsonSerializer.Serialize(ea.Ea);
                cmd.Parameters.Add("@paramJsonEaRef", SqlDbType.NVarChar).Value = JsonSerializer.Serialize(ea.EaRef);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }

                await reader.CloseAsync();
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(ea.Ea) + JsonSerializer.Serialize(ea.EaRef);
            return res;
        }
        
        public async Task<object> SetReportePromocion(string authorization, ReportePromocionRawDto promo)
        {
            var res = "";
            int rowcount = 0;
            var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_setReportePromocion", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@paramJson", SqlDbType.NVarChar).Value = JsonSerializer.Serialize(promo);

                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }

                await reader.CloseAsync();
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(ea.Ea) + JsonSerializer.Serialize(ea.EaRef);
            return res;
        }
        
        public async Task<object> SetReporteDoblesUbicaciones(string authorization, ReporteDobleUbicacionRawDto[] reporte)
        {
            var res = "";
            int rowcount = 0;
            // var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            // var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            // var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_setReporteDoblesUbicaciones", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                // cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@paramJson", SqlDbType.NVarChar).Value = JsonSerializer.Serialize(reporte);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }

                await reader.CloseAsync();
            
                // if (res.Contains("null"))
                if (string.IsNullOrEmpty(res))
                    throw new NotContentException("Sin datos");
            }
            catch (JsonException jsEx)
            {
                throw new NotContentException("Sin datos");
            }
            catch (NotContentException ex)
            {
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(grupo);
            return res;
        }
        
        public async Task<object> SetReportePlvMenudo(string authorization, ReportePlvMenudoDto reporte)
        {
            var res = "";
            int rowcount = 0;
            // var tok = authorization.Replace("Bearer ", "").Replace(System.Environment.NewLine, "");
            // var jwttoken = new JwtSecurityTokenHandler().ReadJwtToken(tok);
            // var person = jwttoken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;

            var conn = new SqlConnection(_context.ConnRaw);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_app_setReportePlvMenudo", conn);
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                // cmd.Parameters.Add("@idPerson", SqlDbType.NVarChar).Value = person;
                cmd.Parameters.Add("@paramJson", SqlDbType.NVarChar).Value = JsonSerializer.Serialize(reporte);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    res += reader.GetString(0);
                }

                await reader.CloseAsync();
            
                // if (res.Contains("null"))
                if (string.IsNullOrEmpty(res))
                    throw new NotContentException("Sin datos");
            }
            catch (JsonException jsEx)
            {
                throw new NotContentException("Sin datos");
            }
            catch (NotContentException ex)
            {
                throw new NotContentException(ex.Message);
            }
            catch (Exception e)
            {
                throw new Helpers.SqlException("Error: " + e.ToString());
            }
            finally
            {
                conn.Close();
            }

            // return JsonSerializer.Serialize(grupo);
            return res;
        }
        
        #endregion
        
    }
}