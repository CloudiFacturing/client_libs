using AuthentificationWSDL2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSM_TestApp
{
    class Program
    {

        private static KeystoneManagerWSDL ksManager;

        static void Main(string[] args)
        {
            TEST_GetToken(true);
            
            if (!ksManager.IsValid) { Console.ReadLine(); return; }

            TEST_GetUsername();            
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
