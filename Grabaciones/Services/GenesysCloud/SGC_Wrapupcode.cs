using Grabaciones.Models;
using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Model;

namespace Grabaciones.Services.GenesysCloud
{
    public class SGC_Wrapupcode
    {
        public static List<GC_Wrapupcode> ObtenerWrapupcode() {

            var apiWrapupcode = new ObjectsApi();
            var apiRouting = new RoutingApi();
            var wrPageSize = 100;
            var wrPageNumber = 1;

            List<GC_Wrapupcode> listwrapupcode = new List<GC_Wrapupcode>();
            bool flag = true;

            while(flag)
            {
                //UserEntityListing resultUsers = apiInstance5.GetUsers(pageSize: oclPagesize, pageNumber: oclPageNumber, state: "any");
                WrapupCodeEntityListing resultwrapup = apiRouting.GetRoutingWrapupcodes(pageSize: wrPageSize, pageNumber: wrPageNumber);
                foreach (var oWrapUps in resultwrapup.Entities)
                {
                    listwrapupcode.Add(new GC_Wrapupcode() {
                       id=  oWrapUps.Id,
                       name= oWrapUps.Name
                    });
                }

                if (resultwrapup.PageCount>resultwrapup.PageNumber)
                {
                    wrPageNumber++;
                    flag = true;
                }
                else
                {
                    flag = false;
                }

            }

            return listwrapupcode;
        }
    }
}
