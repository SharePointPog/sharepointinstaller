using System.Collections.Specialized;

namespace CodePlex.SharePointInstaller.CommandsApi
{
    public interface IPluggableCommand 
#if !SP2010
        : Microsoft.SharePoint.StsAdmin.ISPStsadmCommand
#endif
    {
        bool IsSyntaxValid(StringDictionary keyValues, bool validateUrl, out string message);
#if SP2010 //TODO: rather switch pluggable commands to utilize Microsoft.SharePoint.PowerShell.SPCmdlet for 2010
        int Run(string command, StringDictionary keyValues, out string output);
#endif
    }

#if SP2010
    public enum ErrorCodes
    {
        GeneralError = -1,
        SyntaxError = -2
    }
#endif
}
