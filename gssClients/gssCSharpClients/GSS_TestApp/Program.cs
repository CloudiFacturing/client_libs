using AuthentificationWSDL.CFAuth;
using AuthentificationWSDL.CMAuth;
using GssInteropClass.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GSS_TestApp
{
    class Program
    {
        private static KeystoneManagerWSDL_CF ksManager;
        //private static KeystoneManagerWSDL_CM ksManager;
        private static GenericFileStorage fileStorage;


        static void Main(string[] args)
        {
            TEST_GetToken(true);

            if (!ksManager.IsValid) { Console.ReadLine(); return; }

            TEST_GetUsername();            

            TEST_SwiftInteractions();
            
            TEST_TxtFileStreamReading();

            //*/
            Console.Read();
        }

        private static void TEST_GetToken(bool isStatic)
        {
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("# TEST > Token #");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("");

            //default login
            string username = "morel";
            string password = "ulahRPCkFXuq2hg3"; 
            string project = "cloudflow";

            if (!isStatic)
            {
                Console.Out.Write("Username = ");
                username = Console.ReadLine();
                Console.Out.Write("Password = ");
                password = Console.ReadLine();
                Console.Out.Write("Project = ");
                project = Console.ReadLine();
            }


            ksManager = new KeystoneManagerWSDL_CF(username, password, project);
            //ksManager = new KeystoneManagerWSDL_CM(username, password, project);

            if (ksManager.IsValid)
            {
                Console.Out.WriteLine("Authentification: Success !");
                Console.Out.WriteLine("Token = " + ksManager.TokenId);
            }
            else
            {
                Console.Out.WriteLine("Authentification: Fail !");
            }

            Console.Out.WriteLine("");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("");
        }

        private static void TEST_GetUsername()
        {
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("# TEST > Token #");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("");

            if (ksManager.IsValid && ksManager.Username != null)
            {
                Console.Out.WriteLine("Username = " + ksManager.Username);
            }
            else
            {
                Console.Out.WriteLine("Unable to get the username !");
            }

            Console.Out.WriteLine("");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("");
        }

        private static void TEST_FileInteractions()
        {
            Console.Out.WriteLine("##########################");
            Console.Out.WriteLine("# TEST > FileInteraction #");
            Console.Out.WriteLine("##########################");
            Console.Out.WriteLine("");


            fileStorage = new GenericFileStorage(ksManager.Username, ksManager.TokenId, StorageType.FILE, "cloudflow");

            List<FileDescription> fds = fileStorage.GetFileDescriptions();

            FileDescription myFile;

            string filename = "image4.PNG";

            List<FileDescription> fdsResult = fds.Where(fd => fd.getVisualName() == filename).ToList();
            if (fdsResult.Count() == 0)
            {
                myFile = fileStorage.UploadFile(@"D:\CloudFlow\" + filename);
            }
            else
            {
                myFile = fdsResult.First();
            }
            fileStorage.DownloadFile(myFile, @"D:\CloudFlow\DL\", "");


            Console.Out.WriteLine("");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("");
        }

        private static void TEST_SwiftInteractions()
        {
            Console.Out.WriteLine("###########################");
            Console.Out.WriteLine("# TEST > SwiftInteraction #");
            Console.Out.WriteLine("###########################");
            Console.Out.WriteLine("");

            string folderName = "file";

            fileStorage = new GenericFileStorage(ksManager.Username, ksManager.TokenId, StorageType.SWIFT, ksManager.Project);

            List<FileDescription> fds = fileStorage.GetFileDescriptions();

            //Test du create & delete folder            
            if (fds.Where(f => f.getVisualName().Equals(folderName)).Count() == 0)
            {
                fileStorage.CreateFolder(folderName);
            }
           
            //Test du upload & download
            fds = fileStorage.GetFileDescriptions(folderName);
            FileDescription myFile;

            string filename = "image2.png";

            List<FileDescription> fdsResult = fds == null ? null : fds.Where(fd => fd.getUniqueName().EndsWith(folderName + "/" + filename)).ToList();
            if (fdsResult == null || fdsResult.Count() == 0)
            {
                myFile = fileStorage.UploadFile(@"D:\CloudFlow\" + filename, folderName);
            }
            else
            {
                myFile = fdsResult.First();
            }
            fileStorage.DownloadFile(myFile, @"D:\CloudFlow\DL\", "");


            Console.Out.WriteLine("");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("");
        }

        private static void TEST_TxtFileStreamReading()
        {
            Console.Out.WriteLine("###########################");
            Console.Out.WriteLine("# TEST > DL from txt file #");
            Console.Out.WriteLine("###########################");
            Console.Out.WriteLine("");


            fileStorage = new GenericFileStorage(ksManager.Username, ksManager.TokenId, StorageType.FILE, "");

            byte[] fileBytes;
            fileStorage.DownloadFile("swift://cloudflow/morel/file/Europe.Stellba.lic", out fileBytes);

            string fileString = System.Text.Encoding.UTF8.GetString(fileBytes);

            Console.Out.WriteLine(fileString);

            Console.Out.WriteLine("");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("");
        }
        
        private static void writeLog(List<string> logs)
        {
            using (StreamWriter writer = new StreamWriter(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\perfos.log", false))
            {
                foreach (string logLine in logs)
                {
                    writer.WriteLine(logLine);
                }
            }

        }

    }
}
