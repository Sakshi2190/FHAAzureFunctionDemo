using Corolar.Identity.Tokens.Standard;
using CorolarCloud.ExceptionApi.Standard;
using CorolarCloud.ExceptionApi.Standard.Types;
using CorolarCloud.LoggingAPI.Standard;
using CorolarCloud.LoggingAPI.Standard.Types;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FHA_Demo
{
    public class CorolarHelper
    {
        #region Corolar Log Message API Method

       public static string LogMessage(string _data,string _accessTokenURL, string _clientId, string _clientSecret, string _resourceName)
        {
            //Declare Logging Request object
            try
            {
                            
                string _corolaraccesstoken = CorolarHelper.GenerateAccessToken(_accessTokenURL, _clientId, _clientSecret); 
                
                string _baseURL = Environment.GetEnvironmentVariable("LoggingURL");
                
                LogRequest _logRequest = new LogRequest(_baseURL, _corolaraccesstoken, _data, _resourceName);

                //Make call to the API using API Wrapper
                var _result = LogHelper.Log(_logRequest);

                return _result.IsSuccess.Value.ToString();


            }
            catch (Exception _exceptionMessage)
            {
               return CorolarHelper.LogException(_exceptionMessage.Message, _accessTokenURL, _clientId, _clientSecret, _resourceName, _data);

            }
        }
        #endregion

        #region Corolar Log exception Message API Method
        public static string LogException(string _exceptionMessage, string _accessTokenURL, string _clientId, string _clientSecret, string _resourceName, string _associatedMessage)
        {
            //Declare Exception request object
            try
            {
                string _corolaraccesstoken = CorolarHelper.GenerateAccessToken(_accessTokenURL, _clientId, _clientSecret);

                string _baseURL = Environment.GetEnvironmentVariable("ExceptionURL");

                ExceptionRequestSeverity _serverity = ExceptionRequestSeverity._1;
                int _eventCode = 9902;

                ExceptionRequest _exceptionRequest = new ExceptionRequest(_baseURL, _corolaraccesstoken, _serverity, _eventCode, _exceptionMessage, _resourceName);
                _exceptionRequest.AssociatedMessage = _associatedMessage;

                //Make call to the API using API Wrapper
                var _result = ExceptionHelper.Log(_exceptionRequest);

                return _result.IsSuccess.Value.ToString();

            }
            catch (Exception ex)
            {
                return ex.InnerException.ToString();
            }
        }
        #endregion

        #region Corolar AccessToken Generator API Method
        /// <summary>
        /// Generate JWT token for Corolar API based on client id & secret
        /// </summary>
        /// <param name="accessTokenBaseUri"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        public static string GenerateAccessToken(string accessTokenBaseUri, string clientId, string clientSecret)
        {
            try
            {
                //Fetch Corolar Access token
                TokenService ts = new TokenService(accessTokenBaseUri);

                //Invoke token service
                AccessTokenResponse tokenRes = ts.GetCorolarStsToken(new CorolarStsAccessTokenRequest { ClientId = clientId, Secret = clientSecret });

                if (!(bool)tokenRes.IsSuccess)
                {
                    //Corolar Nuget Package V5
                    throw new Exception(tokenRes.Error);
                }
                else
                {
                    return tokenRes.AccessToken;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        #endregion

    }
}
