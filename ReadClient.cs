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
using System.Xml.Serialization;
using System.Diagnostics;

namespace Project4Starter
{
    using Util = Utilities;

    ///////////////////////////////////////////////////////////////////////
    // Client class sends and receives messages in this version
    // - commandline format: /L http://localhost:8085/CommService 
    //                       /R http://localhost:8080/CommService
    //   Either one or both may be ommitted

    class ReadClient
    {
        string localUrl { get; set; } = "http://localhost:8081/CommService";
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
        public static void fromXML(string filename, out int num_getval, out int num_children, out int num_containstring)
        {
            XDocument doc = XDocument.Load("numqueries.xml");
            num_getval = Int32.Parse(doc.Root.Element("Message").Element("getValue").Value);
            num_children = Int32.Parse(doc.Root.Element("Message").Element("children").Value);
            num_containstring = Int32.Parse(doc.Root.Element("Message").Element("containstring").Value);
        }


        static void Main(string[] args)
        {
            "Requirement #4: Starting reand client".title();
            Console.Write("\n  starting CommService client");
            Console.Write("\n =============================\n");
            "Requeirement #7".title();

            Console.Title = "Read Client #1";

            List<string> queries = new List<string> { "getValue", "children", "containstring", "Presist" };
            List<string> str2find = new List<string> { "Popvic", "Duncan", "Ginobili", "Belinneli", "Parker", "LeMarcus", "Danny", "Diaw", "Kwaii", "Spurs" };
            List<string> listofkeys = new List<string> { "key1", "key2", "key3", "key4", "key5", "key6", "key7", "key8", "key9" };
            ReadClient Rclnt = new ReadClient();
            Rclnt.processCommandLine(args);
            //List<Message> msgs = new List<Message>();
            //Dictionary<string, int> stats = new Dictionary<string, int>();
            //stats = ReadClient.fromXML("readqueries.xml", ref msgs, Rclnt.getlocalurl(), Rclnt.getremoteurl());
            int num_getval = 0, num_children = 0, num_containstring = 0;
            int num_getval_s = 0, num_children_s = 0, num_containstring_s = 0;
            Stopwatch ticker = new Stopwatch();
            ticker.Start();
            "Requirement #7: Loading read client messages configrations from xml file".title();
            ReadClient.fromXML("numqueries.xml", out num_getval, out num_children, out num_containstring);

            string localPort = Util.urlPort(Rclnt.getlocalurl());
            string localAddr = Util.urlAddress(Rclnt.getlocalurl());
            Receiver rcvr = new Receiver(localPort, localAddr);

            "Requirement #4c".title();

            /* Grabs messages from the server and parse the message */
            Action serviceAction = () =>
            {
                "Requirement #8: Start receiving messages and display the received information:".title();
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
                    if (msgback.content == "Succeed")
                    {
                        Console.Write("\n \n Response received. \n");
                        if (msgback.querytype == "getValue")
                        {
                            Console.Write("\n The element with key " + msgback.strkey + " is:");
                            Console.Write(msgback.strelem.showElement());
                            num_getval_s++;
                        }
                        if (msgback.querytype == "children")
                        {
                            Console.Write("\n The children of element with key " + msgback.strkey + " are:");
                            foreach (string key in msgback.response_strchild.Keys)
                            {
                                Console.Write("\n -------{0}----------", key);
                                Console.Write("\n" + msgback.response_strchild[key].showElement());
                                if (num_children_s < num_children)
                                    num_children_s++;
                            }
                        }
                        if (msgback.querytype == "containstring")
                        {
                            num_containstring_s++;
                            Console.Write("\n Elements contains string \"" +  msgback.strkey + "\" in their metadata are:");
                            foreach (string key in msgback.response_strchild.Keys)
                            {
                                Console.Write("\n -------{0}----------", key);
                                Console.Write("\n" + msgback.response_strchild[key].showElement());
                            }
                        }
                        continue;
                    }
                }
            };

            if (rcvr.StartService())
            {
                rcvr.doService(serviceAction);
            }

            Sender sndr = new Sender(Rclnt.getlocalurl());  // Sender needs localUrl for start message

            Message msg = new Message();
            msg.fromUrl = Rclnt.getlocalurl();
            msg.toUrl = Rclnt.getremoteurl();

            Console.Write("\n  sender's url is {0}", msg.fromUrl);
            Console.Write("\n  attempting to connect to {0}\n", msg.toUrl);

            if (!sndr.Connect(msg.toUrl))
            {
                Console.Write("\n  could not connect in {0} attempts", sndr.MaxConnectAttempts);
                sndr.shutdown();
                rcvr.shutDown();
                return;
            }

