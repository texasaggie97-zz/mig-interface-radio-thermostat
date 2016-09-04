/*
    This file is part of HomeGenie Project source code.

    HomeGenie is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    HomeGenie is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with HomeGenie.  If not, see <http://www.gnu.org/licenses/>.  
*/

/*
 *     Author: Generoso Martello <gene@homegenie.it>
 *     Project Homepage: http://homegenie.it
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;

using System.Net.NetworkInformation;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MIG.Interfaces.HomeAutomation
{
    /// <summary>
    /// Net Helper class.\n
    /// Class instance accessor: **Net**
    /// </summary>
    [Serializable]
    public class NetHelper
    {

        private string webServiceUrl = "";
        private string method = "post";
        private string putData = "";
        private NameValueCollection customHeaders = new NameValueCollection();
        private NetworkCredential networkCredential = null;

        //private object httpSyncLock = new object();

        private bool defaultCredentials;

        public NetHelper()
        {
            // TODO: SSL connection certificate validation:
            // TODO: this is just an hack to fix certificate issues on mono < 4.0,
            // TODO: not meant to be used in production enviroment
            //ServicePointManager.ServerCertificateValidationCallback = Validator;
        }

        // TODO: this is just an hack not meant to be used in production enviroment
        /*
        public static bool Validator(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors
        )
        {
            return true;
        }
        */

        #region HTTP client

        /// <summary>
        /// Set the web service URL to call. 
        /// </summary>
        /// <returns>NetHelper.</returns>
        /// <param name="serviceUrl">Service URL.</param>
        /// <remarks />
        /// <example>
        /// Example:
        /// <code>
        /// var iplocation = Net.WebService("http://freegeoip.net/json/").GetData(); 
        /// Program.Notify("IP to Location", iplocation.city);
        /// </code>
        /// </example>
        public NetHelper WebService(string serviceUrl)
        {
            this.method = "";
            this.customHeaders.Clear();
            this.webServiceUrl = serviceUrl;
            return this;
        }

        /// <summary>
        /// Sends the specified string data using the PUT method.
        /// </summary>
        /// <param name="data">Data to send.</param>
        public NetHelper Put(string data)
        {
            this.method = "PUT";
            this.putData = data;
            return this;
        }

        /// <summary>
        /// Sends the specified string data using the POST method.
        /// </summary>
        /// <param name="data">String containing post data fields and values in the form field1=value1&filed2=value2&...&fieldn=valuen.</param>
        public NetHelper Post(string data)
        {
            this.method = "POST";
            this.putData = data;
            return this;
        }

        /// <summary>
        /// Adds the specified HTTP header to the HTTP request.
        /// </summary>
        /// <returns>NetHelper.</returns>
        /// <param name="name">Header name.</param>
        /// <param name="value">Header value.</param>
        public NetHelper AddHeader(string name, string value)
        {
            customHeaders.Add(name, value);
            return this;
        }

        /// <summary>
        /// Call the web service url.
        /// </summary>
        /// <returns>String containing the server response.</returns>
        public string Call()
        {
            string returnvalue = "";
            //lock(httpSyncLock)
            using (var webClient = new WebClient())
            {
                try
                {
                    webClient.Encoding = Encoding.UTF8;
                    if (this.networkCredential != null)
                    {
                        webClient.Credentials = networkCredential;
                    }
                    else if (this.defaultCredentials)
                    {
                        webClient.UseDefaultCredentials = true;
                    }
                    webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0)");
                    if (customHeaders.Count > 0)
                        webClient.Headers.Add(customHeaders);
                    if (this.method == "")
                    {
                        returnvalue = webClient.DownloadString(this.webServiceUrl);
                    }
                    else
                    {
                        byte[] data = Encoding.UTF8.GetBytes(this.putData);
                        byte[] responsebytes = webClient.UploadData(this.webServiceUrl, this.method, data);
                        returnvalue = Encoding.UTF8.GetString(responsebytes);
                    }
                }
                catch (Exception ex)
                {
                    MigService.Log.Error(ex);
                }
                finally
                {
                    webClient.Dispose();
                }
            }
            return returnvalue;
        }

        // TODO: deprecate this (Program.RunAsyncTask can do the trick)
        [Obsolete("use 'Program.RunAsyncTask' instead")]
        public void CallAsync()
        {
            var t = new Thread(() =>
            {
                this.Call();
            });
            t.Start();
        }

        /// <summary>
        /// Call the web service url and returns the server response.
        /// </summary>
        /// <returns>The returned value can be a simple string or an object containing all fields mapped from the JSON/XML response (see Net.WebService example).</returns>
        public dynamic GetData()
        {
            dynamic returnValue = null;
            string response = this.Call();
            string json = "[" + response + "]";
            try
            {
                returnValue = (JsonConvert.DeserializeObject(json) as JArray)[0];
            }
            catch
            {
                returnValue = response;
            }
            return returnValue;
        }

        /// <summary>
        /// Call the web service url and returns the server response as binary data.
        /// </summary>
        /// <returns>Byte array containing the raw server response.</returns>
        public byte[] GetBytes()
        {
            byte[] responseBytes = null;
            //lock(httpSyncLock)
            using (var webClient = new WebClient())
            {
                try
                {
                    if (this.networkCredential != null)
                    {
                        webClient.Credentials = networkCredential;
                    }
                    else if (this.defaultCredentials)
                    {
                        webClient.UseDefaultCredentials = true;
                    }
                    webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0)");
                    if (customHeaders.Count > 0)
                        webClient.Headers.Add(customHeaders);
                    responseBytes = webClient.DownloadData(this.webServiceUrl);
                }
                catch (Exception ex)
                {
                    MigService.Log.Error(ex);
                }
                finally
                {
                    webClient.Dispose();
                }
            }
            return responseBytes;
        }

        #endregion


        #region Ping client

        /// <summary>
        /// Ping the specified remote host.
        /// </summary>
        /// <param name="remoteAddress">IP or DNS address.</param>
        public bool Ping(string remoteAddress)
        {
            bool success = false;
            using (var pingClient = new Ping())
            {
                var options = new PingOptions();
                //
                // Use the default Ttl value which is 128,
                // but change the fragmentation behavior.
                options.DontFragment = true;
                //
                // Create a buffer of 32 bytes of data to be transmitted.
                string data = "01010101010101010101010101010101";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 1000;
                var reply = pingClient.Send(remoteAddress, timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                {
                    success = true;
                }
            }
            return success;
        }

        #endregion

        /// Use provided credentials when connecting.
        /// </summary>
        /// <returns>NetHelper.</returns>
        /// <param name="user">Username.</param>
        /// <param name="pass">Password.</param>
        public NetHelper WithCredentials(string user, string pass)
        {
            this.networkCredential = new NetworkCredential(user, pass);
            return this;
        }

        public NetHelper WithDefaultCredentials()
        {
            this.defaultCredentials = true;
            return this;
        }

        public void Reset()
        {
            this.webServiceUrl = "";
            this.networkCredential = null;
        }

        private class WebClient : System.Net.WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                // Disable Keep-Alive (this lead to poor performance, so let's keep it disabled by default)
                //if (w is HttpWebRequest)
                //{
                //    (w as HttpWebRequest).KeepAlive = false;
                //}
                // WebClient default timeout set to 10 seconds
                w.Timeout = 10 * 1000;
                return w;
            }
        }

    }

    public delegate void AsyncFunction();

    public class Utility
    {
        internal static Thread RunAsyncTask(AsyncFunction functionBlock)
        {
            var asyncTask = new Thread(() =>
            {
                try
                {
                    functionBlock();
                }
                catch (Exception ex)
                {
                    MigService.Log.Error(ex);
                }
            });
            asyncTask.Start();
            return asyncTask;
        }
    }

}
