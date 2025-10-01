using Newtonsoft.Json;

namespace Grabaciones.Models
{
    public class EC_ConfiguracionTransformacionXML
    {
        [JsonProperty("OUTBOUND")]
        public OutboundConfig? Outbound { get; set; }

        [JsonProperty("INBOUND")]
        public InboundConfig? Inbound { get; set; }
    }

    // Equivalencias de OUTBOUND
    public class OutboundConfig
    {
        [JsonProperty("chOptyTipifProducto_c")]
        public Dictionary<string, string> ? ChOptyTipifProducto { get; set; }

        [JsonProperty("tOptyTipifSubResultado_c")]
        public Dictionary<string, string> ? TOptyTipifSubResultado { get; set; }
    }

    // Equivalencias de INBOUND
    public class InboundConfig
    {
        [JsonProperty("tSRLineaNegocio_c")]
        public Dictionary<string, LineaNegocioItem> ? TSRLineaNegocio { get; set; }

        [JsonProperty("tSRDisposicion_c")]
        public Dictionary<string, string> ? TSRDisposicion { get; set; }
    }

    // Modelo para los items de línea de negocio
    public class LineaNegocioItem
    {
        [JsonProperty("ramo")]
        public string ? Ramo { get; set; }

        [JsonProperty("producto")]
        public string ? Producto { get; set; }
    }
}
