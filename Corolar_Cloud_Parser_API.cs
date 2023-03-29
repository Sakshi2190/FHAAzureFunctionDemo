using CorolarCloud.HL7ParserApi.Standard;
using CorolarCloud.HL7ParserApi.Standard.Types;
using FHA_Demo;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;

namespace Excelleris.Int.On.Out.Olis.Functions
{
    public class Corolar_Cloud_Parser_API
    {
        [FunctionName("Corolar_Cloud_Parser")]
        public static AssemblerResponse Assembler([ActivityTrigger] IDurableActivityContext context)
        {
            var (_data, _accessTokenURL, _clientId, _clientSecret, _resourceName, _schemaName) = context.GetInput<(string, string, string, string, string, string)>();


            string _corolaraccesstoken = CorolarHelper.GenerateAccessToken(_accessTokenURL, _clientId, _clientSecret);
            try
            {
                ///Declare parser request object
                String _baseURL = Environment.GetEnvironmentVariable("Corolar_ParserUrl");
                
                AssemblerRequest _assemblerRequest = new AssemblerRequest(_baseURL, _corolaraccesstoken, _data, _resourceName);
                _assemblerRequest.Config.UseSpecificXmlSchema = true;
                _assemblerRequest.Config.SpecificXmlSchemaName = _schemaName;
                _assemblerRequest.LogMessage = false;

                ///Make call to the API using API Wrapper
                var result = HL7Parser.Assemble(_assemblerRequest);

                ///Result  Response
                return result;
            }
            catch (Exception _exceptionMessage)
            {
                AssemblerResponse _assemblerResponse = new AssemblerResponse();
                _assemblerResponse.ErrorMessage = _exceptionMessage.InnerException.ToString();
                return _assemblerResponse;
            }
        }
    }
}
