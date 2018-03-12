using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuthentificationWSDL
{
    abstract public class KeystoneManagerWSDL
    {
        abstract public string KEYSTONE_PROJECT { get; }

        abstract public Projects Project { get; }
        abstract public string ProjectName { get; }
        abstract public bool IsValid { get; }
        abstract public bool HasToken { get; }
        abstract public bool IsServerAvailable { get; }

        abstract public string KeystoneManagerURI { get; }

        public string Username { get; internal set; }
        public string Password { get; internal set; }
        public string TokenId { get; internal set; }


        internal static bool isServerAvailable = false;
        private Thread keystoneManagerServerTestThread = null;
        private static bool isThreadAlive = true;

        public KeystoneManagerWSDL()
        {
            this.keystoneManagerServerTestThread = new Thread(testServer);
            this.keystoneManagerServerTestThread.Start();
        }


        public static KeystoneManagerWSDL GetKeystoneManagerWSDL(string projectName, string tokenId)
        {
            switch (KeystoneManagerWSDL.ParseProjectName(projectName))
            {
                case Projects.CAxMan:
                default:
                    return new AuthentificationWSDL.CMAuth.KeystoneManagerWSDL_CM(tokenId, projectName);
            }
        }

        public static KeystoneManagerWSDL GetKeystoneManagerWSDL(string projectName, string username, string password)
        {
            switch (KeystoneManagerWSDL.ParseProjectName(projectName))
            {
                case Projects.CAxMan:
                default:
                    return new AuthentificationWSDL.CMAuth.KeystoneManagerWSDL_CM(username, password, projectName);
            }
        }

        public static Projects ParseProjectName(string projectName)
        {
            Projects project = Projects.CAxMan;

            return project;
        }

        /// <summary>
        /// Check if the KSM is available to give a token
        /// </summary>
        /// <returns></returns>
        private void testServer()
        {
            Uri uriResult;
            string url;
            HttpWebRequest request;
            HttpWebResponse response;

            // Build the requests

            while (true)
            {
                try
                {
                    //Get the URL
                    url = this.KeystoneManagerURI + "?wsdl";
                    Uri.TryCreate(url, UriKind.Absolute, out uriResult);

                    //Creating the HttpWebRequest
                    request = WebRequest.Create(uriResult) as HttpWebRequest;
                    request.Timeout = 14000;

                    //Setting the Request method HEAD, you can also use GET too.
                    request.Method = "HEAD";

                    //Set the Event for https encryption
                    ServicePointManager.ServerCertificateValidationCallback +=
                                delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                                {
                                    return true;
                                };
                }
                catch { continue; }
                Thread.Sleep(1000);

                break;
            }

            while (KeystoneManagerWSDL.isThreadAlive)
            {
                try
                {
                    //Getting the Web Response.
                    response = request.GetResponse() as HttpWebResponse;
                    //Set TRUE if the Status code == 200
                    response.Close();
                    KeystoneManagerWSDL.isServerAvailable = (response.StatusCode == HttpStatusCode.OK);
                }
                catch
                {
                    //Any exception will set FALSE.
                    KeystoneManagerWSDL.isServerAvailable = false;
                }
                /*
                
                KeystoneManagerWSDL.isServerAvailable = Uri.TryCreate(this.KeystoneManagerURI, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                //*/
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Disconnect from the KSM
        /// </summary>
        public void Disconnect()
        {
            this.Password = "";
            this.TokenId = null;
        }

        public void Dispose()
        {
            KeystoneManagerWSDL.isThreadAlive = false;            
        }
    }

}
