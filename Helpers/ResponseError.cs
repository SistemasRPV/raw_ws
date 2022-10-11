using Newtonsoft.Json;

namespace raw_ws.Helpers
{
    /// <summary>
    /// Clase tipo respuesta de error (payload de respuesta) utilizada en el middleware de control de excepciones
    /// </summary>
    public class ResponseError
    {
        public int Code { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this).ToLower();
        }
    }
}
