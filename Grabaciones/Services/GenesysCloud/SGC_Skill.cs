using Grabaciones.Models;
using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Model;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace Grabaciones.Services.GenesysCloud
{
    public class SGC_Skill
    {
        public  async static Task<List<GC_Skill>> ObtenerSkills()
        {
            List<GC_Skill> listSkill = new List<GC_Skill>();
            var apiInstance = new RoutingApi();
            var pageSize = 100;  // int? | Page size (optional)  (default to 25)
            var pageNumber = 1;  // int? | Page number (optional)  (default to 1)
            //var name = "name_example";  // string | Filter for results that start with this value (optional) 
            //var id = new List<string>(); // List<string> | id (optional) 
            bool flag = true; // Variable para controlar el bucle while

            while (flag)
            {
                try
                {
                    // Get the list of routing skills.
                    SkillEntityListing result = await apiInstance.GetRoutingSkillsAsync(pageSize, pageNumber, null, null);

                    foreach (var item in result.Entities)
                    {
                        listSkill.Add(new GC_Skill
                        {
                            skillID = item.Id,
                            skillname = item.Name
                        });
                    }

                    if(result.PageNumber < result.PageCount)
                    {
                        pageNumber++;
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                    }

                }
                catch (Exception e)
                {
                    Debug.Print("Exception when calling Routing.GetRoutingSkills: " + e.Message);
                }
            }

            return listSkill;
        }
    }
}
