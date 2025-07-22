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
        public bool xmlAudioDescargado { get; set; }
        public string? xmlRutaCompletaAudioGSM { get; set; }
        public string? xmlNombreAudioExcel { get; set; }
        public string? xmlUrlGCAudio { get; set; }
        public string? xmldirectorioFTP{ get; set; }
        public string? xmlArchivolocal { get; set; }
        public string? xmlFileStateRecording { get; set; }


        /// <summary>
        // -------- campos para la informacion de yanbal ---
        public string? IdRecording { get; set; }
        public string? ConversationId { get; set; }
        public string? Direction { get; set; }
        public string? ConversationStartTime { get; set; }
        public string? ConversationEndTime { get; set; }
        public string? Userid { get; set; }
        public string? Agentid { get; set; }
        public string? WrapUpCode { get; set; }
        public long? Duration { get; set; }
        public long? ACW { get; set; }
        public string? ANI { get; set; }
        public string? QueueName { get; set; }
        public string? NameDivision { get; set; }
        public string? IVRSelection { get; set; }
        public long? HoldTime { get; set; }
        public string? Dnis { get; set; }



        #region Datos para pacifico
            public string ? p_nameCampaignCola { get; set; }
            public string? p_empresa { get; set; }
            public string? p_dNICliente { get; set; }
            public string? p_apellidoPaterno { get; set; }
            public string? p_apellidoMaterno { get; set; }
            public string? p_nombres { get; set; }
            public string? p_telefono { get; set; }
            public string? p_fechaDeServicio { get; set; }
            public string? p_horaDeServicio { get; set; }
            public string? p_NroAsesor { get; set; }
            public string? p_Proceso { get; set; }
            public string? p_vdn { get; set; }
            public string? p_skill { get; set; }
            public string? p_ramo { get; set; }
            public string? p_producto { get; set; }
            public string? p_resultado { get; set; }
            public string? p_subResultado { get; set; }
        #endregion



        // datos segun Excel
        public string? eProveedor { get; set; } = "KONECTA";
        public string? eFecha { get; set; }
        public string? eAnio { get; set; }
        public string? eMes { get; set; }
        public string? eDia { get; set; }
        public string? eHora { get; set; }
        public string? eNombreApellidos { get; set; }
        public string? eDniTitular { get; set; }
        public string? eCelularCliente { get; set; }
        public string? eDniAsesor { get; set; }
        public string? eDatosdelLogindelAsesor { get; set; }

        #endregion
    }
}
