using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AuthentificationWR
{
    public class KeystoneManager
    {
        #region constant variables
        public const string KEYSTONE_URL = @"https://openstack.arctur.si/keystone/v2.0/tokens/";
        public const string KEYSTONE_TENANT = "cloudflow";
        #endregion


        public Uri WebAddress { get; private set; }
        public string TenantName { get; private set; }        
        public string Password { get; private set; }

        public bool HasToken { get; private set; }

        public bool IsActive { get; set; }

        private KeystoneResult keystoneResult;

        /// <summary>
        /// Token request date
        /// </summary>
        public DateTime TokenRequestDate
        {
            get
            {
                if (this.HasToken)
                {
                    return this.keystoneResult.access.token.issued_at;
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }

        /// <summary>
        /// Token expiration date
        /// </summary>
        public DateTime TokenExpirationDate
        {
            get
            {
                if (this.HasToken)
                {
                    return this.keystoneResult.access.token.expires;
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }

        /// <summary>
        /// Token ID
        /// </summary>
        private string tokenId = string.Empty;
        public string TokenId
        {
            get
            {
                if (this.HasToken)
                {
                    return this.keystoneResult.access.token.id;
                }
                else
                {
                    return this.tokenId;
                }
            }
            set { this.tokenId = value; }
        }

        /// <summary>
        /// Username
        /// </summary>
        private string username = string.Empty;
        public string Username 
        { 
            get{
                if (this.HasToken)
                {
                    return this.keystoneResult.access.user.name;
                }
                else
                {
                    return this.username;
                }
            }
            set { this.username = value; }
        }

        /// <summary>
        /// Manager for the Keystone authentification
        /// </summary>
        /// <param name="webAddress">Web Service Server address</param>
        /// <param name="username">Username use to connect</param>
        /// <param name="password">Password use to connect</param>
        /// <param name="tenantName">Web Service tenant name</param>
        public KeystoneManager(string username, string password, string tenantName, string webAddress)
        {
            if (webAddress == null)
            {
                this.WebAddress = new Uri(KeystoneManager.KEYSTONE_URL);
            }
            else
            {
                this.WebAddress = new Uri(webAddress);
            }

            if (tenantName == null)
            {
                this.TenantName = KeystoneManager.KEYSTONE_TENANT;
            }
            else
            {
                this.TenantName = tenantName;
            }

            this.Username = username;
            this.Password = password;

            this.GetToken();

            this.IsActive = true;
        }

        /// <summary>
        /// Manager for the Keystone authentification
        /// </summary>
        /// <param name="webAddress">Web Service Server address</param>
        /// <param name="password">Password use to connect</param>
        /// <param name="tenantName">Web Service tenant name</param>
        public KeystoneManager(string tokenId, string tenantName = null, string webAddress = null)
        {
            if (webAddress == null)
            {
                this.WebAddress = new Uri(KeystoneManager.KEYSTONE_URL);
            }
            else
            {
                this.WebAddress = new Uri(webAddress);
            }

            if (tenantName == null)
            {
                this.TenantName = KeystoneManager.KEYSTONE_TENANT;
            }
            else
            {
                this.TenantName = tenantName;
            }


            this.TokenId = tokenId;

            this.GetUsername();

            this.IsActive = true;
        }

        /// <summary>
        /// Check if the KSM is available to give a token
        /// </summary>
        /// <returns></returns>
        public bool IsServerAvailable()
        {
            MyWebClient client = new MyWebClient();    
            
            try
            {
                string result = client.DownloadString(this.WebAddress);
                return true;
            }
            catch(WebException wex)
            {
                if (wex.Status == WebExceptionStatus.ProtocolError) return true;

                return false;
            }
        }

        private class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 2000;
                return w;
            }
        }


        /// <summary>
        /// Send a new Token request and get it
        /// </summary>
        /// <returns>Status of the token reception. True = Token receive. False = Token not receive</returns>
        public bool GetToken()
        {
            try
            {
                KeystoneRequest ksRequest = new KeystoneRequest(this.TenantName, this.Username, this.Password);
                string jsonRequest = CommunicationManager.GetJsonFromObject(ksRequest);

                string jsonResult = CommunicationManager.SendJsonRequest(this.WebAddress, jsonRequest);

                this.keystoneResult = CommunicationManager.GetObjectFromJson(jsonResult, typeof(KeystoneResult)) as KeystoneResult;
            }
            catch (Exception ex)
            {
                return this.HasToken = false;
            }

            return this.HasToken = true;
        }

        /// <summary>
        /// Send a new Token request and get it
        /// </summary>
        /// <returns>Status of the token reception. True = Token receive. False = Token not receive</returns>
        public bool GetUsername()
        {
            try
            {
                UsernameRequest ksRequest = new UsernameRequest(this.TokenId, this.TenantName);
                string jsonRequest = CommunicationManager.GetJsonFromObject(ksRequest);

                string jsonResult = CommunicationManager.SendJsonRequest(this.WebAddress, jsonRequest);

                this.keystoneResult = CommunicationManager.GetObjectFromJson(jsonResult, typeof(KeystoneResult)) as KeystoneResult;
            }
            catch (Exception ex)
            {
                return false;
            }

            return this.HasToken = true;
        }
    }

    #region KeystoneRequest classes > use for the authentification request creation

    public class KeystoneRequest
    {
        public Authentification auth;

        public KeystoneRequest(string tenantName, string username, string password)
        {
            this.auth = new Authentification();
            this.auth.tenantName = tenantName;
            this.auth.passwordCredentials = new PasswordCredentials();
            this.auth.passwordCredentials.username = username;
            this.auth.passwordCredentials.password = password;
        }
    }
    
    public class Authentification
    {
        public string tenantName;
        public PasswordCredentials passwordCredentials;
    }

    public class PasswordCredentials
    {
        public string username;
        public string password;
    }

    public class UsernameRequest
    {
        public AuthentificationFromToken auth;

        public UsernameRequest(string sessionToken, string tenantName)
        {
            this.auth = new AuthentificationFromToken();
            this.auth.token = new TokenInformation() { id = sessionToken };

            this.auth.tenantName = tenantName;
        }
    }

    public class AuthentificationFromToken
    {
        public TokenInformation token;
        public String tenantName;
    }

    public class TokenInformation
    {
        public string id;
    }

    #endregion

    #region KeystoneResult classes  > use for the JSON deserialization
    public class KeystoneResult
    {
        public Access access;

        public static Type GetType()
        {
            return Type.GetType("CloudFlow.WebServices.KeystoneResult");
        }
    }

    public class Access
    {
        public Token token;
        public List<ServiceCatalog> serviceCatalog;
        public User user;
        public Metadata metadata;
    }

    public class Token
    {
        public DateTime issued_at;
        public DateTime expires;
        public string id;
        public Tenant tenant;
    }

    public class Tenant
    {
        public string description;
        public bool enabled;
        public string id;
        public string name;
    }

    public class ServiceCatalog
    {
        public List<EndPoints> endpoints;

        public List<string> endpoints_links;

        public string type;
        public string name;
    }

    public class EndPoints
    {
        public string adminURL;
        public string region;
        public string internalURL;
        public string id;
        public string publicURL;
    }

    public class User
    {
        public string username;
        public List<string> roles_links;
        public string id;
        public List<Role> roles;
        public string name;
    }

    public class Role
    {
        public string name;
    }

    public class Metadata
    {
        public string is_admin;
        public List<string> roles;
    }
    #endregion
}
