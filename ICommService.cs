/////////////////////////////////////////////////////////////////////////
// ICommService.cs - Contract for WCF message-passing service          //
// ver 1.2                                                             //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Project #4    //
// Tianhang Zhang, CSE681 - Software Modeling and Analysis, Project #4 //
/////////////////////////////////////////////////////////////////////////
/*
 * Additions to C# Console Wizard generated code:
 * - Added reference to System.ServiceModel
 * - Added using System.ServiceModel
 * - Added reference to System.Runtime.Serialization
 * - Added using System.Runtime.Serialization
 */
/*
 * Maintenance History:
 * --------------------
 * ver 1.2 : 30 Nov 2015
 * - Modification to the datamembers to fulfill the
 * - needs of both kinds of clients.
 * ver 1.1 : 29 Oct 2015
 * - added comment in data contract
 * ver 1.0 : 18 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml;

namespace Project4Starter
{
  [ServiceContract (Namespace ="Project4Starter")]
  public interface ICommService
  {
    [OperationContract(IsOneWay = true)]
    void sendMessage(Message msg);
  }

  [DataContract]
  public class Message
  {
    [DataMember]
    public string fromUrl { get; set; }
    [DataMember]
    public string toUrl { get; set; }
    [DataMember]
    public string content { get; set; } // content to indicate the call of message and the result of query
    [DataMember]
    public List<string> response_lstrkey { get; set; } // contains respond message from server with list of strings
    [DataMember] 
    public List<int> response_lintkey { get; set; }  // contains respond message from server with list of integers
        [DataMember]
    public string clienttype { get; set; }  // indicate the type of client of read or write
    [DataMember]
    public string querytype { get; set; } // query type is different for read and write clients
    [DataMember]
    public int intkey { get; set; }  // contains keys of type integer
    [DataMember]
    public string strkey { get; set; }  // contains keys of type string
    [DataMember]
    public List<string> elem { get; set; }  // contains response message with list of database elements
    [DataMember]
    public string strcontain { get; set; }  // contains the specific string to be found in metadata
    [DataMember]
    public Dictionary<int, DBElement<int, PL_String>> response_intchild { get; set; } // contains children found of an element
    [DataMember]
    public Dictionary<string, DBElement<string, PL_ListOfStrings>> response_strchild { get; set; } // contains children found of an element
    [DataMember]
    public DBElement<int, PL_String> intelem { get; set; } // contains an database element
    [DataMember]
    public DBElement<string, PL_ListOfStrings> strelem { get; set; } // contains an database element
    }
}
