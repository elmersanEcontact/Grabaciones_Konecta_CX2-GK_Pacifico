using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Client;
using PureCloudPlatform.Client.V2.Extensions;

namespace Grabaciones.Services.GenesysCloud
{
    public class SGC_Autentication
    {
        private readonly IConfiguration _config;
        private HttpClient _httpClient;

        public SGC_Autentication(IConfiguration configuration, HttpClient httpClient)
        {
            _config = configuration;
            _httpClient = httpClient;

        }
        public static void Autentication(IConfiguration config)
        {
            string clientId = config.GetValue<string>("GenesysCloud:ClientID");
            string clientSecret = config.GetValue<string>("GenesysCloud:ClientSecret");

            //Set Region
            PureCloudRegionHosts region = PureCloudRegionHosts.us_east_1;
            PureCloudPlatform.Client.V2.Client.Configuration.Default.ApiClient.setBasePath(region);


            // Configure SDK Settings
            var accessTokenInfo = PureCloudPlatform.Client.V2.Client.Configuration.Default.ApiClient.PostToken(clientId, clientSecret);
            PureCloudPlatform.Client.V2.Client.Configuration.Default.AccessToken = accessTokenInfo.AccessToken;
        }
    }
}
