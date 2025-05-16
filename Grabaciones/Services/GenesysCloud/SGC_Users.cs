using Grabaciones.Models;
using Grabaciones.Services.Econtact;
using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Client;
using PureCloudPlatform.Client.V2.Model;
using System.Diagnostics;

namespace Grabaciones.Services.GenesysCloud
{
    public class SGC_Users
    {
        public static List<GC_Users> ObtenerUsuarios()
        {

            List<GC_Users> queueUser = new List<GC_Users>();




            var apiInstance = new UsersApi();
            var pageSize = 56;  // int? | Page size (optional)  (default to 25)
            var pageNumber = 56;  // int? | Page number (optional)  (default to 1)
            var id = new List<string>(); // List<string> | A list of user IDs to fetch by bulk (optional) 
            var jabberId = new List<string>(); // List<string> | A list of jabberIds to fetch by bulk (cannot be used with the \"id\" parameter) (optional) 
            var sortOrder = "asc";  // string | Ascending or descending sort order (optional)  (default to ASC)
            var expand = new List<string>(); // List<string> | Which fields, if any, to expand. Note, expand parameters are resolved with a best effort approach and not guaranteed to be returned. If requested expand information is absolutely required, it's recommended to use specific API requests instead. (optional) 
            var integrationPresenceSource = "integrationPresenceSource_example";  // string | Gets an integration presence for users instead of their defaults. This parameter will only be used when presence is provided as an \"expand\". When using this parameter the maximum number of users that can be returned is 100. (optional) 
            var state = "any";  // string | Only list users of this state (optional)  (default to active)


            bool flag = true;
            var userPagesize = 100;
            var userpageNumber = 1;
            try
            {
                while (flag)
                {
                    // Get list of queues.
                    UserEntityListing resultUsers = apiInstance.GetUsers(userPagesize, userpageNumber, null, null, sortOrder, null, null, state);
                    Debug.WriteLine(resultUsers);

                    foreach (var result in resultUsers.Entities)
                    {
                        queueUser.Add(new GC_Users
                        {
                            UserId = result.Id,
                            UserName = result.Name,
                            UserEmail = result.Email,

                        });
                    }
                    if (resultUsers.PageNumber < resultUsers.PageCount)
                    {
                        userpageNumber++;
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

            return queueUser;
        }
    }
}
