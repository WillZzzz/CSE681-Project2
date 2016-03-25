/////////////////////////////////////////////////////////////////////
// UtilityExtensions.cs - define methods to simplify project code  //
// Ver 1.2                                                         //
// Application: Demonstration for CSE687-OOD, Project#2            //
// Language:    C#, ver 6.0, Visual Studio 2015                    //
// Platform:    Dell XPS2700, Core-i7, Windows 10                  //
// Author:      Jim Fawcett, CST 4-187, Syracuse University        //
//              (315) 443-3948, jfawcett@twcny.rr.com              //
/////////////////////////////////////////////////////////////////////
// Copyright (c) James W. Fawcett, 2015                            //
// All rights granted to users provided this notice is retained    //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package implements utility extensions and static test functions
 * that are used by many other packages in this solution.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: UtilityExtensions.cs
 *
 * Build Process:  devenv Project2Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 1.2 : 13 Oct 15
 * - minor changes to comments
 * ver 1.1 : 10 Oct 15
 * - added copyright notice
 * ver 1.0 : 13 Sep 15
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
  ///////////////////////////////////////////////////////////////////////
  // UtilityExtensions class
  // - provides extension methods useful for most applications
  //
  public static class UtilityExtensions
  {
    public static void title(this string aString, char underline = '-')
    {
      Console.Write("\n  {0}", aString);
      Console.Write("\n {0}", new string(underline, aString.Length + 2));
    }
  }
  ///////////////////////////////////////////////////////////////////////
  // Utilities class
  // - provides static methods useful for testing instance relationships
  //
  public static class Utilities  // these are not extension methods
  {
    public static void showEquality(object obj1, object obj2, string msg)
    {
      Write("\n  {0}", msg);
      if (obj1.Equals(obj2))  // same state
        Write("\n    objects have same state");
      else
        Write("\n    objects do not have same state");

      if (ReferenceEquals(obj1, obj2)) // same reference value, e.g., same object
        Write("\n    both are references to same object");
      else
        Write("\n    different objects");
    }
    public static void showDateTimeEquality(DateTime obj1, DateTime obj2, string msg)
    {
      Write("\n  {0}", msg);
      if (obj1.ToString() == obj2.ToString())  // same state
        Write("\n    DateTimes have same state");
      else
        Write("\n    DateTimes do not have same state");

      if (ReferenceEquals(obj1, obj2)) // same reference value, e.g., same object
        Write("\n    both are references to same object");
      else
        Write("\n    different objects");
    }
    public static void showListEquality<T>(IEnumerable<T> obj1, IEnumerable<T> obj2, string msg)
    {
      Write("\n  {0}", msg);
      if (obj1.SequenceEqual(obj2))  // same state
        Write("\n    Lists have same state");
      else
        Write("\n    Lists do not have same state");

      if (ReferenceEquals(obj1, obj2)) // same reference value, e.g., same object
        Write("\n    both are references to same object");
      else
        Write("\n    different objects");
    }
  }
  public class TestUtilityExtensions
  {
    static void Main(string[] args)
    {
      "Testing UtilityExtensions.title".title();
      Write("\n\n");
    }
  }
}
