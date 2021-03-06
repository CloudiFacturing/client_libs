﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using GssInteropClass.CM.Gss;

namespace GssInteropClass.File
{
    public class GenericFileStorage
    {
        private Encoding fileEncoding = Encoding.Unicode;

        private int bufferSize = 1024;
        public int BufferSize { get { return this.bufferSize; } }
        
        private string sessionToken = "";
        public string SessionToken { get { return this.sessionToken; } }

        private StorageType type;
        public StorageType Type { get { return this.type; } }

        private string projectFolder = "";
        public string ProjectFolder { get { return this.projectFolder; } }

        private string currentFolder = "";
        public string CurrentFolder { get { return this.currentFolder; } }

        private FileUtilitiesClient fileUtils;

        private string filePrefix = string.Empty;
        public string FilePrefix
        {
            get
            {
                if (this.filePrefix == string.Empty)
                {
                    switch (this.Type)
                    {
                        case StorageType.PLM:
                            this.filePrefix = "plm://";
                            break;
                        case StorageType.SWIFT:
                            this.filePrefix = $"swift://";
                            break;
                    }
                }

                return this.filePrefix;
            }
        }

        #region Constructors

        public GenericFileStorage(string sessionToken, StorageType type, string projectFolder)
        {
            this.bufferSize = 1024;
            
            this.sessionToken = sessionToken;
            this.type = type;
            this.projectFolder = projectFolder;

            this.fileUtils = new FileUtilitiesClient();
        }

        public GenericFileStorage(string sessionToken, StorageType type, string projectFolder, string endPointConfigurationName)
            : this(sessionToken, type, projectFolder)
        {
            this.fileUtils = new FileUtilitiesClient(endPointConfigurationName);
        }

        public GenericFileStorage(string sessionToken, StorageType type, string projectFolder, string endPointConfigurationName, string remoteAccess)
            : this(sessionToken, type, projectFolder)
        {
            this.fileUtils = new FileUtilitiesClient(endPointConfigurationName, remoteAccess);
        }

        public GenericFileStorage(string sessionToken, StorageType type, string projectFolder, string endPointConfigurationName, System.ServiceModel.EndpointAddress remoteAccess)
            : this(sessionToken, type, projectFolder)
        {
            this.fileUtils = new FileUtilitiesClient(endPointConfigurationName, remoteAccess);
        }

        #endregion
        
        /// <summary>
        /// Get all file descriptions from the current "folder"
        /// </summary>
        /// <returns></returns>
        public List<FileDescription> GetFileDescriptions()
        {
            return this.GetFileDescriptions("");
        }

        /// <summary>
        /// Get all file descriptions from a specific "folder"
        /// </summary>
        /// <param name="folderName">Folder name (sample: "work/test1/")</param>
        /// <returns></returns>
        public List<FileDescription> GetFileDescriptions(string folderName)
        {
            if (folderName.StartsWith(filePrefix))
            {
                return this.GetFileDescriptions(folderName, "");
            }
            else
            {
                return this.GetFileDescriptions(this.FilePrefix, folderName);
            }
        }

        /// <summary>
        /// Get all file descriptions from a specific "folder" & a forced prefix
        /// </summary>
        /// <param name="prefix">Forced prefix (sample: "swift://caxman/user/")</param>       
        /// <param name="folderName">Folder name (sample: "work/test1/")</param>
        /// <returns></returns>
        public List<FileDescription> GetFileDescriptions(string prefix, string folderName)
        {
            resourceInformation[] ressourcesInformationList = fileUtils.listFilesMinimal(prefix + folderName, this.SessionToken);

            if (ressourcesInformationList == null) return new List<FileDescription>(0);

            List<FileDescription> fileDescriptions = new List<FileDescription>(ressourcesInformationList.Length);

            foreach (var file in ressourcesInformationList)
            {
                string visualName = file.visualName;

                fileDescriptions.Add(new FileDescription(visualName, file.uniqueName, new FileIdentifier(file.uniqueName), file.type));
            }

            return fileDescriptions;
        }

