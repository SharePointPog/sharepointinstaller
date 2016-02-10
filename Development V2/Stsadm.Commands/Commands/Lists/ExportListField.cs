using System.Collections.Specialized;
using System.IO;
using System.Text;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;

namespace Stsadm.Commands.Lists
{
    /// <summary>
    /// This class holds the command which exports a certain field into an XML file from the list specified either by its display or internal name.
    /// </summary>
    /// <remarks>
    /// With this command it's possible to export any given list field into an XML file for future importing/updating in another lists.
    /// </remarks>
    public class ExportListField : SPOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportListField"/> class.
        /// </summary>
        public ExportListField()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the list view URL."));
            parameters.Add(new SPParam("fielddisplayname", "title", false, null, new SPNonEmptyValidator(), "Please specify the field's display name to export."));
            parameters.Add(new SPParam("fieldinternalname", "name", false, null, new SPNonEmptyValidator(), "Please specify the field's internal name to export."));
            parameters.Add(new SPParam("outputfile", "output", false, null, new SPDirectoryExistsAndValidFileNameValidator(), "Make sure the output directory exists and a valid filename is provided."));

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nExports a list field (column) to a file.\r\n\r\nParameters:");
            sb.Append("\r\n\t-url <list view URL>");
            sb.Append("\r\n\t-fielddisplayname <field display name> / -fieldinternalname <field internal name>");
            sb.Append("\r\n\t-outputfile <file to output field schema to>");
            Init(parameters, sb.ToString());
        }

        #region ISPStsadmCommand Members

        /// <summary>
        /// Gets the help message.
        /// </summary>
        /// <param name="command">The command name.</param>
        /// <returns>The help message to display when this command is run with -help switch.</returns>
        public override string GetHelpMessage(string command)
        {
            return HelpMessage;
        }

        /// <summary>
        /// Runs the specified command.
        /// </summary>
        /// <param name="command">The command name.</param>
        /// <param name="keyValues">
        /// The following parameters are expected:<![CDATA[
        ///         -url list view URL>
        ///         -fielddisplayname <field display name> | -fieldinternalname <field internal name>
        ///         -outputfile <file to output field schema to>]]>
        /// </param>
        /// <param name="output">The output to display.</param>
        /// <returns>The status code of the operation (zero means success).</returns>
        public override int Execute(string command, StringDictionary keyValues, out string output)
        {
            output = string.Empty;

            
            SPBinaryParameterValidator.Validate("fielddisplayname", Params["fielddisplayname"].Value, "fieldinternalname", Params["fieldinternalname"].Value);

            string url = Params["url"].Value;
            string fieldTitle = Params["fielddisplayname"].Value;
            string fieldName = Params["fieldinternalname"].Value;
            bool useTitle = Params["fielddisplayname"].UserTypedIn;
            bool useName = Params["fieldinternalname"].UserTypedIn;

            SPField field = Utilities.GetField(url, fieldName, fieldTitle, useName, useTitle);

            File.WriteAllText(Params["outputfile"].Value, field.SchemaXml);

            return OUTPUT_SUCCESS;
        }



        #endregion
    }
}
