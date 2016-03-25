///////////////////////////////////////////////////////////////////
// DBExtensions.cs - define extension methods for Display        //
// Ver 3.1                                                       //
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
 * This package implements extensions methods to support 
 * displaying DBElements and DBEngine instances and building
 * DBEngine<Key, Value> instances from XML files.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: 
 *   DBExtensions.cs, DBEngine.cs, DBElement.cs, 
 *   PayloadWrapper.cs, UtilityExtensions.cs
 *
 * Build Process:  devenv Project2Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 3.1 : 17 Oct 15
 * - fixed bug in convertStringToKey function.  Now correctly
 *   converts to both ints and strings.
 * ver 3.0 : 11 Oct 15
 * - replaced two type-specific extension methods bool FromXml(...)
 *   with one generic method bool FromXml<Key, Value, Data>(...).
 * ver 2.2 : 10 Oct 15
 * - converted showDB<Key, Value, Data>() to operate on instances
 *   from classes that implement the IQuery<Key, Value> interface.
 *   Now showDB works for DBEngines, QueryEngines, and VirtuaDBs
 * ver 2.1 : 04 Oct 15
 * - moved ShowMetaData() to DBElement.MetaDataToString()
 * - minor modifications to showElement<Key, Data>()
 * - fixed bug in FromXml: was using "children" instead of "keys"
 *   to find child keys in XML
 * ver 2.0 : 03 Oct 15
 * - eliminated ToXml extension methods as DBElement<Key, Data>
 *   and DBEngine<Key, Value> now have ToXml methods
 * ver 1.2 : 24 Sep 15
 * - reduced the number of methods and simplified
 * ver 1.1 : 15 Sep 15
 * - added a few comments
 * ver 1.0 : 13 Sep 15
 * - first release
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static System.Console;
using System.Xml.Serialization;
using System.IO;

namespace Project4Starter
{
  /////////////////////////////////////////////////////////////////////////
  // Extension methods class 
  // - Extension methods are static methods of a static class
  //   that extend an existing class by adding functionality
  //   not part of the original class.
  // - These methods are all extending the DBElement<Key, Data> class.
  //
  public static class DBElementExtensions
  {
    //----< write details of element - here to accomodate older code >---
    /*
     * This function is here to avoid breaking older code
     */
    public static string showElement<Key, Data>(this DBElement<Key, Data> elem) where Data : class, IClone, IPersist, new()
    {
      return elem.ToString();
    }
  }
  public abstract class keyWrapper<Key>
  {
    abstract public Key value(string str);
  }
  public class IntWrapper : keyWrapper<int>
  {
    public override int value(string str)
    {
      return Int32.Parse(str);
    }
  }
  public class StringWrapper : keyWrapper<string>
  {
    public override string value(string str)
    {
      return str;
    }
  }
  public static class DBEngineExtensions
  {
    //----< write db elements out to Console >---------------------------

    public static void showDB<Key, Value, Data>(this IQuery<Key, Value> db)
      where Data : class, IClone, IPersist, new()
    {
      foreach (Key key in db.Keys())
      {
        Value value;
        db.getValue(key, out value);
        DBElement<Key, Data> elem = value as DBElement<Key, Data>;
        if (elem == null)
        {
          Write("\n  getValue returned null element");
          break;
        }
        Write("\n\n  -- key = {0} --", key);
        Write(elem.showElement());
      }
    }
    //----< Helper for FromXml Extension >-------------------------------
    /*
     * This is a helper function for the FromXml<key, Value, Data>(...)
     * extension method, below.
     *  
     * It works for keys of type string and int but won't work for most 
     * other types.  Fortunately, almost all db keys are either ints or 
     * strings.
     */
    public static Key convertStringToKey<Key>(string val)
    {
      System.Type keyType = typeof(Key);
      if (keyType.IsClass)
        return (Key)(object)val;
      else
        return (Key)(object)Int32.Parse(val);
    }
    //----< FromXml() extension >------------------------------------------

