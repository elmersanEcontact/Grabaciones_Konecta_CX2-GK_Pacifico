using Grabaciones.Models;
using Grabaciones.Services.Econtact;
using Grabaciones.Services.GenesysCloud;
using Grabaciones.Services.Interface;

using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Client;
using PureCloudPlatform.Client.V2.Extensions;
using PureCloudPlatform.Client.V2.Model;

using System.Diagnostics;
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Office.Interop;
using Excel = Microsoft.Office.Interop.Excel;
using DocumentFormat.OpenXml.Drawing;
using Newtonsoft.Json.Linq;
using System.Text;
using DocumentFormat.OpenXml.Bibliography;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Runtime.CompilerServices;




namespace Grabaciones.Services.Repositorio
{
    public class RDescargaDiaria: IDescargaDiaria
    {
        private readonly IConfiguration _config;
        private readonly IEC_Metodos _ecMetodos;

        public RDescargaDiaria(IConfiguration configuration, IEC_Metodos ecMetodos)
        {
            _config = configuration;
            _ecMetodos = ecMetodos;
            
        }

        #region Metodo Descarga diaria
        public async Task<ResponseRepositorio> DescargaDiaria(DateTime FechaInicio, DateTime FechaFin)
        {
            await EC_EscribirLog.EscribirLogAsync("Se inicia proceso de descarga " + FechaInicio.ToString("yyyy-MM-dd HH:mm:ss") + " - " + FechaFin.ToString("yyyy-MM-dd HH:mm:ss"));

            #region Traer el numero de la semana
            string vRespuestSemana = await _ecMetodos.ObtenerNombreSemanaUltimoDia(FechaInicio);

            string[] arrNombresemanaUltimoDia = vRespuestSemana.Split('|'); //await _ecMetodos.GetWeekRangeAsync(FechaInicio, FechaFin);
            string _nombresemana = arrNombresemanaUltimoDia[0];
            int anio = FechaInicio.Year;
            string NombreDelMes = _ecMetodos.ObtenerNombreDelMes(FechaInicio);
            
            #endregion

            #region valido que el valor de fechainicio sea el ultimo día de la semana
            //bool ultimodiadelasemana = FechaInicio.DayOfWeek == DayOfWeek.Sunday;
            bool ultimodiadelasemana = arrNombresemanaUltimoDia[1] == "Domingo";
            int _iAnio = FechaInicio.Year;
            #endregion

            #region Enviroment
            List<XmlGrabaciones> L_GC_RecordingsXml_Path = new List<XmlGrabaciones>();
            List<GC_LeerCsv> ArchivosCsvJuntos = new List<GC_LeerCsv>();

            string ?xmlRutaPrincipal = _config.GetValue<string>("ConfiguracionAudio:RutaPrincipal");
            string? xmlOrganizacion = _config.GetValue<string>("ConfiguracionAudio:Organization");
            string ?xmlNombreCarpeta = _config.GetValue<string>("ConfiguracionAudio:NombreCarpeta");
            string ?xmlCliente = _config.GetValue<string>("ConfiguracionAudio:Cliente");
            
            List<EC_Paises> ? paises = _config.GetSection("LConfiguracionPaises:ListadoPaises").Get<List<EC_Paises>>();
            int TotalGrabaciones = _config.GetValue<int>("LConfiguracionPaises:TotalGrabaciones");
            int peticionesConcurrentes = 25;//int.Parse(ConfigurationManager.AppSettings["PeticionesConcurrentes"]);
            string ? connectionString = _config.GetValue<string>("ConnectionStrings:DefaultConnection");
            string ? nombreBucket = _config.GetValue<string>("ConfiguracionAWS:bucketName");

            string _directorioGrabaciones = $"{xmlRutaPrincipal}\\{xmlOrganizacion}\\{xmlCliente}";


            DateTime FechaEjecucion = DateTime.Now;
            string HoraEjecucion = FechaEjecucion.ToString("HHmmss");
            EC_Helpers _helpers = new EC_Helpers();

            RecordingApi recordingApi = new RecordingApi();
            #endregion

            #region Autenticacion -- validar cuando falle en el método
            try
            {
                SGC_Autentication.Autentication(_config);
            }
            catch (Exception ex)
            {
                return new ResponseRepositorio { statusCode = 400, message = "Error: " + ex.Message.ToString() };
                throw;
            }
            #endregion

            #region Obtener Divisiones -- validar cuando falle el método
            List<GC_Division> ListDivisions = sGC_Division.ObtenerDivision();
            #endregion

            #region obtener usuarios
            List<GC_Users> GC_Users = await SGC_Users.ObtenerUsuarios();
            #endregion

            #region Obtener Colas
            //List<GC_Queue> GC_Queues = SGC_Queue.ObtenerColas();
            List<GC_Queue> GC_Queues = await SGC_Queue.ObtenerColasPorDivision(ListDivisions.Where(d => d.name.Equals("PACIFICO")).Select(d => d.id).ToList());
            //List<GC_Queue> GC_Queues = await SGC_Queue.ObtenerColasPorDivision(ListDivisions.Where(d => d.name.Equals("PACIFICO ECOMMERCE")).Select(d => d.id).ToList());
            #endregion

            #region Obtener las campañas por division
            List<EC_Campaign> GC_Campaigns = await SGC_Campaign.GetCampaing(ListDivisions.Where(d => d.name.Equals("PACIFICO")).Select(d => d.id).ToList());
            //List<EC_Campaign> GC_Campaigns = await SGC_Campaign.GetCampaing(ListDivisions.Where(d => d.name.Equals("PACIFICO ECOMMERCE")).Select(d => d.id).ToList());
            #endregion

            #region Obtener Wrapupcode(Tipificaciones)  -- validar cuando falle el método
            //List<GC_Wrapupcode> ListWrapupcode = await  SGC_Wrapupcode.ObtenerWrapupcode(ListDivisions.Where(d=>d.name.Equals("PACIFICO")).ToList());
            #endregion

            #region Obtener Skill
            //List<GC_Skill> GC_Skills = await SGC_Skill.ObtenerSkills();
            #endregion

            #region filtrar colas a buscar
            // Suponiendo que tienes las listas GC_Queues y paises ya inicializadas

            // Agrupa las colas cuya inicial (según la lista de países) se encuentre al inicio del nombre, seguida de '_'
            // Filtrar las colas cuyo nombre comience exactamente con la inicial seguida de '_' y agruparlas por país

            //List<EC_GruposPaisCola> PaisCola = SGC_Queue.ObtenerColasAgrupadasPorPais(GC_Queues, paises, TotalGrabaciones, GC_Skills);

            //string json2 = JsonConvert.SerializeObject(groupedQueues, Formatting.Indented);
            #endregion

            #region Configurar fechas de evaluación
            DateTime vFechaInicio = FechaInicio;
            DateTime vFechaFin = FechaFin;
            DateTime vFechaInicioIntervalo = vFechaInicio;
            //DateTime vFechaFinIntervalo = FechaFin;
            DateTime vFechaFinIntervalo = vFechaFin;//vFechaInicioIntervalo.AddDays(0);

            string rangoFechas = "";
            string ValueSegmentQuery = "";
            #endregion

            #region bucle para descarga de audios
            List<XmlGrabaciones> listXmlGrabaciones = new List<XmlGrabaciones>();
            List<GC_ImprimirExcel> listImprimirExcel = new List<GC_ImprimirExcel>();
            List<EC_CSVYanbal> listImprimirCSV = new List<EC_CSVYanbal>();
            List<EC_Metadata> listMetadata = new List<EC_Metadata>();
            string divisionName = string.Empty;

           
                rangoFechas = vFechaInicioIntervalo.ToString("yyyy-MM-ddTHH:mm:ss") + "/" + vFechaFinIntervalo.ToString("yyyy-MM-ddTHH:mm:ss");
                ValueSegmentQuery = vFechaInicioIntervalo.ToString("yyyy-MM-ddTHH:mm:ss") + ".000Z/" + vFechaFinIntervalo.ToString("yyyy-MM-ddTHH:mm:ss" + ".000Z");

                #region Obtener las conversaciones según fecha
                await EC_EscribirLog.EscribirLogAsync("Las conversaciones a evaluar son del rango: "+rangoFechas);

                // se extrae todas las conversaciones por país
                //List<AnalyticsConversationWithoutAttributes> conversationDetails = await SGC_ConversationsDetailsQuery.ObtenerConversaciones_x_Cola(rangoFechas, ValueSegmentQuery, _config, PaisCola);
                
                List<AnalyticsConversationWithoutAttributes> conversationDetails = await SGC_ConversationsDetailsQuery.ObtenerConversaciones(rangoFechas, ValueSegmentQuery, _config);

                await EC_EscribirLog.EscribirLogAsync("Se traerá información de un total de: " + conversationDetails.Count().ToString() + " conversaciones(Hits) de todos los paises");
                #endregion

                #region recorrido de cada una de las conversaciones
                if (conversationDetails.Count == 0 || conversationDetails is null)
                {
                    await EC_EscribirLog.EscribirLogAsync($"No hay registros a evaluar");
                }
                else
                {
                    // Semáforo para limitar peticiones concurrentes                    
                    var throttler = new SemaphoreSlim(initialCount: peticionesConcurrentes); // Ajustar según rendimiento deseado
                    var tasks = new List<Task>();
                    var totalConversaciones = conversationDetails.Count;
                    var procesadas = 0;
                    int iConversacion = 1;
                    // ✅ Limitar a 4 peticiones por segundo
                    var rateLimiter = new SimpleRateLimiter(4);

                    foreach (AnalyticsConversationWithoutAttributes conversation in conversationDetails)
                    {
                        await EC_EscribirLog.EscribirLogAsync($"Conversacion[{iConversacion}]");

                        await throttler.WaitAsync(); // Esperar si ya hay demasiadas tareas en ejecución

                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                // ✅ Espera para cumplir 4 llamadas por segundo
                                //await rateLimiter.WaitAsync();

                                await ProcesarConversaciones(conversation, listXmlGrabaciones, 
                                                            listImprimirExcel,
                                                            //PaisCola, 
                                                            vFechaInicioIntervalo, _directorioGrabaciones, 
                                                            rateLimiter, iConversacion, 
                                                            ListDivisions, GC_Queues, GC_Campaigns);

                                int processed = Interlocked.Increment(ref procesadas);
                                if (processed % 100 == 0)
                                {
                                    await EC_EscribirLog.EscribirLogAsync($"Progreso: {processed}/{totalConversaciones} conversaciones procesadas");
                                }
                            }
                            catch (Exception ex)
                            {
                                await EC_EscribirLog.EscribirLogAsync($"Error procesando conversación(ProcesarConversaciones)| {conversation.ConversationId}: {ex.Message}");
                            }
                            finally
                            {
                                throttler.Release(); // Liberar el semáforo
                            }
                        }));

