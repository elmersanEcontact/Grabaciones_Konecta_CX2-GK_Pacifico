using Grabaciones.Services.Interface;
using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Model;
using System.Diagnostics;
using Grabaciones.Services.Econtact;
using PureCloudPlatform.Client.V2.Client;

namespace Grabaciones.Services.GenesysCloud
{
    public class GC_ConversationJobs
    {
        private readonly IConfiguration _config;
        private HttpClient _httpClient;
        private readonly IEC_Metodos _ecMetodos;

        public GC_ConversationJobs(IConfiguration configuration
                                            , HttpClient httpClient
                                            , IEC_Metodos ecMetodos)
        {
            _config = configuration;
            _httpClient = httpClient;
            _ecMetodos = ecMetodos;
        }
        #region Obtener el JobId
        public static string ObtenerJobId(DateTime FechaInicio, DateTime FechaFin, IConfiguration config)
        {

            var apiInstance = new ConversationsApi();
            var body = new AsyncConversationQuery(); // AsyncConversationQuery | query

            #region Configurar fechas de evaluación
            DateTime vFechaInicio = FechaInicio;
            DateTime vFechaFin = FechaFin;
            DateTime vFechaInicioIntervalo = vFechaInicio;
            DateTime vFechaFinIntervalo = FechaFin;
            //DateTime vFechaFinIntervalo =  vFechaInicioIntervalo.AddDays(0);

            string rangoFechas = "";
            string ValueSegmentQuery = "";
            #endregion


            List<SegmentDetailQueryFilter> oSegmentDetailQuery = new List<SegmentDetailQueryFilter>();
            List<ConversationDetailQueryFilter> oConversationDetailQueryFilters = new List<ConversationDetailQueryFilter>();

            List<SegmentDetailQueryPredicate> oSegmentDetailQueryPredicate = new List<SegmentDetailQueryPredicate>();
            List<SegmentDetailQueryPredicate> oSegmentDetailQueryPredicate2 = new List<SegmentDetailQueryPredicate>();
            List<ConversationDetailQueryFilter> oConversationDetailQueryFilter = new List<ConversationDetailQueryFilter>();
            List<ConversationDetailQueryPredicate> oConversationDetailQueryPredicate = new List<ConversationDetailQueryPredicate>();
            List<ConversationDetailQueryPredicate> oConversationDetailQueryPredicate3 = new List<ConversationDetailQueryPredicate>();
            string ? vDivisionId = config.GetValue<string>("GenesysCloud:DivisionId");

            #region  oSegmentDetailQueryPredicate
            oSegmentDetailQuery = new List<SegmentDetailQueryFilter>() {
                new SegmentDetailQueryFilter {
                Type = SegmentDetailQueryFilter.TypeEnum.Or,
                Predicates = new List<SegmentDetailQueryPredicate>()
                    {
                        new SegmentDetailQueryPredicate{
                            Dimension = SegmentDetailQueryPredicate.DimensionEnum.Mediatype,
                            Value = "voice"
                        },
                        new SegmentDetailQueryPredicate{
                            Dimension = SegmentDetailQueryPredicate.DimensionEnum.Mediatype,
                            Value = "callback"
                        },
                    }
                },
                new SegmentDetailQueryFilter
                {
                    Type = SegmentDetailQueryFilter.TypeEnum.Or,
                    Predicates= new List<SegmentDetailQueryPredicate>()
                    {
                        new SegmentDetailQueryPredicate
                        {
                            Dimension = SegmentDetailQueryPredicate.DimensionEnum.Direction,
                            Value = "inbound"
                        },
                        new SegmentDetailQueryPredicate
                        {
                            Dimension = SegmentDetailQueryPredicate.DimensionEnum.Direction,
                            Value="outbound"
                        }
                    }
                },
                #region filtros de Purpose
                new SegmentDetailQueryFilter()
                {
                    Type = SegmentDetailQueryFilter.TypeEnum.And,
                    Clauses = new List<SegmentDetailQueryClause>()
                    {
                        new SegmentDetailQueryClause{
                            Type = SegmentDetailQueryClause.TypeEnum.Or,
                            Predicates = new List<SegmentDetailQueryPredicate>()
                            {
                                new SegmentDetailQueryPredicate{
                                    Dimension = SegmentDetailQueryPredicate.DimensionEnum.Purpose,
                                    Value = "agent"
                                }
                            }
                        }
                    }
                },
                #endregion
                new SegmentDetailQueryFilter
                {
                    Type= SegmentDetailQueryFilter.TypeEnum.And,
                    Predicates = new List<SegmentDetailQueryPredicate>()
                    {
                        new SegmentDetailQueryPredicate
                        {
                            Dimension = SegmentDetailQueryPredicate.DimensionEnum.Recording,
                            Type = SegmentDetailQueryPredicate.TypeEnum.Dimension,
                            Operator = SegmentDetailQueryPredicate.OperatorEnum.Exists,
                        }
                    }
                }
            };
            #endregion

            #region Filtros conversationDetailQuery
            oConversationDetailQueryFilter = new List<ConversationDetailQueryFilter>()
            {
                new ConversationDetailQueryFilter
                {
                    Type = ConversationDetailQueryFilter.TypeEnum.Or,
                    Predicates = new List<ConversationDetailQueryPredicate>()
                    {
                        new ConversationDetailQueryPredicate()
                        {
                            Dimension = ConversationDetailQueryPredicate.DimensionEnum.Divisionid,
                            Value = vDivisionId
                        }
                    }
                },

                //new ConversationDetailQueryFilter
                //{
                //    Type = ConversationDetailQueryFilter.TypeEnum.And,
                //    Predicates = new List<ConversationDetailQueryPredicate>()
                //    {
                //        new ConversationDetailQueryPredicate()
                //        {
                //            Type = ConversationDetailQueryPredicate.TypeEnum.Dimension,
                //            Dimension = ConversationDetailQueryPredicate.DimensionEnum.Conversationid,
                //            Value = "a23bbad7-753f-4842-bdbd-24ed691f3847"
                //        }
                //    }
                //},

            };
            #endregion

            rangoFechas = vFechaInicioIntervalo.ToString("yyyy-MM-ddTHH:mm:ss") + "/" + vFechaFinIntervalo.ToString("yyyy-MM-ddTHH:mm:ss");
            ValueSegmentQuery = vFechaInicioIntervalo.ToString("yyyy-MM-ddTHH:mm:ss") + ".000Z/" + vFechaFinIntervalo.ToString("yyyy-MM-ddTHH:mm:ss" + ".000Z");


            body.Interval = ValueSegmentQuery;
            body.SegmentFilters = oSegmentDetailQuery;
            body.ConversationFilters = oConversationDetailQueryFilter;
            body.Order = AsyncConversationQuery.OrderEnum.Asc;
            body.OrderBy = AsyncConversationQuery.OrderByEnum.Conversationstart;
            body.StartOfDayIntervalMatching = true;

            var json = body.ToJson();

            string jobId = string.Empty;

            int _cJobID = 0;
            while (true)
            {
                try
                {
                    AsyncQueryResponse getJobID = apiInstance.PostAnalyticsConversationsDetailsJobs(body);
                    jobId = getJobID.JobId;
                    //WriteLog.EscribirLog("Intervalo: " + dates + " || JobID: " + getJobID.JobId);
                    break;
                }
                catch (Exception ex )
                {
                    // WriteLog.EscribirLog("Intervalo: " + dates + " || Error: No se pudo obtener Job");
                    EC_EscribirLog.EscribirLog($"Error: {ex.Message.ToString()}");
                    Thread.Sleep(5000);
                }
                _cJobID++;
                if (_cJobID == 3) { break; }
            }
            return jobId;
        }
        #endregion

