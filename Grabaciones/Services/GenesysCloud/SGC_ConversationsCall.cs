using Grabaciones.Services.Econtact;
using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Model;

namespace Grabaciones.Services.GenesysCloud
{
    public class SGC_ConversationsCall
    {
        public static async Task<CallConversation> ObtenerCallConversation(string conversationID)
        {
            var conversationsApi = new ConversationsApi();
            CallConversation resultConversation = new CallConversation();
            try
            {
                resultConversation = await conversationsApi.GetConversationsCallAsync(conversationID);
            }
            catch (Exception ex )
            {
                
                Console.WriteLine("Error en ObtenerCallConversation: " + ex.Message.ToString());
                EC_EscribirLog.EscribirLog($"Error en ObtenerCallConversation: \" {ex.Message.ToString()}");
                throw;
            }
            return resultConversation;
        
        }
    }
}