        /// <summary>
        /// Get the file description for a PLM
        /// </summary>
        /// <param name="path">PLM path</param>
        /// <returns></returns>
        public List<FileDescription> GetPlmFileDescriptions(string path)
        {
            string folderFullPath = this.FilePrefix + path.TrimStart('/');

            resourceInformation[] ressourcesInformationList = fileUtils.listFiles(folderFullPath, this.SessionToken);
            if (ressourcesInformationList == null) return null;

            List<FileDescription> fileDescriptions = new List<FileDescription>(ressourcesInformationList.Length);

            foreach (resourceInformation file in ressourcesInformationList)
            {
                string visualName = System.Web.HttpUtility.UrlDecode(file.visualName);

                fileDescriptions.Add(new FileDescription(visualName, file.uniqueName, new FileIdentifier(file.uniqueName.Substring(file.uniqueName.LastIndexOf('/') + 1)), file.type));
            }

            return fileDescriptions;
        }

        /// <summary>
        /// Check if the file exist with a file description
        /// </summary>
        /// <param name="fd">File description</param>
        /// <returns></returns>
        public bool IsFileExist(FileDescription fd)
        {
            return this.IsFileExist(fd.getUniqueName());
        }

        /// <summary>
        /// Check if the file exist with a file gss address
        /// </summary>
        /// <param name="cloudFilePath">GSS file address</param>
        /// <returns></returns>
        public bool IsFileExist(string cloudFilePath)
        {
            return fileUtils.containsFile(cloudFilePath, this.SessionToken);
        }

        /// <summary>
        /// Create a folder
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public FileDescription CreateFolder(string folderName)
        {
            string serverFolderName = this.FilePrefix + folderName.Trim('/') + "/";
            var ri = fileUtils.createFolder(serverFolderName, this.SessionToken);

            return new FileDescription(ri.visualName, ri.uniqueName, new FileIdentifier(""), ri.type);
        }

        /// <summary>
        /// Create a folder in another gss space
        /// </summary>
        /// <param name="prefix">New gss space path</param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public FileDescription CreateFolder(string prefix, string folderName)
        {
            string serverFolderName = prefix + folderName.Trim('/');
            var ri = fileUtils.createFolder(serverFolderName, this.SessionToken);

            return new FileDescription(ri.visualName, ri.uniqueName, new FileIdentifier(""), ri.type);
        }

        /// <summary>
        /// Delete a folder
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public bool DeleteFolder(string folderName)
        {
            string serverFolderName = this.FilePrefix + folderName.Trim('/') + "/";
            return fileUtils.deleteFolder(serverFolderName, this.SessionToken);
        }

        /// <summary>
        /// Upload a file on the GSS
        /// </summary>
        /// <param name="localFilePath">Local file path</param>
        /// <param name="compress">set true to compress the file</param>
        /// <returns></returns>
        public FileDescription UploadFile(string localFilePath, bool compress = true)
        {
            return UploadFile(localFilePath, Path.GetFileName(localFilePath), "", compress);
        }

        /// <summary>
        /// Upload a file on the GSS
        /// </summary>
        /// <param name="localFilePath">Local File path</param>
        /// <param name="distantPath">Cloud filename. For PLM: [folder]/[filename]</param>
        /// <param name="compress">set true to compress the file</param>
        /// <returns></returns>
        public FileDescription UploadFile(string localFilePath, string distantPath, bool compress = true)
        {
            return UploadFile(localFilePath, Path.GetFileName(localFilePath), distantPath, compress);
        }

        /// <summary>
        /// Upload a file on the GSS
        /// </summary>
        /// <param name="localFilePath">Local File path</param>
        /// <param name="cloudFilename">Cloud filename. For PLM: [folder]/[filename]</param>
        /// <param name="distantPath">path folder</param>
        /// <param name="compress">set true to compress the file</param>
        /// <returns></returns>
        public FileDescription UploadFile(string localFilePath, string cloudFilename, string distantPath, bool compress = true)
        {
            string serverFilename;

            if (compress)
            {
                localFilePath = GenericFileStorage.ZipFile(localFilePath);
                cloudFilename += ".zip";
            }

            if (distantPath.StartsWith(this.FilePrefix)) distantPath = distantPath.Substring(FilePrefix.Length);

            string cloudFolderEncoded = distantPath == "" ? "" : System.Web.HttpUtility.UrlEncode(distantPath.Trim('/')) + "/";
            string cloudFilenameEncoded = System.Web.HttpUtility.UrlEncode(cloudFilename);

            serverFilename = this.FilePrefix + cloudFolderEncoded + cloudFilenameEncoded;

            this.DeleteFileIfExist(serverFilename);

            // Now we must ask GSS how to upload the file: 
            // (this calls the service a second time)
            var resourceInfo = fileUtils.getResourceInformation(serverFilename, this.SessionToken);

            var createDescription = resourceInfo.createDescription;

            // Now we create 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(createDescription.url);

            // We set the parameters as described by the service
            request.Method = createDescription.httpMethod;
            request.Headers[createDescription.sessionTokenField] = this.SessionToken;
            request.Timeout = System.Threading.Timeout.Infinite;
            request.KeepAlive = true;

            if (createDescription.headers != null)
            {
                foreach (var headerField in createDescription.headers)
                {
                    request.Headers[headerField.key] = headerField.value;
                }
            }

            //Write the localFile on the server
            using (BinaryWriter writer = new BinaryWriter(request.GetRequestStream()))
            {
                using (BinaryReader reader = new BinaryReader(System.IO.File.OpenRead(localFilePath), fileEncoding))
                {
                    int bufferSize = this.BufferSize;
                    byte[] buffer = new byte[bufferSize];
                    while ((buffer = reader.ReadBytes(bufferSize)).GetLength(0) > 0)
                    {
                        writer.Write(buffer);
                    }
                }
            }

            // In return we will get the id of the newly created file:
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Accepted && response.StatusCode != HttpStatusCode.Created)
                {
                    throw new Exception("Wrong status code returned");
                }

