///////////////////////////////////////////////////////////////////
// QueryEngine.cs - supports making queries on DBEngine          //
// Ver 2.3                                                       //
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
 * This package provids classes QueryEngine and VirtualDB that
 * support making compound queries on a DBEngine<Key, Value>
 * instance.
 */
/*
 * Planned changes:
 * - change VirtualDB.Query(qp) to return a new VirtualDB
 *   instead of List<key> to be consistent with QueryEngine.Query(qp)
 */
/*
 * Maintenance:
 * ------------
 * Required Files: 
 *   QueryEngine.cs, DBElement.cs, DBEngine.cs, 
 *   PayloadWrapper, Display, 
 *   DBExtensions.cs, UtilityExtensions.cs
 *
 * Build Process:  devenv Project2Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 2.4 : 08 Nov 15
 * - added planned change to VirtualDB
 * ver 2.3 : 03 Nov 15
 * - added Children(Key key) and Decendants(Key key) to 
 *   QueryEngine and VirtualDB.
 * ver 2.2 : 13 Oct 15
 * - added VirtualDB<Key, DBElement<Key, Data>>.cloneDB() so users
 *   can't modify the original database.
 * ver 2.1 : 11 Oct 15
 * - factored IDbPayload into IClone and IPersist
 * ver 2.0 : 10 Oct 15
 * - modified QueryEngine to return VirtualDB from Queries
 * - made QueryEngine and VirtualDB implement IQuery<Key, Value>
 *   interface
 * - made VirtualDB return clone values from getValue(...) and
 *   getValues().
 * - Renamed DBFactory to VirtualDB.
 * ver 1.0 : 04 Oct 15
 * - first release
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace Project4Starter
{
  /////////////////////////////////////////////////////////////////////////
  // class VirtualDB is a queryable container of keys
  // - returned by Query() and doQueries() with database with keys of
  //   elements in original database that match query(s).
  // - can be cloned so users of clone can't change original database
  //   values.
  // - was called DBFactory but that name caused a lot of confusion so ...
  //
  public class VirtualDB<Key, Value> : IQuery<Key, Value> 
    where Value : class, IClone, IPersist, new()
  {
    private DBEngine<Key, Value> db;
    List<Key> keys = new List<Key>();
    List<Key> visited_ = new List<Key>();
    public VirtualDB(DBEngine<Key, Value> database) { db = database; }
    public bool getValue(Key key, out Value value)
    {
      if (db.getValue(key, out value))
      {
        //value = value.Clone() as Value;  // client can't change db element
        return true;
      }
      value = null;
      return false;
    }
    public List<Value> getValues()
    {
      List<Value> values = new List<Value>();
      Value val;
      foreach (Key key in Keys())
      {
        getValue(key, out val);
        if (val != null)
        {
          //values.Add(val.Clone() as Value);
          values.Add(val);
        }
      }
      return values;
    }
    public List<Key> Query(Func<Key, bool> f)
    {
      List<Key> matchingKeys = new List<Key>();
      foreach (Key key in Keys())
      {
        if (f.Invoke(key))
        {
          matchingKeys.Add(key);
        }
      }
      return matchingKeys;
    }
    public List<Key> Keys()
    {
      return keys;
    }
    public bool containsKey(Key key)
    {
      return Keys().Contains(key);
    }
    public void addKey(Key key)
    {
      keys.Add(key);
    }
    public void addKeys(List<Key> addkeys)
    {
      keys.AddRange(addkeys);
    }
    public void clear() { keys.Clear(); }
    public VirtualDB<Key, Value> children(Key key)
    {
      List<Key> children_ = new List<Key>();
      Value val = null;
      if (getValue(key, out val))
      {
        IDBElement<Key> elem = val as IDBElement<Key>;
        children_.AddRange(elem.children);
      }
      VirtualDB<Key, Value> vdb = new VirtualDB<Key, Value>(db);
      vdb.addKeys(children_);
      return vdb;
    }
    public VirtualDB<Key, Value> descendants(Key key)
    {
      visited_.Clear();
      return descendantsHelper(key);
    }
    public VirtualDB<Key, Value> descendantsHelper(Key key)
    {
      VirtualDB<Key, Value> vdb = children(key);
      foreach (Key child in vdb.Keys())
      {
        if (!visited_.Contains(child))
        {
          visited_.Add(child);
          descendantsHelper(child);
        }
      }
      vdb.Keys().Clear();
      vdb.addKeys(visited_);
      return vdb;
    }
    public void cloneDB()
    {
      var dbTemp = new DBEngine<Key, Value>();
      foreach(Key key in keys)
      {
        Value val;
        if (getValue(key, out val))
          dbTemp.insert(key, val.Clone() as Value);
      }
      db = dbTemp;
    }
  }
  /////////////////////////////////////////////////////////////////////////
  // class QueryEngine<Key, Value> is responsible for handling
  // queries into DBEngine<Key, Value> instances.
  // - The test stub gives good examples of using this class
  //
  public class QueryEngine<Key, Value> : IQuery<Key, Value> 
    where Value : class, IClone, IPersist, new()
  {
    private DBEngine<Key, Value> db;
    private List<Func<Key, bool>> queryPredicates = new List<Func<Key, bool>>();
    List<Key> visited_ = new List<Key>();

    public QueryEngine(DBEngine<Key, Value> database)
    {
      db = database;
    }
    public VirtualDB<Key, Value> Query(Func<Key, bool> f)
    {
      List<Key> matchingKeys = new List<Key>();
      foreach(Key key in db.Keys())
      {
        if(f.Invoke(key))
        {
          matchingKeys.Add(key);
        }
      }
      VirtualDB<Key, Value> vdb = new VirtualDB<Key, Value>(db);
      vdb.addKeys(matchingKeys);
      return vdb;
    }
    public void add(Func<Key, bool> qp)
    {
      queryPredicates.Add(qp);
    }
    public VirtualDB<Key, Value> doQueries()
    {
      VirtualDB<Key, Value> vdb = new VirtualDB<Key, Value>(db);
      IEnumerable<Key> temp = db.Keys();
      vdb.addKeys(temp.ToList<Key>());
      List<Key> matchingKeys = new List<Key>();
      foreach(var qp in queryPredicates)
      {
        matchingKeys = vdb.Query(qp).ToList<Key>();
        vdb.clear();
        vdb.addKeys(matchingKeys);
      }
      return vdb;
    }
    public bool getValue(Key key, out Value value)
    {
      if (db.getValue(key, out value))
        return true;
      value = null;
      return false;
    }
    public List<Value> getValues()
    {
      List<Value> values = new List<Value>();
      foreach(Key key in Keys())
      {
        Value val;
        if (getValue(key, out val) && val != null)
          values.Add(val);
      }
      return values;
    }
    public List<Key> Keys()
    {
      return db.Keys() as List<Key>;
    }
    public bool containsKey(Key key)
    {
      return db.containsKey(key);
    }
    public VirtualDB<Key, Value> children(Key key)
    {
      List<Key> children_ = new List<Key>();
      Value val = null;
      if (getValue(key, out val))
      {
        IDBElement<Key> elem = val as IDBElement<Key>;
        children_.AddRange(elem.children);
      }
      VirtualDB<Key, Value> vdb = new VirtualDB<Key, Value>(db);
      vdb.addKeys(children_);
      return vdb;
    }
    public VirtualDB<Key, Value> descendants(Key key)
    {
      visited_.Clear();
      return descendantsHelper(key);
    }
    public VirtualDB<Key, Value> descendantsHelper(Key key)
    {
      VirtualDB<Key, Value> vdb = children(key);
      foreach (Key child in vdb.Keys())
      {
        if (!visited_.Contains(child))
        {
          visited_.Add(child);
          descendantsHelper(child);
        }
      }
      vdb.Keys().Clear();
      vdb.addKeys(visited_);
      return vdb;
    }
  }
#if (TEST_QUERYENGINE)
  class TestQueryEngine
  {
    static void Main(string[] args)
    {
      DBEngine<string, DBElement<string, PL_ListOfStrings>> db = new DBEngine<string, DBElement<string, PL_ListOfStrings>>();
      DBElement<string, PL_ListOfStrings> elem = new DBElement<string, PL_ListOfStrings>();
      elem.name = "elem1";
      elem.descr = "Test element for Queries";
      elem.children.AddRange(new List<string> { "Key2", "Key3" });
      elem.payload = new PL_ListOfStrings(new List<string> { "item1", "item2", "item3", "item4" });
      db.insert("Key1", elem.Clone() as DBElement<string, PL_ListOfStrings>);

      elem.name = "elem2_IsNot_elem3";
      elem.children.Clear();
      elem.payload.theWrappedData = new List<string> { "item5", "item6" };
      db.insert("Key2", elem.Clone() as DBElement<string, PL_ListOfStrings>);

      elem.name = "elem3";
      elem.children.AddRange(new List<string> { "Key4", "Key5" });
      elem.payload.theWrappedData = new List<string> { "item7", "item3" };
      db.insert("Key3", elem.Clone() as DBElement<string, PL_ListOfStrings>);

      elem.name = "elem4";
      elem.children.Clear();
      elem.children.Add("Key3");
      elem.payload.theWrappedData = new List<string> { "item7", "item3" };
      db.insert("Key4", elem.Clone() as DBElement<string, PL_ListOfStrings>);

      elem.name = "elem5";
      elem.children.Clear();
      elem.payload.theWrappedData = new List<string> { "item7", "item3" };
      db.insert("Key5", elem.Clone() as DBElement<string, PL_ListOfStrings>);

      db.showDB<string, DBElement<string, PL_ListOfStrings>, PL_ListOfStrings>();
      WriteLine();

      "Testing Queries".title();
      WriteLine();

      QueryEngine<string, DBElement<string, PL_ListOfStrings>> qe =
        new QueryEngine<string, DBElement<string, PL_ListOfStrings>>(db);

      Write("\n --- Testing QueryEngine<Key, Value>.Query(Func<Key, bool>) ---");
      WriteLine();

      "simple query for elements with children".title();

      Func<string, bool> qp = (string key) =>
      {
        DBElement<string, PL_ListOfStrings> qelem;
        if (db.getValue(key, out qelem))
          if (qelem.children.Count() > 0)
            return true;
        return false;
      };

      List<string> keys = qe.Query(qp).Keys() as List<string>;

      // Lambda to display query results

      Action display = () =>
      {
        foreach (string key in keys)
        {
          DBElement<string, PL_ListOfStrings> qelem;
          qe.getValue(key, out qelem);
          {
            Write("\n  {0} has {1} children", qelem.name, qelem.children.Count());
          }
        }
        WriteLine();
      };

      display.Invoke();

      Write("\n --- Testing QueryEngine<Key, Value>.doQueries() ---");
      WriteLine();

      Func<string, bool> qp2 = (string key) =>
      {
        DBElement<string, PL_ListOfStrings> qelem;
        if (db.getValue(key, out qelem))
          if (qelem.name.IndexOf('3') > -1)
            return true;
        return false;
      };

      "query for elements with names that contain the letter 3".title();
      keys = qe.Query(qp2).Keys() as List<string>;
      display.Invoke();

      "query for elements with children".title();
      qe.add(qp);
      "query for elements with names that contain the letter 3".title();
      qe.add(qp2);

      keys = qe.doQueries().Keys() as List<string>;
      display.Invoke();
      WriteLine();

      "testing children(Key key)".title();
      db.showDB<string, DBElement<string, PL_ListOfStrings>, PL_ListOfStrings>();
      WriteLine();

      Action<List<string>> showKeys = (los) =>
      {
        los.DisplayCommaSeparatedList();
      };
      VirtualDB<string, DBElement<string, PL_ListOfStrings>> vdbc = null;
      foreach (string key in qe.Keys())
      {
        Write("\n  -- {0} --", key);
        vdbc = qe.children(key);
        showKeys(vdbc.Keys());
      }
      WriteLine();

      "testing descendants(Key key)".title();
      foreach(string key in qe.Keys())
      {
        vdbc = qe.descendants(key);
        Console.Write("\n -- {0} --", key);
        showKeys(vdbc.Keys());
      }
      Write("\n\n");
    }
#endif
  }
}