    public static bool FromXml<Key, Value, Data>(this DBEngine<Key, DBElement<Key, Data>> newDB, XDocument doc)
      where Data : class, IClone, IPersist, new()
    {
      IEnumerable<XElement> keys = doc.Root.Elements("key");

      DBElement<Key, Data> dbElem;
      List<Key> dbKeys = newDB.Keys().ToList();

      foreach (var key in keys)
      {
        var dbKey = convertStringToKey<Key>(key.Value);
        if (dbKeys.Contains(dbKey))
        {
          Write("\n  database already contains key \"{0}\" so skipping insert", key.Value);
        }

        XElement elem = key.NextNode as XElement;  // get the <element> node
        if (elem == null)
        {
          Write("\n  unexpected null element");
          break;
        }
        dbElem = new DBElement<Key, Data>();
        dbElem.payload = new Data();
        IEnumerable<XElement> children = elem.Elements();
        foreach (var child in children)
        {
          switch (child.Name.ToString())
          {
            case "name":
              dbElem.name = child.Value;
              break;
            case "descr":
              dbElem.descr = child.Value;
              break;
            case "timeStamp":
              dbElem.timeStamp = DateTime.Parse(child.Value);
              break;
            case "keys":
              IEnumerable<XElement> newkeys = child.Elements();
              foreach (var newkey in newkeys)
              {
                Key childKey = convertStringToKey<Key>(newkey.Value);
                dbElem.children.Add(childKey);
              }
              break;
            case "payload":
              dbElem.payload = dbElem.payload.FromXml(child.ToString()) as Data;
              break;
          }
        }
        newDB.insert(dbKey, dbElem);
      }
      return true;
    }
    ///////////////////////////////////////////////////////////////////////
    // The following two FromXml() functions have been turned into a single
    // generic function FromXml<Key, Value, Data>(...), above, with a little
    // help from the payload wrappers. I'll remove these, below, as soon as I'm
    // confident that the generic function works in many tested scenarios.

    ////----< FromXml() extension >------------------------------------------

    //public static bool FromXml(this DBEngine<int, DBElement<int, PL_String>> newDB, XDocument doc)
    //{
    //  IEnumerable<XElement> keys = doc.Root.Elements("key");
    //
    //  DBElement<int, PL_String> dbElem;
    //  List<int> dbKeys = newDB.Keys().ToList();
    //
    //  foreach (var key in keys)
    //  {
    //    if (dbKeys.Contains(Int32.Parse(key.Value)))
    //    {
    //      Write("\n  database already contains key \"{0}\" so skipping insert", key.Value);
    //    }
    //
    //    XElement elem = key.NextNode as XElement;  // get the <element> node
    //    if (elem == null)
    //    {
    //      Write("\n  unexpected null element");
    //      break;
    //    }
    //    dbElem = new DBElement<int, PL_String>();
    //    dbElem.payload = new PL_String();
    //    IEnumerable<XElement> children = elem.Elements();
    //    foreach (var child in children)
    //    {
    //      switch (child.Name.ToString())
    //      {
    //        case "name":
    //          dbElem.name = child.Value;
    //          break;
    //        case "descr":
    //          dbElem.descr = child.Value;
    //          break;
    //        case "timeStamp":
    //          dbElem.timeStamp = DateTime.Parse(child.Value);
    //          break;
    //        case "keys":
    //          IEnumerable<XElement> newkeys = child.Elements();
    //          foreach (var newkey in newkeys)
    //            dbElem.children.Add(Int32.Parse(newkey.Value));
    //          break;
    //        case "payload":
    //          dbElem.payload = new PL_String(child.Value);
    //          break;
    //      }
    //    }
    //    newDB.insert(Int32.Parse(key.Value), dbElem);
    //  }
    //  return true;
    //}
    ////----< FromXml() extension >------------------------------------------

