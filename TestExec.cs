///////////////////////////////////////////////////////////////////
// TestExec.cs - Test Requirements for Project #4                //
// Ver 1.0                                                       //
// Application: Demonstration for CSE681-SMA, Project#4          //
// Language:    C#, ver 6.0, Visual Studio 2015                  //          //
// Author: Tianhang Zhang, tzhan116@syr.edu                      //
///////////////////////////////////////////////////////////////////

/*
 * Package Operations:
 * -------------------
 * This package demonstrates that the remote NoSqlDb meets all requirements
 * of Project #4, F2015.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: 
 *   TestExec.cs,  DBElement.cs, DBEngine, PayloadWrapper, Display, 
 *   QueryEngine.cs, ItemFactory.cs, 
 *   DBExtensions.cs, UtilityExtensions.cs
 *
 *
 * Maintenance History:
 * --------------------
 * ver 1.0 : 20 Nov 15
 * - first release
 *
 */
//ToDo: 6 - add Persistance package

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using System.Threading;
using static System.Console;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

namespace Project4Starter
{

    class ProcessStarter
    {
        static bool isWClnt = false;
        static int wNo = -1;
        static bool isRClnt = false;
        static int rNo = -1;
        static bool isWPFClnt = false;
        static bool isLog = false;
        static int portNo = 8082;

        public bool startProcess(string process, string localUrl)
        {
            process = Path.GetFullPath(process);
            //Console.Write("\n  fileSpec - \"{0}\"", process);

            StringBuilder lg = new StringBuilder("");
            if (isLog) { lg.Append("o "); }
            lg.Append("/l ").Append(localUrl).Append(" /r ");
            lg.Append("http://localhost:8080/CommService");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = process,
                Arguments = lg.ToString(),
                // set UseShellExecute to true to see child console, false hides console
                UseShellExecute = true
            };
            try
            {
                Process p = Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// parse command line arguments to decide which client to start and whether log
        /// is needed
        /// </summary>
        /// <param name="args">command line arguments</param>
        static public void processCommandLine(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if ((args.Length >= i + 1) && (args[i] == "/r" || args[i] == "/R"))
                {
                    isRClnt = true;
                    rNo = 1;
                    int value;
                    if ((args.Length >= i + 2) && (int.TryParse(args[i + 1], out value)))
                    {
                        rNo = value;
                    }
                }
                if ((args.Length >= i + 1) && (args[i] == "/w" || args[i] == "/W"))
                {
                    isWClnt = true;
                    wNo = 1;
                    if ((args.Length >= i + 2) && (args[i + 1] == "o" || args[i] == "O"))
                    {
                        isLog = true;
                    }
                    int value;
                    if ((args.Length >= i + 2) && (int.TryParse(args[i + 1], out value)))
                    {
                        wNo = value;
                        if ((args.Length >= i + 3) && (args[i + 2] == "o" || args[i] == "O"))
                        {
                            isLog = true;
                        }
                    }
                }
                if ((args.Length >= i + 1) && (args[i] == "/p" || args[i] == "/P"))
                {
                    isWPFClnt = true;
                }
            }
        }

        // return port no if write client return odd num
        // otherwise if read client return even num
        static private string portStr(string s)
        {
            if (s == "r")
            {
                if ((portNo % 2) == 0) portNo = portNo + 2;
                else ++portNo;
            }
            else
            {
                if ((portNo % 2) == 0) portNo = portNo + 1;
                else portNo = portNo + 2;
            }
            return portNo.ToString();
        }

        static void Main(string[] args)
        {
            processCommandLine(args);


            Console.Write("\n  current directory is: \"{0}\"", Directory.GetCurrentDirectory());
            ProcessStarter pServer = new ProcessStarter();
            string sUrl = Utilities.makeUrl("localhost", "8080");
            pServer.startProcess("Server/bin/Debug/Server.exe", sUrl);
            if (isRClnt)
            {
                for (int i = 1; i <= rNo; ++i)
                {
                    ProcessStarter rClnt = new ProcessStarter();
                    string rUrl = Utilities.makeUrl("localhost", portStr("r"));
                    rClnt.startProcess("Client2/bin/Debug/Client2.exe", rUrl);
                    Console.Write("\n  Read Client started");
                }

            }
            if (isWClnt)
            {
                for (int i = 1; i <= wNo; ++i)
                {
                    ProcessStarter wClnt = new ProcessStarter();
                    string wUrl = Utilities.makeUrl("localhost", portStr("w"));
                    wClnt.startProcess("Client/bin/Debug/Client.exe", wUrl);
                    Console.Write("\n  Write Client started");
                }
            }
            if (isWPFClnt)
            {
                ProcessStarter wClnt = new ProcessStarter();
                wClnt.startProcess("WpfClient/bin/Debug/WpfApplication1.exe", "8081");
                Console.Write("\n  WPF Client started");
            }
            Console.Write("\n  press key to exit: ");
            Console.ReadKey();
        }
    }
}
