///////////////////////////////////////////////////////////////////
// PayloadWrapper.cs - Wrapper supports cloning, ToString()      //
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
 * This package shows how to write wrapper classes for the DBElement's
 * payload that provides Clone(), ToString(), ToXml(), and FromXml() 
 * where that is possible, e.g., we have access to all the members.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: 
 *   PayloadWrapper, UtilityExtensions.cs
 *
 * Build Process:  devenv Project2Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 2.1 : 13 Oct 15
 * - minor changes to comments
 * ver 2.0 : 11 Oct 15
 * - factored IDbPayload into IClone and IPersist
 * - added member functions IPersist FromXml(string xml) in
 *   PayloadWrapper, PL_String, and PL_ListOfStrings
 * ver 1.2 : 10 Oct 15
 * - added IDbPayload interface, make PayloadWrapper implement it.
 * - modified PL_String and PL_ListOfStrings accordingly.
 * ver 1.1 : 04 Oct 15
 * - minor changes in ToString() functions for both PL_String and 
 *   PL_ListOfStrings to modify whitespace handling
 * ver 1.0 : 24 Sep 15
 * - first release
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;
using System.Xml;
using System.Xml.Linq;

namespace Project4Starter
{
  ///////////////////////////////////////////////////////////////////////
  // IClone interface
  // - contract for building copies of PayloadWrapper<Data>
  //   and DBElement<Key, Data>
  //
  public interface IClone
  {
    IClone Clone();
  }
  //////////////////////////////////////////////////////////////////////
  // IPersist interface
  // - contract for Persisting and Unpersisting PayloadWrapper<Data>
  //   and DBElement<Key, Data>
  //
  public interface IPersist
  {
    string ToXml();
    IPersist FromXml(string xml);
  }
  ///////////////////////////////////////////////////////////////////////
  // PayloadWrapper<Data> class
  // - provides guaranteed implementations of the IClone and IPersist
  //   interfaces needed to build generic DBElement<Key, Data> classes.
  //
  public abstract class PayloadWrapper<Data> : IClone, IPersist where Data : new()
  {
    public Data theWrappedData { get; set; } = new Data();
    abstract public IClone Clone();
    public abstract override bool Equals(object obj);
    public abstract override int GetHashCode();
    override abstract public string ToString();
    abstract public string ToXml();
    abstract public IPersist FromXml(string xml);
    public string typeName()
    {
      return this.GetType().Name;
    }
  }
  ///////////////////////////////////////////////////////////////////////
  // PL_String class
  // - wraps string payloads to support cloning and persistance
  //
  public class PL_String : PayloadWrapper<StringBuilder>
  {
    public PL_String()
    {
      theWrappedData = new StringBuilder("");
    }
    public PL_String(string aString)
    {
      theWrappedData = new StringBuilder(aString);
    }
    public override IClone Clone()
    {
      PayloadWrapper<StringBuilder> cloned = 
        new PL_String(String.Copy(theWrappedData.ToString()));
      return cloned;
    }
    public override string ToString()
    {
      return theWrappedData.ToString();
    }
    public override string ToXml()
    {
      StringBuilder accum = 
        new StringBuilder(
          String.Format("\n    <payload>{0}</payload>", theWrappedData.ToString())
        );
      return accum.ToString();
    }
    public override IPersist FromXml(string xml)
    {
      XDocument doc = XDocument.Parse(xml);
      XElement payloadElem = doc.Descendants("payload").First();
      return new PL_String(payloadElem.Value);
    }
    public override bool Equals(object obj)
    {
      return theWrappedData.Equals(obj as StringBuilder);
    }
    public override int GetHashCode()
    {
      return theWrappedData.GetHashCode();
    }
  }
  ///////////////////////////////////////////////////////////////////////
  // PL_ListOfStrings class
  // - wraps List<string> payloads to support cloning and persistance
  //
  public class PL_ListOfStrings : PayloadWrapper<List<string>>
  {
    public PL_ListOfStrings()
    {
      theWrappedData = new List<string>();
    }
    public PL_ListOfStrings(List<string> list)
    {
      theWrappedData = list;
    }
    public override IClone Clone()
    {
      PL_ListOfStrings los = new PL_ListOfStrings();
      los.theWrappedData = new List<string>();
      foreach (string item in theWrappedData)
        los.theWrappedData.Add(String.Copy(item));
      return los;
    }
    public override bool Equals(object obj)
    {
      PayloadWrapper<List<string>> plw = obj as PayloadWrapper<List<string>>;
      if (theWrappedData.Count() != plw.theWrappedData.Count())
        return false;
      for (int i = 0; i < theWrappedData.Count(); ++i)
        if (theWrappedData[i] != plw.theWrappedData[i])
          return false;
      return true;
    }
    public override int GetHashCode()
    {
      return theWrappedData.GetHashCode();
    }
    public override string ToString()
    {
      StringBuilder accum = new StringBuilder();
      bool first = true;
      foreach (string item in theWrappedData)
      {
        if (first)
        {
          accum.Append(string.Format("{0}", item));
          first = false;
        }
        else
        {
          accum.Append(string.Format(", {0}", item));
        }
      }
      return accum.ToString();
    }
    public override string ToXml()
    {
      StringBuilder accum = new StringBuilder();
      accum.Append("\n    <payload>");
      foreach (string item in theWrappedData)
        accum.Append(string.Format("\n      <item>{0}</item>", item));
      accum.Append("\n    </payload>");
      return accum.ToString();
    }
    public override IPersist FromXml(string xml)
    {
      XDocument doc = XDocument.Parse(xml);
      XElement payloadElem = doc.Descendants("payload").First();
      IEnumerable<XElement> newitems = payloadElem.Elements();
      PL_ListOfStrings los = new PL_ListOfStrings();
      foreach (var newitem in newitems)
        los.theWrappedData.Add(newitem.Value);
      return los;
    }
  }
#if (TEST_PAYLOADWRAPPER)

  class TestPayLoadWrapper
  {
    static void Main(string[] args)
    {
      "Testing Payload Wrapper".title('=');
      WriteLine();

      Write("\n  All tests have been moved to PayloadWrapperTest.cs to avoid circular references.");

      Write("\n\n");
    }
  }
#endif
}