    //public static bool FromXml(this DBEngine<string, DBElement<string, PL_ListOfStrings>> newDB, XDocument doc)
    //{
    //  IEnumerable<XElement> keys = doc.Root.Elements("key");
    //
    //  DBElement<string, PL_ListOfStrings> dbElem;
    //  List<string> dbKeys = newDB.Keys().ToList();
    //
    //  foreach (var key in keys)
    //  {
    //    if (dbKeys.Contains(key.Value))
    //    {
    //      Write("\n  database already contains key \"{0}\" so skipping insert", key.Value);
    //    }
    //
    //    XElement elem = key.NextNode as XElement;  // get the <element> node
    //    if (elem == null)
    //    {
    //      Write("\n  unexpected null element");
    //      break;
    //    }
    //    dbElem = new DBElement<string, PL_ListOfStrings>();
    //    dbElem.payload = new PL_ListOfStrings();
    //    IEnumerable<XElement> children = elem.Elements();
    //    foreach (var child in children)
    //    {
    //      switch (child.Name.ToString())
    //      {
    //        case "name":
    //          dbElem.name = child.Value;
    //          break;
    //        case "descr":
    //          dbElem.descr = child.Value;
    //          break;
    //        case "timeStamp":
    //          dbElem.timeStamp = DateTime.Parse(child.Value);
    //          break;
    //        case "keys":
    //          IEnumerable<XElement> newkeys = child.Elements();
    //          foreach (var newkey in newkeys)
    //            dbElem.children.Add(newkey.Value);
    //          break;
    //        case "payload":
    //          IEnumerable<XElement> newitems = child.Elements();
    //          foreach (var newitem in newitems)
    //            dbElem.payload.theWrappedData.Add(newitem.Value);
    //          break;
    //      }
    //    }
    //    newDB.insert(key.Value, dbElem);
    //  }
    //  return true;
    //}
  }
#if (TEST_DBEXTENSIONS)

