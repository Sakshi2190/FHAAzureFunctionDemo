using System;
using System.Collections.Generic;
using System.Text;

namespace FHA_Demo
{
    public class PreprocessorResponse
    {
        public string errorMessage { get; set; }
        public bool isSuccess { get; set; }
        public string message { get; set; }

        public PreprocessorResponse(string _errorDetails, bool _result, string _outputFHIRMessage)
        {
            errorMessage = _errorDetails;
            isSuccess = _result;
            message = _outputFHIRMessage;

        }

    }
}
