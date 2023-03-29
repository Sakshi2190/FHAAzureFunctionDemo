using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs.ServiceBus;

namespace FHA_Demo
{
    public class FHA_Orchestrator_Client
    {
        #region Public Variables 
        //Variable Assignment
        static int _waitInterval = System.Int32.Parse(Environment.GetEnvironmentVariable("OrchestratorStateCheckInterval"));
        const string _orchestrator = "FHA_Orchestrator";
        #endregion

        #region Orchestration Trigger
        [FunctionName("FHA_Orchestator_Client")]
        public static async Task Run([ServiceBusTrigger("%FHA_SBQueue%", Connection = "AzureWebJobsServiceBus")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageReceiver,
             string lockToken,

            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {

            try
            {
                log.LogInformation($"FHA client service Started");

            Helper _inputVariables = new Helper
            {
                content = Encoding.UTF8.GetString(message.Body),
                sessionID = message.SessionId
            };

             if (string.IsNullOrWhiteSpace(_inputVariables.content)) await messageReceiver.DeadLetterMessageAsync(message, "Message content is empty.", "Message content is empty.");

             if (_inputVariables.content != null)
                {
                    
                    string instanceID = await starter.StartNewAsync(_orchestrator, null, _inputVariables);
                    await messageReceiver.CompleteMessageAsync(message);
                    log.LogInformation($"FHA client Started with ID: {instanceID}");
                }
                else
                {
                    await messageReceiver.DeadLetterMessageAsync(message, "Something is wrong with message object", "Something is wrong with message object");
                }
            }
            catch (Exception ex)
            {
                string exceptionText = "FHA Demo Orchestrator Failed with error : " + ex.ToString();
                log.LogError(exceptionText);
                await messageReceiver.DeadLetterMessageAsync(message, exceptionText, exceptionText);
            }
        }
        #endregion

    }


    /// Old code

    ////Do not start next orchestrator instance until current instance finishes
    //var status = await starter.GetStatusAsync(instanceID);

    //while (true)
    //{
    //    //Wait for X milisec
    //    await Task.Delay(_waitInterval);

    //    status = await starter.GetStatusAsync(instanceID);
    //    if (status.RuntimeStatus == OrchestrationRuntimeStatus.Failed
    //        || status.RuntimeStatus == OrchestrationRuntimeStatus.Terminated
    //        || status.RuntimeStatus == OrchestrationRuntimeStatus.Canceled)
    //    {
    //        log.LogError($"FHA orchestrator executed with an exception for Session Id: {message.SessionId} Instance Id: {instanceID} Exception: {status.Output}");
    //        break;
    //    }
    //    else if (status.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
    //    {
    //        log.LogInformation($"FHA orchestrator executed with success for Session Id: {message.SessionId} Instance Id: {instanceID}");
    //        break;
    //    }
    //}
}