  class TestDBExtensions
  {
    static void Main(string[] args)
    {
      "Testing DBExtensions Package".title('=');
      WriteLine();

      Write("\n --- Test DBElement<int,PL_String> ---");
      DBElement<int, PL_String> elem1 = new DBElement<int, PL_String>();
      elem1.payload = new PL_String("a payload");
      Write(elem1.showElement<int, PL_String>());
      WriteLine();

      Write("\n --- Test DBEngine<int, DBElement<int, PL_String>> ---");
      DBEngine<int, DBElement<int, PL_String>> dbs = 
        new DBEngine<int, DBElement<int, PL_String>>();
      dbs.insert(1, elem1);
      dbs.showDB<int, DBElement<int,PL_String>, PL_String>();
      WriteLine();

      Write("\n --- Test DBElement<string, ListOfString> ---");
      DBElement<string, PL_ListOfStrings> elemLos1 = new DBElement<string, PL_ListOfStrings>();
      elemLos1.name = "elemLos1";
      elemLos1.descr = "element with PL_ListOfStrings payload";
      elemLos1.payload = new PL_ListOfStrings();
      elemLos1.payload.theWrappedData.AddRange(new List<string> { "one", "two", "three" });
      Write(elemLos1.showElement<string, PL_ListOfStrings>());

      Write("\n --- Test DBEngine<string, PL_ListOfStrings> ---");
      DBEngine<string, DBElement<string, PL_ListOfStrings>> dbl =
        new DBEngine<string, DBElement<string, PL_ListOfStrings>>();
      dbl.insert("first", elemLos1);
      dbl.showDB<string, DBElement<string, PL_ListOfStrings>, PL_ListOfStrings>();
      WriteLine();

      Write("\n --- change elemLos1 timeStamp and payload ---\n");
      elemLos1.timeStamp = DateTime.Today;  // this changes both elements "first" and "second" in dbl
      elemLos1.payload.theWrappedData.AddRange(new List<string> { "four", "five" });
      Write(elemLos1.showElement());
      Write("\n --- insert changed elemLos1 into db ---");
      dbl.insert("second", elemLos1);

      Write("\n --- here's the result ---\n");
      dbl.showDB<string, DBElement<string, PL_ListOfStrings>, PL_ListOfStrings>();
      Write("\n --- first and second elements are identical ---");
      Write("\n --- even though different when inserted! ---");
      WriteLine();

      "Test cloning DBElement<string, PL_ListOfStrings>".title();
      DBElement<string, PL_ListOfStrings> elemLos2 = new DBElement<string, PL_ListOfStrings>();
      elemLos2.name = "elemLos2";
      elemLos2.descr = "element with PL_ListOfStrings payload";
      elemLos2.payload = new PL_ListOfStrings();
      elemLos2.payload.theWrappedData.AddRange(new List<string> { "alpha", "beta", "gamma" });
      Write(elemLos2.showElement());
      Write("\n --- insert elemLos2 Clone into db ---");
      dbl.insert("third", elemLos2.Clone() as DBElement<string, PL_ListOfStrings>);

      Write("\n --- change elemLos2 timeStamp and payload ---\n");
      elemLos2.timeStamp = DateTime.Today;    // this only changes element "fourth" in dbl
      elemLos2.payload.theWrappedData.AddRange(new List<string> { "delta", "epsilon" });
      Write(elemLos2.showElement());
      Write("\n --- insert Clone of changed elemLos2 into db ---");
      dbl.insert("fourth", elemLos2.Clone() as DBElement<string, PL_ListOfStrings>);
      Write("\n --- here's the result ---\n");
      dbl.showDB<string, DBElement<string, PL_ListOfStrings>, PL_ListOfStrings>();
      Write("\n --- third and fourth db elements are different ---");
      WriteLine();

      "Test DBElement<int, PL_String> XML generation".title();
      WriteLine();
      Write(elem1.ToXml());
      WriteLine();

      DBElement<int, PL_String> elem2 = elem1.Clone() as DBElement<int, PL_String>;
      elem2.children.AddRange(new List<int> { 1, 2, 7 });
      Write(elem2.ToXml());
      WriteLine();

      "Test DBEngine<int, DBElement<int,PL_String>> XML generation".title();
      DBEngine<int, DBElement<int, PL_String>> db = 
        new DBEngine<int, DBElement<int, PL_String>>();
      db.insert(15, elem1);
      db.insert(16, elem2);
      Write(db.ToXml<PL_String>());
      WriteLine();

      "Reading db XML into XDocument object and displaying".title();
      WriteLine();
      XDocument doc = XDocument.Parse(db.ToXml<PL_String>());
      Write(doc.ToString());
      WriteLine();

      "Parsing XDocument to find db parts".title();
      DBEngine<int, DBElement<int, PL_String>> newDB = new DBEngine<int, DBElement<int, PL_String>>();
      //newDB.insert(15, elem1);
      //newDB.insert(16, elem2);
      newDB.FromXml<int, DBElement<int, PL_String>, PL_String>(doc);
      WriteLine();

      "Resulting Database values:".title();
      newDB.showDB<int, DBElement<int, PL_String>, PL_String>();
      WriteLine();

      "Test DBElement<string, ListOfString> XML generation".title();
      Write(elemLos1.ToXml());
      WriteLine();

      DBElement<string, PL_ListOfStrings> elemLos3 = elemLos2.Clone() as DBElement<string, PL_ListOfStrings>;
      elemLos3.children.AddRange(new List<string> { "key1", "key2", "key3" });
      Write(elemLos3.ToXml());
      WriteLine();

      "Test DBEngine<string, DBElement<string, PL_ListOfString>> XML generation".title();
      DBEngine<string, DBElement<string, PL_ListOfStrings>> dblos = 
        new DBEngine<string, DBElement<string, PL_ListOfStrings>>();
      dblos.insert("key15", elemLos1);
      dblos.insert("key16", elemLos3);
      Write(dblos.ToXml<PL_ListOfStrings>());
      WriteLine();

      "Reading db XML into XDocument object and displaying".title();
      WriteLine();
      XDocument newdoc = XDocument.Parse(dblos.ToXml<PL_ListOfStrings>());
      Write(newdoc.ToString());
      WriteLine();

      "Parsing XDocument to find db parts".title();
      DBEngine<string, DBElement<string, PL_ListOfStrings>> newerDB = new DBEngine<string, DBElement<string, PL_ListOfStrings>>();
      newerDB.insert("key15", elemLos1);
      newerDB.FromXml<string, DBElement<string, PL_ListOfStrings>, PL_ListOfStrings>(newdoc);
      WriteLine();

      "Resulting Database values:".title();
      newerDB.showDB<string, DBElement<string, PL_ListOfStrings>, PL_ListOfStrings>();
      Write("\n\n");
    }
  }
#endif
}
