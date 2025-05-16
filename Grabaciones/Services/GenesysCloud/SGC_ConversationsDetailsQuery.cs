using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Spreadsheet;
using Grabaciones.Models;
using Grabaciones.Services.Econtact;
using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Client;
using PureCloudPlatform.Client.V2.Model;
using System.Collections;
using System.Collections.Generic;

namespace Grabaciones.Services.GenesysCloud
{
    public class SGC_ConversationsDetailsQuery
    {
        private readonly IConfiguration _config;
        private HttpClient _httpClient;

        public SGC_ConversationsDetailsQuery(IConfiguration configuration, HttpClient httpClient)
        {
            _config = configuration;
            _httpClient = httpClient;
        }

        #region Obtener las conversaciones por colas según el país
        public static async Task<List<AnalyticsConversationWithoutAttributes>> ObtenerConversaciones_x_Cola(string rangoFechas, string ValueSegmentQuery, IConfiguration config, List<EC_GruposPaisCola> PaisCola)
        {
            List<AnalyticsConversationWithoutAttributes> conversacionesPaisCola = new List<AnalyticsConversationWithoutAttributes>();
            int contadorPais = 1;
            #region Recorrido país por país
            foreach (var item in PaisCola)
            {
                


                EC_EscribirLog.EscribirLog($"Pais: {contadorPais} de {PaisCola.Count} - {item.pais}");
                //List<SegmentDetailQueryFilter> oSegmentDetailQueryxCola = new List<SegmentDetailQueryFilter>();
                List<SegmentDetailQueryPredicate> oSegmentDetailQueryxCola = new List<SegmentDetailQueryPredicate>(); // Nuevo: Predicados para colas
                List<SegmentDetailQueryPredicate> oSegmentDetailAptitudes = new List<SegmentDetailQueryPredicate>(); // Nuevo: Predicados para colas


                string vPais = item.pais;
                int vCantidadGrabaciones = item.cantidadGrabaciones;
                EC_EscribirLog.EscribirLog($"Se extraera para {vPais} un total de {vCantidadGrabaciones} conversaciones");

                //validacion para saber si el país a evaluar es PERU o BOLIVIA
                if (vPais.ToUpper() == "PERU" || vPais.ToUpper() == "BOLIVIA")
                {
                    EC_EscribirLog.EscribirLog($"Se asigna la cola para {item.pais} por la cola HUB_Peru-Bolivia ");
                    oSegmentDetailQueryxCola.Add(new SegmentDetailQueryPredicate()
                    {
                        Dimension = SegmentDetailQueryPredicate.DimensionEnum.Queueid, // Asegúrate de que "Queueid" es una dimensión válida
                        Operator = SegmentDetailQueryPredicate.OperatorEnum.Matches,
                        Value = "aa8d9650-8fba-4a06-ad60-e96a99f27233"
                    });

                    EC_EscribirLog.EscribirLog($"Se asigna las aptitudes para {item.pais} por la cola HUB_Peru-Bolivia ");
                    foreach (var aptitud in item.aptitudes)
                    {
                        oSegmentDetailAptitudes.Add(new SegmentDetailQueryPredicate()
                        {
                            Dimension = SegmentDetailQueryPredicate.DimensionEnum.Requestedroutingskillid, // Asegúrate de que "Queueid" es una dimensión válida
                            Operator = SegmentDetailQueryPredicate.OperatorEnum.Matches,
                            Value = aptitud.skillID
                        });
                    }
                }
                else
                {
                    foreach (var queueId in item.colas)
                    {
                        oSegmentDetailQueryxCola.Add(new SegmentDetailQueryPredicate()
                        {
                            Dimension = SegmentDetailQueryPredicate.DimensionEnum.Queueid, // Asegúrate de que "Queueid" es una dimensión válida
                            Operator = SegmentDetailQueryPredicate.OperatorEnum.Matches,
                            Value = queueId.QueueId
                        });
                    }
                }


                #region Configurar filtros
                List<SegmentDetailQueryFilter> oSegmentDetailQuery = new List<SegmentDetailQueryFilter>();
                List<ConversationDetailQueryFilter> oConversationDetailQueryFilter = new List<ConversationDetailQueryFilter>();

                List<ConversationDetailQueryPredicate> oConversationDetailQueryPredicate = new List<ConversationDetailQueryPredicate>();
                #endregion

                #region instancias
                ConversationsApi conversationsApi = new ConversationsApi();
                ConversationQuery body = new ConversationQuery();
                AnalyticsConversationQueryResponse vconversationDetails = new AnalyticsConversationQueryResponse();
                List<AnalyticsConversationQueryResponse> vlistconversationDetails = new List<AnalyticsConversationQueryResponse>();
                List<AnalyticsConversationWithoutAttributes> conversaciones = new List<AnalyticsConversationWithoutAttributes>();

                int iPageIndex = 1;
                int iPageSize = 0;

                #endregion

                #region  oSegmentDetailQueryPredicate
                oSegmentDetailQuery = new List<SegmentDetailQueryFilter>()
                {
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
                    #region filtros de purpose
                    new SegmentDetailQueryFilter()
                    {
                        Type = SegmentDetailQueryFilter.TypeEnum.And,
                        Predicates = new List<SegmentDetailQueryPredicate>()
                        {
                            new SegmentDetailQueryPredicate
                            {
                                Dimension = SegmentDetailQueryPredicate.DimensionEnum.Purpose,
                                Value = "agent"
                            }
                        },
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
                    },
                    //new SegmentDetailQueryFilter
                    //{
                    //    Type = SegmentDetailQueryFilter.TypeEnum.Or,
                    //    Predicates = oSegmentDetailQueryxCola
                    //},
                    //new SegmentDetailQueryFilter
                    //{
                    //    Type = SegmentDetailQueryFilter.TypeEnum.Or,
                    //    Predicates = oSegmentDetailAptitudes
                    //}
                };

                // Agregar filtro por cola si existe
                if (oSegmentDetailQueryxCola != null && oSegmentDetailQueryxCola.Any())
                {
                    oSegmentDetailQuery.Add(new SegmentDetailQueryFilter
                    {
                        Type = SegmentDetailQueryFilter.TypeEnum.Or,
                        Predicates = oSegmentDetailQueryxCola
                    });
                }

                // Agregar filtro por aptitudes si existe
                if (oSegmentDetailAptitudes != null && oSegmentDetailAptitudes.Any())
                {
                    oSegmentDetailQuery.Add(new SegmentDetailQueryFilter
                    {
                        Type = SegmentDetailQueryFilter.TypeEnum.Or,
                        Predicates = oSegmentDetailAptitudes
                    });
                }

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
                                Dimension = ConversationDetailQueryPredicate.DimensionEnum.Originatingdirection,
                                Value = "inbound"
                            }
                        }
                    },
                    new ConversationDetailQueryFilter
                    {
                        Type = ConversationDetailQueryFilter.TypeEnum.Or,
                        Predicates = new List<ConversationDetailQueryPredicate>()
                        {
                            new ConversationDetailQueryPredicate()
                            {
                                Metric = ConversationDetailQueryPredicate.MetricEnum.Tconversationduration,
                                Range = new NumericRange
                                        {
                                            Gte = 50000
                                        }
                            }
                        }
                    }

