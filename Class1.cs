using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Text;
using System.Xml;

namespace DataMinerPackageCreatorHelperClass
{
    /// <summary>
    /// Provides methods for creating protocol xml file with C# code from MS Visual Studio solution.
    /// </summary>
    /// <remarks>
    /// Built to work with Skyline.DataMiner.CICD.DMApp.Automation
    /// </remarks>

    public static class XmlBuilder
    {   
        private static readonly string _xmlFile = "protocol.xml";
        private static readonly string _updatedXmlFile = "protocolWithCs.xml";

        /// <summary>
        /// Creates protocolWithCs.xml file by inserting QAction code to the protocol.xml file located in the MS Visual Studio solution. <br/>
        /// Out: string path to the protocolWithCs.xml file. <br/><br/>
        /// 
        /// Requires read access to solutionFolderPath. <br/>
        /// Requires read/write access to the destinationFolderPath. <br/>
        /// 
        /// destinationFolderPath must be different from solutionFolderPath (default)
        /// 
        /// </summary>
        /// <remarks>
        /// Exceptions: <br/><br/>
        /// 
        /// 'solutionFolderPath and destinationFolderPath cannot be the same' exception (only if isSameDestAllowed == false [default]) <br/>
        /// 'File protocol.xml in [solutionFolderPath] was not found.' exception. <br/>
        /// 'File protocolWithCs.xml in [destinationFolderPath] already exists. Please delete or rename the file and try again.' exception. <br/>
        /// </remarks>

        public static bool ProtocolXmlFileBuilder (string solutionFolderPath, string destinationFolderPath, out string protocolWithCsXmlPath, bool isSameDestAllowed = false)
        {
            if (!isSameDestAllowed)
            {
                if (String.Equals(solutionFolderPath, destinationFolderPath))
                {
                    throw new Exception("solutionPathFolder and destinationFolderPath cannot be the same.");
                }
            }

            protocolWithCsXmlPath =  UpdateXmlFileWithCs(solutionFolderPath, destinationFolderPath);

            return true;
        }

        /// <summary>
        /// Creates byte[] of protocol.xml by inserting QAction code to the protocol.xml file in the MS Visual Studio solution. <br/>
        /// Out: byte[] of the complete protocol.xml file. <br/><br/>
        /// 
        /// Requires read access to solutionFolderPath. <br/>
        /// Optional LOH CompactOnce mode can be enabled by setting isLohGcCompactOnceEnabled == true.
        /// </summary>
        /// <remarks>
        /// 
        /// Exceptions: <br/><br/>
        /// 
        /// 'File protocol.xml in [solutionFolderPath] was not found.' exception.
        /// </remarks>

