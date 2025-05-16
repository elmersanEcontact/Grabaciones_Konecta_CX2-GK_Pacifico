namespace Grabaciones.Models
{

    public class EC_Paises
    {
        public string ?pais{ get; set; } = string.Empty;
        public string ?inicial{ get; set; } = string.Empty;
        public int porcentaje{ get; set; }
        public List<string>? Aptitudes { get; set; } = new List<string>();
    }
    
}
