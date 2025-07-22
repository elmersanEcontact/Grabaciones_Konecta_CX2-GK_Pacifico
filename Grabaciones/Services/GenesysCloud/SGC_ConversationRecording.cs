using DocumentFormat.OpenXml.Drawing.Diagrams;
using Grabaciones.Models;
using Grabaciones.Services.Econtact;
using Grabaciones.Services.Interface;
using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Client;
using PureCloudPlatform.Client.V2.Model;

using Polly;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using Polly.Retry;

namespace Grabaciones.Services.GenesysCloud
{
    public class SGC_ConversationRecording
    {
        public static async Task<Recording> ObtenerDatosGrabacionMP3(
            string _conversationId, 
            string _recordingId, 
            IConfiguration _config, 
            SimpleRateLimiter rateLimiter)
        {            
            var apiInstance = new RecordingApi();
            var conversationId = _conversationId;  // string | Conversation ID
            var recordingId = _recordingId;  // string | Recording ID
            var formatId = _config.GetValue<string>("ConfiguracionAudio:Formato");   // string | The desired media format. Valid values:WAV,WEBM,WAV_ULAW,OGG_VORBIS,OGG_OPUS,MP3,NONE (optional)  (default to WEBM)
            var fileName = _conversationId + "_" + _recordingId;  // string | the name of the downloaded fileName (optional) 
            var download = true;  // bool? | requesting a download format of the recording. Valid values:true,false (optional)  (default to false)

            Recording result = new Recording();
            int intento = 0;
            
            AsyncRetryPolicy retryPolicy = Polly.Policy
                   .Handle<ApiException>()
                   .Or<HttpRequestException>()
                   .Or<TimeoutException>()
                   .Or<Exception>()
                   .WaitAndRetryAsync(
                       retryCount: 5,
                        sleepDurationProvider: retryAttempt => 
                        {
                            intento = retryAttempt;
                            return TimeSpan.FromSeconds(30); // espera constante
                        },  // siempre espera 10 segundos
                        onRetryAsync: async (exception, timeSpan, retryCount, context) =>
                        {
                            // Aquí se puede registrar el error y el intento actual
                            string tipoError = exception.GetType().Name;
                            string mensajeError = exception.Message;
                            string stackTrace = exception.StackTrace?.Split('\n').FirstOrDefault(); // Línea relevante

                            await EC_EscribirLog.EscribirLogAsync($"[Reintento {retryCount}] Tipo de error: {tipoError}. " +
                                                 $"|Mensaje: {mensajeError}. Stack: {stackTrace}. " +
                                                 $"|Esperando {timeSpan.TotalSeconds} segundos antes del siguiente reintento." +
                                                 $"|ConversationID:{conversationId} - RecordingId:{recordingId}");
                        });

            result = await retryPolicy.ExecuteAsync(async () =>
            {
                await rateLimiter.WaitAsync();

                await EC_EscribirLog.EscribirLogAsync(
                    $"Intento [{intento}] para GetConversationRecordingAsync | ConversationID:{conversationId} - RecordingId:{recordingId}");

                try
                {
                    var response = await apiInstance.GetConversationRecordingAsync(
                        conversationId, recordingId, formatId, null, null, null, download, fileName, null, null);

                    if (response == null)
                        throw new Exception("Respuesta nula de GetConversationRecordingAsync [Se vuelve a intentar]");

                    return response;
                }
                catch (Exception ex)
                {
                    string mensaje = ex.Message ?? "";

                    if (mensaje.Contains("Rate limit exceeded"))
                        await Task.Delay(TimeSpan.FromSeconds(40));
                    else
                        await Task.Delay(TimeSpan.FromSeconds(30));

                    throw;
                }
            });

            return result;
        }
    }
}
