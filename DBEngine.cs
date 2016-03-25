///////////////////////////////////////////////////////////////////
// DBEngine.cs - define noSQL database                           //
// Ver 2.2                                                       //
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
 * This package implements DBEngine<Key, Value> where Value
 * is the DBElement<key, Data> type.
 *
 * All of the code in this package is generic.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: DBEngine.cs, DBElement.cs, PayloadWrapper.cs, and
 *                 UtilityExtensions.cs only if you enable the test stub
 *
 * Build Process:  devenv Project2Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 2.2 : 13 Oct 15
 * - minor changes to comments
 * ver 2.1 : 11 Oct 15
 * - factored IDbPayload into IClone and IPersist in constraints for
 *   ToXml().
 * - modified some of this package's comments
 * ver 2.0 : 10 Oct 15
 * - added IQuery<Key, Value> interface
 * - made DBEngine<Key, Value> implement the interface
 * ver 1.3 : 03 Oct 15
 * - added methods remove(key), clear(), ToXml<Data>()
 * ver 1.2 : 24 Sep 15
 * - removed extensions methods and tests in test stub
 * - testing is now done in DBEngineTest.cs to avoid circular references
 * ver 1.1 : 15 Sep 15
 * - fixed a casting bug in one of the extension methods
 * ver 1.0 : 08 Sep 15
 * - first release
 */
/*
 * Plans: 
 * - Think about adding a FromXml(string Xml) member function that replaces
 *   the FromXml(...) extensions methods defined in DBExtensions.
 * - Add Sharding package based on the persistance implemented here.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace Project4Starter
{
  ///////////////////////////////////////////////////////////////////////
  // interface IQuery<Key, Value>
  // - a contract for all database objects that can be queried, e.g.,
  //   DBEngine, QueryEngine, and VirtualDB (used to be DBFactory)
  public interface IQuery<Key, Value>
  {
    bool getValue(Key key, out Value val);
    List<Value> getValues();
    List<Key> Keys();
    bool containsKey(Key key);
  }
  ///////////////////////////////////////////////////////////////////////
  // class DBEngine<Key, Value>
  // - this is the NoSqlDb
  //
  public class DBEngine<Key, Value> : IQuery<Key, Value>
  {
    public Dictionary<Key, Value> dbStore { get; }

    public DBEngine()
    {
      dbStore = new Dictionary<Key, Value>();
    }
    public bool insert(Key key, Value val)
    {
      if (dbStore.Keys.Contains(key))
        return false;
      dbStore[key] = val;
      return true;
    }
    public bool getValue(Key key, out Value val)
    {
      if (dbStore.Keys.Contains(key))
      {
        val = dbStore[key];
        return true;
      }
      val = default(Value);
      return false;
    }
    public List<Value> getValues()
    {
      List<Value> values = new List<Value>();
      Value val;
      foreach(Key key in Keys())
      {
        getValue(key, out val);
        if (val != null)
          values.Add(val);
      }
      return values;
    }
    public List<Value> getValues(List<Key> keys)
    {
      List<Value> values = new List<Value>();
      Value val;
      foreach(Key key in keys)
      {
        getValue(key, out val);
        if(val != null)
          values.Add(val);
      }
      return values;
    }
    public List<Key> Keys()
    {
      return dbStore.Keys.ToList<Key>();
    }
    public bool containsKey(Key key)
    {
      return dbStore.ContainsKey(key);
    }
    public bool remove(Key key)
    {
      if (Keys().Contains(key))
      {
        dbStore.Remove(key);
        return true;
      }
      return false;
    }
    public void clear()
    {
      List<Key> origKeys = new List<Key>();
      foreach (Key origKey in Keys())
        origKeys.Add(origKey);
      foreach (Key key in origKeys)
        dbStore.Remove(key);
    }

    public string ToXml<Data>() where Data : class, IClone, IPersist, new()
    {
      DBElement<Key, Data> elem;
      StringBuilder accum = new StringBuilder();
      accum.Append("\n<noSqlDb>");
      string keyTypeName = typeof(Key).Name;
      string payloadTypeName = typeof(Data).Name;
      accum.Append(string.Format("\n  <keyType>{0}</keyType>", keyTypeName));
      accum.Append(String.Format("\n  <payloadType>{0}</payloadType>", payloadTypeName));
      foreach (Key key in Keys())
      {
        Value val;
        accum.Append(string.Format("\n  <key>{0}</key>", key));
        getValue(key, out val);
        elem = val as DBElement<Key, Data>;
        if (elem == null)
        {
          Write("\n  --- getValue return null ---");
          break;
        }
        accum.Append(elem.ToXml());
      }
      accum.Append("\n</noSqlDb>");
      return accum.ToString();
    }
  }

#if(TEST_DBENGINE)

  class TestDBEngine
  {
    static void Main(string[] args)
    {
      "Testing DBEngine Package".title('=');
      WriteLine();

      Write("\n  All testing of DBEngine class moved to DBEngineTest package.");
      Write("\n  This allow use of DBExtensions package without circular dependencies.");

      Write("\n\n");
    }
  }
#endif
}
