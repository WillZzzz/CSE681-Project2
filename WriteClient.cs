/////////////////////////////////////////////////////////////////////////
// Client1.cs - CommService client sends and receives messages         //
// ver 2.1                                                             //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Project #4    //
/////////////////////////////////////////////////////////////////////////
/*
 * Additions to C# Console Wizard generated code:
 * - Added using System.Threading
 * - Added reference to ICommService, Sender, Receiver, Utilities
 *
 * Note:
 * - in this incantation the client has Sender and now has Receiver to
 *   retrieve Server echo-back messages.
 * - If you provide command line arguments they should be ordered as:
 *   remotePort, remoteAddress, localPort, localAddress
 */
/*
 * Maintenance History:
 * --------------------
 * ver 2.1 : 29 Oct 2015
 * - fixed bug in processCommandLine(...)
 * - added rcvr.shutdown() and sndr.shutDown() 
 * ver 2.0 : 20 Oct 2015
 * - replaced almost all functionality with a Sender instance
 * - added Receiver to retrieve Server echo messages.
 * - added verbose mode to support debugging and learning
 * - to see more detail about what is going on in Sender and Receiver
 *   set Utilities.verbose = true
 * ver 1.0 : 18 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;
using System.Xml;
using System.Diagnostics;

namespace Project4Starter
{
    using Util = Utilities;

    ///////////////////////////////////////////////////////////////////////
    // Client class sends and receives messages in this version
    // - commandline format: /L http://localhost:8085/CommService 
    //                       /R http://localhost:8080/CommService
    //   Either one or both may be ommitted

    class WriteClient
    {
        string localUrl { get; set; } = "http://localhost:8082/CommService";
        string remoteUrl { get; set; } = "http://localhost:8080/CommService";

        //----< retrieve urls from the CommandLine if there are any >--------

        public string getlocalurl() { return this.localUrl; }
        public string getremoteurl() { return this.remoteUrl; }
        public void setlocalurl(string local) { this.localUrl = local; }
        public void setremoteurl(string remote) { this.remoteUrl = remote; }
        public void processCommandLine(string[] args)
        {
            if (args.Length == 0)
                return;
            localUrl = Util.processCommandLineForLocal(args, localUrl);
            remoteUrl = Util.processCommandLineForRemote(args, remoteUrl);
        }

        /* This function reads message configrations from xml file and pass the statics to the client */
        public static void fromXML(string filename, out int num_add, out int num_delete, out int num_edit)
        {
            XDocument doc = XDocument.Load("numqueries.xml");
            num_add = Int32.Parse(doc.Root.Element("Message").Element("addition").Value);
            num_delete = Int32.Parse(doc.Root.Element("Message").Element("deletion").Value);
            num_edit = Int32.Parse(doc.Root.Element("Message").Element("edition").Value);
        }

        static void Main(string[] args)
        {
            "Requirement #4: Starting reand client".title();
            Console.Write("\n  starting CommService client");
            Console.Write("\n =============================\n");

            "Requirement #5".title();

            Console.Title = "Write Client #1";

            WriteClient Wclnt = new WriteClient();
            Wclnt.processCommandLine(args);

            int num_add = 0, num_delete = 0, num_edit = 0;
            int num_add_s = 0, num_delete_s = 0, num_edit_s = 0;
            Stopwatch ticker = new Stopwatch();
            ticker.Start();
            "Requirement #5: Loading write client message configration from xml file".title();
            WriteClient.fromXML("numqueries.xml", out num_add, out num_delete, out num_edit);

            string localPort = Util.urlPort(Wclnt.getlocalurl());
            string localAddr = Util.urlAddress(Wclnt.getlocalurl());
            Receiver rcvr = new Receiver(localPort, localAddr);

            Sender sndr = new Sender(Wclnt.getlocalurl());  // Sender needs localUrl for start message

            Message msg = new Message();
            Message msg1 = new Message();
            Message msg2 = new Message();
            msg.fromUrl = Wclnt.getlocalurl();
            msg.toUrl = Wclnt.getremoteurl();
            msg1.fromUrl = Wclnt.getlocalurl();
            msg1.toUrl = Wclnt.getremoteurl();
            msg2.fromUrl = Wclnt.getlocalurl();
            msg2.toUrl = Wclnt.getremoteurl();
            msg1.content = "query";
            msg2.content = "query";

            Console.Write("\n  sender's url is {0}", msg.fromUrl);
            Console.Write("\n  attempting to connect to {0}\n", msg.toUrl);

            if (!sndr.Connect(msg.toUrl))
            {
                Console.Write("\n  could not connect in {0} attempts", sndr.MaxConnectAttempts);
                sndr.shutdown();
                //rcvr.shutDown();
                return;
            }
            "Requirement #4a".title();
            /* Grabs messages from the server and parse the message */
            Action serviceAction = () =>
            {
                Message msgback = null;
                while (true)
                {
                    msgback = rcvr.getMessage();   // note use of non-service method to deQ messages
                    Console.Write("\n  Received message:");
                    Console.Write("\n  sender is {0}", msgback.fromUrl);
                    Console.Write("\n  content is {0}\n", msgback.content);

                    if (msgback.content == "Failed")
                    {
                        Console.Write("\n Query failed"); // query failed
                        continue;
                    }
                    if (msgback.content == "Addition succeed")
                    {
                        Console.Write("\n \n Addition succeed. \n");
                        num_add_s++;
                        continue;
                    }
                    if (msgback.content == "Edition succeed")
                    {
                        Console.Write("\n \n Addition succeed. \n");
                        num_edit_s++;
                        continue;
                    }
                    if (msgback.content == "Deletion succeed")
                    {
                        Console.Write("\n \n Addition succeed. \n");
                        num_delete_s++;
                        continue;
                    }
                }
            };

            if (rcvr.StartService())
            {
                rcvr.doService(serviceAction);
            }

            /* Client generate messages according to the configrations from the xml file*/
            "Testing adding an element remotedly".title();
            int counter = 0;
            while (true)
            {
                ++counter;
                msg1.clienttype = "WriteClient";
                msg2.clienttype = "WriteClient";
                msg1.intkey = counter;
                msg2.strkey = counter.ToString();
                msg1.querytype = "add";
                msg2.querytype = "add";
                msg1.elem = new List<string>();
                msg2.elem = new List<string>();
                msg1.elem.Add("new added name #" + counter.ToString());
                msg1.elem.Add("new added descr #" + counter.ToString());
                msg1.elem.Add("new added payload #" + counter.ToString());
                msg1.elem.Add("new added children #" + (counter - 1).ToString() + (counter - 2).ToString());
                msg2.elem.Add("new added name #" + counter.ToString());
                msg2.elem.Add("new added descr #" + counter.ToString());
                msg2.elem.Add("new added payload #" + counter.ToString());
                msg2.elem.Add("new added children #" + (counter - 1).ToString() + (counter - 2).ToString());
                Console.Write("\n  sending message {0}", counter.ToString());
                Console.Write("from Write Client 1");
                if (!sndr.sendMessage(msg1))
                    return;
                if (!sndr.sendMessage(msg2))
                    return;
                Thread.Sleep(100);
                if (counter >= num_add)
                    break;
            }

            counter = 0;
            "Testing editing an element remotedly".title();
            while (true)
            {
                ++counter;
                msg1.clienttype = "WriteClient";
                msg2.clienttype = "WriteClient";
                msg1.intkey = counter;
                msg2.strkey = counter.ToString();
                msg1.querytype = "edit";
                msg2.querytype = "edit";
                msg1.elem.Add("new edited name #" + counter.ToString());
                msg1.elem.Add("new edited descr #" + counter.ToString());
                msg1.elem.Add("new edited payload #" + counter.ToString());
                msg1.elem.Add("new edited children #" + (counter - 1).ToString() + (counter - 2).ToString());
                msg2.elem.Add("new edited name #" + counter.ToString());
                msg2.elem.Add("new edited descr #" + counter.ToString());
                msg2.elem.Add("new edited payload #" + counter.ToString());
                msg2.elem.Add("new edited children :" + (counter - 1).ToString() + (counter - 2).ToString());
                Console.Write("\n  sending message {0}", counter.ToString());
                Console.Write("from Write Client 1");
                if (!sndr.sendMessage(msg1))
                    return;
                if (!sndr.sendMessage(msg2))
                    return;
                Thread.Sleep(100);
                if (counter >= num_edit)
                    break;
            }

            counter = 0;
            "Testing deleting an element remotedly".title();
            while (true)
            {
                ++counter;
                msg1.clienttype = "WriteClient";
                msg2.clienttype = "WriteClient";
                msg1.intkey = counter;
                msg2.strkey = counter.ToString();
                msg1.querytype = "delete";
                msg2.querytype = "delete";
               Console.Write("\n  sending message {0}", counter.ToString());
                Console.Write("from Write Client 1");
                if (!sndr.sendMessage(msg1))
                    return;
                if (!sndr.sendMessage(msg2))
                    return;
                Thread.Sleep(100);
                if (counter >= num_delete)
                    break;
            }
            msg.content = "done";
            sndr.sendMessage(msg);

            ticker.Stop();
            long milisec = 0;
            milisec = ticker.ElapsedMilliseconds;
            // shut down this client's Receiver and Sender by sending close messages
            rcvr.shutDown();
            sndr.shutdown();

            /* Show the statistical result of all write queries*/
            Console.Write("\n Total queries sent are: ");
            Console.Write("\n Addition:" + num_add.ToString());
            Console.Write("\n Addtion suceed:" + num_add_s.ToString());
            Console.Write("\n Addtion failed:" + (num_add - num_add_s).ToString());
            Console.Write("\n Deletion:" + num_delete.ToString());
            Console.Write("\n Deletion suceed:" + num_delete_s.ToString());
            Console.Write("\n Deletion failed:" + (num_delete - num_delete_s).ToString());
            Console.Write("\n Edition:" + num_edit.ToString());
            Console.Write("\n Edition suceed:" + num_edit_s.ToString());
            Console.Write("\n Edition failed:" + (num_edit - num_edit_s).ToString());
            "Requirement #5: High-resolution timer:".title();
            Console.Write("\n Total elapsed time is " + milisec.ToString() + "ms.");

            // Wait for user to press a key to quit.
            // Ensures that client has gotten all server replies.
            Util.waitForUser();
            Console.Write("\n\n");
        }
    }

#if (TEST_WriteClient)
    class TestWriteClient
    {
        static void Main(string[] args)
        {
            WriteClient rtcln = new WriteClient();
            Run.(rtcln.Main());
        }
    }
#endif
}

