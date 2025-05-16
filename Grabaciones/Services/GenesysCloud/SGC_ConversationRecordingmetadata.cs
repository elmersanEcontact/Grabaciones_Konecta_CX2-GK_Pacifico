using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Model;


namespace Grabaciones.Services.GenesysCloud
{
    public class SGC_ConversationRecordingmetadata
    {
        public static async Task<List<RecordingMetadata>> ObtenerConversationRecordingmetadata(string conversationId, DateTime vFechaInicioIntervalo)
        {
            var recordingApi = new RecordingApi();
            List<RecordingMetadata> recordingMetadata = new List<RecordingMetadata>();
            List<RecordingMetadata> recordingMetadataRespuesta = new List<RecordingMetadata>();

            try
            {
                //recordingMetadata = recordingApi.GetConversationRecordingmetadata(conversationId);
                recordingMetadata = await recordingApi.GetConversationRecordingmetadataAsync(conversationId);

            }
            catch (Exception ex)
            {

                Console.WriteLine("Error GetConversationRecordingmetadataAsync: " + conversationId +"-"+ex.Message.ToString());

            }

            #region Validar si la grabacion existe segun la fecha de inicio
            //foreach (var _item in recordingMetadata) { 

            //    DateTime _startTime = DateTime.Parse(_item.StartTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
            //    _startTime = _startTime.AddHours(-5).Date;

            //    // Validar si coinciden en el mismo día
            //    bool isSameDay = _startTime == vFechaInicioIntervalo.Date;

            //    if (isSameDay)
            //    {
            //        recordingMetadataRespuesta.Add(_item);
            //    }
            //}

            foreach (var _item in recordingMetadata)
            {

                DateTime _startTime = DateTime.Parse(_item.StartTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
                _startTime = _startTime.AddHours(-5).Date;

                // Validar si coinciden en el mismo día
                bool isSameDay = _startTime == vFechaInicioIntervalo.Date;
                
                recordingMetadataRespuesta.Add(_item);
               
            }

            #endregion

            return recordingMetadataRespuesta;

        }
    }
}
