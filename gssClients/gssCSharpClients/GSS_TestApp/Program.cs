using AuthentificationWSDL2;
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
        private static KeystoneManagerWSDL ksManager;
        private static GenericFileStorage fileStorage;


        static void Main(string[] args)
        {
            TEST_GetToken(true);

            if (!ksManager.IsValid) { Console.ReadLine(); return; }

            TEST_GetUsername();            

            TEST_SwiftInteractions();

            TEST_Perfos();

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
            string username = "caxman";
            string password = "andR0b1n"; 
            string project = "caxman";

            if (!isStatic)
            {
                Console.Out.Write("Username = ");
                username = Console.ReadLine();
                Console.Out.Write("Password = ");
                password = Console.ReadLine();
                Console.Out.Write("Project = ");
                project = Console.ReadLine();
            }


            ksManager = new KeystoneManagerWSDL(username, password, project);

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
            Console.Out.WriteLine("##########################");
            Console.Out.WriteLine("# TEST > FileInteraction #");
            Console.Out.WriteLine("##########################");
            Console.Out.WriteLine("");

            string folderName = "file";

            fileStorage = new GenericFileStorage(ksManager.Username, ksManager.TokenId, StorageType.SWIFT, ksManager.Project);

            //fileStorage.CreateFolder("swift://cloudflow/", folderName); 

            List<FileDescription> fds = fileStorage.GetFileDescriptions();

            //Test du create & delete folder            
            if (fds.Where(f => f.getVisualName().Equals(folderName)).Count() == 0)
            {
                fileStorage.CreateFolder(folderName);
            }
            /*
            fileStorage.DeleteFolder(folderName);
            fds = fileStorage.GetFileDescriptions();
            fileStorage.CreateFolder(folderName);
            //*/

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

        private static void TEST_PlmInteractions()
        {
            Console.Out.WriteLine("##########################");
            Console.Out.WriteLine("# TEST > PLM Interaction #");
            Console.Out.WriteLine("##########################");
            Console.Out.WriteLine("");


            fileStorage = new GenericFileStorage(ksManager.Username, ksManager.TokenId, StorageType.PLM, "cloudflow");


            string plmRootPath = "InitialRepository/Missler/";
            browsePlm(plmRootPath);


            Console.Out.WriteLine("");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine(" FINISHED ");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("");
        }

        private static void browsePlm(string rootFolder)
        {
            Console.Out.WriteLine("");
            Console.Out.WriteLine("### " + rootFolder + " ###");
            Console.Out.WriteLine("");

            List<FileDescription> fds = fileStorage.GetPlmFileDescriptions(rootFolder);

            if (fds == null) return;

            foreach (var fd in fds)
            {
                Console.Out.WriteLine("  " + fd.getVisualName());
                Console.Out.WriteLine("     " + fd.getId());
                Console.Out.WriteLine("     " + fd.getUniqueName());
                Console.Out.WriteLine("     " + fd.getType());
                Console.Out.WriteLine("");

                if (fd.getType() == "FILE")
                {
                    try
                    {
                        var tt = fileStorage.DownloadFile(fd, @"D:\CloudFlow\DL\", "");
                        Console.Out.WriteLine("DL completed :)");
                    }
                    catch (Exception ex)
                    {
                        Console.Out.WriteLine("DL FAIL !!! \r\n" + ex.ToString());
                    }
                    Console.Out.WriteLine("");
                }

                browsePlm(fd.getUniqueName());
            }
        }

        private static void TEST_Perfos()
        {
            Console.Out.WriteLine("##########################");
            Console.Out.WriteLine("# TEST > Perfos          #");
            Console.Out.WriteLine("##########################");
            Console.Out.WriteLine("");

            List<string> perfos = new List<string>();

            double ulTotalTime = 0.0;
            double dlTotalTime = 0.0;

            string filename = "image2.png";
            FileInfo fi = new FileInfo(@"D:\CloudFlow\" + filename);
            long fileSize = fi.Length;

            int bufferSize = 128;

            for (int l = 0; l < 4; l++)
            {
                perfos.Add("##########################");
                perfos.Add("# " + bufferSize + " #");
                perfos.Add("##########################");

                fileStorage = new GenericFileStorage(ksManager.Username, ksManager.TokenId, StorageType.SWIFT, "cloudflow");
                fileStorage.BufferSize = bufferSize;

                string folderName = "perfos";

                List<FileDescription> fds = fileStorage.GetFileDescriptions();
                //Test du create & delete folder            
                if (fds.Where(f => f.getVisualName().Equals(folderName)).Count() == 0)
                {
                    fileStorage.CreateFolder(folderName);
                }



                Console.Out.WriteLine("");

                for (int j = 0; j < 10; j++)
                {
                    fds = fileStorage.GetFileDescriptions(folderName);
                    FileDescription myFile;


                    List<FileDescription> fdsResult = fds == null ? null : fds.Where(fd => fd.getUniqueName().EndsWith(folderName + "/" + filename)).ToList();
                    if (fdsResult != null && fdsResult.Count() != 0)
                    {
                        fileStorage.RemoveFile(fdsResult.First());
                    }

                    DateTime startDt = DateTime.Now;
                    myFile = fileStorage.UploadFile(@"D:\CloudFlow\" + filename, folderName);
                    double nbSec = (DateTime.Now - startDt).TotalSeconds;
                    double bw = fileSize / 1024 / nbSec;
                    perfos.Add("UL = " + nbSec + " s" + " >> " + bw + " ko/s");

                    ulTotalTime += nbSec;

                    startDt = DateTime.Now;
                    fileStorage.DownloadFile(myFile, @"D:\CloudFlow\DL\", "");
                    nbSec = (DateTime.Now - startDt).TotalSeconds;
                    bw = fileSize / 1024 / nbSec;
                    perfos.Add("DL = " + nbSec + " s" + " >> " + bw + " ko/s");

                    dlTotalTime += nbSec;

                    Console.Out.Write(" -");
                }

                bufferSize = bufferSize * 2;

                perfos.Add(">>>> UL moyen = " + fileSize * 10 / ulTotalTime);
                perfos.Add(">>>> DL moyen = " + fileSize * 10 / dlTotalTime);

                writeLog(perfos);
                ulTotalTime = 0;
                dlTotalTime = 0;
            }
            Console.Out.WriteLine("");
            Console.Out.WriteLine("");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("");
        }

        private static int requestNumber = 500;
        private static List<string> logs = new List<string>();
        private static void TEST_KSM()
        {
            Console.Out.WriteLine("##########################");
            Console.Out.WriteLine("# TEST > KSM #");
            Console.Out.WriteLine("##########################");
            Console.Out.WriteLine("");

            List<Thread> thList = new List<Thread>(Environment.ProcessorCount * 2);

            logs.Add("Thread name;Start date;Request duration");

            for (int j = 0; j < Environment.ProcessorCount * 2; j++)
            {
                Thread th = new Thread(AskKeystones);
                th.Name = "Thread " + j;
                th.Start();
                thList.Add(th);
            }


            foreach (Thread th in thList)
            {
                while (th.IsAlive)
                {
                    Thread.Sleep(100);
                }
            }

            writeLog(logs);

            Console.Out.WriteLine("");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine(" FINISHED ");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("");
        }



        private static void AskKeystones()
        {
            Random rdm = new Random();

            while (requestNumber > 0)
            {
                requestNumber--;

                string username = "morel";
                string password = "ulahRPCkFXuq2hg3";

                DateTime startDate = DateTime.Now;
                var ksm = new KeystoneManagerWSDL(username, password, null);
                //Thread.Sleep(rdm.Next(200));
                DateTime endDate = DateTime.Now;

                //lock (logs)
                {
                    logs.Add(Thread.CurrentThread.Name + ";" + startDate.ToString("HH:mm:ss.fff") + ";" + (endDate - startDate).TotalSeconds);
                }
            }


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
