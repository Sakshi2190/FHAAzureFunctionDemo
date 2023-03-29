using CorolarCloud.FHIRTransform.Standard;
using CorolarCloud.FHIRTransform.Standard.Types;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.Text;

namespace FHA_Demo
{
    public class Corolar_FhirTransformation_API
    {
        [FunctionName("FHIR_Transformation")]
        public static FHIRTransformResponse FHIRTransformAPIWrapper([ActivityTrigger] IDurableActivityContext context)
        {
            try
            {
                ///Declare Transformation Request object
                string _baseURL = Environment.GetEnvironmentVariable("FHIRTransformationURL");
                string _outputresourceformat = Environment.GetEnvironmentVariable("OutputFormat"); ;


                var (_data, _accessTokenURL, _clientId, _clientSecret, _resourceName,_xsltName) = context.GetInput<(string, string, string, string, string, string)>();

                string _corolaraccesstoken = CorolarHelper.GenerateAccessToken(_accessTokenURL, _clientId, _clientSecret);

                FHIRTransformRequest fhirtransformationRequest = new FHIRTransformRequest(_baseURL, _corolaraccesstoken, _data, _xsltName, _outputresourceformat, _resourceName);
                fhirtransformationRequest.LogMessages = false;

                ///Make call to the API using API Wrapper
                var _result = FHIRTransformHelper.Transform(fhirtransformationRequest);


                return _result;


            }
            catch (Exception _exceptionMessage)
            {
                FHIRTransformResponse _response = new FHIRTransformResponse();
                _response.ErrorMessage = _exceptionMessage.InnerException.ToString();
                _response.IsSuccess = false;
                return _response;
            }
        }
    }
}
