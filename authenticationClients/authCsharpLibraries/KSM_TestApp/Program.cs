using AuthentificationWSDL;
using AuthentificationWSDL.CFAuth;
using AuthentificationWSDL.CMAuth;
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
        private static KeystoneManagerWSDL_CM ksManager_CM;
        private static KeystoneManagerWSDL_CF ksManager_CF;

        static void Main(string[] args)
        {
            TEST_GetTokenCF(true);
            if (ksManager_CF.IsValid) TEST_GetUsername_CF();

            TEST_GetTokenCM(true);
            if (ksManager_CM.IsValid) TEST_GetUsername_CM();

            Console.ReadLine();
        }


        private static void TEST_GetTokenCF(bool isStatic)
        {
            Console.Out.WriteLine("##################");
            Console.Out.WriteLine("# TEST > Token CF #");
            Console.Out.WriteLine("##################");
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


            ksManager_CF = new KeystoneManagerWSDL_CF(username, password, project);

            if (ksManager_CF.IsValid)
            {
                Console.Out.WriteLine("Authentification: Success !");
                Console.Out.WriteLine("Token = " + ksManager_CF.TokenId);
            }
            else
            {
                Console.Out.WriteLine("Authentification: Fail !");
            }

            Console.Out.WriteLine("");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("");
        }

        private static void TEST_GetTokenCM(bool isStatic)
        {
            Console.Out.WriteLine("##################");
            Console.Out.WriteLine("# TEST > Token CM #");
            Console.Out.WriteLine("##################");
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


            ksManager_CM = new KeystoneManagerWSDL_CM(username, password, project);

            if (ksManager_CM.IsValid)
            {
                Console.Out.WriteLine("Authentification: Success !");
                Console.Out.WriteLine("Token = " + ksManager_CM.TokenId);
            }
            else
            {
                Console.Out.WriteLine("Authentification: Fail !");
            }

            Console.Out.WriteLine("");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("");
        }

        private static void TEST_GetUsername_CF()
        {
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("# TEST > Token #");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("");

            if (ksManager_CF.IsValid && ksManager_CF.Username != null)
            {
                Console.Out.WriteLine("Username = " + ksManager_CF.Username);
            }
            else
            {
                Console.Out.WriteLine("Unable to get the username !");
            }

            Console.Out.WriteLine("");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("");
        }

        private static void TEST_GetUsername_CM()
        {
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("# TEST > Token #");
            Console.Out.WriteLine("################");
            Console.Out.WriteLine("");

            if (ksManager_CM.IsValid && ksManager_CM.Username != null)
            {
                Console.Out.WriteLine("Username = " + ksManager_CM.Username);
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