        public static bool ProtocolXmlBytesArrayBuilder (string solutionFolderPath, out byte[] protocolWithCsXmlBytesArray, bool isLohGcCompactOnceEnabled = false)
        {
            protocolWithCsXmlBytesArray = XmlFileWithCsByteArray(solutionFolderPath, isLohGcCompactOnceEnabled);

            if (protocolWithCsXmlBytesArray != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static string UpdateXmlFileWithCs(string solutionFolderPath, string destinationFolderPath)
        {
            Dictionary<string, string> csFilePathDictionary = new Dictionary<string, string>();
            string xmlInSolutionPath = $"{solutionFolderPath}\\{_xmlFile}";
            string updatedXmlPath = $"{destinationFolderPath}\\{_updatedXmlFile}";

            if (!File.Exists(xmlInSolutionPath))
            { 
                throw new Exception($"File {_xmlFile} in {solutionFolderPath} was not found.");
            }

            if (File.Exists(updatedXmlPath))
            {
                throw new Exception($"File {_updatedXmlFile} in {destinationFolderPath} already exists. Please delete or rename the file and try again.");
            }

            csFilePathDictionary = CsFilePathDictionary(solutionFolderPath, xmlInSolutionPath);

            using (StreamReader xmlReader = new StreamReader(xmlInSolutionPath))
            using (StreamWriter writer = new StreamWriter(updatedXmlPath, append: true))
            {
                while (true)
                {
                    string xmlLine = xmlReader.ReadLine();

                    if (xmlLine.Contains("QAction id="))
                    {
                        string id = xmlLine.Substring(xmlLine.IndexOf("\"") + 1, ((xmlLine.IndexOf("name") - 2) - (xmlLine.IndexOf("\"") + 1)));

                        string csFilePath = string.Empty;

                        csFilePathDictionary.TryGetValue(id, out csFilePath);

                        if (File.Exists(csFilePath))
                        {
                            bool xmlLineModReq = xmlLine.Contains("/>");
                            bool firstLine = true;

                            if (xmlLineModReq)
                            {
                                xmlLine = xmlLine.Replace("/", "");
                                writer.WriteLine(xmlLine);
                            }
                            else
                            {
                                writer.WriteLine(xmlLine);
                            }

                            using (StreamReader csReader = new StreamReader(csFilePath))
                            {
                                while (true)
                                {
                                    if (csReader.EndOfStream)
                                    {
                                        writer.WriteLine($"{csReader.ReadLine()}]]>");
                                        break;
                                    }
                                    else if (firstLine)
                                    {
                                        writer.WriteLine($"<![CDATA[{csReader.ReadLine()}");
                                    }
                                    else
                                    {
                                        writer.WriteLine(csReader.ReadLine());
                                    }

                                    firstLine = false;
                                }

                                if (xmlLineModReq)
                                {
                                    writer.WriteLine("</QAction>");
                                }
                            }
                        }
                    }
                    else
                    {
                        writer.WriteLine(xmlLine);
                    }

                    if (xmlReader.EndOfStream)
                    {
                        break;
                    }
                }
            }

            return updatedXmlPath;
        }

        private static byte[] XmlFileWithCsByteArray(string solutionFolderPath, bool isLohGcEnabled)
        {
            Dictionary<string, string> csFilePathDictionary = new Dictionary<string, string>();
            string xmlInSolutionPath = $"{solutionFolderPath}\\{_xmlFile}";
            string xmlWithCsString = String.Empty;

            if (!File.Exists(xmlInSolutionPath))
            {
                throw new Exception($"File {_xmlFile} in {solutionFolderPath} was not found.");
            }

            csFilePathDictionary = CsFilePathDictionary(solutionFolderPath, xmlInSolutionPath);

            if (isLohGcEnabled)
            {
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();
            }

            using (StreamReader xmlReader = new StreamReader(xmlInSolutionPath))
            {
                while (true)
                {
                    string stringLine = String.Empty;

                    stringLine = xmlReader.ReadLine();

                    if (stringLine.Contains("QAction id="))
                    {
                        string id = stringLine.Substring(stringLine.IndexOf("\"") + 1, ((stringLine.IndexOf("name") - 2) - (stringLine.IndexOf("\"") + 1)));

                        string csFilePath = string.Empty;

                        csFilePathDictionary.TryGetValue(id, out csFilePath);

                        if (File.Exists(csFilePath))
                        {
                            bool xmlLineModReq = stringLine.Contains("/>");
                            bool firstLine = true;

                            if (xmlLineModReq)
                            {
                                xmlWithCsString += stringLine.Replace("/", "");
                                xmlWithCsString += "\r";
                            }
                            else
                            {
                                xmlWithCsString += stringLine;
                                xmlWithCsString += "\r";
                            }

                            using (StreamReader csReader = new StreamReader(csFilePath))
                            {
                                while (true)
                                {
                                    if (csReader.EndOfStream)
                                    {
                                        xmlWithCsString += $"{csReader.ReadLine()}]]>";
                                        xmlWithCsString += "\r";
                                        break;
                                    }
                                    else if (firstLine)
                                    {
                                        xmlWithCsString += $"<![CDATA[{csReader.ReadLine()}";
                                        xmlWithCsString += "\r";
                                    }
                                    else
                                    {
                                        xmlWithCsString += csReader.ReadLine();
                                        xmlWithCsString += "\r";
                                    }

                                    firstLine = false;
                                }

                                if (xmlLineModReq)
                                {
                                    xmlWithCsString += "</QAction>";
                                    xmlWithCsString += "\r";
                                }
                            }
                        }
                    }
                    else
                    {
                        xmlWithCsString += stringLine;
                        xmlWithCsString += "\r";
                    }

                    if (xmlReader.EndOfStream)
                    {
                        break;
                    }
                }
            }

            return Encoding.UTF8.GetBytes(xmlWithCsString);
        }

        private static Dictionary<string, string> CsFilePathDictionary(string solutionPath, string xmlInSolutionPath)
        {
            Dictionary<string, string> csFilePathDictionary = new Dictionary<string, string>();

            XmlDocument doc = new XmlDocument();
            doc.Load(xmlInSolutionPath);

            XmlNodeList nodes = doc.GetElementsByTagName("QAction");

            foreach (XmlNode node in nodes)
            {
                XmlAttribute qId = node.Attributes["id"];

                if (qId != null)
                {
                    csFilePathDictionary.Add(qId.Value, $"{solutionPath}\\QAction_{qId.Value}\\QAction_{qId.Value}.cs");
                }
            }

            return csFilePathDictionary;
        }
    }
}