            /* Client generate messages according to the configrations from the xml file*/
            /* Persist database to xml file*/
            "Requirement #4b".title();
            Console.Write("\n  sending message to persist database ");
            Console.Write(" from Read Client 1");
            msg.content = "query";
            msg.clienttype = "ReadClient";
            msg.querytype = "Persist";
            if (!sndr.sendMessage(msg))
                return;
            Thread.Sleep(100);

            int counter = 0;
            "Testing query for the value of a specific key".title();
            while (true)
            {
                ++counter;
                //ReadClient.fromXML("Rmessage"+counter.ToString()+".xml", ref msg);
                Console.Write("\n  sending message {0}", counter.ToString());
                Console.Write("from Read Client 1");
                msg.content = "query";
                msg.clienttype = "ReadClient";
                Random r = new Random();
                msg.querytype = "getValue";
                int index = r.Next(str2find.Count);
                msg.strcontain = str2find.ElementAt(index);
                index = r.Next(listofkeys.Count);
                msg.strkey = listofkeys.ElementAt(index);
                if (!sndr.sendMessage(msg))
                    return;
                Thread.Sleep(100);
                if (counter >= num_getval)
                    break;
            }

            counter = 0;
            "Testing query fot children of a specific key".title();
            while (true)
            {
                ++counter;
                //ReadClient.fromXML("Rmessage"+counter.ToString()+".xml", ref msg);
                Console.Write("\n  sending message {0}", counter.ToString());
                Console.Write("from Read Client 1");
                msg.content = "query";
                msg.clienttype = "ReadClient";
                Random r = new Random();
                msg.querytype = "children";
                int index = r.Next(str2find.Count);
                msg.strcontain = str2find.ElementAt(index);
                index = r.Next(listofkeys.Count);
                msg.strkey = listofkeys.ElementAt(index);
                if (!sndr.sendMessage(msg))
                    return;
                Thread.Sleep(100);
                if (counter >= num_children)
                    break;
            }

            counter = 0;
            "\n Testing query for elements with a specific string in their metadata".title();
            while (true)
            {
                ++counter;
                //ReadClient.fromXML("Rmessage"+counter.ToString()+".xml", ref msg);
                Console.Write("\n  sending message {0}", counter.ToString());
                Console.Write("from Read Client 1");
                msg.content = "query";
                msg.clienttype = "ReadClient";
                Random r = new Random();
                msg.querytype = "containstring";
                int index = r.Next(str2find.Count);
                msg.strcontain = str2find.ElementAt(index);
                index = r.Next(listofkeys.Count);
                msg.strkey = listofkeys.ElementAt(index);
                if (!sndr.sendMessage(msg))
                    return;
                Thread.Sleep(100);
                if (counter >= num_containstring)
                    break;
            }
            msg.content = "done";
            sndr.sendMessage(msg);

            ticker.Stop();
            long milisec = 0;
            milisec = ticker.ElapsedMilliseconds;

            // - serviceAction defines what the server does with received messages
            // - This serviceAction just announces incoming messages and echos them
            //   back to the sender.  
            // - Note that demonstrates sender routing works if you run more than
            //   one client.


            // shut down this client's Receiver and Sender by sending close messages
            rcvr.shutDown();
            sndr.shutdown();
            /* Show the statistical result of all read queries*/
            Console.Write("\n Total queries sent are: ");
            Console.Write("\n Get value of key:" + num_getval.ToString());
            Console.Write("\n Get value of key suceed:" + num_getval_s.ToString());
            Console.Write("\n Get value of key failed:" + (num_getval - num_getval_s).ToString());
            Console.Write("\n Get children of key:" + num_children.ToString());
            Console.Write("\n Get children of key suceed:" + num_children_s.ToString());
            Console.Write("\n Get children of key failed:" + (num_children - num_children_s).ToString());
            Console.Write("\n Get elements containing specified string:" + num_containstring.ToString());
            Console.Write("\n Get elements containing specified string suceed:" + num_containstring_s.ToString());
            Console.Write("\n Get elements containing specified string failed:" + (num_containstring - num_containstring_s).ToString());
            "Requirement #8: High-resolution timer:".title();
            Console.Write("\n Total elapsed time is " + milisec.ToString() + "ms.");
            // Wait for user to press a key to quit.
            // Ensures that client has gotten all server replies.
            Util.waitForUser();
            Console.Write("\n\n");
        }
    }
#if (TEST_ReadClient)
    class TestReadClient
    {
        static void Main(string[] args)
        {
            ReadClient rdcln = new ReadClient();
            Run.(rdcln.Main());
        }
    }
#endif
}
