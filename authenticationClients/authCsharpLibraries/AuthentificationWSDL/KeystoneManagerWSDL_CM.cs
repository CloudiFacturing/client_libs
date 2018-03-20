using AuthentificationWSDL.CMAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthentificationWSDL.CMAuth
{
    public class KeystoneManagerWSDL_CM : KeystoneManagerWSDL
    {
        #region constant variables
        public override string KEYSTONE_PROJECT { get { return "caxman"; } }
        #endregion

        private AuthentificationWSDL.CMAuth.AuthManager client = null;

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
                try
                {
                    return this.client.validateSessionToken(this.TokenId);
                }
                catch
                {
                    return false;
                }
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
        internal KeystoneManagerWSDL_CM(string project)
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
        public KeystoneManagerWSDL_CM(string username, string password, string project)
            : this(project)
        {
            if (project != null)
            {
                this.Username = username;
                this.Password = password;

                try
                {
                    this.TokenId = this.client.getSessionToken(this.Username, this.Password, this.projectName);
                }
                catch (Exception ex)
                {
                    this.TokenId = null;
                }
            }
        }

        /// <summary>
        /// Manager for the Keystone authentification
        /// </summary>
        /// <param name="webAddress">Web Service Server address</param>
        /// <param name="password">Password use to connect</param>
        /// <param name="project">Web Service tenant name</param>
        public KeystoneManagerWSDL_CM(string tokenId, string project = null)
            : this(project)
        {

            this.TokenId = tokenId;

            if(this.IsValid) this.Username = this.client.getUsername(this.TokenId);
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
