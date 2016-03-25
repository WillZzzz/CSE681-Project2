/////////////////////////////////////////////////////////////////////////
// Server.cs - CommService server                                      //
// ver 2.3                                                             //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Project #4    //
// Tianhang Zhang, CSE681 - Software Modeling and Analysis, Project #4 //
/////////////////////////////////////////////////////////////////////////
/*
 * Additions to C# Console Wizard generated code:
 * - Added reference to ICommService, Sender, Receiver, Utilities
 *
 * Note:
 * - This server now receives and then sends back received messages.
 */
/*
 * Plans:
 * - Add message decoding and NoSqlDb calls in performanceServiceAction.
 * - Provide requirements testing in requirementsServiceAction, perhaps
 *   used in a console client application separate from Performance 
 *   Testing GUI.
 */
/*
 * Maintenance History:
 * --------------------
 * ver 2.4 : 20 Nov 2015
 * - added processing of both read and write messages
 * ver 2.3 : 29 Oct 2015
 * - added handling of special messages: 
 *   "connection start message", "done", "closeServer"
 * ver 2.2 : 25 Oct 2015
 * - minor changes to display
 * ver 2.1 : 24 Oct 2015
 * - added Sender so Server can echo back messages it receives
 * - added verbose mode to support debugging and learning
 * - to see more detail about what is going on in Sender and Receiver
 *   set Utilities.verbose = true
 * ver 2.0 : 20 Oct 2015
 * - Defined Receiver and used that to replace almost all of the
 *   original Server's functionality.
 * ver 1.0 : 18 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Timers;

namespace Project4Starter
{
  using Util = Utilities;

///////////////////////////////////////////////////////////////////////
// These aliases simplify code text, but make it harder
// to remember exactly what the code is trying to do.
//

using DBint = DBEngine<int, DBElement<int, PL_String>>;
using DBstr = DBEngine<string, DBElement<string, PL_ListOfStrings>>;
using DBElemint = DBElement<int, PL_String>;
using DBElemstr = DBElement<string, PL_ListOfStrings>;

    class Server
  {
    string address { get; set; } = "localhost";
    string port { get; set; } = "8080";

        // Databases are initialized with server startup

    //DBEngine<int, DBElement<int, PL_String>> db1 = new DBEngine<int, DBElement<int, PL_String>>();
    //DBEngine<string, DBElement<string, PL_ListOfStrings>> db2 = new DBEngine<string, DBElement<string, PL_ListOfStrings>>();

    //----< quick way to grab ports and addresses from commandline >-----

        public void ProcessCommandLine(string[] args)
    {
      if (args.Length > 0)
      {
        port = args[0];
      }
      if (args.Length > 1)
      {
        address = args[1];
      }
    }

        /* This function catch the contents in read messages and execute as required */
        public void processRMessage(Message msg, DBint db1, DBstr db2, ref Message msgback)
        {
            if (msg.strkey == null)
            {
                if (msg.querytype == "getValue")
                {
                    if (db1.containsKey(msg.intkey))
                    {
                        DBElemint tmp_elem = new DBElemint();
                        db1.getValue(msg.intkey, out tmp_elem);
                        msgback.intelem = tmp_elem;
                        Console.Write("\n Element found!");
                        msgback.content = "Retrieving succeed";
                    }
                    else
                    {
                        msgback.content = "Fail";
                        Console.Write("\n Element not found :<");
                    }
                }
                if (msg.querytype == "children")
                {
                    if (db1.containsKey(msg.intkey))
                    {
                        Dictionary<int, DBElemint> response_child = new Dictionary<int, DBElemint>();
                        DBElemint elem = new DBElemint();
                        db1.getValue(msg.intkey, out elem);
                        foreach (int key in elem.children)
                        {
                            DBElemint child = new DBElemint();
                            db1.getValue(key, out child);
                            response_child.Add(key, child);
                        }
                        msgback.response_intchild = response_child;
                        Console.Write("\n Retrieving children succeed!");
                        msgback.content = "Succeed";
                    }
                    else
                    {
                        msgback.content = "Fail";
                        Console.Write("\n Retrieving children failed :<");
                    }
                }
            }
            if (msg.intkey == 0)
            {
                if (msg.querytype == "getValue")
                {
                    if (db2.containsKey(msg.strkey))
                    {
                        DBElemstr tmp_elem = new DBElemstr();
                        db2.getValue(msg.strkey, out tmp_elem);
                        msgback.strelem = tmp_elem;
                        Console.Write("\n Element found!");
                        msgback.content = "Retrieving succeed";
                    }
                    else
                    {
                        Console.Write("\n Element not found :<");
                        msgback.content = "Fail";
                    }
                }
                if (msg.querytype == "children")
                {
                    if (db2.containsKey(msg.strkey))
                    {
                        Dictionary<string, DBElemstr> response_child = new Dictionary<string, DBElemstr>();
                        DBElemstr elem = new DBElemstr();
                        db2.getValue(msg.strkey, out elem);
                        foreach (string key in elem.children)
                        {
                            DBElemstr child = new DBElemstr();
                            db2.getValue(key, out child);
                            response_child.Add(key, child);
                        }
                        msgback.response_strchild = response_child;
                        Console.Write("\n Retrieving children succeed!");
                        msgback.content = "Succeed";
                    }
                    else
                    {
                        Console.Write("\n Retrieving children failed :<");
                        msgback.content = "Fail";
                    }
                }
                if (msg.querytype == "containstring")
                {
                    Dictionary<string, DBElemstr> response_child = new Dictionary<string, DBElemstr>();
                    foreach (string key in db2.Keys())
                    {
                        DBElemstr elem = new DBElemstr();
                        db2.getValue(key, out elem);
                        bool match = false;
                        if (elem.name.Contains(msg.strcontain))
                            match = true;
                        if (elem.descr.Contains(msg.strcontain))
                            match = true;
                        if (match)
                            response_child.Add(key, elem);
                    }
                    if (response_child.Count() > 0)
                    {
                        Console.Write("\n Found matching element!");
                        msgback.response_strchild = response_child;
                        msgback.content = "Matching element found";
                    }
                    else
                    {
                        Console.Write("\n No matching element found :<");
                        msgback.content = "No element match this string";
                    }
                }
                if (msg.querytype == "Persist")
                {
                    XDocument doc1 = new XDocument();
                    doc1 = XDocument.Parse(db1.ToXml<DBElemint>());
                    XDocument doc2 = new XDocument();
                    doc2 = XDocument.Parse(db2.ToXml<DBElemstr>());
                    doc1.Save("PersistedRemotedly1.xml");
                    doc2.Save("PersistedRemotedly2.xml");
                }
            }
        }
        /* This function catch the contents in write messages and execute as required */
        public void processWMessage(Message msg, DBint db1, DBstr db2, ref Message msgback)
        {
            if (msg.strkey == null)
            {
                if (msg.querytype == "add")
                {
                    Console.Write("\n \n Add new element to database;");
                    Console.Write("\n Original database:");
                    db1.showDB<int, DBElemint, PL_String>();
                    DBElemint elem = new DBElemint();
                    elem.name = msg.elem.ElementAt<string>(0);
                    elem.descr = msg.elem.ElementAt<string>(1);
                    elem.payload = new PL_String(msg.elem.ElementAt<string>(2));
                    //elem.children = msg.elem.ElementAt<string>(0);
                    db1.insert(msg.intkey, elem);
                    msgback.content = "Addition succeed";
                    Console.Write("\n \n New database:");
                    db1.showDB<int, DBElemint, PL_String>();
                }
                if (msg.querytype == "edit")
                {
                    Console.Write("\n \n Edit an existing element;");
                    Console.Write("\n Original database:");
                    db1.showDB<int, DBElemint, PL_String>();
                    if (db1.Keys().Contains(msg.intkey))
                    {
                        DBElemint elem = new DBElemint();
                        db1.getValue(msg.intkey, out elem);
                        if (msg.elem.ElementAt<string>(0) != null)
                            elem.name = msg.elem.ElementAt<string>(0);
                        if (msg.elem.ElementAt<string>(1) != null)
                            elem.descr = msg.elem.ElementAt<string>(0);
                        if (msg.elem.ElementAt<string>(2) != null)
                            elem.payload = new PL_String(msg.elem.ElementAt<string>(2));
                        db1.remove(msg.intkey);
                        db1.insert(msg.intkey, elem);
                    }
                    msgback.content = "Edition succeed";
                    Console.Write("\n \n New database:");
                    db1.showDB<int, DBElemint, PL_String>();
                }
                if ( msg.querytype == "delete")
                {
                    Console.Write("\n \n Delete existing element from database;");
                    Console.Write("\n Original database:");
                    db1.showDB<int, DBElemint, PL_String>();
                    if (db1.Keys().Contains(msg.intkey))
                        db1.remove(msg.intkey);
                    msgback.content = "Deletion succeed";
                    Console.Write("\n \n New database:");
                    db1.showDB<int, DBElemint, PL_String>();
                }
            }
            if (msg.intkey == 0)
            {
                if (msg.querytype == "add")
                {
                    if (db2.containsKey(msg.strkey))
                    {
                        Console.Write("\n \n Add new element to database;");
                        Console.Write("\n Original database:");
                        db2.showDB<string, DBElemstr, PL_ListOfStrings>();
                        DBElemstr elem = new DBElemstr();
                        elem.name = msg.elem.ElementAt<string>(0);
                        elem.descr = msg.elem.ElementAt<string>(1);
                        List<string> tmp_ls = new List<string>();
                        tmp_ls.Add("msg.elem.ElementAt<string>(2)");
                        elem.payload = new PL_ListOfStrings(tmp_ls);
                        //elem.children = msg.elem.ElementAt<string>(0);
                        db2.insert(msg.strkey, elem);
                        msgback.content = "Addition succeed";
                        Console.Write("\n \n New database:");
                        db2.showDB<string, DBElemstr, PL_ListOfStrings>();
                    }
                    else
                        msgback.content = "Addition failed! Key exists.";
                }
                if (msg.querytype == "edit")
                {
                    Console.Write("\n \n Edit an existing element;");
                    Console.Write("\n Original database:");
                    db2.showDB<string, DBElemstr, PL_ListOfStrings>();
                    if (db2.Keys().Contains(msg.strkey))
                    {
                        DBElemstr elem = new DBElemstr();
                        db2.getValue(msg.strkey, out elem);
                        if (msg.elem.ElementAt<string>(0) != null)
                            elem.name = msg.elem.ElementAt<string>(0);
                        if (msg.elem.ElementAt<string>(1) != null)
                            elem.descr = msg.elem.ElementAt<string>(0);
                        if (msg.elem.ElementAt<string>(2) != null)
                        {
                            List<string> tmp_ls = new List<string>();
                            tmp_ls.Add("msg.elem.ElementAt<string>(2)");
                            elem.payload = new PL_ListOfStrings(tmp_ls);
                        }
                        db2.remove(msg.strkey);
                        db2.insert(msg.strkey, elem);
                        msgback.content = "Edition succeed";
                        Console.Write("\n \n New database:");
                        db2.showDB<string, DBElemstr, PL_ListOfStrings>();
                    }
                    else
                        msgback.content = "Edition failed! Key doesn't exist.";
                }
                if (msg.querytype == "delete")
                {
                    Console.Write("\n \n Delete existing element from database;");
                    Console.Write("\n Original database:");
                    db2.showDB<string, DBElemstr, PL_ListOfStrings>();
                    if (db1.Keys().Contains(msg.intkey))
                    {
                        db2.remove(msg.strkey);
                        msgback.content = "Deletion succeed";
                        Console.Write("\n \n New database:");
                        db2.showDB<string, DBElemstr, PL_ListOfStrings>();
                    }
                    else
                        msgback.content = "Deletion failed! Key doesn't exist.";
                }
            }
        }

    static void Main(string[] args)
    {
        XDocument doc1 = XDocument.Load("db1.xml");
        XDocument doc2 = XDocument.Load("db2.xml");
        DBint db1 = new DBint();
        DBstr db2 = new DBstr();
        db1.FromXml<int, DBElemint, PL_String>(doc1);
        db2.FromXml<string, DBElemstr, PL_ListOfStrings>(doc2);
      
      Util.verbose = false;
      Server srvr = new Server();
      srvr.ProcessCommandLine(args);

      Console.Title = "Server";
      Console.Write(String.Format("\n  Starting CommService server listening on port {0}", srvr.port));
      Console.Write("\n ====================================================\n");

      "Requirement #3: Starting server".title();

      Sender sndr = new Sender(Util.makeUrl(srvr.address, srvr.port));
      Receiver rcvr = new Receiver(srvr.port, srvr.address);

      // - serviceAction defines what the server does with received messages
      // - This serviceAction just announces incoming messages and echos them
      //   back to the sender.  
      // - Note that demonstrates sender routing works if you run more than
      //   one client.

      Action serviceAction = () =>
      {
        Message msg = null;
        while (true)
        {
          msg = rcvr.getMessage();   // note use of non-service method to deQ messages
          Console.Write("\n  Received message:");
          Console.Write("\n  sender is {0}", msg.fromUrl);
          Console.Write("\n  content is {0}\n", msg.content);

          if (msg.content == "connection start message")
          {
            continue; // don't send back start message
          }
          if (msg.content == "query")
              {
                  Console.Write("\n Query received. \n");
                  Message msgback = new Message();
                  msgback = msg;
                  msgback.toUrl = msg.fromUrl;
                  msgback.fromUrl = msg.toUrl;
                  if (msg.clienttype == "WriteClient")
                    srvr.processWMessage(msg, db1, db2, ref msgback);
                  if (msg.clienttype == "ReadClient")
                      srvr.processRMessage(msg, db1, db2, ref msgback);
                  Console.Write("\n Sending back responsing message.");
                  sndr.sendMessage(msgback);
                  continue;
              }
          if (msg.content == "done")
          {
            Console.Write("\n  client has finished\n");
            continue;
          }
          if (msg.content == "closeServer")
          {
            Console.Write("received closeServer");
            break;
          }
          msg.content = "received " + msg.content + " from " + msg.fromUrl;

          // swap urls for outgoing message
          Util.swapUrls(ref msg);

#if (TEST_WPFCLIENT)
          /////////////////////////////////////////////////
          // The statements below support testing the
          // WpfClient as it receives a stream of messages
          // - for each message received the Server
          //   sends back 1000 messages
          //
          int count = 0;
          for (int i = 0; i < 1000; ++i)
          {
            Message testMsg = new Message();
            testMsg.toUrl = msg.toUrl;
            testMsg.fromUrl = msg.fromUrl;
            testMsg.content = String.Format("test message #{0}", ++count);
            Console.Write("\n  sending testMsg: {0}", testMsg.content);
            sndr.sendMessage(testMsg);
          }
#else
          /////////////////////////////////////////////////
          // Use the statement below for normal operation
          sndr.sendMessage(msg);
#endif
        }
      };

      if (rcvr.StartService())
      {
        rcvr.doService(serviceAction); // This serviceAction is asynchronous,
      }                                // so the call doesn't block.
      Util.waitForUser(); 
    }
  }
#if (TEST_Server)
    class TestServer
    {
        static void Main(string[] args)
        {
            Server serv = new Server();
            Run.(serv.Main());
        }
    }
#endif
}
