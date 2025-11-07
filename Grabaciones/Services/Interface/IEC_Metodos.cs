using Grabaciones.Models;
using PureCloudPlatform.Client.V2.Model;

namespace Grabaciones.Services.Interface
{
    public interface IEC_Metodos
    {
        public Task<bool> CrearDirectorio(string Ruta);

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
        public string ObtenerNombreDelMes(System.DateTime startDate);
        Task EnviarDatostablaExcel(GC_ImprimirExcel DatosTablaExcel);
        Task<List<GC_Select_DatosTablaExcel>> ObtenerDatosBD(string nombredelasemana);
        public void EscribirLog(string Message);
        Task CrearArchivoCsv(List<EC_CSVYanbal> listImprimirCVS);
        Task<List<GC_LeerCsv>> LeerArchivosCsv(string ruta);

        Task<string> EliminarCaracteresEspeciales(string cadena);

        Task<string> GuardarMetadataEnBaseDatos(List<EC_CSVYanbal> listImprimirCSV, string connectionString);

        Task<string> EnviarGrabaciones_a_Bucket(string nombreBucket, List<EC_CSVYanbal> listImprimirCSV, int anio, string nombredelMes, string rutaLocal);

        Task<string> GetDivisionName(List<GC_Division> ListDivisions, string divisionID);

        Task<string> GetCampaignName(AnalyticsConversationWithoutAttributes conversation, List<EC_Campaign> listCampaign);

        Task<string> GetCampaignName60DiasMas(AnalyticsConversation conversation, List<EC_Campaign> listCampaign);

        Task<string> GetNumeroTelefono(AnalyticsConversationWithoutAttributes conversation,string direccionOrigen);

        Task<string> GetNumeroTelefono60DiasMas(AnalyticsConversation conversation, string direccionOrigen);

        Task<string> GetDNIAsesor(CallConversation callConversation);

        Task<string> GetDNIAsesor60DiasMas(List<AnalyticsParticipant> participants);

        Task<string> GetQueueName(AnalyticsConversationWithoutAttributes conversation, List<GC_Queue> listQueue);

        Task<string> GetQueueName60DiasMas(AnalyticsConversation conversation, List<GC_Queue> listQueue);

        Task<EC_ParametrosApiPacifico> ObtenerParametroPacifico(CallConversation callConversation, EC_ConfiguracionTransformacionXML configuracionTransformacionXML, string direction);

        Task<EC_ParametrosApiPacifico> ObtenerParametroPacifico60DiasMas(AnalyticsConversation conversation,EC_ConfiguracionTransformacionXML configuracionTransformacionXML, string direction);

        Task CreateUpdateXMLGC(List<XmlGrabaciones> listMetadata);

        Task CargaSFTPAmazon(string localFilePath, string directorioRemoto, string rutaArchivoRemoto);

        Task<bool> SubirArchivosSFTPKonecta(string archivo, string periodo);

        Task<EC_ParametrosApiPacifico> GetDatosPacificoAsync(string wsGcId, string conversationId, EC_ConfiguracionTransformacionXML configuracionTransformacionXML, string direction);

        Task<EC_ConfiguracionTransformacionXML> LeerEquivalenciasJson();

        Task<bool> EnviarGrabaciones_a_Bucket2(string rutaLocal, string rutaBucket);

        Task<bool> TestConexionSFTP();
    }
}
