using Grabaciones.Models;

using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Model;

namespace Grabaciones.Services.GenesysCloud
{
    public class sGC_Division
    {
      
        public static List<GC_Division> ObtenerDivision()
        {
            var apiInstance = new ObjectsApi();
            var divPagesize = 100;
            var divpageNumber = 1;

            List<GC_Division> divisionList = new List<GC_Division>();
            bool flag = true;
            while(flag) {

                AuthzDivisionEntityListing resultDivision = apiInstance.GetAuthorizationDivisions(pageSize: divPagesize, pageNumber: divpageNumber);
                foreach (var division in resultDivision.Entities)
                {
                    //gc_Division.id =division.Id;

                    divisionList.Add(new GC_Division
                    {
                        id = division.Id,
                        name = division.Name
                        //homeDivision = division.HomeDivision
                    });

                }
                if(resultDivision.PageNumber<resultDivision.PageCount)
                {
                    divpageNumber++;
                    flag = true;
                }
                else
                {
                    flag= false;
                }
            }
            return divisionList;
        }
    }
}