                string returnedID = "";
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    returnedID = reader.ReadToEnd().Trim();
                }

                if (string.IsNullOrEmpty(returnedID))
                {
                    returnedID = resourceInfo.uniqueName;
                }

                //var test = fileUtils.getResourceInformation(returnedID, this.SessionToken);

                return new FileDescription(resourceInfo.visualName, resourceInfo.uniqueName, new FileIdentifier(returnedID), resourceInfo.type);
            }
            catch (WebException wex)
            {
                throw new Exception("Generic File Storage Exception", wex);
            }
        }

        /// <summary>
        /// Download a file from the GSS
        /// </summary>
        /// <param name="file">File description</param>
        /// <param name="folderPath">local folder where the file will be download</param>
        /// <param name="filename">Force a local filename</param>
        /// <param name="autoUnzip">Unzip the file if the extension is *.zip</param>
        /// <returns></returns>
        public string DownloadFile(FileDescription file, string folderPath, string filename = "", bool autoUnzip = true)
        {
            string fileId = "";

            // Now we may call the service a second time to download it:
            switch (this.Type)
            {
                case StorageType.PLM:
                case StorageType.SWIFT:
                default:
                    fileId = file.getUniqueName();
                    break;
            }

            return this.DownloadFile(fileId, folderPath, filename, autoUnzip);
        }

        /// <summary>
        /// Download a file from the GSS
        /// </summary>
        /// <param name="fileId">gss file address</param>
        /// <param name="folderPath">local folder where the file will be download</param>
        /// <param name="filename">Force a local filename</param>
        /// <param name="autoUnzip">Unzip the file if the extension is *.zip</param>
        /// <returns></returns>
        public string DownloadFile(string fileId, string folderPath, string filename = "", bool autoUnzip = true)
        {
            if (!fileUtils.containsFile(fileId, this.SessionToken)) throw new Exception(string.Format("File \"{0}\" does not exist on the GSS", fileId));

            var resourceInfo = fileUtils.getResourceInformation(fileId, this.SessionToken);
            var readDescription = resourceInfo.readDescription;

            if (!readDescription.supported) throw new Exception("Read operation not allowed!");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpWebRequest downloadRequest = (HttpWebRequest)WebRequest.Create(readDescription.url);
            // We set the parameters as described by the service
            downloadRequest.Method = readDescription.httpMethod;
            downloadRequest.Timeout = Timeout.Infinite;
            downloadRequest.KeepAlive = true;

            if (readDescription.headers != null)
            {
                foreach (var headerField in readDescription.headers)
                {
                    downloadRequest.Headers[headerField.key] = headerField.value;
                }
            }

            // In return we will get the id of the newly created file:
            HttpWebResponse responseFromDownload = (HttpWebResponse)downloadRequest.GetResponse();
            if (responseFromDownload.StatusCode != HttpStatusCode.OK && responseFromDownload.StatusCode != HttpStatusCode.Accepted)
            {
                throw new Exception("Wrong status code returned");
            }

            string localFilePath = folderPath.TrimEnd('\\') + @"\";
            if (string.IsNullOrEmpty(filename))
            {
                localFilePath += Path.GetFileName(fileId);
            }
            else
            {
                localFilePath += filename;
            }

            //Write the server file on the computer
            using (BinaryWriter writer = new BinaryWriter(System.IO.File.OpenWrite(localFilePath), fileEncoding))
            {
                using (BinaryReader reader = new BinaryReader(responseFromDownload.GetResponseStream()))
                {
                    int bufferSize = this.BufferSize;
                    byte[] buffer = new byte[bufferSize];
                    while ((buffer = reader.ReadBytes(bufferSize)).GetLength(0) > 0)
                    {
                        writer.Write(buffer);
                    }
                }
            }

            if (autoUnzip && Path.GetExtension(localFilePath).ToLower().Equals(".zip"))
            {
                localFilePath = GenericFileStorage.UnzipFile(localFilePath);
            }

            return localFilePath;
        }

        /// <summary>
        /// Download a file from the GSS in memory
        /// </summary>
        /// <param name="file">File description</param>
        /// <param name="outBinaries">Byte array with the file datas</param>
        /// <returns></returns>
        public string DownloadFile(FileDescription file, out byte[] outBinaries)
        {
            return this.DownloadFile(file.getId().getUuid(), out outBinaries);
        }

        /// <summary>
        /// Download a file from the GSS in memory
        /// </summary>
        /// <param name="fileId">GSS file address</param>
        /// <param name="outBinaries">Byte array with the file datas</param>
        /// <returns></returns>
        public string DownloadFile(string fileId, out byte[] outBinaries)
        {
            if (!fileUtils.containsFile(fileId, this.SessionToken)) throw new Exception(string.Format("File \"{0}\" does not exist on the GSS", fileId));

            var resourceInfo = fileUtils.getResourceInformation(fileId, this.SessionToken);
            var readDescription = resourceInfo.readDescription;

            HttpWebRequest downloadRequest = (HttpWebRequest)WebRequest.Create(readDescription.url);
            // We set the parameters as described by the service
            downloadRequest.Method = readDescription.httpMethod;
            downloadRequest.Timeout = System.Threading.Timeout.Infinite;
            downloadRequest.KeepAlive = true;

            if (readDescription.headers != null)
            {
                foreach (var headerField in readDescription.headers)
                {
                    downloadRequest.Headers[headerField.key] = headerField.value;
                }
            }

            // In return we will get the id of the newly created file:
            HttpWebResponse responseFromDownload = (HttpWebResponse)downloadRequest.GetResponse();
            if (responseFromDownload.StatusCode != HttpStatusCode.OK && responseFromDownload.StatusCode != HttpStatusCode.Accepted)
            {
                throw new Exception("Wrong statuscode returned");
            }

            List<byte> byteList = new List<byte>();

            using (BinaryReader reader = new BinaryReader(responseFromDownload.GetResponseStream()))
            {
                int bufferSize = this.BufferSize;
                byte[] buffer = new byte[bufferSize];
                while ((buffer = reader.ReadBytes(bufferSize)).GetLength(0) > 0)
                {
                    byteList.AddRange(buffer.ToList());
                }
            }

            outBinaries = byteList.ToArray();

            return "";
        }

        /// <summary>
        /// Remove a file asynchronously
        /// </summary>
        /// <param name="file">File description</param>
        public void RemoveFileAsync(FileDescription file)
        {
            try
            {
                this.RemoveFileAsync(file.getId().getUuid());
            }
            catch { }
        }

        /// <summary>
        /// Remove a file asynchronously
        /// </summary>
        /// <param name="fileId">GSS file address</param>
        public void RemoveFileAsync(string fileId)
        {
            Thread th = new Thread(() => this.RemoveFile(fileId, true));
            try
            {
                th.Start();
            }
            catch { }
        }

        /// <summary>
        /// Remove a file
        /// </summary>
        /// <param name="file">File description</param>
        public void RemoveFile(FileDescription file, bool blockExceptions = false)
        {
            this.RemoveFile(file.getId().getUuid(), blockExceptions);
        }

        /// <summary>
        /// Remove a file
        /// </summary>
        /// <param name="fileId">GSS file address</param>
        /// <param name="blockExceptions">True to not throw any exception</param>
        public void RemoveFile(string fileId, bool blockExceptions = false)
        {
            try
            {
                if (!fileUtils.containsFile(fileId, SessionToken)) throw new Exception("File does not exist: " + fileId);

                var resourceInformation = fileUtils.getResourceInformation(fileId, SessionToken);

                var deleteDescription = resourceInformation.deleteDescription;

                HttpWebRequest deleteRequest = (HttpWebRequest)WebRequest.Create(deleteDescription.url);
                // We set the parameters as described by the service
                deleteRequest.Method = deleteDescription.httpMethod;
                deleteRequest.Headers[deleteDescription.sessionTokenField] = SessionToken;

                // Set any relevant headers
                if (deleteDescription.headers != null)
                {
                    foreach (var headerField in deleteDescription.headers)
                    {
                        deleteRequest.Headers[headerField.key] = headerField.value;
                    }
                }

                //Console.WriteLine(deleteDescription.url);

                HttpWebResponse responseFromDelete = (HttpWebResponse)deleteRequest.GetResponse();

                // make sure we succeeded
                if (responseFromDelete.StatusCode != HttpStatusCode.OK && responseFromDelete.StatusCode != HttpStatusCode.Accepted && responseFromDelete.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new Exception("Wrong statuscode returned");
                }
            }
            catch (Exception ex)
            {
                if (!blockExceptions) throw new Exception("Fail to remove the file on the GSS " + fileId, ex);
            }
        }

        /// <summary>
        /// Delete a file on the GSS if it exists
        /// </summary>
        /// <param name="fileId">GSS file address</param>
        public void DeleteFileIfExist(string fileId)
        {
            if (fileUtils.containsFile(fileId, this.SessionToken))
            {
                //Console.Out.Write(string.Format("{0} > {1}", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), "Delete the existing file > "));
                this.RemoveFile(fileId);
                //Console.Out.Write("OK");
            }
            else
            {
                //Console.Out.WriteLine(string.Format("{0} > {1}", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), "File not exists"));
            }
        }


        #region Static methods

        /// <summary>
        /// Zip a file
        /// </summary>
        /// <param name="filename">Local file path</param>
        /// <returns>Local zip file path</returns>
        public static string ZipFile(string filename)
        {
            string zippedFilename = filename + ".zip";

            GenericFileStorage.deleteFile(zippedFilename);

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var demoFile = archive.CreateEntryFromFile(filename, Path.GetFileName(filename));
                }

                using (var fileStream = new FileStream(zippedFilename, FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }
            }

            //GenericFileStorage.deleteFile(filename);

            return zippedFilename;
        }

        /// <summary>
        /// Unzip a file
        /// </summary>
        /// <param name="filePath">Zip file path</param>
        /// <returns>Unzip file path</returns>
        public static string UnzipFile(string filePath)
        {
            string resultFilename = "";

            ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(filePath);
            var zippedFile = archive.GetEntry(Path.GetFileNameWithoutExtension(filePath));

            resultFilename = Path.ChangeExtension(filePath, "").TrimEnd('.');

            if (System.IO.File.Exists(resultFilename)) System.IO.File.Delete(resultFilename);

            zippedFile.ExtractToFile(resultFilename);

            archive.Dispose();

            GenericFileStorage.deleteFile(filePath);

            return resultFilename;
        }


        /// <summary>
        /// Delete a local file if it exists
        /// </summary>
        /// <param name="filePath">Local file path</param>
        private static void deleteFile(string filePath)
        {
            try
            {
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

            }
            catch { }
        }

        /// <summary>
        /// Get the GSS file address after zip or unzip
        /// </summary>
        /// <param name="gssFileId">GSS file address</param>
        /// <param name="newExtension">new GSS file extension</param>
        /// <param name="hasToRemoveZip">Remove the zip extension ?</param>
        /// <returns>GSS file address</returns>
        public static string GetResultFilename(string gssFileId, string newExtension, bool hasToRemoveZip = false)
        {
            string resultGssId = gssFileId;
            bool isZipped = false;

            //Remove .zip extension
            if (Path.GetExtension(gssFileId) == ".zip")
            {
                resultGssId = Path.ChangeExtension(resultGssId, "").TrimEnd('.');
                isZipped = !hasToRemoveZip;
            }

            resultGssId = Path.ChangeExtension(resultGssId, newExtension);

            if (isZipped) resultGssId += ".zip";

            return resultGssId;
        }

        /// <summary>
        /// Encode the URL (transform the non URL compatible characters on the input string)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string UrlEncode(string source)
        {
            return System.Web.HttpUtility.UrlEncode(source);
        }

        #endregion

    }
}
