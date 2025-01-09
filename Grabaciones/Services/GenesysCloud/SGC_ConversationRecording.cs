using DocumentFormat.OpenXml.Drawing.Diagrams;
using Grabaciones.Models;
using Grabaciones.Services.Econtact;
using Grabaciones.Services.Interface;
using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Client;
using PureCloudPlatform.Client.V2.Model;

namespace Grabaciones.Services.GenesysCloud
{
    public class SGC_ConversationRecording
    {
        public static Recording ObtenerDatosGrabacionMP3(string _conversationId, string _recordingId, IConfiguration _config)
        {            
            var apiInstance = new RecordingApi();
            var conversationId = _conversationId;  // string | Conversation ID
            var recordingId = _recordingId;  // string | Recording ID
            var formatId = _config.GetValue<string>("ConfiguracionAudio:Formato");   // string | The desired media format. Valid values:WAV,WEBM,WAV_ULAW,OGG_VORBIS,OGG_OPUS,MP3,NONE (optional)  (default to WEBM)
            var fileName = _conversationId + "_" + _recordingId;  // string | the name of the downloaded fileName (optional) 
          //  var mediaFormats = new List<string> { formatId }; // List<string> | All acceptable media formats. Overrides formatId. Valid values:WAV,WEBM,WAV_ULAW,OGG_VORBIS,OGG_OPUS,MP3 (optional) 
            var download = true;  // bool? | requesting a download format of the recording. Valid values:true,false (optional)  (default to false)

            //mediaFormats.Add(formatId);
            Recording result = new Recording();
            int minIntentos = 0;
            int maxIntentos = 3;
            int delayMilliseconds = 5000;


            try
            {
                //Recording resultado = null;
                // Gets a specific recording.
                result = apiInstance.GetConversationRecording(conversationId, recordingId, formatId, null, null, null, download, fileName, null, null);

                //result = resultado is null ? Task.Delay(5000); : resultado;
                if (result is null)
                {
                    Thread.Sleep(delayMilliseconds);
                    result = ObtenerDatosGrabacionMP3(_conversationId, _recordingId, _config);
                }
            }
            catch (ApiException ex)
            {
                if (ex.ErrorCode == 504)
                {

                    EC_EscribirLog.EscribirLog("Error en Metodo: ObtenerDatosGrabacionMP3 por timeOut" + ex.Message);
                    Thread.Sleep(delayMilliseconds);
                    result = ObtenerDatosGrabacionMP3(_conversationId, _recordingId, _config);

                }

                Thread.Sleep(delayMilliseconds);
                result = ObtenerDatosGrabacionMP3(_conversationId, _recordingId, _config);
                EC_EscribirLog.EscribirLog("Error en: Metodo: ObtenerDatosGrabacionMP3" + ex.Message);
            }

            return result;

        }
    }
}
