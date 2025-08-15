using System.Text.Json.Serialization;

namespace Grabaciones.Models
{
    public class EC_ConfiguracionApisPacfico
    {
        public TokenConfig Token { get; set; }
        public ObtenerDatosConfig ObtenerDatos { get; set; }
    }

    public class TokenConfig
    {
        public string Url { get; set; }
        [JsonPropertyName("Ocp-Apim-Subscription-Key")]
        public string OcpApimSubscriptionKey { get; set; }
        [JsonPropertyName("clientcredential")]
        public string ClientCredential { get; set; }
        [JsonPropertyName("resource")]
        public string Resource { get; set; }
    }

    public class ObtenerDatosConfig
    {
        public string UrlDatos { get; set; }
        [JsonPropertyName("Ocp-Apim-Subscription-Key")]
        public string OcpApimSubscriptionKey { get; set; }
    }
}