                    #region Opciones para filtro
                    //new ConversationDetailQueryFilter
                    //{
                    //    Type = ConversationDetailQueryFilter.TypeEnum.Or,
                    //    Predicates = new List<ConversationDetailQueryPredicate>()
                    //    {
                    //        new ConversationDetailQueryPredicate()
                    //        {
                    //            Dimension = ConversationDetailQueryPredicate.DimensionEnum.Divisionid,
                    //            Value = vDivisionId
                    //        }
                    //    }
                    //},
                    //new ConversationDetailQueryFilter
                    //{
                    //    Type = ConversationDetailQueryFilter.TypeEnum.And,
                    //    Predicates= new List<ConversationDetailQueryPredicate>()
                    //    {
                    //        new ConversationDetailQueryPredicate()
                    //        {
                    //            Metric = ConversationDetailQueryPredicate.MetricEnum.Thandle,
                    //            Operator = ConversationDetailQueryPredicate.OperatorEnum.Exists                    
                    //        },
                    //        new ConversationDetailQueryPredicate()
                    //        {
                    //            Metric = ConversationDetailQueryPredicate.MetricEnum.Ttalk,
                    //            Operator = ConversationDetailQueryPredicate.OperatorEnum.Exists
                    //        }
                    //    }
                    //}
                    ////,
                    ////new ConversationDetailQueryFilter
                    ////{
                    ////    Type = ConversationDetailQueryFilter.TypeEnum.And,
                    ////    Predicates = new List<ConversationDetailQueryPredicate>()
                    ////    {
                    ////        new ConversationDetailQueryPredicate()
                    ////        {
                    ////            Type = ConversationDetailQueryPredicate.TypeEnum.Metric,
                    ////            Operator = ConversationDetailQueryPredicate.OperatorEnum.Exists,
                    ////            Metric = ConversationDetailQueryPredicate.MetricEnum.Tanswered
                    ////        }
                    ////    }
                    ////}
                    //,new ConversationDetailQueryFilter
                    //{
                    //    Type = ConversationDetailQueryFilter.TypeEnum.And,
                    //    Predicates = new List<ConversationDetailQueryPredicate>()
                    //    {
                    //        new ConversationDetailQueryPredicate()
                    //        {
                    //            Type = ConversationDetailQueryPredicate.TypeEnum.Dimension,
                    //            Dimension = ConversationDetailQueryPredicate.DimensionEnum.Conversationid,
                    //            //Value = "9af1fd44-dcc0-4c6c-8c9c-c7c6a7c97b1f" //bolivia
                    //            //Value = "db5ef4b3-dbf6-48a4-b4c8-9b7b03ad0129" //peru
                    //            Value = "5e9e9572-b003-463f-a18b-2ce97881c64e" // ecuador
                    //        }
                    //    }
                    //}
                    #endregion  

                };
                #endregion

                body.Interval = rangoFechas;
                body.SegmentFilters = oSegmentDetailQuery;
                body.ConversationFilters = oConversationDetailQueryFilter;
                body.Order = ConversationQuery.OrderEnum.Asc;
                body.OrderBy = ConversationQuery.OrderByEnum.Conversationstart;

                // se obtiene la cantidad de lotes según el tamaño de lote y la cantidad de grabaciones
                int tamanioLote = 100;
                int cantidadLotes = (int)Math.Ceiling(Math.Ceiling((double)item.cantidadGrabaciones / tamanioLote)); // Calcular el número de lotes necesarios

                for (int i = 0; i < cantidadLotes; i++)
                {
                    int cantidadGrabacionesPorLote = Math.Min(tamanioLote, item.cantidadGrabaciones - (i * tamanioLote));
                    iPageSize = cantidadGrabacionesPorLote;
                    PagingSpec Paginacion = new PagingSpec(iPageSize, iPageIndex);
                    body.Paging = Paginacion;

                    var objBody = body.ToJson();
                    Console.WriteLine(objBody);
                    try
                    {
                        vconversationDetails = await conversationsApi.PostAnalyticsConversationsDetailsQueryAsync(body); // conversationsApi.PostAnalyticsConversationsDetailsQuery(body);

                        if (vconversationDetails.TotalHits > 0 && vconversationDetails.Conversations!= null)
                        {
                            
                            //conversaciones.Add(vconversationDetails.Conversations.ToList<>);
                            foreach (var conversation in vconversationDetails.Conversations)
                            {
                                conversaciones.Add(conversation);
                            }
                            iPageIndex++;
                        }
                        else
                        {
                            EC_EscribirLog.EscribirLog($"No se han encontrado conversaciones para el pais {vPais}");
                        }

                    }
                    catch (ApiException e)
                    {
                        EC_EscribirLog.EscribirLog("Error PostAnalyticsConversationsDetailsQuery :" + e.Message.ToString());
                        Console.WriteLine("Error PostAnalyticsConversationDetailQuery: " + e.Message.ToString());
                        throw;
                    }
                    catch(Exception e)
                    {
                        EC_EscribirLog.EscribirLog("Error en ObtenerConversaciones_x_Cola :" + e.Message.ToString());
                        Console.WriteLine("Error PostAnalyticsConversationDetailQuery: " + e.Message.ToString());
                        throw;
                    }

                }

                if (conversaciones.Count() > 0)
                {
                    EC_EscribirLog.EscribirLog($"Se han extraido un total de {conversaciones.Count()} conversaciones para el pais {vPais}");
                    conversacionesPaisCola.AddRange(conversaciones);
                }

                //  return conversaciones;
                //Console.WriteLine(item.pais);
                //Console.WriteLine(item.cola);
                //Console.WriteLine(item.cantidadGrabaciones);
                contadorPais++;
            }
            #endregion

            return conversacionesPaisCola;
        }
        #endregion

        #region Obtener conversaciones
        public static List<AnalyticsConversationWithoutAttributes> ObtenerConversaciones(string rangoFechas, string ValueSegmentQuery, IConfiguration config)
        {
            #region Configurar filtros
            List<SegmentDetailQueryFilter> oSegmentDetailQuery = new List<SegmentDetailQueryFilter>();
            List<ConversationDetailQueryFilter> oConversationDetailQueryFilter = new List<ConversationDetailQueryFilter>();

            List<ConversationDetailQueryPredicate> oConversationDetailQueryPredicate = new List<ConversationDetailQueryPredicate>();
            List<ConversationDetailQueryPredicate> oConversationDetailQueryPredicate3 = new List<ConversationDetailQueryPredicate>();
           // string vDivisionId = config.GetValue<string>("GenesysCloud:DivisionId");
            #endregion

            #region instancias
            ConversationsApi conversationsApi = new ConversationsApi();
            ConversationQuery body = new ConversationQuery();
            AnalyticsConversationQueryResponse vconversationDetails = new AnalyticsConversationQueryResponse();
            List<AnalyticsConversationQueryResponse> vlistconversationDetails = new List<AnalyticsConversationQueryResponse>();
            List<AnalyticsConversationWithoutAttributes> conversaciones = new List<AnalyticsConversationWithoutAttributes> ();

            int iPageIndex = 1;
            int iPageSize = 100;

            #endregion

            #region  oSegmentDetailQueryPredicate

            oSegmentDetailQuery = new List<SegmentDetailQueryFilter>() 
            {
                //new SegmentDetailQueryFilter {
                //Type = SegmentDetailQueryFilter.TypeEnum.Or,
                //Predicates = new List<SegmentDetailQueryPredicate>()
                //    {
                //        new SegmentDetailQueryPredicate{
                //            Dimension = SegmentDetailQueryPredicate.DimensionEnum.Mediatype,
                //            Value = "voice"
                //        },
                //        new SegmentDetailQueryPredicate{
                //            Dimension = SegmentDetailQueryPredicate.DimensionEnum.Mediatype,
                //            Value = "callback"
                //        },
                //    }
                //},
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
                #region filtros de purpose
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
                            Dimension = ConversationDetailQueryPredicate.DimensionEnum.Originatingdirection,
                            Value = "inbound"
                        }
                    }
                },

                

                //new ConversationDetailQueryFilter
                //{
                //    Type = ConversationDetailQueryFilter.TypeEnum.Or,
                //    Predicates = new List<ConversationDetailQueryPredicate>()
                //    {
                //        new ConversationDetailQueryPredicate()
                //        {
                //            Dimension = ConversationDetailQueryPredicate.DimensionEnum.Divisionid,
                //            Value = vDivisionId
                //        }
                //    }
                //},
                //new ConversationDetailQueryFilter
                //{
                //    Type = ConversationDetailQueryFilter.TypeEnum.And,
                //    Predicates= new List<ConversationDetailQueryPredicate>()
                //    {
                //        new ConversationDetailQueryPredicate()
                //        {
                //            Metric = ConversationDetailQueryPredicate.MetricEnum.Thandle,
                //            Operator = ConversationDetailQueryPredicate.OperatorEnum.Exists                    
                //        },
                //        new ConversationDetailQueryPredicate()
                //        {
                //            Metric = ConversationDetailQueryPredicate.MetricEnum.Ttalk,
                //            Operator = ConversationDetailQueryPredicate.OperatorEnum.Exists
                //        }
                //    }
                //}
                ////,
                ////new ConversationDetailQueryFilter
                ////{
                ////    Type = ConversationDetailQueryFilter.TypeEnum.And,
                ////    Predicates = new List<ConversationDetailQueryPredicate>()
                ////    {
                ////        new ConversationDetailQueryPredicate()
                ////        {
                ////            Type = ConversationDetailQueryPredicate.TypeEnum.Metric,
                ////            Operator = ConversationDetailQueryPredicate.OperatorEnum.Exists,
                ////            Metric = ConversationDetailQueryPredicate.MetricEnum.Tanswered
                ////        }
                ////    }
                ////}
                //new ConversationDetailQueryFilter
                //{
                //    Type = ConversationDetailQueryFilter.TypeEnum.And,
                //    Predicates = new List<ConversationDetailQueryPredicate>()
                //    {
                //        new ConversationDetailQueryPredicate()
                //        {
                //            Type = ConversationDetailQueryPredicate.TypeEnum.Dimension,
                //            Dimension = ConversationDetailQueryPredicate.DimensionEnum.Conversationid,
                //            Value = "6871f525-a384-4212-8ed9-49c1d6ddcadf"
                //        }
                //    }
                //}

            };
            #endregion

			body.Interval = rangoFechas;
            body.SegmentFilters = oSegmentDetailQuery;
            body.ConversationFilters = oConversationDetailQueryFilter;
            body.Order = ConversationQuery.OrderEnum.Asc;
            body.OrderBy = ConversationQuery.OrderByEnum.Conversationstart;

            bool flag = true;
            int tConversaciones =0; //total de conversaciones extraidas

            var objBody = body.ToJson();

            Console.WriteLine(objBody);

            while (flag)
            {
                PagingSpec Paginacion = new PagingSpec(iPageSize, iPageIndex);
                body.Paging = Paginacion;

                try
                {
                    vconversationDetails = conversationsApi.PostAnalyticsConversationsDetailsQuery(body);

                    if(vconversationDetails.TotalHits > 0 ) { 
                        //conversaciones.Add(vconversationDetails.Conversations.ToList<>);
                        foreach( var conversation in vconversationDetails.Conversations)
                        {
                            conversaciones.Add(conversation);
                        }

                        tConversaciones = tConversaciones+vconversationDetails.Conversations.Count;

                        if (tConversaciones <vconversationDetails.TotalHits)
                        {
                            iPageIndex++;
                        }
                        else
                        { flag=false; }
                    }
                    else
                    {
                        flag=false;
                    }

                }
                catch (ApiException e)
                {
                    EC_EscribirLog.EscribirLog("Error PostAnalyticsConversationsDetailsQuery :"+ e.Message.ToString());
                   Console.WriteLine("Error PostAnalyticsConversationDetailQuery: " + e.Message.ToString());
                   throw;
                }

            }

            return conversaciones;
        }
        #endregion
    }
}
