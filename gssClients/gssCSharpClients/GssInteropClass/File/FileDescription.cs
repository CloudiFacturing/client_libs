using System;

namespace GssInteropClass.File
{
    public class FileDescription
    {
        private String visualName;
        private String uniqueName;
        private FileIdentifier id;
        private String type;

        public string VisualName
        {
            get
            {
                return System.Web.HttpUtility.UrlDecode(this.visualName);
            }
        }

        public string Type
        {
            get { return this.type; }
        }

        public bool IsFile
        {
            get
            {
                if (this.type.ToLower() != "folder") return true;

                return false;
            }
        }

        public FileDescription(String visualName, String uniqueName, FileIdentifier id, String type)
        {
            this.visualName = visualName;
            this.uniqueName = uniqueName;
            this.id = id;
            this.type = type;
        }


        /// <summary>
        /// Empty constructor
        /// </summary>
        public FileDescription() { }



        /// <summary>
        /// return the visualName
        /// </summary>
        /// <returns></returns>
        public String getVisualName()
        {
            return visualName;
        }

        /**
         * @param visualName the visualName to set
         */
        public void setVisualName(String visualName)
        {
            this.visualName = visualName;
        }

        /**
         * @return the uniqueName
         */
        public String getUniqueName()
        {
            return uniqueName;
        }

        /**
         * @param uniqueName the uniqueName to set
         */
        public void setUniqueName(String uniqueName)
        {
            this.uniqueName = uniqueName;
        }

        /**
         * @return the id
         */
        public FileIdentifier getId()
        {
            return id;
        }

        /**
         * @param id the id to set
         */
        public void setId(FileIdentifier id)
        {
            this.id = id;
        }

        /**
         * @return the type
         */
        public String getType()
        {
            return type;
        }

        /**
         * @param type the type to set
         */
        public void setType(String type)
        {
            this.type = type;
        }

        public override string ToString()
        {
            return this.getVisualName() + "(" + this.getId() + ")";
        }
    }
}
