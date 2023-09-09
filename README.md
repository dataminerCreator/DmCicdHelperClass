Assembly (version 1.0.0.1):

    Helper class for use with Skyline.DataMiner.CICD.DMApp.Automation

    Provides methods for creating protocol xml file with C# code from MS Visual Studio solution.



Public methods:

    ----------------------------------------------------------------------------------------------------------------------------------------------------------------------

	public static bool ProtocolXmlFileBuilder (string solutionFolderPath, string destinationFolderPath, out string protocolWithCsXmlPath, bool isSameDestAllowed = false)

	Creates protocolWithCs.xml file by inserting QAction code to the protocol.xml file located in the MS Visual Studio solution.
    Out: string path to the protocolWithCs.xml file.

    Requires read access to solutionFolderPath.
    Requires read/write access to the destinationFolderPath.
    
    destinationFolderPath must be different from solutionFolderPath (default)

    Exceptions:

    'solutionFolderPath and destinationFolderPath cannot be the same' exception (only if isSameDestAllowed == false [default])
    'File protocol.xml in [solutionFolderPath] was not found.' exception.
    'File protocolWithCs.xml in [destinationFolderPath] already exists. Please delete or rename the file and try again.' exception.

    ----------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static bool ProtocolXmlBytesArrayBuilder (string solutionFolderPath, out byte[] protocolWithCsXmlBytesArray, bool isLohGcCompactOnceEnabled = false)

    Creates byte[] of protocol.xml by inserting QAction code to the protocol.xml file in the MS Visual Studio solution.
    Out: byte[] of the complete protocol.xml file.

    Requires read access to solutionFolderPath.
    Optional LOH CompactOnce mode can be enabled by setting isLohGcCompactOnceEnabled == true.

    Exceptions:

    'File protocol.xml in [solutionFolderPath] was not found.' exception.

     ----------------------------------------------------------------------------------------------------------------------------------------------------------------------


Private methods:

    ----------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private static Dictionary<string, string> CsFilePathDictionary(string solutionPath, string xmlInSolutionPath)
    
    Returns Dictionary containing QAction id (Key) and the file path to the .cs file (Value).

    ----------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private static byte[] XmlFileWithCsByteArray(string solutionFolderPath, bool isLohGcEnabled)

    ----------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private static string UpdateXmlFileWithCs(string solutionFolderPath, string destinationFolderPath)

    ----------------------------------------------------------------------------------------------------------------------------------------------------------------------

Example implementation:

    Creating .dmprotocol

    ------ Example 1 -------------

    using Skyline.AppInstaller;
    using DataMinerPackageCreatorHelperClass;

    string solutionFolderPath = "C:\\Users\\iuser\\repos\\MyVSProjectFolder";
    string destinationFolderPath = "C:\\Users\\iuser\\repos\\TempFolder";
    string protocolWithCsPath = String.Empty;

    bool success = XmlBuilder.ProtocolXmlFileBuilder(solutionFolderPath, destinationFolderPath, out protocolWithCsPath);
    
    if (success)
    {
        var protocolBuilder = new AppPackageProtocol.AppPackageProtocolBuilder("MyProtocol", "1.0.1", protocolWithCsPath);

        IAppPackageProtocol protocol = protocolBuilder.Build();

        protocol.CreatePackage($"{destinationFolderPath}\\"MyProtocol.dmprotocol");

        File.Delete(protocolWithCsPath);
    }

    ------ End of example 1 ------


    ------ Example 2 -------------

    using Skyline.AppInstaller;
    using DataMinerPackageCreatorHelperClass;

    string solutionFolderPath = "C:\\Users\\iuser\\repos\\MyVSProjectFolder";
    string destinationFolderPath = "C:\\Users\\iuser\\repos\\TempFolder";
    byte[] protocolBytes;

    bool success = XmlBuilder.ProtocolXmlBytesArrayBuilder(solutionFolderPath, out protocolBytes);

    if (success)
    {
        var protocolBuilder = new AppPackageProtocol.AppPackageProtocolBuilder("MyProtocol", "1.0.1", protocolBytes);

        IAppPackageProtocol protocol = protocolBuilder.Build();

        protocol.CreatePackage($"{destinationFolderPath}\\"MyProtocol.dmprotocol");
    }

    ------ End of example 2 ------