using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FHA_Demo
{
    public class FHIR_PreProcessor
    {
        
        [FunctionName("Fhir_Preprocessor")]
        public static async Task<PreprocessorResponse> Preprocessor([ActivityTrigger] IDurableActivityContext context)
        {
            var (_input, _preprocessorEndpoint) = context.GetInput<(string, string)>();

            using (var client = new HttpClient())
            {
                var content = new StringContent(_input, Encoding.UTF8, "application/xml");



                var response = await client.PostAsync(_preprocessorEndpoint, content);

                var _output = await response.Content.ReadAsStringAsync();

                 var _fhirResponse =  FHIR_PreProcessor.ParseJsonToResponse(_output);

                return _fhirResponse;
            }
        }

        public static PreprocessorResponse ParseJsonToResponse(string _jsonOutput)
        {
            dynamic _jsonResponse = JsonConvert.DeserializeObject(_jsonOutput);
            string _error = _jsonResponse.errorMessage;
            bool _result = _jsonResponse.isSuccess;
            string _out = _jsonResponse.message;

            return new PreprocessorResponse(_error, _result, _out);
        }
    }
}
