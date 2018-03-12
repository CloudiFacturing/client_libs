using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace GssInteropClass.File
{
    abstract public class GenericFileStorage
    {
        abstract public int BufferSize { get; }

        abstract public string Username { get; }
        abstract public string SessionToken { get; }
        abstract public StorageType Type { get; }

        abstract public string ProjectFolder { get; }

        abstract public string FilePrefix { get; }

        abstract public string CurrentFolder { get; }

        #region Constructors

        public GenericFileStorage()
        {
        }

        #endregion


        /// <summary>
        /// Get all file descriptions from the current "folder"
        /// </summary>
        /// <returns></returns>
        abstract public List<FileDescription> GetFileDescriptions();

        /// <summary>
        /// Get all file descriptions from a specific "folder"
        /// </summary>
        /// <param name="folderName">Folder name (sample: "work/test1/")</param>
        /// <returns></returns>
        abstract public List<FileDescription> GetFileDescriptions(string folderName);

        /// <summary>
        /// Get all file descriptions from a specific "folder" & a forced prefix
        /// </summary>
        /// <param name="prefix">Forced prefix (sample: "swift://caxman/user/")</param>       
        /// <param name="folderName">Folder name (sample: "work/test1/")</param>
        /// <returns></returns>
        abstract public List<FileDescription> GetFileDescriptions(string prefix, string folderName);

        /// <summary>
        /// Get the file description for a PLM
        /// </summary>
        /// <param name="path">PLM path</param>
        /// <returns></returns>
        abstract public List<FileDescription> GetPlmFileDescriptions(string path);

        /// <summary>
        /// Check if the file exist with a file description
        /// </summary>
        /// <param name="fd">File description</param>
        /// <returns></returns>
        abstract public bool IsFileExist(FileDescription fd);

        /// <summary>
        /// Check if the file exist with a file gss address
        /// </summary>
        /// <param name="cloudFilePath">GSS file address</param>
        /// <returns></returns>
        abstract public bool IsFileExist(string cloudFilePath);

        /// <summary>
        /// Create a folder
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        abstract public FileDescription CreateFolder(string folderName);

        /// <summary>
        /// Create a folder in another gss space
        /// </summary>
        /// <param name="prefix">New gss space path</param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        abstract public FileDescription CreateFolder(string prefix, string folderName);

        /// <summary>
        /// Delete a folder
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        abstract public bool DeleteFolder(string folderName);

        /// <summary>
        /// Upload a file on the GSS
        /// </summary>
        /// <param name="localFilePath">Local file path</param>
        /// <param name="compress">set true to compress the file</param>
        /// <returns></returns>
        abstract public FileDescription UploadFile(string localFilePath, bool compress = true);

        /// <summary>
        /// Upload a file on the GSS
        /// </summary>
        /// <param name="localFilePath">Local File path</param>
        /// <param name="distantPath">Cloud filename. For PLM: [folder]/[filename]</param>
        /// <param name="compress">set true to compress the file</param>
        /// <returns></returns>
        abstract public FileDescription UploadFile(string localFilePath, string distantPath, bool compress = true);

        /// <summary>
        /// Upload a file on the GSS
        /// </summary>
        /// <param name="localFilePath">Local File path</param>
        /// <param name="cloudFilename">Cloud filename. For PLM: [folder]/[filename]</param>
        /// <param name="distantPath">path folder</param>
        /// <param name="compress">set true to compress the file</param>
        /// <returns></returns>
        abstract public FileDescription UploadFile(string localFilePath, string cloudFilename, string distantPath, bool compress = true);

        /// <summary>
        /// Download a file from the GSS
        /// </summary>
        /// <param name="file">File description</param>
        /// <param name="folderPath">local folder where the file will be download</param>
        /// <param name="filename">Force a local filename</param>
        /// <param name="autoUnzip">Unzip the file if the extension is *.zip</param>
        /// <returns></returns>
        abstract public string DownloadFile(FileDescription file, string folderPath, string filename = "", bool autoUnzip = true);

        /// <summary>
        /// Download a file from the GSS
        /// </summary>
        /// <param name="fileId">gss file address</param>
        /// <param name="folderPath">local folder where the file will be download</param>
        /// <param name="filename">Force a local filename</param>
        /// <param name="autoUnzip">Unzip the file if the extension is *.zip</param>
        /// <returns></returns>
        abstract public string DownloadFile(string fileId, string folderPath, string filename = "", bool autoUnzip = true);

        /// <summary>
        /// Download a file from the GSS in memory
        /// </summary>
        /// <param name="file">File description</param>
        /// <param name="outBinaries">Byte array with the file datas</param>
        /// <returns></returns>
        abstract public string DownloadFile(FileDescription file, out byte[] outBinaries);

        /// <summary>
        /// Download a file from the GSS in memory
        /// </summary>
        /// <param name="fileId">GSS file address</param>
        /// <param name="outBinaries">Byte array with the file datas</param>
        /// <returns></returns>
        abstract public string DownloadFile(string fileId, out byte[] outBinaries);

        /// <summary>
        /// Remove a file asynchronously
        /// </summary>
        /// <param name="file">File description</param>
        abstract public void RemoveFileAsync(FileDescription file);

        /// <summary>
        /// Remove a file asynchronously
        /// </summary>
        /// <param name="fileId">GSS file address</param>
        abstract public void RemoveFileAsync(string fileId);

        /// <summary>
        /// Remove a file
        /// </summary>
        /// <param name="file">File description</param>
        /// <param name="file">True to not throw any exception</param>
        abstract public void RemoveFile(FileDescription file, bool blockExceptions = false);

        /// <summary>
        /// Remove a file
        /// </summary>
        /// <param name="fileId">GSS file address</param>
        /// <param name="blockExceptions">True to not throw any exception</param>
        abstract public void RemoveFile(string fileId, bool blockExceptions = false);

        /// <summary>
        /// Delete a file on the GSS if it exists
        /// </summary>
        /// <param name="fileId">GSS file address</param>
        abstract public void DeleteFileIfExist(string fileId);
        
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
