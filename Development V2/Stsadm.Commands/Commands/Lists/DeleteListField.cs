using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;

namespace Stsadm.Commands.Lists
{
    /// <summary>
    /// This class holds the command which deletes a given field in the list specified either by its display or internal name.
    /// </summary>
    /// <remarks>
    /// This is a simple command to allow deleting list fields by passing in either the display name or the internal name 
    /// (with the internal name being the safest approach). If the display name was passed in then a check is made to ensure that 
    /// there's only one field with that display name - if there're more then it returns back the matches along with the internal name 
    /// so that the command can be attempted again.
    /// </remarks>
    public class DeleteListField : SPOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteListField"/> class.
        /// </summary>
        public DeleteListField()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the list view URL."));
            parameters.Add(new SPParam("fielddisplayname", "title", false, null, new SPNonEmptyValidator(), "Please specify the field's display name to delete."));
            parameters.Add(new SPParam("fieldinternalname", "name", false, null, new SPNonEmptyValidator(), "Please specify the field's internal name to delete."));
            parameters.Add(new SPParam("force", "force", false, null, null));

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nDeletes a field (column) from a list.\r\n\r\nParameters:");
            sb.Append("\r\n\t-url <list view URL>");
            sb.Append("\r\n\t-fielddisplayname <field display name> / -fieldinternalname <field internal name>");
            sb.Append("\r\n\t[-force (used to force deletion if AllowDeletion property is false)]");
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
        /// <param name="keyValues">The following parameters are expected:<![CDATA[
        ///         -url <list view URL>
        ///         -fielddisplayname <field display name> | -fieldinternalname <field internal name>
        ///         [-force (used to force deletion if the field is marked as not allowing deletion)]]]>
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
            bool force = Params["force"].UserTypedIn;

            SPField field = Utilities.GetField(url, fieldName, fieldTitle, useName, useTitle);
            
            if (field.ReadOnlyField && force)
            {
                field.ReadOnlyField = false;
                field.Update();
            }
            if (!field.CanBeDeleted)
            {
                if (field.FromBaseType)
                {
                    throw new Exception(
                        "The field is derived from a base type and cannot be deleted.  You must delete the field from the base type.");
                }
                else if (field.Sealed)
                {
                    if (force)
                    {
                        field.Sealed = false;
                        field.Update();
                    }
                    else
                        throw new Exception("This field is sealed and cannot be deleted - specify \"-force\" to ignore this setting and attempt deletion regardless.");
                }
                else if (field.AllowDeletion.HasValue && !field.AllowDeletion.Value && !force)
                {
                    throw new Exception(
                        "Field is marked as not allowing deletion - specify \"-force\" to ignore this setting and attempt deletion regardless.");
                }
                else if (field.AllowDeletion.HasValue && !field.AllowDeletion.Value && force)
                {
                    field.AllowDeletion = true;
                    field.Update();
                }
                else
                {
                    throw new Exception("Field cannot be deleted.");
                }
            }
            if (field.Hidden)
            {
                if (force)
                {
                    if (field.CanToggleHidden)
                    {
                        field.Hidden = false;
                        field.Update();
                    }
                    else
                    {
                        MethodInfo setFieldBoolValue = field.GetType().GetMethod("SetFieldBoolValue",
                                BindingFlags.NonPublic | BindingFlags.Public |
                                BindingFlags.Instance | BindingFlags.InvokeMethod,
                                null, new Type[] { typeof(string), typeof(bool) }, null);

                        //field.SetFieldBoolValue("Hidden", false);
                        setFieldBoolValue.Invoke(field, new object[] { "Hidden", false });
                        //field.SetFieldBoolValue("CanToggleHidden", true);
                        setFieldBoolValue.Invoke(field, new object[] { "CanToggleHidden", true });
                        field.Update();
                    }
                }
                else
                    throw new Exception(
                        "You cannot delete hidden fields - specify \"-force\" to ignore this restriction and attempt deletion regardless.");
            }
            field.Delete();

            return OUTPUT_SUCCESS;
        }



        #endregion
    }
}
