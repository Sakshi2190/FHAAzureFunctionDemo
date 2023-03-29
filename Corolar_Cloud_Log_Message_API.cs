using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.Text;

namespace FHA_Demo
{
    public class Corolar_Cloud_Log_Message_API
    {
        #region Corolar Log Message API Method

        [FunctionName("Log_Message_To_Corolar")]
        public static string LogMessage([Microsoft.Azure.WebJobs.Extensions.DurableTask.ActivityTrigger] IDurableActivityContext context)
        {
            //Declare Logging Request object
            try
            {
                var (_data, _accessTokenURL, _clientId, _clientSecret, _resourceName, _xsltName) = context.GetInput<(string, string, string, string, string, string)>();
                var _out = CorolarHelper.LogMessage(_data, _accessTokenURL, _clientId, _clientSecret, _resourceName);

                return _out;

            }
            catch (Exception _exceptionMessage)
            {
                return _exceptionMessage.InnerException.ToString();
            }
        }
        #endregion
    }
}
