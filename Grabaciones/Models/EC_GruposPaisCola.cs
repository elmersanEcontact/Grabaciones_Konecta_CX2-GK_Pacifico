namespace Grabaciones.Models
{
    public class EC_GruposPaisCola
    {
        public string ?pais { get; set; }
        public List<GC_Queue>? colas { get; set; }
        public List<GC_Skill>? aptitudes { get; set; }
        public int cantidadGrabaciones { get; set; }

    }
}
