using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FHA_Demo
{
   public class ServiceBusHelper
    {
        public static async Task<bool> SendMessageToTopic(SBQueueMessage msgObj)
        {
            try
            {
                var queueClient = new QueueClient(msgObj.ConnectionString, msgObj.QueueName);

                var message = new Message(Encoding.UTF8.GetBytes(msgObj.Content));
               // message.SessionId = msgObj.SessionID;

                //if (!msgObj.UserProperties.Equals(null) && msgObj.UserProperties.Count > 0)
                //{
                //    foreach (KeyValuePair<string, string> entry in msgObj.UserProperties)
                //    {
                //        //Add user properties
                //        message.UserProperties.Add(entry.Key, entry.Value);
                //    }
                //}

                //To Queue
                await queueClient.SendAsync(message);
                await queueClient.CloseAsync();
            }
            catch (Exception SB)
            {
                throw SB;
            }
            return true;
        }

    }
}
