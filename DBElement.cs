///////////////////////////////////////////////////////////////////
// DBElement.cs - Define element for noSQL database              //
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
 * This package implements the DBElement<Key, Data> type, used by 
 * DBEngine<key, Value> where Value is DBElement<Key, Data>.
 *
 * The DBElement<Key, Data> state consists of metadata and an
 * instance of the Data type.
 *
 * All of the code in this package is generic.
 *
 * I intend this DBElement type to be used for all construction of
 * database elements.  That ensures uniformity of structure within
 * the database.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: DBElement.cs, PayloadWrapper.cs, UtilityExtensions.cs
 *
 * Build Process:  devenv Project2Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 3.1 : 13 Oct 15
 * - added thrown exception in FromXml since its functionality
 *   is found in DBExtensions.FromXml, applied to DBEngines.
 * ver 3.0 : 11 Oct 15
 * - factored the IDbPayload interface into IClone and IPersist
 * ver 2.2 : 10 Oct 15
 * - added copyright notice
 * ver 2.1 : 04 Oct 15
 * - Refactored by converting DBExtensions.ShowMetaData() to
 *   MetaDataToString() and moving into this package
 * ver 2.0 : 03 Oct 15
 * - Required DBElement to use PayloadWrapper for payload type.
 *   That allows us to define ToXml in DBElement because the compiler
 *   now knows that payload has the ToXml method.
 * ver 1.1 : 24 Sep 15
 * - removed extension methods, removed tests from test stub
 * - Testing now  uses DBEngineTest.cs
 * ver 1.0 : 13 Sep 15
 * - first release
 *
 */
 //ToDo: 1a - move FromXml functionality from DBExtensions applied to DBEngine<Key, Value>
 //ToDo: 1b - to DBElement<Key, Data> override of IPersist.FromXml(string xml)
 //ToDo: 1c - DBExtensions will provide the outer shell but DBElement will provide most
 //ToDo: 1d - of the processing.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace Project4Starter
{
  public interface IDBElement<Key> : IClone, IPersist 
  {
    string name { get; set; }
    string descr { get; set; }
    DateTime timeStamp { get; set; }
    List<Key> children { get; set; }
  }
  /////////////////////////////////////////////////////////////////////
  // DBElement<Key, Data> class
  // - Instances of this class are the "values" in our key/value 
  //   noSQL database.
  // - Key and Data are unspecified classes, to be supplied by the
  //   application that uses the noSQL database.
  //   See the teststub below for examples of use.
  // - IClone and IPersist declare Clone and ToXml methods so DBElement
  //   can use them in its own Clone and ToXml methods
  public class DBElement<Key, Data> : IDBElement<Key> 
    where Data : class, IClone, IPersist, new()
  {
    public string name { get; set; }                    // metadata    |
    public string descr { get; set; }                   // metadata    |
    public DateTime timeStamp { get; set; }             // metadata   value
    public List<Key> children { get; set; }             // metadata    |
    public Data payload { get; set; }                   // data        |

    public DBElement()
    {
      name = "unnamed";
      descr = "undescribed";
      timeStamp = DateTime.Now;
      children = new List<Key>();
    }
    public DBElement(string Name, string Descr)
    {
      name = Name;
      descr = Descr;
      timeStamp = DateTime.Now;
      children = new List<Key>();
    }

    public string MetaDataToString()
    {
      StringBuilder accum = new StringBuilder();
      accum.Append(String.Format("\n  name: {0}", name));
      accum.Append(String.Format("\n  desc: {0}", descr));
      accum.Append(String.Format("\n  time: {0}", timeStamp));
      if (children.Count() > 0)
      {
        accum.Append(String.Format("\n  Children: "));
        bool first = true;
        foreach (Key key in children)
        {
          if (first)
          {
            accum.Append(String.Format("{0}", key.ToString()));
            first = false;
          }
          else
            accum.Append(String.Format(", {0}", key.ToString()));
        }
      }
      return accum.ToString();
    }

    public override string ToString()
    {
      StringBuilder accum = new StringBuilder();
      accum.Append(MetaDataToString());
      if (payload != null)
      {
        accum.Append("\n  ");
        accum.Append(payload.ToString());
      }
      return accum.ToString();
    }

    public string ToXml()
    {
      StringBuilder accum = new StringBuilder();
      accum.Append("\n  <element>");
      accum.Append(String.Format("\n    <name>{0}</name>", name));
      accum.Append(String.Format("\n    <descr>{0}</descr>", descr));
      accum.Append(String.Format("\n    <timeStamp>{0}</timeStamp>", timeStamp.ToString()));
      if (children.Count > 0)
      {
        accum.Append("\n    <keys>");
        foreach (Key key in children)
        {
          accum.Append(String.Format("\n      <key>{0}</key>", key));
        }
        accum.Append("\n    </keys>");
      }
      accum.Append(String.Format("{0}", payload.ToXml()));
      accum.Append("\n  </element>");
      return accum.ToString();
    }
    //----< instance from XML is not implemented for DBElements >--------
    /*
     * This is here to complete implementation of the IPersist Interface.
     * Eventually much of the DBEngine fromXml(string xml) functionality
     * will be moved here.
     */
    public IPersist FromXml(string xml)
    {
      string msg =
        "\n\n  FromXml in DBElement has no useful functionality.\n"
      + "  All that processing occurs in FromXml in DBExtensions\n"
      + "  used by instances of DBEngine.";
      System.Exception ex = new Exception(msg);
      throw ex;
    }
    public IClone Clone()
    {
      DBElement<Key, Data> cloned = new DBElement<Key, Data>();
      cloned.name = String.Copy(name);
      cloned.descr = String.Copy(descr);
      cloned.timeStamp = DateTime.Parse(timeStamp.ToString());
      cloned.children = new List<Key>();
      cloned.children.AddRange(children);
      cloned.payload = payload.Clone() as Data;
      return cloned;
    }
  }

#if (TEST_DBELEMENT)


  class TestDBElement
  {
    static void Main(string[] args)
    {
      "Testing DBElement Package".title('=');
      WriteLine();

      Write("\n  All testing of DBElement class moved to DBElementTest package.");
      Write("\n  This allow use of DBExtensions package without circular dependencies.");

      Write("\n\n");
    }
  }
#endif
}
