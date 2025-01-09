using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Model;


namespace Grabaciones.Services.GenesysCloud
{
    public class SGC_ConversationRecordingmetadata
    {
        public static List<RecordingMetadata> ObtenerConversationRecordingmetadata(string conversationId, DateTime vFechaInicioIntervalo)
        {
            var recordingApi = new RecordingApi();
            List<RecordingMetadata> recordingMetadata = new List<RecordingMetadata>();
            List<RecordingMetadata> recordingMetadataRespuesta = new List<RecordingMetadata>();

            try
            {
                recordingMetadata = recordingApi.GetConversationRecordingmetadata(conversationId);

            }
            catch (Exception ex)
            {

                Console.WriteLine("Error GetConversationRecordingmetadata: " + conversationId +"-"+ex.Message.ToString());

            }

            foreach(var _item in recordingMetadata) { 
                   
                DateTime _startTime = DateTime.Parse(_item.StartTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
                _startTime = _startTime.AddHours(-5).Date;

                // Validar si coinciden en el mismo día
                bool isSameDay = _startTime == vFechaInicioIntervalo.Date;

                if (isSameDay)
                {
                    recordingMetadataRespuesta.Add(_item);
                }


            }

            return recordingMetadataRespuesta;

        }
    }
}
