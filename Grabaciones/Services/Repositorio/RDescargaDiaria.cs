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
            EC_EscribirLog.EscribirLog("Se inicia proceso de descarga " + FechaInicio.ToString("yyyy-MM-dd HH:mm:ss") + " - " + FechaFin.ToString("yyyy-MM-dd HH:mm:ss"));

            #region Traer el numero de la semana
            string vRespuestSemana = await _ecMetodos.ObtenerNombreSemanaUltimoDia(FechaInicio);

            string[] arrNombresemanaUltimoDia = vRespuestSemana.Split('|'); //await _ecMetodos.GetWeekRangeAsync(FechaInicio, FechaFin);

            string _nombresemana = arrNombresemanaUltimoDia[0];
            #endregion

            #region valido que el valor de fechainicio sea el ultimo día de la semana
            //bool ultimodiadelasemana = FechaInicio.DayOfWeek == DayOfWeek.Sunday;
            bool ultimodiadelasemana = arrNombresemanaUltimoDia[1] == "Domingo";
            #endregion

            #region Enviroment
            List<XmlGrabaciones> L_GC_RecordingsXml_Path = new List<XmlGrabaciones>();
            List<GC_LeerCsv> ArchivosCsvJuntos = new List<GC_LeerCsv>();

            string xmlRutaPrincipal = _config.GetValue<string>("ConfiguracionAudio:RutaPrincipal");
            string xmlNombreCarpeta = _config.GetValue<string>("ConfiguracionAudio:NombreCarpeta");
            string xmlCliente = _config.GetValue<string>("ConfiguracionAudio:Cliente");
            string xmlOrganizacion = _config.GetValue<string>("ConfiguracionAudio:Organization");
            string xmlEmpresa = _config.GetValue<string>("ConfiguracionAudio:Empresa");
            string xmlFormato = _config.GetValue<string>("ConfiguracionAudio:Formato");
            string xmlRutaFtp = _config.GetValue<string>("ConfiguracionAudio:RutaFtp").Replace("\\",@"\");
            
            string DirectorioAudio = string.Concat(xmlRutaPrincipal, "/", xmlOrganizacion, "/", xmlCliente, "/", xmlNombreCarpeta);
            string _directorioGrabaciones = string.Concat(DirectorioAudio, "-", _nombresemana);


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
            List<GC_Division> ListDivisions = new List<GC_Division>();
            ListDivisions = sGC_Division.ObtenerDivision();
            #endregion

            #region Obtener Wrapupcode(Tipificaciones)  -- validar cuando falle el método
            List<GC_Wrapupcode> ListWrapupcode = new List<GC_Wrapupcode>();
            ListWrapupcode = SGC_Wrapupcode.ObtenerWrapupcode();
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

            while (vFechaInicioIntervalo < vFechaFin)
            {
                int conteoConversaciones = 1;
                rangoFechas = vFechaInicioIntervalo.ToString("yyyy-MM-ddTHH:mm:ss") + "/" + vFechaFinIntervalo.ToString("yyyy-MM-ddTHH:mm:ss");
                ValueSegmentQuery = vFechaInicioIntervalo.ToString("yyyy-MM-ddTHH:mm:ss") + ".000Z/" + vFechaFinIntervalo.ToString("yyyy-MM-ddTHH:mm:ss" + ".000Z");

                #region Obtener las conversaciones según fecha
                EC_EscribirLog.EscribirLog("Las conversaciones a evaluar son del rango:"+rangoFechas);
                List<AnalyticsConversationWithoutAttributes> conversationDetails = SGC_ConversationsDetailsQuery.ObtenerConversaciones(rangoFechas, ValueSegmentQuery, _config);

                EC_EscribirLog.EscribirLog("Se traerá información de un total de: " + conversationDetails.Count().ToString() + "conversaciones(Hits)");
                #endregion

                #region recorrido de cada una de las conversaciones
                if (conversationDetails.Count == 0 || conversationDetails is null)
                {
                    Console.WriteLine("No hay registros a evaluar");
                }
                else
                {
                    foreach(var conversation in conversationDetails)
                    {
                        Console.WriteLine("Item: " + conteoConversaciones + " - Conversacion:" + conversation.ConversationId);
                        #region metadata de conversacion según conversationId

                        List<RecordingMetadata> vRecordingMetadata = SGC_ConversationRecordingmetadata.ObtenerConversationRecordingmetadata(conversation.ConversationId, vFechaInicioIntervalo);
                       
                        string ? vOriginatingDirection =  conversation.OriginatingDirection.ToString();
                        foreach (var iRecording in vRecordingMetadata)
                        {
                           
                            //if(iRecording.StartTime.to)

                            XmlGrabaciones xmlGrabaciones = new XmlGrabaciones();

                            DateTime _startTime = DateTime.Parse(iRecording.StartTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
                            DateTime _endTime = DateTime.Parse(iRecording.EndTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
                            TimeSpan diferenciaSegundos = _endTime - _startTime;
                            int nDiferenciaSegundos = diferenciaSegundos.Seconds;

                            string recordinId = iRecording.Id;
                            Recording DatosMP3 = new Recording();
                            EC_EscribirLog.EscribirLog("Se extrae la información de las grabaciones de la conversación: " + iRecording.ConversationId + " y de la grabacion: " + iRecording.Id + " - con una duracion de " + nDiferenciaSegundos.ToString());
                            DatosMP3 = SGC_ConversationRecording.ObtenerDatosGrabacionMP3(iRecording.ConversationId, iRecording.Id, _config);
                            
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
                                string _FechaRecording = StartTime.ToString("dd/MM/yyyy");
                                string _HoraRecording = StartTime.ToString("HH:mm:ss");
                                string _Telefono = "";
                                string ?_direction = vOriginatingDirection;
                                string ?_Proceso = string.Empty;
                                string ?_Wrapup = string.Empty;
                                string ?diferencia_segundos = "0";
                                #endregion

                                #region Datos para Excel
                                string eParteDisco = "";
                                string eFecha = StartTime.ToString("yyyy-MM-dd");
                                string eAnio = StartTime.ToString("yyyy");
                                string eMes = StartTime.ToString("MM");
                                string eDia = StartTime.ToString("dd");
                                string eHora = StartTime.ToString("HH");
                                string eNombreApellidos = "NNN";
                                string eDniTitular = "00000000";
                                string ePlaca = "NNN";
                                string ePlan = "NNN";
                                string ePrima = "NNN";
                                string eCelularCliente = "000000000";
                                string eFijoCliente = "0000000";
                                string eDniAsesor = "00000000";
                                string NombreApellidosAsesor = "";
                                string eCodigo = "";
                                string eEtiqueta = "";
                                string eParteGrabacion = "";
                                string eDatosdelLogindelAsesor = "";
                                #endregion = "";

                                string _directorio = string.Concat(DirectorioAudio, "-", _nombresemana);
                               // string _directorio = string.Concat(DirectorioAudio, "/", _anio, "/", _mes, "/", _dia);
                                string _NomenclaturaAudioMP3 = "";
                                string _NomenclaturaAudioGSM = "";
                                string _Audiomp3 = "";
                                string _Audiogsm = "";
                                string _NombreAudioExcel = "";
                                string _urlAudio = "";
                                string _directorioFTP = "";
                                string _archivolocal = "";

                                if (!DatosMP3.MediaUris.ContainsKey("S"))
                                {
                                    _urlAudio = "NoExisteUri";
                                }
                                else if(DatosMP3.MediaUris["S"].MediaUri is null)
                                {
                                    _urlAudio = "NoExisteUri";
							    }
                                else
                                {
                                    _urlAudio = DatosMP3.MediaUris["S"].MediaUri;
                                }   
                            
                                string username = "";
                                List<User> _users = new List<User>();
                                _users = DatosMP3.Users;
                                username = _users.Count() > 0 ? _users[0].Username : "NN";

                            #endregion

                            #region llamadas Inbound
                            if (vOriginatingDirection == "Inbound")
                            {
                                Console.WriteLine("Llamadas Entrantes");
								CallConversation resultConversationInbound = SGC_ConversationsCall.ObtenerCallConversation(conversation.ConversationId);

								foreach (var oResultParticipant in resultConversationInbound.Participants)
								{
									if (oResultParticipant.Purpose == "agent")
									{
										foreach (var Aitem in oResultParticipant.Attributes)
										{
											string _key = Aitem.Key;
											string _value = Aitem.Value;

											ePlaca = _key == "vPlaca" ? _value == "" ? ePlaca : _value : ePlaca;
											ePlan = _key == "vPlan" ? _value == "" ? ePlan : _value : ePlan;

											eDniTitular = _key == "sDni" ? _value == "" ? eDniTitular : _value : eDniTitular;
											ePrima = _key == "sNombres" ? _value == "" ? ePrima : _value : ePrima;
											eNombreApellidos = _key == "sNombres" ? _value == "" ? eNombreApellidos : _value : eNombreApellidos;
											eFijoCliente = _key == "sNombres" ? _value == "" ? eFijoCliente : _value : eFijoCliente;
											eDniAsesor = _key == "sNombres" ? _value == "" ? eDniAsesor : _value : eDniAsesor;
											NombreApellidosAsesor = _key == "sNombres" ? _value == "" ? NombreApellidosAsesor : _value : NombreApellidosAsesor;
											eCodigo = _key == "sNombres" ? _value == "" ? eCodigo : _value : eCodigo;
											//eEtiqueta = _key == "sNombres" ? _value == "" ? eEtiqueta : _value : eEtiqueta;
											eParteGrabacion = _key == "sNombres" ? _value == "" ? eParteGrabacion : _value : eParteGrabacion;
											eDatosdelLogindelAsesor = _key == "sNombres" ? _value == "" ? eDatosdelLogindelAsesor : _value : eDatosdelLogindelAsesor;
											eParteDisco = _key == "sNombres" ? _value == "" ? eParteDisco : _value : eParteDisco;
										}
									}
								}

								#region Se obtiene el numero de celular
								foreach (AnalyticsParticipantWithoutAttributes oParticipant in conversation.Participants)
								{
									foreach (var _sess in oParticipant.Sessions)
									{
										if (_sess.MediaType == AnalyticsSession.MediaTypeEnum.Voice)
										{
											if (_direction == "Inbound")
											{
												_Telefono = _ecMetodos.ReemplazarTelefonoxVacio(_sess.Ani);
											}
											else if (_direction == "Outbound")
											{
												_Telefono = _ecMetodos.ReemplazarTelefonoxVacio(_sess.Dnis);
											}
										}
										break;

									}
								}
								#endregion
							}
							#endregion

							#region Llamadas Outbound
							else if(vOriginatingDirection == "Outbound")
							{
								Console.WriteLine("Llamadas Salientes");
								#region si el participant[0] es customer se busca los datos en la base de datos
								if (conversation.Participants[0].Purpose == AnalyticsParticipantWithoutAttributes.PurposeEnum.Customer)
                                {
									#region Se obtienen los datos desde la base de datos
									#endregion
									foreach (var _isession in conversation.Participants[0].Sessions)
                                    {
                                        string vContactlistId = _isession.OutboundContactListId;
                                        string vContactId = _isession.OutboundContactId;
                                        string Url = @"https://apigenesyscloud.grupokonecta.pe/RimacSoatDatosContactlist_Services/v1/DatosContactlist";

                                        HttpClient httpClient = new HttpClient();
                                        using (var client = new HttpClient())
                                        {
                                            var parametros = "{'idContactlist':'" + vContactlistId + "','idContact':'" + vContactId + "'}";

                                            dynamic jsonstring = JObject.Parse(parametros);

                                            var httpcontent = new StringContent(jsonstring.ToString(), Encoding.UTF8, "application/json");

                                            var response = await client.PostAsync(Url, httpcontent);

                                            if(response.StatusCode == System.Net.HttpStatusCode.NoContent)
                                            {
                                                EC_EscribirLog.EscribirLog($"No se encontro información para contactlist:{vContactlistId} - contact: {vContactId}");
                                            }
                                            else
                                            {
                                                var rest = response.Content.ReadAsStringAsync().Result;

											    JObject jsonObject = JObject.Parse(rest);

											    ePlan = (string)jsonObject["plan"];
											    ePlaca = (string)jsonObject["placa"];
											    eNombreApellidos = (string)jsonObject["nombresapellidos"];
											    eDniTitular = (string)jsonObject["dnititular"];
											    ePrima = (string)jsonObject["prima"];
                                                break;
                                            }
										}
                                    }
                                }
                                #endregion
                                
								#region si el participant[0] es agent se busca los datos en los Attributes de la APi llamadas manuales
								if (conversation.Participants[0].Purpose == AnalyticsParticipantWithoutAttributes.PurposeEnum.Agent)
                                {
									#region Se obtienen los datos de participants
									CallConversation resultConversation = SGC_ConversationsCall.ObtenerCallConversation(conversation.ConversationId);
								    foreach (var oResultParticipant in resultConversation.Participants)
								    {
										string _nombre = "";
										string _apellidoPaterno = "";
										foreach (var Aitem in oResultParticipant.Attributes)
										{
										    string _key = Aitem.Key;
										    string _value = Aitem.Value;

										    ePlaca = _key == "vPlaca" ? _value : ePlaca;
										    _nombre   = _key == "vNombre" ? _value == "" ? _nombre : _value : _nombre;
											_apellidoPaterno = _key == "vApellidoPaterno" ? _value == "" ? _apellidoPaterno : _value : _apellidoPaterno;
											eNombreApellidos = string.Concat(_nombre.Split(' ')[0], " ", _apellidoPaterno) ;
										    eDniTitular = _key == "vDocumento" ? _value == "" ? eDniTitular : _value : eDniTitular;
									    }
								    }
									#endregion
								}
                                #endregion


                                #region Se obtiene el numero de celular
                                bool _flagNumeroCelular = true;
                                foreach (AnalyticsParticipantWithoutAttributes oParticipant in conversation.Participants)
                                {
									foreach (var _sess in oParticipant.Sessions)
								    {
									    if (_sess.MediaType == AnalyticsSession.MediaTypeEnum.Voice)
									    {
										    if (_direction == "Inbound")
										    {
											    _Telefono = _ecMetodos.ReemplazarTelefonoxVacio(_sess.Ani);
                                                _flagNumeroCelular = false;
										    }
										    else if (_direction == "Outbound")
										    {
											    _Telefono = _ecMetodos.ReemplazarTelefonoxVacio(_sess.Dnis);
                                                _flagNumeroCelular = false;
                                            }
									    }
									    break;

								    }

                                    if (!_flagNumeroCelular) { break; }
                                }
								#endregion

							}
                            #endregion

                            string NombredelAudio = string.Concat(eDia, "-",eMes, "-",eAnio, "_", _RecordingId,"_", eNombreApellidos.Replace(" ", "-").Replace(@"\", "").Replace(@"/", ""), "_",_Telefono);
                            NombredelAudio = _ecMetodos.EliminarCaracteresEspeciales(NombredelAudio);
                            EC_EscribirLog.EscribirLog($"Nombre del audio=>{NombredelAudio}");
                            _NomenclaturaAudioMP3 = NombredelAudio + "." + xmlFormato;
							_NomenclaturaAudioGSM = NombredelAudio + ".gsm";
                            _NombreAudioExcel = _NomenclaturaAudioGSM;
                            _directorioFTP = string.Concat(xmlRutaFtp, @"-", _nombresemana);
                            //_directorioFTP = string.Concat(xmlRutaFtp, @"\", _anio, @"\", _mes, @"\", _dia);
                            _archivolocal = string.Concat(NombredelAudio, ".gsm");


                            _Audiomp3 = string.Concat(_directorio, "/", _NomenclaturaAudioMP3);
							_Audiogsm = string.Concat(_directorio, "/", _NomenclaturaAudioGSM);

							#region Crear el objeto xml
                            
							xmlGrabaciones.xmlRecordingID = recordinId;
                            xmlGrabaciones.conversationID = conversation.ConversationId;
                            xmlGrabaciones.xmlempresa = xmlEmpresa;
                            xmlGrabaciones.xmlOrganization = xmlOrganizacion;
                            xmlGrabaciones.xmlDNICliente = "";
                            xmlGrabaciones.xmlRutadeAudio = _directorio;
							xmlGrabaciones.xmlRutaCompletaAudioMP3 = _directorio + "/" + _NomenclaturaAudioMP3;
							xmlGrabaciones.xmlRutaCompletaAudioGSM = _directorio + "/" + _NomenclaturaAudioGSM;
							xmlGrabaciones.xmlNombreAudioExcel = _NombreAudioExcel;
                            xmlGrabaciones.eParteDisco = eParteDisco;
                            xmlGrabaciones.eFecha = eFecha;
                            xmlGrabaciones.eAnio = eAnio;
                            xmlGrabaciones.eMes = eMes;
                            xmlGrabaciones.eDia = eDia;
                            xmlGrabaciones.eHora = eHora;
                            xmlGrabaciones.eNombreApellidos = eNombreApellidos==""?"NNN":eNombreApellidos;
                            xmlGrabaciones.eDniTitular = eDniTitular;
                            xmlGrabaciones.ePlaca = ePlaca;
                            xmlGrabaciones.ePlan = ePlan;
                            xmlGrabaciones.ePrima = ePrima;
                            eCelularCliente = _ecMetodos.ValidarSiesCelular(_Telefono);
                            xmlGrabaciones.eCelularCliente = eCelularCliente;
                            eFijoCliente = _ecMetodos.ValidarSiesFijo(_Telefono);
                            xmlGrabaciones.eFijoCliente = eFijoCliente;
                            xmlGrabaciones.eDniAsesor = eDniAsesor;
                            xmlGrabaciones.eNombreApellidosAsesor = NombreApellidosAsesor;
                            xmlGrabaciones.eCodigo = eCodigo;
                            xmlGrabaciones.eEtiqueta = _NomenclaturaAudioGSM;
                            xmlGrabaciones.eParteGrabacion = eParteGrabacion;
                            xmlGrabaciones.eDatosdelLogindelAsesor = eDatosdelLogindelAsesor;
                            xmlGrabaciones.xmlUrlGCAudio = _urlAudio;
                            xmlGrabaciones.xmldirectorioFTP = _directorioFTP.Replace("\\",@"\");
                            xmlGrabaciones.xmlArchivolocal = _archivolocal;

							#endregion
							listXmlGrabaciones.Add(xmlGrabaciones);

                        }
                        
                        #endregion
                    }
                }
                #endregion

                vFechaInicioIntervalo = vFechaInicioIntervalo.AddDays(1);
                vFechaFinIntervalo = vFechaFinIntervalo.AddDays(1);
				//listXmlGrabaciones.Clear();

			}
            #endregion

            #region Descarga de las grabaciones
            foreach (var iGrabaciones in listXmlGrabaciones)
            {
                if (iGrabaciones.xmlUrlGCAudio == "NoExisteUri")
                {
                    Console.WriteLine("No existe audio para la grabacion: " + iGrabaciones.xmlRecordingID);
                }
                else
                {
                    #region Crear directorio
                    try
                    {
                        _ecMetodos.CrearDirectorio(iGrabaciones.xmlRutadeAudio);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message.ToString());
                        throw;
                    }
                    #endregion

                    #region Descargar audio
                    try
                    {
                        await _ecMetodos.DownloadFileAsync(iGrabaciones.xmlRutaCompletaAudioMP3,iGrabaciones.xmlUrlGCAudio);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message.ToString());
                        throw;
                    }
                    #endregion

                    #region Convertir el audio descargado en GSM
                    try
                    {
                        bool respuestaAudio;

                        GC_ImprimirExcel objExcel = new GC_ImprimirExcel();

                        try
                        {
                            respuestaAudio  = _ecMetodos.ConvertMp3ToGsm(iGrabaciones.xmlRutaCompletaAudioMP3, iGrabaciones.xmlRutaCompletaAudioGSM);

                        }
                        catch (Exception ex)
                        {
                            EC_EscribirLog.EscribirLog($"Error en _ecMetodos.ConvertMp3ToGsm: {ex.Message}");
                            throw;
                        }

                        if (respuestaAudio)
                        {
                            objExcel.semana = _nombresemana;
                            objExcel.directorioExcel = @"" + iGrabaciones.xmlRutadeAudio + @"\" + iGrabaciones.eProveedor + "_" + iGrabaciones.eProducto + "_" + iGrabaciones.eSponsor + "_" + iGrabaciones.eCanal + "-" + _nombresemana + ".xlsx";
                            objExcel.archivoExcel = @"" + iGrabaciones.eProveedor + "_" + iGrabaciones.eProducto + "_" + iGrabaciones.eSponsor + "_" + iGrabaciones.eCanal + "-" + _nombresemana + ".xlsx";
                            objExcel.ruta = @"" + xmlRutaFtp+"-"+_nombresemana  + @"\" + iGrabaciones.xmlNombreAudioExcel;
                            objExcel.proveedor = iGrabaciones.eProveedor;
                            objExcel.producto = iGrabaciones.eProducto;
                            objExcel.parteDisco = iGrabaciones.eParteDisco;
                            objExcel.canal = iGrabaciones.eCanal;
                            objExcel.sponsor = iGrabaciones.eSponsor;
                            objExcel.fecha = iGrabaciones.eFecha;
                            objExcel.anio = iGrabaciones.eAnio;
                            objExcel.mes = iGrabaciones.eMes;
                            objExcel.dia = iGrabaciones.eDia;
                            objExcel.hora = iGrabaciones.eHora;
                            objExcel.nombresYApellidosdelTitular = iGrabaciones.eNombreApellidos;
                            objExcel.dniDelTitular = iGrabaciones.eDniTitular;
                            objExcel.nPlaca = iGrabaciones.ePlaca;
                            objExcel.plan = iGrabaciones.ePlan;
                            objExcel.prima = iGrabaciones.ePrima;
                            objExcel.celularDelCliente = iGrabaciones.eCelularCliente;
                            objExcel.fijoDelCliente = iGrabaciones.eFijoCliente;
                            objExcel.dniDelAsesor = iGrabaciones.eDniAsesor;
                            objExcel.nombresYApellidosdelAsesor = iGrabaciones.eNombreApellidosAsesor;
                            objExcel.codigo = iGrabaciones.eCodigo;
                            objExcel.etiqueta = iGrabaciones.eEtiqueta;
                            objExcel.parteGrabacion = iGrabaciones.eParteGrabacion;
                            objExcel.datoDelLoginDelAsesor = iGrabaciones.eDatosdelLogindelAsesor;
                            objExcel.conversationId = iGrabaciones.conversationID;
                            objExcel.recordingId = iGrabaciones.xmlRecordingID;
                            objExcel.archivoCsv = $"{iGrabaciones.xmlRutadeAudio}"+@"\" + iGrabaciones.eAnio + iGrabaciones.eMes + iGrabaciones.eDia;
                            listImprimirExcel.Add(objExcel);

                        }

                    }
                    catch (Exception ex)
                    {
                        EC_EscribirLog.EscribirLog($"Error en ConvertMp3ToGsm: {ex.Message.ToString()}");
                        throw;
                    }
                    #endregion

                    #region Subir archivo a FTP
                    var result =  _ecMetodos.UploadFTPAudios(iGrabaciones.xmldirectorioFTP, iGrabaciones.xmlRutaCompletaAudioGSM, iGrabaciones.xmlArchivolocal);
                    #endregion

                }
            }
            #endregion

            #region crear archivo csv por día

            try
            {
                var respuesta = _ecMetodos.CrearArchivoCsv(listImprimirExcel);
            }
            catch (Exception ex)
            {
                EC_EscribirLog.EscribirLog($"Error al crear el archivo csv: {ex.Message.ToString()}");
                throw;
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
    }
}

