///////////////////////////////////////////////////////////////////
// Display.cs - define methods to simplify display actions       //
// Ver 2.1                                                       //
// Application: Demonstration for CSE687-OOD, Project#2          //
// Language:    C#, ver 6.0, Visual Studio 2015                  //
// Platform:    Dell XPS2700, Core-i7, Windows 10                //
// Author:      Jim Fawcett, CST 4-187, Syracuse University      //
//              (315) 443-3948, jfawcett@twcny.rr.com            //
///////////////////////////////////////////////////////////////////
// Copyright (c) James W. Fawcett, 2015                          //
// All rights granted to users provided this notice is retained  //
///////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package implements and tests display functions
 * for DBElement<Key, Data> and DBEngine<Key, Value> instances.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: Display.cs, DBEngine.cs, DBElement.cs,
 *                 PayloadWrapper.cs,  
 *                 DBExtensions.cs, UtilityExtensions.cs
 *
 * Build Process:  devenv Project2Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 2.1 : 13 Oct 15
 * - minor changes to comments
 * ver 2.0 : 10 Oct 15
 * - eliminated most of the original methods, added:
 *     DisplayCommaSeparatedList<T>(),
 *     ShowRelationships<Key, Value, Data>(),
 *     ShowPayload<Key, Value, Data>(), 
 *     ShowView<Key, Value, Data>(Action<Key, DBElement<Key, Data>> view)
 * ver 1.2 : 24 Sep 15
 * - minor tweeks to extension methods to use names from
 *   DBExtensions
 * ver 1.1 : 15 Sep 15
 * - fixed a couple of minor bugs and added more comments
 * ver 1.0 : 13 Sep 15
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using static System.Console;

namespace Project4Starter
{
  ///////////////////////////////////////////////////////////////////////
  // These aliases simplify code text, but make it harder
  // to remember exactly what the code is trying to do.
  //
  using DBElemS = DBElement<int, PL_String>;
  using DBElemL = DBElement<string, PL_ListOfStrings>;
  using DBS = DBEngine<int, DBElement<int, PL_String>>;
  using DBL = DBEngine<string, DBElement<string, PL_ListOfStrings>>;

  /*
   *  The purpose of these extension methods is to provide a flexible, easy to use, facility
   *  for displaying the contents of types that implement the IQuery<Key, Value> interface, e.g., 
   *  DBEngine<Key, Value>, QueryEngine<Key, Value>, and VirtualDB<Key, Value>.
   *
   *  Note that VirtualDB used to be called DBFactory.  That caused so much confusion that I
   *  renamed the class.  The new name is more accurate.
   */
  public static class DisplayExtensions
  {
    //----< Display helper function >------------------------------------

    public static void DisplayCommaSeparatedList<T>(this IEnumerable<T> collection, string msg="\n  ")
    {
      bool first = true;
      foreach (T t in collection)
      {
        if (first)
        {
          if (msg.Length > 0)
            Write("{0}{1}", msg, t.ToString());
          else
            Write("\n  {0}", t.ToString());
          first = false;
        }
        else
        {
          Write(", {0}", t.ToString());
        }
      }
      WriteLine();
    }
    //----< show view of parent/child relationships >--------------------

    public static void showRelationships<Key, Value, Data>(this IQuery<Key, Value> db)
      where Data : class, IClone, IPersist, new()
    {
      foreach (Key key in db.Keys())
      {
        Value val;
        if (db.getValue(key, out val) && val != null)
        {
          DBElement<Key, Data> elem = val as DBElement<Key, Data>;
          Write("\n  Key: {0}, Name: {1}", key, elem.name);
          elem.children.DisplayCommaSeparatedList<Key>("Children");
        }
      }
    }
    //----< show payload view for each element in database >-------------

    public static void showPayload<Key, Value, Data>(this IQuery<Key, Value> db)
      where Data : class, IClone, IPersist, new()
    {
      foreach (Key key in db.Keys())
      {
        Value val;
        if (db.getValue(key, out val) && val != null)
        {
          DBElement<Key, Data> elem = val as DBElement<Key, Data>;
          Write("\n  Key: {0}, Name: {1}", key, elem.name);
          Write("\n  Payload: {0}", elem.payload.ToString());
          WriteLine();
        }
      }
    }
    //----< show user defined view - done by supplying a lambda >--------

    public static void showView<Key, Value, Data>(this IQuery<Key, Value> db, Action<Key, DBElement<Key, Data>> view)
      where Data : class, IClone, IPersist, new()
    {
      foreach (Key key in db.Keys())
      {
        Value val;
        if (db.getValue(key, out val) && val != null)
        {
          DBElement<Key, Data> elem = val as DBElement<Key, Data>;
          view.Invoke(key, elem);
        }
      }
    }
  }

#if (TEST_DISPLAY)

  class TestDisplay
  {
    /////////////////////////////////////////////////////////////////////
    // These are function aliases - same comments as above
    //
    Action<IQuery<int, DBElemS>> showDBS = (IQuery<int, DBElemS> dbs) =>
    {
      dbs.showDB<int, DBElement<int, PL_String>, PL_String>();
    };

