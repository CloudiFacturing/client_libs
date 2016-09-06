using AuthentificationWSDL.CFAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthentificationWSDL.CFAuth
{
    public class KeystoneManagerWSDL_CF : KeystoneManagerWSDL
    {
        #region constant variables
        public override string KEYSTONE_PROJECT { get { return "cloudflow"; } }
        #endregion

        private AuthentificationWSDL.CFAuth.AuthManager client = null;

        public override Projects Project
        {
            get { return Projects.CAxMan; }
        }

        private string projectName;
        public override string ProjectName
        {
            get
            {
                return this.HasToken ? this.client.getProject(this.TokenId) : this.projectName;
            }
        }


        public override bool IsValid
        {
            get
            {
                if (this.TokenId == null) return false;

                return this.client.validateSessionToken(this.TokenId);
            }
        }

        public override bool HasToken
        {
            get
            {
                return this.TokenId != null;
            }
        }

        public override string KeystoneManagerURI
        {
            get
            {
                return this.client.Url;
            }
        }
        
        /// <summary>
        /// Manager for the Keystone authentification
        /// </summary>
        /// <param name="project">Project name</param>
        internal KeystoneManagerWSDL_CF(string project)
        {
            if (this.client == null) this.client = new AuthManager();

            if (project == null)
            {
                this.projectName = this.KEYSTONE_PROJECT;
            }
            else
            {
                this.projectName = project;
            }
        }

        /// <summary>
        /// Manager for the Keystone authentification
        /// </summary>
        /// <param name="webAddress">Web Service Server address</param>
        /// <param name="username">Username use to connect</param>
        /// <param name="password">Password use to connect</param>
        /// <param name="project">Web Service tenant name</param>
        public KeystoneManagerWSDL_CF(string username, string password, string project)
            : this(project)
        {
            this.Username = username;
            this.Password = password;

            try
            {
                this.TokenId = this.client.getSessionToken(this.Username, this.Password, this.projectName);
            }
            catch (Exception)
            {
                this.TokenId = null;
            }
        }

        /// <summary>
        /// Manager for the Keystone authentification
        /// </summary>
        /// <param name="webAddress">Web Service Server address</param>
        /// <param name="password">Password use to connect</param>
        /// <param name="project">Web Service tenant name</param>
        public KeystoneManagerWSDL_CF(string tokenId, string project)
            : this(project)
        {

            this.TokenId = tokenId;

            this.Username = this.client.getUsername(this.TokenId);

        }

        /// <summary>
        /// Check if the KSM is available to give a token
        /// </summary>
        /// <returns></returns>
        public override bool IsServerAvailable
        {
            get { return KeystoneManagerWSDL.isServerAvailable; }
        }

    }
}
