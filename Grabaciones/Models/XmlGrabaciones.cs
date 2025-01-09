namespace Grabaciones.Models
{
    public class XmlGrabaciones
    {

        #region Datos para el XML
        public string? xmlRecordingID { get; set; }
        public string? conversationID { get; set; }
        public string? xmlempresa { get; set; }
        public string? xmlOrganization { get; set; }
        public string? xmlDNICliente { get; set; }
        public string? xmlNombresCliente { get; set; }
        public string? xmlFecha_Inicio { get; set; }
        public string? xmlHora_Inicio { get; set; }
        public string? xmlFecha_Fin { get; set; }
        public string? xmlHora_Fin { get; set; }
        public string? xmlDuracion { get; set; }
        public string? xmlAgenteSkill { get; set; }
        public string? xmlAgenteLogin { get; set; }
        public string? xmlTipoLlamada { get; set; }
        public string? xmlTipificacion { get; set; }
        public string? xmlRutadeAudio { get; set; }
        public string? xmlNomenclaturaAudio { get; set; }
        public string? xmlRutaCompletaAudioMP3 { get; set; }
        public string? xmlRutaCompletaAudioGSM { get; set; }
        public string? xmlNombreAudioExcel { get; set; }
        public string? xmlUrlGCAudio { get; set; }
        public string? xmldirectorioFTP{ get; set; }
        public string? xmlArchivolocal { get; set; }





        // datos segun Excel
        public string? eProveedor { get; set; } = "KONECTA";
        public string? eProducto { get; set; } = "SOAT";
        public string? eParteDisco { get; set; }
        public string? eCanal { get; set; } = "TELEMARKETING";
        public string? eSponsor { get; set; } = "RIMAC";
        public string? eFecha { get; set; }
        public string? eAnio { get; set; }
        public string? eMes { get; set; }
        public string? eDia { get; set; }
        public string? eHora { get; set; }
        public string? eNombreApellidos { get; set; }
        public string? eDniTitular { get; set; }
        public string? ePlaca { get; set; }
        public string? ePlan { get; set; }
        public string? ePrima { get; set; }
        public string? eCelularCliente { get; set; }
        public string? eFijoCliente { get; set; }
        public string? eDniAsesor { get; set; }
        public string? eNombreApellidosAsesor { get; set; }
        public string? eCodigo { get; set; }
        public string? eEtiqueta { get; set; }
        public string? eParteGrabacion { get; set; }
        public string? eDatosdelLogindelAsesor { get; set; }

        #endregion
    }
}
