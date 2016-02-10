using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;

namespace Stsadm.Commands.Lists
{
    /// <summary>
    /// This class holds the command which updates a field in the list specified either by its display or internal name
    /// </summary>
    /// <remarks>
    /// The command updates a field (column) using the provided input XML.  Use exportlistfield to get the existing schema and then modify the results 
    /// (the 'Name' attribute of the Field node must not change unless the fieldinternalname or fielddisplayname is passed in 
    /// as this attribute is what is used to determine which field to update).
    /// </remarks>
    public class UpdateListField : SPOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateListField"/> class.
        /// </summary>
        public UpdateListField()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the list view URL."));
            parameters.Add(new SPParam("fielddisplayname", "title", false, null, new SPNonEmptyValidator(), "Please specify the field's display name to delete."));
            parameters.Add(new SPParam("fieldinternalname", "name", false, null, new SPNonEmptyValidator(), "Please specify the field's internal name to delete."));
            parameters.Add(new SPParam("inputfile", "input", false, null, new SPDirectoryExistsAndValidFileNameValidator(), "Make sure a valid filename is provided."));
            parameters.Add(new SPParam("sortindex", "index", false, null, new SPIntRangeValidator(0, int.MaxValue)));

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nUpdates a field (column) using the provided input XML.  Use exportlistfield to get the existing schema and then modify the results (note that the 'Name' attribute of the Field node must not change unless the fieldinternalname or fielddisplayname is passed in as this attribute is what is used to determine which field to update).\r\n\r\nParameters:");
            sb.Append("\r\n\t-url <list view URL>");
            sb.Append("\r\n\t[-inputfile <input file containing the field schema information>]");
            sb.Append("\r\n\t[-fielddisplayname <field display name> / -fieldinternalname <field internal name>]");
            sb.Append("\r\n\t[-sortindex <field order index>]");
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
        ///         -url <list view URL>
        ///         [-inputfile <input file containing the field schema information>]
        ///         [-fielddisplayname <field display name> | -fieldinternalname <field internal name>]
        ///         [-sortindex <field order index>]
        /// 
        /// ]]>
        /// </param>
        /// <param name="output">The output to display.</param>
        /// <returns>The status code of the operation (zero means success).</returns>
        public override int Execute(string command, StringDictionary keyValues, out string output)
        {
            output = string.Empty;

            

            string url = Params["url"].Value;
            string fieldTitle = Params["fielddisplayname"].Value;
            string fieldName = Params["fieldinternalname"].Value;
            bool useTitle = Params["fielddisplayname"].UserTypedIn;
            bool useName = Params["fieldinternalname"].UserTypedIn;
            bool inputFileProvided = Params["inputfile"].UserTypedIn;


            if (!inputFileProvided && !Params["sortindex"].UserTypedIn)
            {
                throw new SPSyntaxException("You must either specify an input file with changes or a sort index.");
            }
            if (!inputFileProvided && !useTitle && !useName)
            {
                throw new SPSyntaxException(
                    "You must specify either an input file with changes or the field name to update.");
            }

            SPList list;
            SPField field;
            XmlDocument xmlDoc = new XmlDocument();
            string xml = null;

            if (inputFileProvided)
            {
                xml = File.ReadAllText(Params["inputfile"].Value);
                xmlDoc.LoadXml(xml);
            }

            if (!inputFileProvided || useTitle || useName)
            {
                field = Utilities.GetField(url, fieldName, fieldTitle, useName, useTitle);
                list = field.ParentList;
            }
            else
            {
                list = Utilities.GetListFromViewUrl(url);
                string internalName = xmlDoc.DocumentElement.GetAttribute("Name");
                field = list.Fields.GetFieldByInternalName(internalName);
            }

            if (inputFileProvided)
            {
                field.SchemaXml = xml;
                field.Update();
            }

            if (Params["sortindex"].UserTypedIn)
            {
                int sortIndex = int.Parse(Params["sortindex"].Value);
                ImportListField.ReorderField(list, field, sortIndex);
            }

            return OUTPUT_SUCCESS;
        }



        #endregion
    }
}
