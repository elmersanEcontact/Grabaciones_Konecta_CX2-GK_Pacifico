using Grabaciones.Models;

namespace Grabaciones.Services.Interface
{
    public interface IDescargaMayor60Dias
    {
        public Task<ResponseRepositorio> DescargaMayor60Dias(DateTime FechaInicio, DateTime FechaFin);
    }
}
