using Grabaciones.Models;
using Grabaciones.Services.Econtact;
using Grabaciones.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace Grabaciones.Controllers
{
    [ApiController]
    [Route("v1")]
    public class GrabacionesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GrabacionesController> _logger;
        private IDescargaDiaria _descargaDiaria;
        private IDescargaMayor60Dias _descargaMayor60dias;
        private IEC_Metodos _ECmetodos;

        public  GrabacionesController( IConfiguration configuration,
                                        ILogger<GrabacionesController> logger,
                                        IDescargaDiaria descargadiaria,
                                        IDescargaMayor60Dias descargaMayor60Dias,
                                        IEC_Metodos ec_Metodos)
        {
            _configuration = configuration;
            _logger = logger;
            _descargaDiaria = descargadiaria;
            _descargaMayor60dias = descargaMayor60Dias;
            _ECmetodos = ec_Metodos;
        }


        [HttpPost]
        [Route("grabaciones")]
        public async Task<IActionResult> Grabaciones(ReqGrabaciones _request)
        {

            // DateTime FechaActual = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            DateTime FechaInicio = DateTime.ParseExact(_request.startTime, "yyyy-MM-ddTHH:mm:ss", null).AddHours(5);
            DateTime FechaFin = DateTime.ParseExact(_request.endTime, "yyyy-MM-ddTHH:mm:ss", null).AddHours(5);

            DateTime diaActual = DateTime.Today;
            TimeSpan diferencia_dias = diaActual - FechaInicio;
            int vDiferenciaDias = diferencia_dias.Days;

            ResponseRepositorio vresponseRepositorio = new ResponseRepositorio();

            if (FechaFin < FechaInicio)
            {
                return BadRequest(new { codigo = 400, Mensaje = "La fecha Inicio no puede ser mayor a la fecha final" });
            }
            else if (vDiferenciaDias <= 30)
            {
                try
                {
                    Console.WriteLine("Descarga en rango menor a 60 días");
                    _ECmetodos.EscribirLog("La descarga es para el metodo menor a 60 días");
                    vresponseRepositorio = await _descargaDiaria.DescargaDiaria(FechaInicio, FechaFin);
                    return Ok(vresponseRepositorio);

                }
                catch (Exception e)
                {
                    return BadRequest(new { e.StackTrace, e.Message });
                }
            }
            else
            {
                try
                {
                    _ECmetodos.EscribirLog("La descarga es para el metodo mayor a 60 días");
                    Console.WriteLine("Descarga en rango mayor a 60 días");
                    vresponseRepositorio = await _descargaMayor60dias.DescargaMayor60Dias(FechaInicio, FechaFin);
                    return Ok(vresponseRepositorio);

                }
                catch (Exception e)
                {
                    return BadRequest(new { e.InnerException, e.Message });
                }
            }


        }


        #region EndPoint de Prueba
        [HttpGet]
        [Route("prueba")]
        public IActionResult Prueba()
        {
            return Ok("Hooola esta prueba es con Net core 7.0");
        }
        #endregion
    }
}
