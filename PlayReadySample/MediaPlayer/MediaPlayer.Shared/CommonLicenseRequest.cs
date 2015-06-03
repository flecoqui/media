﻿    //*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MediaPlayer
{
    /// <summary>
    /// Common Class used to retrieve the PlayReady License.
    /// Same source code for Windows Store and Windows Phone Application based on HttpClient class.
    /// </summary>
    public class CommonLicenseRequest
    {
        private string _lastErrorMessage;
        public string GetLastErrorMessage()
        {
            return _lastErrorMessage;
        }
        public CommonLicenseRequest()
        {
            _lastErrorMessage = string.Empty;
        }
        /// <summary>
        /// Invoked to acquire the PlayReady license.
        /// </summary>
        /// <param name="licenseServerUri">License Server URI to retrieve the PlayReady license.</param>
        /// <param name="httpRequestContent">HttpContent including the Challenge transmitted to the PlayReady server.</param>
        public async virtual Task<HttpContent> AcquireLicense(Uri licenseServerUri, HttpContent httpRequestContent)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("msprdrm_server_redirect_compat", "false");
                httpClient.DefaultRequestHeaders.Add("msprdrm_server_exception_compat", "false");

                HttpResponseMessage response = await httpClient.PostAsync(licenseServerUri, httpRequestContent);
                response.EnsureSuccessStatusCode();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _lastErrorMessage = string.Empty;
                    return response.Content;
                }
                else
                {
                    _lastErrorMessage = "AcquireLicense - Http Response Status Code: " + response.StatusCode.ToString();
                }
            }
            catch (Exception exception)
            {
                _lastErrorMessage = exception.Message;
                return null;
            }
            return null;
        }
    }
}
