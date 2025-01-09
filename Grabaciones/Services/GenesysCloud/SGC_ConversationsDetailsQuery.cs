using Grabaciones.Services.Econtact;
using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Client;
using PureCloudPlatform.Client.V2.Model;
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
        public static List<AnalyticsConversationWithoutAttributes> ObtenerConversaciones(string rangoFechas, string ValueSegmentQuery, IConfiguration config)
        {
            #region Configurar filtros
            List<SegmentDetailQueryFilter> oSegmentDetailQuery = new List<SegmentDetailQueryFilter>();
            List<ConversationDetailQueryFilter> oConversationDetailQueryFilter = new List<ConversationDetailQueryFilter>();

            List<ConversationDetailQueryPredicate> oConversationDetailQueryPredicate = new List<ConversationDetailQueryPredicate>();
            List<ConversationDetailQueryPredicate> oConversationDetailQueryPredicate3 = new List<ConversationDetailQueryPredicate>();
            string vDivisionId = config.GetValue<string>("GenesysCloud:DivisionId");
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
                            Dimension = ConversationDetailQueryPredicate.DimensionEnum.Divisionid,
                            Value = vDivisionId
                        }
                    }
                },
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
                //            Value = "4311315c-cef9-4f45-8868-f1b5373ca8f2"
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
    }
}