                        iConversacion++;
                    }

                    // Esperar a que todas las tareas terminen
                        await Task.WhenAll(tasks);
                }
                #endregion

                await EC_EscribirLog.EscribirLogAsync($"Fin de la extracción de informacion de grabaciones para los días {vFechaInicioIntervalo} - {vFechaFinIntervalo}");


            #endregion

            #region crear lista para crear excel
            await EC_EscribirLog.EscribirLogAsync($"Se descargaran un total de {listXmlGrabaciones.Count()} grabaciones en MP3");

            if (listXmlGrabaciones != null && listXmlGrabaciones.Count() > 0)
            {


                #region Crear archivo xml
                await _ecMetodos.CreateUpdateXMLGC(listXmlGrabaciones);
                #endregion


                foreach (var iGrabaciones in listXmlGrabaciones)
                {
                    if (iGrabaciones.xmlUrlGCAudio == "NoExisteUri")
                    {
                        await EC_EscribirLog.EscribirLogAsync("No existe audio para la grabacion: " + iGrabaciones.xmlRecordingID);
                    }
                    else
                    {
                            
                            listMetadata.Add(new EC_Metadata {

                                empresa = iGrabaciones.xmlempresa,
                                dNICliente = "dNICliente",
                                apellidoPaterno = "apellidoPaterno",
                                apellidoMaterno = "apellidoMaterno",
                                nombres = "nombres",
                                telefono = "telefono",
                                fechaDeServicio = "fechaDeServicio",
                                horaDeServicio = "horaDeServicio",
                                NroAsesor = "NroAsesor",
                                Proceso = "Proceso",
                                vdn = "vdn",
                                skill = "skill",
                                ramo = "ramo",
                                producto = "producto",
                                resultado = "resultado",
                                subResultado = "subResultado"

                            });



                            #region Subir archivo a FTP
                            //if (respuestaAudio)
                            //{
                            //    try
                            //    {

                            //  var result = _ecMetodos.UploadFTPAudios(iGrabaciones.xmldirectorioFTP, iGrabaciones.xmlRutaCompletaAudioGSM, iGrabaciones.xmlArchivolocal);

                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        await EC_EscribirLog.EscribirLogAsync($"Error en UploadFTPAudios: {ex.Message.ToString()}");
                            //        throw;
                            //    }
                            //}
                            #endregion

                            #region subir a repositorio de amazon S3
                            //if (respuestaAudio) {
                            //    try
                            //    {
                            //     var resultS3 = _ecMetodos.SubirArchivosSFTAmazon(iGrabaciones.xmlRutaCompletaAudioGSM, _nombresemana, iGrabaciones.eAnio);
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        await EC_EscribirLog.EscribirLogAsync($"Error al subir archivo al S3 de Konecta: {ex.Message}");
                            //        throw;
                            //    }
                            //}
                            #endregion

                        }
                }

                
                #region crear archivo csv por día
                //try
                //{
                //    var respuesta = _ecMetodos.CrearArchivoCsv(listImprimirCSV);
                //}
                //catch (Exception ex)
                //{
                //    await EC_EscribirLog.EscribirLogAsync($"Error al crear el archivo csv: {ex.Message.ToString()}");
                //    throw;
                //}
                #endregion

                #region Insertar metadata en la base de datos
                //string respuestaSQL = await _ecMetodos.GuardarMetadataEnBaseDatos(listImprimirCSV, connectionString);
                //string respuestaOkErrorSQl = respuestaSQL.Split('|')[0];

                //if (respuestaOkErrorSQl == "Ok")
                //{
                //    await EC_EscribirLog.EscribirLogAsync($"Se insertaron correctamente los registros en la base de datos");
                //}
                //else
                //{
                //    await EC_EscribirLog.EscribirLogAsync($"Error al insertar los registros en la base de datos: {respuestaSQL}");
                //}
                #endregion

                #region Enviar las grabaciones a Bucket AWS
                //await EC_EscribirLog.EscribirLogAsync($"Inicio de proceso de envio de grabaciones a Bucket");
                //string respuestaBucketAWS = await _ecMetodos.EnviarGrabaciones_a_Bucket(nombreBucket, listImprimirCSV, anio, NombreDelMes, "DirectorioAudio");
                #endregion
            }
            #endregion

            #region solo ejecutar cuando es el ultimo día de la semana el día es domingo
            if (ultimodiadelasemana)
            {

                #region Leer los archivos csv que estan en la ruta y unirlos en  un archivo excel

                string rutadeArchivo = _directorioGrabaciones;
                //string tmp_ruta = listImprimirExcel[0].archivoCsv;
                //// Encontrar la posición del último '\'
                //int lastBackslashIndex = tmp_ruta.LastIndexOf('\\');

                //// Si se encuentra el último '\' y no es el primer carácter
                //if (lastBackslashIndex >= 0)
                //{
                //    // Extraer la cadena hasta el último '\'
                //    rutadeArchivo = tmp_ruta.Substring(0, lastBackslashIndex);
                //}

                ArchivosCsvJuntos = await _ecMetodos.LeerArchivosCsv(rutadeArchivo);
               
                #endregion

                //#region si es el ultimo dia de la semana traer la lista desde la base de datos para imprimir el excel
                // List<GC_Select_DatosTablaExcel> resultParaExcel =await _ecMetodos.ObtenerDatosBD(_nombresemana);
                //#endregion

                #region Imprimir Excel con los audios descargados
                _ecMetodos.CrearArchivoExcel(ArchivosCsvJuntos);

                // await _ecMetodos.SubirArchivosSFTAmazon(ArchivosCsvJuntos[0].ArchivoExcel,_nombresemana,_iAnio.ToString());

                #endregion

                #region Envio de correo
                
                string _nombrearchivo = ArchivosCsvJuntos[0].ArchivoExcel.ToString();
                string _asunto = string.Concat(@"Rimac Soat Konecta | Carga de audios a SFTP ",_nombresemana);
                string _cuerpo = string.Concat("Buen día,\r\n\r\nSe procede con la carga automática en el SFTP de los audios de Rimac Soat correspondientes a ",_nombresemana," .\r\n\r\nSaludos cordiales\r\n");
                await _ecMetodos.EnviarCorreo(_asunto, _nombresemana);
                #endregion 
            }
            #endregion 

            return new ResponseRepositorio { statusCode = 200, message = "Ok" };

        }
        #endregion

        #region getRecordingStatus
        // Plot conversationId and recordingId to request for batchdownload Recordings
        private static BatchDownloadJobStatusResult getRecordingStatus(BatchDownloadJobSubmissionResult recordingBatchRequest)
        {
            Console.WriteLine("Processing the recordings...");
            RecordingApi recordingApi = new RecordingApi();
            BatchDownloadJobStatusResult result = new BatchDownloadJobStatusResult();

            result = recordingApi.GetRecordingBatchrequest(recordingBatchRequest.Id);

            if (result.ExpectedResultCount != result.ResultCount)
            {
                Console.WriteLine("Batch Result Status:" + result.ResultCount + " / " + result.ExpectedResultCount);

                // Simple polling through recursion
                Thread.Sleep(5000);
                return getRecordingStatus(recordingBatchRequest);
            }

            // Once result count reach expected.
            return result;
        }
        #endregion

        #region Metodo para el proceso de las conversaciones
        private async Task ProcesarConversaciones(
            AnalyticsConversationWithoutAttributes conversation,
            List<XmlGrabaciones> listXmlGrabaciones,
            List<GC_ImprimirExcel> listImprimirExcel,
           // List<EC_GruposPaisCola> PaisCola,
            DateTime vFechaInicioIntervalo,
            string DirectorioGrabaciones,
            SimpleRateLimiter rateLimiter, int iconversation,
            List<GC_Division> ListDivisions ,
            List<GC_Queue> listQueues,
            List<EC_Campaign> listCampaign
        )
        {
            string? xmlFormato = _config.GetValue<string>("ConfiguracionAudio:Formato");
            string? xmlRutaFtp = _config.GetValue<string>("SFTPConfiguration:remoteDirectory");
            string? xmlEmpresa = _config.GetValue<string>("ConfiguracionAudio:Empresa");
            string? xmlOrganizacion = _config.GetValue<string>("ConfiguracionAudio:Organization");

            #region variables iniciales
                string direccion = string.Empty;
                string recordingId = string.Empty;
                string conversationId = conversation.ConversationId;
                string? direction = conversation.OriginatingDirection == null ? "" : conversation.OriginatingDirection.ToString();
                DateTime conversationStartTime = (DateTime)conversation.ConversationStart;
                DateTime conversationEndTime = (DateTime)conversation.ConversationEnd;
                string userId = string.Empty;
                string agentId = string.Empty;
                string wrapupcode = string.Empty;
                long duration = 0;
                long acw = 0;
                string ani = string.Empty;

                string direccionOrigen = string.Empty;
                string nameQqueue = string.Empty;
                string nameCampaignCola = string.Empty;
                string nameDivision = string.Empty;
                string phoneNumber = string.Empty;
                string dniAsesor = string.Empty;
                string direccionAudio = string.Empty;

            // datos para el XML

                string xmlDniCliente = "xmlDniCliente";
                string xmlApellidoPaterno = "xmlApellidoPaterno";
                string xmlApellidoMaterno = "xmlApellidoMaterno";
                string xmlNombres = "xmlNombres";
                string xmlTelefono = "xmlTelefono";
                string xmlNumeroAsesor = string.Empty;
                string xmlFechaDeServicio = string.Empty;
                string xmlHoraDeServicio = string.Empty;
                string xmlProceso = "xmlProceso";
                string xmlVdn = "xmlVdn";
                string xmlSkill = "xmlSkill";
                string xmlRamo = "xmlRamo";
                string xmlProducto = "xmlProducto";
                string xmlResultado = "xmlResultado";
                string xmlSubResultado = "xmlSubResultado";

                #endregion

                if (conversation.ConversationId is null || conversation.ConversationId == "")
                {
                    await EC_EscribirLog.EscribirLogAsync($"No se obtuvo la conversación: {iconversation} - conversationId: {conversation.ConversationId}");
                    return;
                }
                else
                {
                    //obtener la direccion de origen
                    direccionOrigen = conversation.OriginatingDirection.ToString().ToUpper();
                    //obtener el nombre de la division
                    nameDivision = await _ecMetodos.GetDivisionName(ListDivisions, conversation.DivisionIds[0]);
                    // Obtener número de telefono
                    phoneNumber = await _ecMetodos.GetNumeroTelefono(conversation,direccionOrigen);
                    // obtener los datos de la conversacion
                    CallConversation callConversation = await SGC_ConversationsCall.ObtenerCallConversation(conversation.ConversationId);

                    //Obtener DNI del Asesor
                    dniAsesor = await _ecMetodos.GetDNIAsesor(callConversation);

                    //Lista para obtener los campos al llamar la api de pacifico
                    EC_ParametrosApiPacifico metadataPacifico = new EC_ParametrosApiPacifico();

                    metadataPacifico = await _ecMetodos.ObtenerParametroPacifico(callConversation);

                //metadataPacifico = await _ecMetodos.ObtenerParametroPacifico(callConversation);

                

                #region Resultados desde api de pacifico
                        xmlDniCliente = metadataPacifico.tNumDoc_c;
                        xmlApellidoPaterno = metadataPacifico.tPerApellidoPaterno_c;
                        xmlApellidoMaterno = metadataPacifico.tPerApellidoMaterno_c;
                        xmlNombres = metadataPacifico.tPerNombre_c;
                        xmlTelefono = phoneNumber;
                        xmlNumeroAsesor = dniAsesor;
                        xmlProceso = "NNN";
                        xmlVdn = metadataPacifico.tVDN_c;
                        xmlSkill = "NNN";
                        xmlRamo = metadataPacifico.chOptyTipifProducto_c;
                        xmlProducto = metadataPacifico.chOptyTipifProducto_c;
                        xmlResultado = metadataPacifico.chOptyTipifProducto_c;
                        xmlSubResultado = metadataPacifico.chOptyTipifProducto_c;
                    #endregion


                if (conversation.OriginatingDirection == AnalyticsConversationWithoutAttributes.OriginatingDirectionEnum.Outbound)
                    {
                        await EC_EscribirLog.EscribirLogAsync($"Se obtinen datos Outbound de la conversación: {iconversation} - conversationId: {conversation.ConversationId}");
                    
                        //obtener el nombre de la campaña
                        nameCampaignCola = await _ecMetodos.GetCampaignName(conversation, listCampaign);

                    }
                    else if (conversation.OriginatingDirection == AnalyticsConversationWithoutAttributes.OriginatingDirectionEnum.Inbound)
                    {
                        await EC_EscribirLog.EscribirLogAsync($"Se obtinen datos Inbound de la conversación: {iconversation} - conversationId: {conversation.ConversationId}");

                        nameCampaignCola = await _ecMetodos.GetQueueName(conversation, listQueues);
                    
                    }
                    else
                    {
                        await EC_EscribirLog.EscribirLogAsync($"No se obtuvo el tipo de dirección de la conversación: {iconversation} - conversationId: {conversation.ConversationId}");
                        return;
                    }

                }

                #region metadata de conversacion según conversationId

                List<RecordingMetadata> vRecordingMetadata = await SGC_ConversationRecordingmetadata.ObtenerConversationRecordingmetadata(conversation.ConversationId, vFechaInicioIntervalo);

                string? vOriginatingDirection = conversation.OriginatingDirection.ToString();
                string? nombreDivision = await _ecMetodos.GetDivisionName(ListDivisions, conversation.DivisionIds[0]);

                foreach (var iRecording in vRecordingMetadata)
                {

                    XmlGrabaciones xmlGrabaciones = new XmlGrabaciones();

                    recordingId = iRecording.Id;

                    DateTime _startTime = DateTime.Parse(iRecording.StartTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    DateTime _endTime = DateTime.Parse(iRecording.EndTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    TimeSpan diferenciaSegundos = _endTime - _startTime;
                    int nDiferenciaSegundos = diferenciaSegundos.Seconds;

                    Recording DatosMP3 = new Recording();
                    await EC_EscribirLog.EscribirLogAsync("Se extrae la información de las grabaciones de la conversación: " + iRecording.ConversationId + " y de la grabacion: " + iRecording.Id + " - con una duracion de " + nDiferenciaSegundos.ToString());
                    
                    //obtener los datos dela grabacion en MP3
                    if(iRecording.FileState == RecordingMetadata.FileStateEnum.Archived)
                    {
                        
                        await EC_EscribirLog.EscribirLogAsync($"Grabacion disponible: {iRecording.ConversationId} - grabacion: {iRecording.Id}");
                            
                    }
                    else
                    {
                        DatosMP3 = await SGC_ConversationRecording.ObtenerDatosGrabacionMP3(iRecording.ConversationId, iRecording.Id, _config, rateLimiter);
                        await EC_EscribirLog.EscribirLogAsync($"Grabacion no disponible: {iRecording.ConversationId} - grabacion: {iRecording.Id}");
                        //continue;
                    }

                    //DatosMP3 = await SGC_ConversationRecording.ObtenerDatosGrabacionMP3(iRecording.ConversationId, iRecording.Id, _config, rateLimiter);

                    if(DatosMP3 is null)
                    {
                        await EC_EscribirLog.EscribirLogAsync($"No se obtuvo la grabacion de la conversacion[{iconversation}]: {iRecording.ConversationId} - grabacion: {iRecording.Id}");
                        break;
                    }
                    #region datos que ayudan en la generación de archivo xml y descarga ed audio

                    #region se establecen valores de la fecha de grabacion
                    string _ConversationID = DatosMP3.ConversationId;
                    string _RecordingId = DatosMP3.Id;
                    string _SessionId = DatosMP3.SessionId;
                    DateTime StartTime = DateTime.Parse(DatosMP3.StartTime);
                    DateTime EndTime = DateTime.Parse(DatosMP3.EndTime);
                    string _anio = StartTime.ToString("yyyy");
                    string _mes = StartTime.ToString("MM");
                    string _dia = StartTime.ToString("dd");
                    string _Hour = StartTime.ToString("HH");
                    string _Minute = StartTime.ToString("mm");
                    string _Seconds = StartTime.ToString("ss");
                    xmlFechaDeServicio = StartTime.ToString("dd/MM/yyyy");
                    xmlHoraDeServicio = StartTime.ToString("HH:mm:ss");

                    #endregion

                    #region Datos para Excel
                    string eFecha = StartTime.ToString("yyyy-MM-dd");
                    string eAnio = StartTime.ToString("yyyy");
                    string eMes = StartTime.ToString("MM");
                    string eDia = StartTime.ToString("dd");
                    string eHora = StartTime.ToString("HH");

                    #endregion = "";

                    
                    // string _directorio = string.Concat(DirectorioAudio, "/", _anio, "/", _mes, "/", _dia);
                    string _NomenclaturaAudioMP3 = "";
                    string _Audiomp3 = "";
                    string _NombreAudioExcel = "";
                    string _urlAudio = "";
                    string _directorioAudio = string.Empty;
                    string _directorioFTP = "";
                    string _archivolocal = "";

                    if (!DatosMP3.MediaUris.ContainsKey("S"))
                    {
                        _urlAudio = "NoExisteUri";
                    }
                    else if (DatosMP3.MediaUris["S"].MediaUri is null)
                    {
                        _urlAudio = "NoExisteUri";
                    }
                    else
                    {
                        _urlAudio = DatosMP3.MediaUris["S"].MediaUri;
                    }


                    List<User> _users = new List<User>();
                    _users = DatosMP3.Users;
                    agentId = _users.Count() > 0 ? _users[0].Username : "NN";

                #endregion

                    direccionAudio = direccionOrigen == "OUTBOUND" ? "O" : "I";
                    string NombredelAudio = $"{_anio}{_mes}_{_anio}{_mes}{_dia}{_Hour}{_Minute}{_Seconds}_{phoneNumber}_{dniAsesor}_{direccionAudio}"; //string.Concat(eDia, "-",eMes, "-",eAnio, "_", _RecordingId,"_", eNombreApellidos.Replace(" ", "-").Replace(@"\", "").Replace(@"/", ""), "_",_Telefono);
                    NombredelAudio = await _ecMetodos.EliminarCaracteresEspeciales(NombredelAudio);
                    await EC_EscribirLog.EscribirLogAsync($"Nombre del audio=>{NombredelAudio}");
                    _NomenclaturaAudioMP3 = NombredelAudio + "." + xmlFormato;
                    _directorioAudio = $"{DirectorioGrabaciones}/{nameDivision}/{direccionOrigen}/{nameCampaignCola}/{_anio}/{_mes}/{_dia}";
                    _Audiomp3 = string.Concat(_directorioAudio, "/", _NomenclaturaAudioMP3);

                    #region Crear el objeto xml

                    xmlGrabaciones.xmlRecordingID = recordingId;
                    xmlGrabaciones.conversationID = conversationId;
                    xmlGrabaciones.xmlempresa = xmlEmpresa;
                    xmlGrabaciones.xmlOrganization = xmlOrganizacion;
                    //-- campos para yanbal
                    xmlGrabaciones.IdRecording = recordingId;
                    xmlGrabaciones.ConversationId = conversationId;
                    xmlGrabaciones.Direction = direction;
                    xmlGrabaciones.Duration = duration;
                    xmlGrabaciones.ConversationStartTime = conversationStartTime.ToString("yyyy-MM-ddTHH:mm:ss");
                    xmlGrabaciones.ConversationEndTime = conversationEndTime.ToString("yyyy-MM-ddTHH:mm:ss");
                    xmlGrabaciones.Userid = userId;
                    xmlGrabaciones.Agentid = agentId;
                    xmlGrabaciones.WrapUpCode = wrapupcode;
                    xmlGrabaciones.ACW = acw;
                    xmlGrabaciones.ANI = ani;
                    xmlGrabaciones.QueueName = nameQqueue;
                    xmlGrabaciones.NameDivision = nameDivision;
                   

                    xmlGrabaciones.xmlRutadeAudio = _directorioAudio;
                    xmlGrabaciones.xmlRutaCompletaAudioMP3 = _Audiomp3;
                    xmlGrabaciones.xmlNombreAudioExcel = _NombreAudioExcel;
                    xmlGrabaciones.eFecha = eFecha;
                    xmlGrabaciones.eAnio = eAnio;
                    xmlGrabaciones.eMes = eMes;
                    xmlGrabaciones.eDia = eDia;
                    xmlGrabaciones.eHora = eHora;

                    xmlGrabaciones.xmlUrlGCAudio = _urlAudio;
                    xmlGrabaciones.xmldirectorioFTP = $"{xmlRutaFtp}/{direccionOrigen}/{nameCampaignCola}/{_anio}/{_mes}/{_dia}"; 
                    xmlGrabaciones.xmlArchivolocal = _archivolocal;

                    //Datos para pacifico
                    xmlGrabaciones.p_nameCampaignCola = nameCampaignCola;
                    xmlGrabaciones.p_empresa = xmlEmpresa;
                    xmlGrabaciones.p_dNICliente = xmlDniCliente;
                    xmlGrabaciones.p_apellidoPaterno = xmlApellidoPaterno;
                    xmlGrabaciones.p_apellidoMaterno = xmlApellidoMaterno;
                    xmlGrabaciones.p_nombres = xmlNombres;
                    xmlGrabaciones.p_telefono = xmlTelefono;
                    xmlGrabaciones.p_fechaDeServicio = $"{_anio}-{_mes}-{_dia}";
                    xmlGrabaciones.p_horaDeServicio = $"{_Hour}:{_Minute}:{_Seconds}";
                    xmlGrabaciones.p_NroAsesor = xmlNumeroAsesor;
                    xmlGrabaciones.p_Proceso = xmlProceso;
                    xmlGrabaciones.p_vdn = xmlVdn;
                    xmlGrabaciones.p_skill = xmlSkill;
                    xmlGrabaciones.p_ramo = xmlRamo;
                    xmlGrabaciones.p_producto = xmlProducto;
                    xmlGrabaciones.p_resultado = xmlResultado;
                    xmlGrabaciones.p_subResultado = xmlSubResultado;


                #region metodo para crear directorio y descargar el audio en MP3
                #region Crear directorio
                try
                    {
                        await _ecMetodos.CrearDirectorio(xmlGrabaciones.xmlRutadeAudio);
                    }
                    catch (Exception ex)
                    {
                        await EC_EscribirLog.EscribirLogAsync($"Error al crearDirectorio: {ex.Message.ToString()}");
                        Console.WriteLine("Error: " + ex.Message.ToString());
                        throw;
                    }
                    #endregion

                    #region Descargar audio
                    try
                    {
                        xmlGrabaciones.xmlAudioDescargado = await _ecMetodos.DownloadFileAsync(xmlGrabaciones.xmlRutaCompletaAudioMP3, xmlGrabaciones.xmlUrlGCAudio);

                        if (xmlGrabaciones.xmlAudioDescargado)
                        {
                            ////subir al repositorio de ftp
                            bool respuestaOkSFTKonecta = await _ecMetodos.SubirArchivosSFTPKonecta(xmlGrabaciones.xmlRutaCompletaAudioMP3, xmlGrabaciones.xmldirectorioFTP);
                            if (respuestaOkSFTKonecta)
                            {
                                await EC_EscribirLog.EscribirLogAsync($"Archivo subido correctamente al SFTP de Konecta: {xmlGrabaciones.xmlRutaCompletaAudioMP3}");
                            }
                            else
                            {
                                await EC_EscribirLog.EscribirLogAsync($"Error al subir archivo al SFTP de Konecta: {xmlGrabaciones.xmlRutaCompletaAudioMP3}");
                                ////subir al repositorio d amazon
                                //  await _ecMetodos.SubirArchivosSFTAmazon(xmlGrabaciones.xmlRutaCompletaAudioMP3, xmlGrabaciones.eAnio, xmlGrabaciones.eMes);
                            }
                        }
                        else // if (xmlGrabaciones.xmlAudioDescargado)
                        {
                            await EC_EscribirLog.EscribirLogAsync($"Error en DownloadFileAsync: Falló la descarga del audio. | conversationID: {xmlGrabaciones.conversationID} | recordingID: {xmlGrabaciones.xmlRecordingID}");
                            continue;
                        }

                    }
                    catch (Exception ex)
                    {
                        await EC_EscribirLog.EscribirLogAsync($"Error en DownloadFileAsync: {ex.Message.ToString()} | conversationID: {xmlGrabaciones.conversationID}| recordingID: {xmlGrabaciones.xmlRecordingID}");
                        continue;
                    }
                    #endregion
                    #endregion

                    #endregion
                        bool yaExiste = listXmlGrabaciones.Any(x =>
                        x.conversationID == xmlGrabaciones.conversationID &&
                        x.IdRecording == xmlGrabaciones.IdRecording);

                    if (!yaExiste)
                    {
                        listXmlGrabaciones.Add(xmlGrabaciones);
                    }

                    #endregion 
                 
                } //fin del foreach de todas las conversaciones

                



        }
        #endregion
    }
}