        #region Validar el estado del JobId
        public static async Task<string> ObtenerEstadoJobAsync(string jobId)
        {
            var apiInstanceJobId = new ConversationsApi();
            string respuesta = "";

            try
            {
                string respuestaEstado;
                AsyncQueryStatus resultJobId = apiInstanceJobId.GetAnalyticsConversationsDetailsJob(jobId);
                respuestaEstado = resultJobId.State.ToString().ToUpper();

                if (respuestaEstado == "PENDING")
                {
                    await Task.Delay(5000); // Esperar 5 segundos antes de la siguiente consulta (ajustar según sea necesario)
                    respuesta = await ObtenerEstadoJobAsync(jobId); // Realizar una nueva consulta       
                }
                else if (respuestaEstado == "FAILED")
                {
                    respuesta = resultJobId.ErrorMessage.ToString();
                }
                else if (respuestaEstado == "")
                {
                    await Task.Delay(5000); // Esperar 5 segundos antes de la siguiente consulta (ajustar según sea necesario)
                    respuesta = await ObtenerEstadoJobAsync(jobId); // Realizar una nueva consulta 
                }
                else if (respuestaEstado == "FULFILLED")
                {
                    respuesta = resultJobId.State.ToString().ToUpper();
                }
            }
            catch (Exception e)
            {
                EC_EscribirLog.EscribirLog("Exception when calling Conversations.GetAnalyticsConversationsDetailsJob: " + e.Message);
                //Debug.Print("Exception when calling Conversations.GetAnalyticsConversationsDetailsJob: " + e.Message);
            }

            return respuesta;
        }
        #endregion

        #region Mostrar los datos del Job
        public static async Task<List<AnalyticsConversation>> ObtenerDatosdelJobResult(DateTime FechaInicio, DateTime FechaFin, IConfiguration config)
        {
            string cursor = string.Empty;
            string _statusJob = string.Empty;
            List<AnalyticsConversation> ListConversation = new List<AnalyticsConversation>();

            #region ObtenerJobId
            string _jobId = ObtenerJobId(FechaInicio, FechaFin, config);
            #endregion

            if (!string.IsNullOrEmpty(_jobId))
            {
                _statusJob = ObtenerEstadoJobAsync(_jobId).Result;

            }

            if (_statusJob != null && _statusJob == "FULFILLED")
            {
                string jobIdResult = _statusJob;

                var apiInstanceResult = new ConversationsApi();
                int pageSize = 100;
                int? JobContadorInteracciones = 0;

                bool bCursor = true;
                AnalyticsConversationAsyncQueryResponse resultJob = new AnalyticsConversationAsyncQueryResponse();
                while (bCursor)
                {

                    try
                    {

                        resultJob = apiInstanceResult.GetAnalyticsConversationsDetailsJobResults(_jobId, cursor, pageSize);
                    }
                    catch (ApiException ex)
                    {
                        EC_EscribirLog.EscribirLog($"error: {ex.Message.ToString()}");
                        throw;
                    }
                    JobContadorInteracciones = resultJob.Conversations.Count;
                    cursor = resultJob.Cursor is null ? null : resultJob.Cursor;
                    bCursor = resultJob.Cursor is null ? false : true;

                    if (JobContadorInteracciones > 0)
                    {
                        if (resultJob.Conversations is null)
                        {
                            EC_EscribirLog.EscribirLog($"Intervalo: {FechaInicio} - {FechaFin}  || Error GetAnalyticsConversationsDetailsJobResults: conversacions null");
                            break;
                        }
                        else
                        {
                            foreach (AnalyticsConversation conversation in resultJob.Conversations)
                            {
                                ListConversation.Add(conversation);

                            }
                        }
                    }
                }
            }
            return ListConversation;
        }
        #endregion
    }
}