    Action<IQuery<string, DBElemL>> showDBL = (IQuery<string, DBElemL> dbl) =>
    {
      dbl.showDB<string, DBElement<string, PL_ListOfStrings>, PL_ListOfStrings>();
    };

    static bool verbose = false;

    static void Main(string[] args)
    {
      "Testing Display Package".title('='); ;
      WriteLine();

      "Test db of scalar elements".title();
      WriteLine();

      DBElement<int, PL_String> elem1 = new DBElement<int, PL_String>();
      elem1.payload = new PL_String("a payload");

      DBElement<int, PL_String> elem2 = new DBElement<int, PL_String>("Darth Vader", "Evil Overlord");
      elem2.payload = new PL_String("The Empire strikes back!");

      var elem3 = new DBElement<int, PL_String>("Luke Skywalker", "Young HotShot");
      elem3.payload = new PL_String("X-Wing fighter in swamp - Oh oh!");

      if (verbose)
      {
        Write("\n --- Test DBElement<int,string> ---");
        WriteLine();
        elem1.showElement();
        WriteLine();
        elem2.showElement();
        WriteLine();
        elem3.showElement();
        WriteLine();

        /* ElementFormatter is not ready for prime time yet */
        //Write(ElementFormatter.formatElement(elem1.showElement<int, string>(), false));
      }

      Write("\n --- Test DBEngine<int,DBElement<int,string>> ---");
      WriteLine();

      int key = 0;
      Func<int> keyGen = () => { ++key; return key; };

      DBEngine<int, DBElement<int, PL_String>> db = new DBEngine<int, DBElement<int, PL_String>>();
      bool p1 = db.insert(keyGen(), elem1);
      bool p2 = db.insert(keyGen(), elem2);
      bool p3 = db.insert(keyGen(), elem3);
      if (p1 && p2 && p3)
        Write("\n  all inserts succeeded");
      else
        Write("\n  at least one insert failed");
      db.showDB<int, DBElement<int, PL_String>, PL_String>();
      WriteLine();

      "Test db of enumerable elements".title();
      WriteLine();

      DBElement<string, PL_ListOfStrings> newelem1 = new DBElement<string, PL_ListOfStrings> ();
      newelem1.name = "newelem1";
      newelem1.descr = "test new type";
      newelem1.payload = new PL_ListOfStrings(new List<string> { "one", "two", "three" });

      DBElement<string, PL_ListOfStrings> newerelem1 = new DBElement<string, PL_ListOfStrings>();
      newerelem1.name = "newerelem1";
      newerelem1.descr = "better formatting";
      newerelem1.payload = new PL_ListOfStrings(new List<string> { "alpha", "beta", "gamma" });
      newerelem1.payload.theWrappedData.Add("delta");
      newerelem1.payload.theWrappedData.Add("epsilon");

      DBElement<string, PL_ListOfStrings> newerelem2 = new DBElement<string, PL_ListOfStrings>();
      newerelem2.name = "newerelem2";
      newerelem2.descr = "better formatting";
      newerelem2.children.AddRange(new List<string> { "first", "second" });
      newerelem2.payload = new PL_ListOfStrings(new List<string> { "a", "b", "c" });
      newerelem2.payload.theWrappedData.Add("d");
      newerelem2.payload.theWrappedData.Add("e");

      if (verbose)
      {
        Write("\n --- Test DBElement<string,List<string>> ---");
        WriteLine();
        newelem1.showElement();
        WriteLine();
        newerelem1.showElement();
        WriteLine();
        newerelem2.showElement();
        WriteLine();
      }

      Write("\n --- Test DBEngine<string,DBElement<string,List<string>>> ---");

      int seed = 0;
      string skey = seed.ToString();
      Func<string> skeyGen = () =>
      {
        ++seed;
        skey = "string" + seed.ToString();
        skey = skey.GetHashCode().ToString();
        return skey;
      };

      DBEngine<string, DBElement<string, PL_ListOfStrings>> newdb =
        new DBEngine<string, DBElement<string, PL_ListOfStrings>>();
      newdb.insert(skeyGen(), newelem1);
      newdb.insert(skeyGen(), newerelem1);
      newdb.insert(skeyGen(), newerelem2);
      newdb.showDB<string, DBElement<string, PL_ListOfStrings>, PL_ListOfStrings>();
      WriteLine();

      Write("\n --- Test ShowRelationships ---");
      WriteLine();
      newdb.showRelationships<string, DBElemL, PL_ListOfStrings>();

      Write("\n --- Test ShowPayload ---");
      WriteLine();
      newdb.showPayload<string, DBElemL, PL_ListOfStrings>();

      Write("\n  --- Test Views ---");
      WriteLine();
      Action<string, DBElement<string, PL_ListOfStrings>> view =
        (string vKey, DBElement<string, PL_ListOfStrings> e) => 
        {
          Write("\n  Key:   {0}", vKey);
          Write("\n  Name:  {0}", e.name);
          Write("\n  Descr: {0}", e.descr);
          WriteLine();
        };
      newdb.showView<string, DBElemL, PL_ListOfStrings>(view);
      Write("\n\n");
    }
  }
#endif
}

