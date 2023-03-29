using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FHA_Demo
{
   public static class SendOutputToQueue
    {
        [FunctionName("SendOutputToQueue")]
        public static async Task<bool> SendOutputToQ([ActivityTrigger] IDurableActivityContext context, ILogger log)

        {
            bool status = false;            
            try
            {
                log.LogInformation("Start SendOutputToQueue_Activity");

                SBQueueMessage _queueMessage = new SBQueueMessage();


                // Construct SB message with properties promoted
                _queueMessage.ConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnection");
                _queueMessage.Content = context.GetInput<string>();
                
                // sBTopicMessage.SessionID = actMsgProps.SessionID;
                _queueMessage.QueueName = Environment.GetEnvironmentVariable("FHA_SendHL7Queue");

                status = await ServiceBusHelper.SendMessageToTopic(_queueMessage);

                log.LogInformation("End a_sendacktriggertosb");

            }
            catch (Exception SB)
            {
                throw new Exception("An error occurred when sending Canonical XML message to IntegrationServiceBus. " + SB.Message);
            }

            return status;

        }
    }
}
