///////////////////////////////////////////////////////////////////
// ItemFactory.cs - creates DBElement<key, Data>                 //
// Ver 1.1                                                       //
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
 * This package implements a simple factory for DBElement<Key, Data>.
 * The intent is that applications that need to build many similar
 * elements can automate some of that processing.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: ItemFactory.cs, DBElement.cs, PayloadWrapper.cs  
 *
 * Build Process:  devenv Project2Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 1.1 : 13 Oct 15
 * - added more options for construction
 * ver 1.0 : 10 Oct 15
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace Project4Starter
{
  using ItmFactIS = ItemFactory<int, PL_String>;
  using ItmFactSL = ItemFactory<string, PL_ListOfStrings>;

  ///////////////////////////////////////////////////////////////////////
  // class ItemFactory<Key, Data>
  // - automation for creating DBElement<Key, Data>
  //
  public class ItemFactory<Key, Data> where Data : class, IClone, IPersist, new()
  {
    static public DBElement<Key, Data> create(string name, string descr, int numChildren)
    {
      return create(name, descr, numChildren, 1);
    }
    static public DBElement<Key, Data> create(string name, string descr)
    {
      return create(name, descr, 0);
    }
    static public DBElement<Key, Data> create(string name)
    {
      return create(name, "undescribed");
    }
    static public DBElement<Key, Data> create()
    {
      return create("unnamed");
    }
    static public DBElement<Key, Data> create(string name, string descr, int numChildren, int numPayload)
    {
      DBElement<Key, Data> elem = new DBElement<Key, Data>();
      elem.name = name;
      elem.descr = descr;
      elem.timeStamp = DateTime.Now;
      elem.children = new List<Key>();
      for (int i = 0; i < numChildren; ++i)
      {
        Key key;
        if(typeof(Key) == typeof(int))
        {
          key = (Key)(object)i;
        }
        else
        {
          key = (Key)(object)("key" + i.ToString());
        }
        elem.children.Add(key);
      }
      elem.payload = new Data();
      if (typeof(Data) == typeof(PL_ListOfStrings))
      {
        List<string> items = new List<string>();
        for (int i = 0; i < numPayload; ++i)
        {
          items.Add("payload" + i.ToString());
        }
        elem.payload = new PL_ListOfStrings(items) as Data;
      }
      else
      {
        elem.payload = new PL_String("payload") as Data;
      }
      return elem;
    }
  }
#if (TEST_ITEMFACTORY)
  class TestItemFactory
  {
    static void Main(string[] args)
    {
      DBElement<int, PL_String> elem1 = ItmFactIS.create();
      Write(elem1.showElement());
      WriteLine();

      DBElement<int, PL_String> elem2 = ItmFactIS.create("elem2", "test element", 3);
      Write(elem2.showElement());
      WriteLine();

      DBElement<string, PL_ListOfStrings> elem3 = ItmFactSL.create();
      Write(elem3.showElement());
      WriteLine();

      DBElement<string, PL_ListOfStrings> elem4 = ItmFactSL.create("elem4", "test element", 2, 3);
      Write(elem4.showElement());
      Write("\n\n");
    }
  }
#endif
}
