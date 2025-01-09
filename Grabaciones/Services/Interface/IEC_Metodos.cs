using Grabaciones.Models;

namespace Grabaciones.Services.Interface
{
    public interface IEC_Metodos
    {
        public void CrearDirectorio(string Ruta);
        Task<bool> DownloadFileAsync(string audiomp3,string urlAudio);
        public string ReemplazarTelefonoxVacio(string telefonoxVacio);
        bool ConvertMp3ToGsm(string inputFile, string outputFile);
        //public void CrearArchivoExcel(List<GC_ImprimirExcel> listImprimirExcel);
        public void CrearArchivoExcel(List<GC_LeerCsv> listImprimirExcel);
        public string ValidarSiesCelular(string telefono);
        public string ValidarSiesFijo(string telefono);
        Task UploadFTPAudios(string directorioFTP, string archivoGSM, string archivoLocal);
        Task UploadFTPArchivo(string directorioFTP,string archivoFTP, string archivoLocal);
        Task EnviarCorreo(string asunto, string _nombresemana);
        Task<string> GetWeekRangeAsync(DateTime startDate, DateTime endDate);

        Task<string> ObtenerNombreSemanaUltimoDia(DateTime startDate);
        Task EnviarDatostablaExcel(GC_ImprimirExcel DatosTablaExcel);
        Task<List<GC_Select_DatosTablaExcel>> ObtenerDatosBD(string nombredelasemana);
        public void EscribirLog(string Message);
        Task CrearArchivoCsv(List<GC_ImprimirExcel> listImprimirExcel);
        Task<List<GC_LeerCsv>> LeerArchivosCsv(string ruta);
        public string EliminarCaracteresEspeciales(string cadena);

    }
}
