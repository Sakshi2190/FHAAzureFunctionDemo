using CorolarCloud.FHIRTransform.Standard.Types;
using CorolarCloud.HL7ParserApi.Standard.Types;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FHA_Demo
{
    public static class Demo_Function_Orchestrator
    {
        #region Public Variables
        //Variable Assignment
        static int waitInterval = System.Int32.Parse(Environment.GetEnvironmentVariable("OrchestratorStateCheckInterval"));
        static string _corolarAccessTokenURL = Environment.GetEnvironmentVariable("CorolarAccessTokenURL");
        static string _corolarExceptionURL = Environment.GetEnvironmentVariable("CorolarExceptionURL");
        static string _corolarClientID = Environment.GetEnvironmentVariable("CorolarClientID");
        static string _corolarClientSecret = Environment.GetEnvironmentVariable("CorolarClientSecret");
        static string _xsltFhirtoXml = Environment.GetEnvironmentVariable("FHIR_Transform_Xslt");
        static string _xsltFhirtoHl7ORU = Environment.GetEnvironmentVariable("FHIR_To_HL7_Transform_Xslt_ORU");
        static string _hL7SchemaName  = Environment.GetEnvironmentVariable("HL7Schema");
        static string _resourceName = Environment.GetEnvironmentVariable("ResourceName") + "-" + Environment.GetEnvironmentVariable("FunctionAppName");
        static string _preprocessorEndpoint = Environment.GetEnvironmentVariable("PreProcessorEndpoint");
        static string _log = "";

        #endregion

        [FunctionName("FHA_Orchestrator")]
        public static async Task<string> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var _parameters = context.GetInput<Helper>();

            try
            {
                //Retry options for GetAccessToken Activity
               // var retryOptions = new RetryOptions(firstRetryInterval: TimeSpan.FromMilliseconds(waitInterval / 2), maxNumberOfAttempts: 1);

                log.LogInformation($"Start FHA_Orchestrator");
                ///Log Message to Corolar Cloud
                _log = await context.CallActivityAsync<string>("Log_Message_To_Corolar", (_parameters.content, _corolarAccessTokenURL, _corolarClientID, _corolarClientSecret, _resourceName));

                log.LogInformation("Input message logged to Corolar");
                JObject _results = JObject.Parse(_parameters.content);
                var token = (JArray)_results.SelectToken("entry");

                if (token != null)
                {
                    foreach (var item in token)
                    {
                        string _childString = JsonConvert.SerializeObject(item.SelectToken("resource"));

                        ///Convert FHIR Json to FHIR Xml
                        var _fhirXml = await context.CallActivityAsync<FHIRTransformResponse>("FHIR_Transformation", (_childString, _corolarAccessTokenURL, _corolarClientID, _corolarClientSecret, _resourceName, _xsltFhirtoXml));
                        log.LogInformation($"message from FHIR JSON to XML converted successfully");

                        
                        if (_fhirXml.IsSuccess.Value)
                        {
                            
                            ///Call Fhir Preprocessor
                            var _canonicalFhir = await context.CallActivityAsync<PreprocessorResponse>("Fhir_Preprocessor", (_fhirXml.Result, _preprocessorEndpoint));

                            ///Log Message to Corolar Cloud
                            _log = await context.CallActivityAsync<string>("Log_Message_To_Corolar", (_canonicalFhir.message, _corolarAccessTokenURL, _corolarClientID, _corolarClientSecret, _resourceName));


                            if (_canonicalFhir.isSuccess)
                            {
                                log.LogInformation($"Preprocessor execution complete");
                                var _output = await context.CallActivityAsync<FHIRTransformResponse>("FHIR_Transformation", (_canonicalFhir.message.ToString(), _corolarAccessTokenURL, _corolarClientID, _corolarClientSecret, _resourceName, _xsltFhirtoHl7ORU));

                                if (_output.IsSuccess.Value)
                                {
                                    log.LogInformation($"FHIRtoV2 execution completed successfully");
                                    
                                    var _parsedResult = await context.CallActivityAsync<AssemblerResponse>("Corolar_Cloud_Parser", (_output.Result, _corolarAccessTokenURL, _corolarClientID, _corolarClientSecret, _resourceName, _hL7SchemaName));

                                    if (_parsedResult.IsSuccess.Value)
                                    {
                                        log.LogInformation($"HL7 message parsed successfully");
                                        ///Log Message to Corolar Cloud
                                        _log = await context.CallActivityAsync<string>("Log_Message_To_Corolar", (_parsedResult.Result, _corolarAccessTokenURL, _corolarClientID, _corolarClientSecret, _resourceName));

                                        DateTime actTimeOut = context.CurrentUtcDateTime.Add(TimeSpan.FromMilliseconds(waitInterval / 10));

                                        ///Send HL7 message to Service bus queue
                                        Task<bool> actTask = context.CallActivityAsync<bool>("SendOutputToQueue", _parsedResult.Result);
                                        log.LogInformation($"Message sent to output queue");
                                        while (true)
                                        {
                                            //Create Durable Timer
                                            await context.CreateTimer(actTimeOut, CancellationToken.None);

                                            if (actTask.IsCompleted && actTask.IsCompletedSuccessfully)
                                            {
                                                log.LogInformation($"Success in Activity Execution: Last Activity from Demo_Function_Orchestrator Orchestrator Instance Id: {context.InstanceId}");
                                                break;
                                            }
                                            else if (actTask.IsFaulted || actTask.IsCanceled)
                                            {
                                                log.LogError($"Error in Activity Execution: Last Activity from Demo_Function_Orchestrator Orchestrator Instance Id: {context.InstanceId} \r\n Exception: {actTask.Exception.Message} \r\n Inner Exception: {actTask.Exception.InnerException}");
                                                throw new Exception(actTask.Exception.Message);
                                            }


                                        }
                                    }

                                    else
                                    {
                                        CorolarHelper.LogException("Corolar HL7 parser execution Failed, error message - " + _parsedResult.ErrorMessage, _corolarAccessTokenURL, _corolarClientID, _corolarClientSecret, _resourceName, _output.Result);
                                        throw new Exception(_parsedResult.ErrorMessage);
                                    }
                                }
                                else
                                {
                                    CorolarHelper.LogException("FHIRtoV2 conversion Failed, error message - " + _output.ErrorMessage, _corolarAccessTokenURL, _corolarClientID, _corolarClientSecret, _resourceName, _canonicalFhir.message);
                                    throw new Exception(_output.ErrorMessage);
                                }
                            }
                            else
                            {
                                CorolarHelper.LogException("PreProcessor Failed, error message - " +_canonicalFhir.errorMessage, _corolarAccessTokenURL, _corolarClientID, _corolarClientSecret, _resourceName, _fhirXml.Result);
                                throw new Exception(_canonicalFhir.errorMessage);
                            }
                        }
                    
                        else
                        {
                            CorolarHelper.LogException(_fhirXml.ErrorMessage, _corolarAccessTokenURL, _corolarClientID, _corolarClientSecret, _resourceName, _childString);
                            throw new Exception(_fhirXml.ErrorMessage);
                        }
                    }
                }
                else
                {
                    CorolarHelper.LogException("FHIR Json is not in expected format, please correct the message and resend.", _corolarAccessTokenURL, _corolarClientID, _corolarClientSecret, _resourceName, _parameters.content);
                    throw new Exception("FHIR Json is not in expected format, please correct the message and resend.");
                }
            }

            catch(Exception ex)
            {
               log.LogError($"Error in Activity Execution: Last Activity from Demo_Function_Orchestrator Orchestrator Instance Id: {context.InstanceId} \r\n Exception: {ex.Message} \r\n Inner Exception: {ex.InnerException}");
               CorolarHelper.LogException(ex.Message,_corolarAccessTokenURL,_corolarClientID,_corolarClientSecret,_resourceName,_parameters.content);
                throw new Exception(ex.Message);
            }

            return context.InstanceId;


        }
    }
}