using DocumentFormat.OpenXml.Office.CoverPageProps;
using Grabaciones.Models;
using Grabaciones.Services.Econtact;
using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Client;
using PureCloudPlatform.Client.V2.Model;
using System.Diagnostics;

namespace Grabaciones.Services.GenesysCloud
{
    public class SGC_Queue
    {
        #region Obtener colas
        public static List<GC_Queue> ObtenerColas() {

            List<GC_Queue> queueList = new List<GC_Queue>();



            var apiInstance = new RoutingApi();
            var pageNumber = 56;  // int? | Page number (optional)  (default to 1)
            var pageSize = 56;  // int? | Page size (optional)  (default to 25)
            var sortOrder = "asc";  // string | Note: results are sorted by name. (optional)  (default to asc)
            var name = "name_example";  // string | Include only queues with the given name (leading and trailing asterisks allowed) (optional) 
            var id = new List<string>(); // List<string> | Include only queues with the specified ID(s) (optional) 
            var divisionId = new List<string>(); // List<string> | Include only queues in the specified division ID(s) (optional) 
            var peerId = new List<string>(); // List<string> | Include only queues with the specified peer ID(s) (optional) 
            var cannedResponseLibraryId = "cannedResponseLibraryId_example";  // string | Include only queues explicitly associated with the specified canned response library ID (optional) 
            var hasPeer = true;  // bool? | Include only queues with a peer ID (optional) 

            bool flag = true;
            var queuePagesize = 100;
            var queuepageNumber = 1;
            try
            {
                while (flag)
                {
                    // Get list of queues.
                    QueueEntityListing resultQueue = apiInstance.GetRoutingQueues(queuepageNumber, queuePagesize, sortOrder, null, null, null, null, null, null);
                    Debug.WriteLine(resultQueue);

                    foreach (var result in resultQueue.Entities)
                    {

                        queueList.Add(new GC_Queue
                        {
                            QueueId = result.Id,
                            QueueName = result.Name.ToUpper(),

                        });
                    }
                    if (resultQueue.PageNumber < resultQueue.PageCount)
                    {
                        queuepageNumber++;
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                    }

                }
            }
            catch(ApiException aEx)
            {
                EC_EscribirLog.EscribirLog($"Error en api GetRoutingQueues| Error code: {aEx.ErrorCode}| ErrorMessage: {aEx.Message}");
            }
            catch (Exception e)
            {
                EC_EscribirLog.EscribirLog($"Exception when calling Routing.GetRoutingQueues: {e.Message}");
            }
            
            return queueList;
        }
        #endregion

        #region Obtener colas
        public static async Task<List<GC_Queue>> ObtenerColasPorDivision(List<string> vDivisionId)
        {

            List<GC_Queue> queueList = new List<GC_Queue>();

            var apiInstance = new RoutingApi();
            var id = new List<string>(); // List<string> | Include only queues with the specified ID(s) (optional) 
            var divisionId = vDivisionId; // List<string> | Include only queues in the specified division ID(s) (optional) 
            var peerId = new List<string>(); // List<string> | Include only queues with the specified peer ID(s) (optional) 
            var sortOrder = "asc";  // string | Note: results are sorted by name. (optional)  (default to asc)
            
            
            var pageNumber = 56;  // int? | Page number (optional)  (default to 1)
            var pageSize = 56;  // int? | Page size (optional)  (default to 25)
            var name = "name_example";  // string | Include only queues with the given name (leading and trailing asterisks allowed) (optional) 
            var cannedResponseLibraryId = "cannedResponseLibraryId_example";  // string | Include only queues explicitly associated with the specified canned response library ID (optional) 
            var hasPeer = true;  // bool? | Include only queues with a peer ID (optional) 



            bool flag = true;
            var queuePagesize = 100;
            var queuepageNumber = 1;
            try
            {
                while (flag)
                {
                    // Get list of queues.
                    //QueueEntityListing resultQueue = apiInstance.GetRoutingQueues(queuepageNumber, queuePagesize, sortOrder, null, null, null, null, null, null);
                    QueueEntityListing resultQueue = await apiInstance.GetRoutingQueuesAsync(queuepageNumber, queuePagesize, sortOrder, null, null, divisionId, null, null, null);
                    Debug.WriteLine(resultQueue);

                    foreach (var result in resultQueue.Entities)
                    {

                        queueList.Add(new GC_Queue
                        {
                            QueueId = result.Id,
                            QueueName = result.Name.ToUpper(),

                        });
                    }
                    if (resultQueue.PageNumber < resultQueue.PageCount)
                    {
                        queuepageNumber++;
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                    }

                }
            }
            catch (ApiException aEx)
            {
                EC_EscribirLog.EscribirLog($"Error en api GetRoutingQueues| Error code: {aEx.ErrorCode}| ErrorMessage: {aEx.Message}");
            }
            catch (Exception e)
            {
                EC_EscribirLog.EscribirLog($"Exception when calling Routing.GetRoutingQueues: {e.Message}");
            }

            return queueList;
        }
        #endregion

        #region Agrupar colas por pais
        public static List<EC_GruposPaisCola> ObtenerColasAgrupadasPorPais(List<GC_Queue> GC_Queues, List<EC_Paises>? paises, int TotalGrabaciones, List<GC_Skill> GC_Skills)
        {
            List<EC_GruposPaisCola> PaisCola = GC_Queues
                                .Where(q => paises.Any(p =>q.QueueName.StartsWith(p.inicial + "_", StringComparison.OrdinalIgnoreCase)))
                                .GroupBy(q =>
                                {
                                    var paisConf = paises.First(p => q.QueueName.StartsWith(p.inicial + "_", StringComparison.OrdinalIgnoreCase));
                                    return paisConf.pais;
                                })
                                .Select(g =>
                                {
                                    var paisConfig = paises.First(p => p.pais.Equals(g.Key, StringComparison.OrdinalIgnoreCase));
                                    // Buscar las aptitudes del país (si existen)
                                    List<GC_Skill> aptitudesPais = new List<GC_Skill>();

                                    if (paisConfig.Aptitudes != null && paisConfig.Aptitudes.Any())
                                    {
                                        aptitudesPais = GC_Skills
                                            .Where(skill => paisConfig.Aptitudes.Contains(skill.skillname))
                                            .ToList();
                                    }
                                    return new EC_GruposPaisCola
                                    {
                                        pais = g.Key,
                                        colas = g.ToList(),
                                        cantidadGrabaciones = (int)Math.Round((double)TotalGrabaciones * paisConfig.porcentaje / 100.0),
                                        aptitudes = aptitudesPais
                                    };
                                })
                                .ToList();

            return PaisCola;
        }
        #endregion
    }
}
