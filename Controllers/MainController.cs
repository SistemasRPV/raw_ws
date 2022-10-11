using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using raw_ws.Data.Dto;
using raw_ws.Repositories;

namespace raw_ws.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        #region [Variables & Constructor]
        private MainRepository _repository;

        public MainController(MainRepository repository)
        {
            _repository = repository;
        }
        #endregion
        
        #region [Public]
        [AllowAnonymous]
        [HttpPost("Auth")]
        public async Task<ActionResult<AuthDto>> Authenticate([FromBody]LoginDto auth)
        {
            return Ok(await _repository.Authenticate(auth));
        }
        #endregion

        #region [Getters]
        [AllowAnonymous]
        [HttpGet("GetAppVersion")]
        public async Task<OkObjectResult> GetAppVersion()
        {
            return Ok(await _repository.GetAppVersion());
        }
        
        [HttpGet("GetTodosCentros")]
        public async Task<OkObjectResult> GetTodosCentros([FromHeader] string authorization, int idUser)
        {
            return Ok(await _repository.GetTodosCentros(authorization, idUser));
        }
        
        [HttpGet("GetTodosTipos")]
        public async Task<OkObjectResult> GetTodosTipos([FromHeader] string authorization)
        {
            return Ok(await _repository.GetTodosTipos(authorization));
        }
        
        [HttpGet("GetCentrosRuta")]
        public async Task<OkObjectResult> GetCentrosRuta([FromHeader] string authorization, int idUser)
        {
            return Ok(await _repository.GetCentrosRuta(authorization, idUser));
        }
        
        [HttpGet("GetVisita")]
        public async Task<OkObjectResult> GetVisita([FromHeader] string authorization, int idUser, int idCentro)
        {
            return Ok(await _repository.GetVisita(authorization, idUser, idCentro));
            
        }

        [HttpGet("GetContactos")]
        public async Task<OkObjectResult> GetContactos([FromHeader] string authorization, int idCentro)
        {
            return Ok(await _repository.GetContactos(authorization, idCentro));
            
        }
        
        [HttpGet("GetDocumentacion")]
        public async Task<OkObjectResult> GetDocumentacion([FromHeader] string authorization, int idCentro, int idUsuario)
        {
            return Ok(await _repository.GetDocumentacion(authorization, idCentro, idUsuario));
        }

        [HttpGet("GetProductos")]
        public async Task<OkObjectResult> GetProductos([FromHeader] string authorization)
        {
            return Ok(await _repository.GetProductos(authorization));
        }

        [HttpGet("GetProductosCompetencia")]
        public async Task<OkObjectResult> GetProductosCompetencia([FromHeader] string authorization)
        {
            return Ok(await _repository.GetProductosCompetencia(authorization));
        }

        [HttpGet("GetReporteProductos")]
        public async Task<OkObjectResult> GetReporteProductos([FromHeader] string authorization, int idCentro)
        {
            return Ok(await _repository.GetReporteProductos(authorization, idCentro));
        }

        [HttpGet("GetReporteProductosCompetencia")]
        public async Task<OkObjectResult> GetReporteProductosCompetencia([FromHeader] string authorization, int idCentro)
        {
            return Ok(await _repository.GetReporteProductosCompetencia(authorization, idCentro));
        }
        
        [HttpGet("GetFotos")]
        public async Task<OkObjectResult> GetFotos([FromHeader] string authorization, int idCentro, int categoria)
        {
            return Ok(await _repository.GetFotos(authorization, idCentro, categoria));
        }
        
        [HttpGet("GetFotos2")]
        public async Task<OkObjectResult> GetFotos2([FromHeader] string authorization, int idVisita, int idCentro, int categoria)
        {
            return Ok(await _repository.GetFotos2(authorization, idVisita, idCentro, categoria));
        }
        
        [HttpGet("GetReporteEa")]
        public async Task<OkObjectResult> GetReporteEa([FromHeader] string authorization, int idCentro)
        {
            return Ok(await _repository.GetReporteEa(authorization, idCentro));
        }
        
        [HttpGet("GetPromociones")]
        public async Task<OkObjectResult> GetPromociones([FromHeader] string authorization, int idCentro)
        {
            return Ok(await _repository.GetPromociones(authorization, idCentro));
        }
        
        [HttpGet("GetIncidencias")]
        public async Task<OkObjectResult> GetIncidencias([FromHeader] string authorization, int idCentro)
        {
            return Ok(await _repository.GetIncidencias(authorization, idCentro));
        }
        
        [HttpGet("GetTipoIncidencias")]
        public async Task<OkObjectResult> GetTipoIncidencias([FromHeader] string authorization)
        {
            return Ok(await _repository.GetTipoIncidencias(authorization));
        }
        
        [HttpGet("GetIncidenciasAcciones")]
        public async Task<OkObjectResult> GetIncidenciasAcciones([FromHeader] string authorization)
        {
            return Ok(await _repository.GetIncidenciasAcciones(authorization));
        }
        
        [HttpGet("GetReportePromociones")]
        public async Task<OkObjectResult> GetReportePromociones([FromHeader] string authorization, int idCentro)
        {
            return Ok(await _repository.GetReportePromociones(authorization, idCentro));
        }
        
        [HttpGet("GetReporteDoblesUbicaciones")]
        public async Task<OkObjectResult> GetReporteDoblesUbicaciones([FromHeader] string authorization, int idVisita)
        {
            return Ok(await _repository.GetReporteDoblesUbicaciones(authorization, idVisita));
        }
        
        [HttpGet("GetReportePlvMenudo")]
        public async Task<OkObjectResult> GetReportePlvMenudo([FromHeader] string authorization, int idVisita)
        {
            return Ok(await _repository.GetReportePlvMenudo(authorization, idVisita));
        }
        

        #endregion
        
        #region [Setters]
        
        [HttpPost("SetContacto")]
        public async Task<ActionResult<string>> SetContacto([FromHeader] string authorization, [FromBody]ContactoRawDto contacto)
        {
            return Ok(await _repository.SetContacto(authorization, contacto));
        }

        [HttpPost("SetReporteProducto")]
        public async Task<ActionResult<string>> SetReporteProducto([FromHeader] string authorization, [FromBody]ReporteProductoDto[] reporte)
        {
            return Ok(await _repository.SetReporteProducto(authorization, reporte));
        }

        [HttpPost("SetReporteProductoCompetencia")]
        public async Task<ActionResult<string>> SetReporteProductoCompetencia([FromHeader] string authorization, [FromBody]ReporteProductoCompetenciaDto[] reporte)
        {
            return Ok(await _repository.SetReporteProductoCompetencia(authorization, reporte));
        }
        
        [HttpPost("SetFoto")]
        public async Task<ActionResult<string>> SetFoto([FromHeader] string authorization, [FromBody]FotoRawInterface foto)
        {
            return Ok(await _repository.SetFoto(authorization, foto));
        }
        
        [HttpPost("SetReporteEa")]
        public async Task<ActionResult<string>> SetReporteEa([FromHeader] string authorization, [FromBody]ReporteEaFullDto ea)
        {
            return Ok(await _repository.SetReporteEa(authorization, ea));
        }
        
        [HttpPost("SetReportePromocion")]
        public async Task<ActionResult<string>> SetReportePromocion([FromHeader] string authorization, [FromBody]ReportePromocionRawDto promo)
        {
            return Ok(await _repository.SetReportePromocion(authorization, promo));
        }
        
        [HttpPost("SetReporteIncidencia")]
        public async Task<ActionResult<string>> SetReporteIncidencia([FromHeader] string authorization, [FromBody]ReporteIncidenciasDto incidencia)
        {
            return Ok(await _repository.SetReporteIncidencia(authorization, incidencia));
        }
        
        [HttpPost("CerrarVisita")]
        public async Task<ActionResult<string>> CerrarVisita([FromHeader] string authorization, int idVisita)
        {
            return Ok(await _repository.CerrarVisita(authorization, idVisita));
            
        }
        
        [HttpPost("SetReporteDoblesUbicaciones")]
        public async Task<ActionResult<string>> SetReporteDoblesUbicaciones([FromHeader] string authorization, [FromBody]ReporteDobleUbicacionRawDto[] reporte)
        {
            return Ok(await _repository.SetReporteDoblesUbicaciones(authorization, reporte));
        }
        
        [HttpPost("SetReportePlvMenudo")]
        public async Task<ActionResult<string>> SetReportePlvMenudo([FromHeader] string authorization, [FromBody]ReportePlvMenudoDto reporte)
        {
            return Ok(await _repository.SetReportePlvMenudo(authorization, reporte));
        }
        
        #endregion

    }
}