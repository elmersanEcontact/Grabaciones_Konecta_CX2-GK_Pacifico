using Grabaciones.Models;
using Grabaciones.Services.Econtact;
using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Client;
using PureCloudPlatform.Client.V2.Model;
using System.Diagnostics;

namespace Grabaciones.Services.GenesysCloud
{
    public class SGC_Campaign
    { 
        public static async Task<List<EC_Campaign>> GetCampaing(List<string> lDivisionId)
        {
            List<EC_Campaign> campaignList = new List<EC_Campaign>();

            bool flag = true;
            // Create an instance of the OutboundApi to access campaign methods
            var apiInstance = new OutboundApi();
            var pageSize = 100;  // int? | Page size. The max that will be returned is 100. (optional)  (default to 25)
            var pageNumber = 1;  // int? | Page number (optional)  (default to 1)
            var divisionId = lDivisionId; // List<string> | Division ID(s) (optional)
            var sortBy = "name";  // string | Sort by (optional) 
            var sortOrder = "ascending";  // string | Sort order (optional)  (default to a)
            
            //var filterType = "filterType_example";  // string | Filter type (optional)  (default to Prefix)
            //var name = "name_example";  // string | Name (optional) 
            //var id = new List<string>(); // List<string> | id (optional) 
            //var contactListId = "contactListId_example";  // string | Contact List ID (optional) 
            //var dncListIds = "dncListIds_example";  // string | DNC list ID (optional) 
            //var distributionQueueId = "distributionQueueId_example";  // string | Distribution queue ID (optional) 
            //var edgeGroupId = "edgeGroupId_example";  // string | Edge group ID (optional) 
            //var callAnalysisResponseSetId = "callAnalysisResponseSetId_example";  // string | Call analysis response set ID (optional) 
            // List<string> | Division ID(s) (optional) 


            try
            {
                while (flag)
                {
                    // Get list of queues.
                    CampaignEntityListing resultCampaign = await apiInstance.GetOutboundCampaignsAsync(pageSize, 
                                                                                                        pageNumber, 
                                                                                                        null, 
                                                                                                        null, 
                                                                                                        null, 
                                                                                                        null, 
                                                                                                        null, 
                                                                                                        null, 
                                                                                                        null, 
                                                                                                        null, 
                                                                                                        divisionId, 
                                                                                                        sortBy, 
                                                                                                        sortOrder);
                    Debug.WriteLine(resultCampaign);

                    foreach (var item in resultCampaign.Entities)
                    {

                        campaignList.Add(new EC_Campaign
                        {
                            IdCampaign = item.Id,
                            NameCampaign = item.Name.ToUpper(),

                        });
                    }
                    if (resultCampaign.PageNumber < resultCampaign.PageCount)
                    {
                        pageNumber++;
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
            return campaignList;
        }
    }
}
