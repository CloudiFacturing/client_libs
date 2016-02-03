using AuthentificationWSDL.CFAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthentificationWSDL2
{
    public class KeystoneManagerWSDL
    {
        #region constant variables
        public const string KEYSTONE_PROJECT = "caxman";
        #endregion

        private AuthManager client = null;

        private string project;
        public string Project
        {
            get
            {
                return this.client.getProject(this.TokenId);
            }
        }


        public string Password { get; private set; }

        public bool IsValid
        {
            get
            {
                return this.client.validateSessionToken(this.TokenId);
            }
        }


        /// <summary>
        /// Token ID
        /// </summary>
        public string TokenId
        {
            get;
            set;
        }

        /// <summary>
        /// Username
        /// </summary>
        private string username = string.Empty;
        public string Username
        {
            get
            {
                return this.username;
            }
            set { this.username = value; }
        }


        /// <summary>
        /// Manager for the Keystone authentification
        /// </summary>
        /// <param name="project">Project name</param>
        internal KeystoneManagerWSDL(string project)
        {
            if (this.client == null) this.client = new AuthManager();
            
            if (project == null)
            {
                this.project = KeystoneManagerWSDL.KEYSTONE_PROJECT;
            }
            else
            {
                this.project = project;
            }
        }

        /// <summary>
        /// Manager for the Keystone authentification
        /// </summary>
        /// <param name="webAddress">Web Service Server address</param>
        /// <param name="username">Username use to connect</param>
        /// <param name="password">Password use to connect</param>
        /// <param name="project">Web Service tenant name</param>
        public KeystoneManagerWSDL(string username, string password, string project)
            : this(project)
        {
            this.Username = username;
            this.Password = password;

            this.TokenId = this.client.getSessionToken(this.Username, this.Password, this.project);
        }

        /// <summary>
        /// Manager for the Keystone authentification
        /// </summary>
        /// <param name="webAddress">Web Service Server address</param>
        /// <param name="password">Password use to connect</param>
        /// <param name="project">Web Service tenant name</param>
        public KeystoneManagerWSDL(string tokenId, string project = null)
            : this(project)
        {

            this.TokenId = tokenId;

            this.Username = this.client.getUsername(this.TokenId);

        }

        /// <summary>
        /// Check if the KSM is available to give a token
        /// </summary>
        /// <returns></returns>
        public bool IsServerAvailable()
        {
            try
            {
                this.client.validateSessionToken(this.TokenId);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
