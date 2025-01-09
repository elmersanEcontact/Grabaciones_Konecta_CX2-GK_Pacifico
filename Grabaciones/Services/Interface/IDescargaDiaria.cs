using Grabaciones.Models;

namespace Grabaciones.Services.Interface
{
    public interface IDescargaDiaria
    {
        public Task<ResponseRepositorio> DescargaDiaria(DateTime FechaInicio, DateTime FechaFin );
    }
}
