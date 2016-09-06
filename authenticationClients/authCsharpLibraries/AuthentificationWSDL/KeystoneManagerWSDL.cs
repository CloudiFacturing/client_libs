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
        static private Thread KeystoneManagerServerTestThread = null;

        public KeystoneManagerWSDL()
        {
            KeystoneManagerServerTestThread = new Thread(testServer);
            KeystoneManagerServerTestThread.Start();
        }

        
        public static KeystoneManagerWSDL GetKeystoneManagerWSDL(string projectName, string tokenId)
        {
            switch (KeystoneManagerWSDL.ParseProjectName(projectName))
            {
                case Projects.CloudFlow:
                    return new AuthentificationWSDL.CFAuth.KeystoneManagerWSDL_CF(tokenId, projectName);
                case Projects.CAxMan:
                default:
                    return new AuthentificationWSDL.CMAuth.KeystoneManagerWSDL_CM(tokenId, projectName);
            }
        }

        public static KeystoneManagerWSDL GetKeystoneManagerWSDL(string projectName, string username, string password)
        {
            switch (KeystoneManagerWSDL.ParseProjectName(projectName))
            {
                case Projects.CloudFlow:
                    return new AuthentificationWSDL.CFAuth.KeystoneManagerWSDL_CF(username, password, projectName);
                case Projects.CAxMan:
                default:
                    return new AuthentificationWSDL.CMAuth.KeystoneManagerWSDL_CM(username, password, projectName);
            }
        }

        public static Projects ParseProjectName(string projectName)
        {
            Projects project = Projects.CAxMan;
            if (!Enum.TryParse(projectName, true, out project))
            {
                project = Projects.CloudFlow; //if the parse does not work, thats mean it is called by an older CloudFlow application
            }

            return project;
        }

        /// <summary>
        /// Check if the KSM is available to give a token
        /// </summary>
        /// <returns></returns>
        private void testServer()
        {
            while (KeystoneManagerServerTestThread.ThreadState == ThreadState.Running)
            {
                ServicePointManager.ServerCertificateValidationCallback +=
                            delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                            {
                                return true;
                            };
                try
                {
                    Uri uriResult;

                    string url = this.KeystoneManagerURI + "?wsdl";
                    Uri.TryCreate(url, UriKind.Absolute, out uriResult);

                    //Creating the HttpWebRequest
                    HttpWebRequest request = WebRequest.Create(uriResult) as HttpWebRequest;
                    request.Timeout = 14000;
                    //Setting the Request method HEAD, you can also use GET too.
                    request.Method = "HEAD";
                    //Getting the Web Response.
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
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
            if (KeystoneManagerServerTestThread != null)
            {
                KeystoneManagerServerTestThread.Abort();
            }
        }
    }

}
