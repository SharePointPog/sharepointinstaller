using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using CodePlex.SharePointInstaller.CommandsApi.SPValidators;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using CodePlex.SharePointInstaller.CommandsApi.OperationHelpers;

namespace Stsadm.Commands.Lists
{
    /// <summary>
    /// This class holds the command which imports a previously exported field to the list specified either by its display or internal name
    /// </summary>
    /// <remarks>
    /// Once the list field is exported it's possible then to import the field into another list either as is or with 
    /// whatever manual modifications you may have made (just be careful that you know what you are doing - it's always better 
    /// to make the modifications using the browser and then export those changes than it is to try and hack the XML directly). 
    /// By default when you run the import command the code will attempt to locate the "ID" attribute and replace it with a new 
    /// GUID value - otherwise you may get errors stating that the field already exists (even if you've changed the "Name" attribute). 
    /// If you don't want the code to do this then you can pass in the "-retainobjectidentity" parameter. 
    /// If a field with the same DisplayName already exists it can only be imported with the "-allowduplicates" switch. If "-addtodefaultview"
    /// is specified then it is also possible to set the order of the field in the view with the "-defaultviewsortindex" parameter.
    /// </remarks>
    public class ImportListField : SPOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportListField"/> class.
        /// </summary>
        public ImportListField()
        {
            SPParamCollection parameters = new SPParamCollection();
            parameters.Add(new SPParam("url", "url", true, null, new SPUrlValidator(), "Please specify the list view URL."));
            parameters.Add(new SPParam("inputfile", "input", false, null, new SPDirectoryExistsAndValidFileNameValidator(), "Make sure a valid filename is provided."));
            parameters.Add(new SPParam("addtodefaultview", "adv"));
            SPEnumValidator fieldOptionsValidator = new SPEnumValidator(typeof(SPAddFieldOptions));
            parameters.Add(new SPParam("addfieldoptions", "fo", false, null, new SPNonEmptyValidator()));
            parameters.Add(new SPParam("retainobjectidentity", "roi"));
            parameters.Add(new SPParam("sortindex", "index", false, null, new SPIntRangeValidator(0, int.MaxValue)));
            parameters.Add(new SPParam("allowduplicates", "duplicates"));
            parameters.Add(new SPParam("defaultviewsortindex", "viewindex", false, null, new SPIntRangeValidator(0, int.MaxValue)));

            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n\r\nImports a field (column) into a list.\r\n\r\nParameters:");
            sb.Append("\r\n\t-url <list view URL>");
            sb.Append("\r\n\t-inputfile <input file containing field schema information>");
            sb.AppendFormat("\r\n\t[-addfieldoptions <{0}>", fieldOptionsValidator.DisplayValue);
            sb.Append("\r\n\t[-addtodefaultview]");
            sb.Append("\r\n\t[-defaultviewsortindex <field order index in the default view>]");
            sb.Append("\r\n\t[-retainobjectidentity]");
            sb.Append("\r\n\t[-sortindex <field order index>] (zero-based)");
            sb.Append("\r\n\t[-allowduplicates]");
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
        ///         -inputfile <input file containing field schema information>
        ///         [-addfieldoptions <default | addtodefaultcontenttype | addtonocontenttype | addtoallcontenttypes | addfieldinternalnamehint | addfieldtodefaultview | addfieldcheckdisplayname>
        ///         [-addtodefaultview]
        ///         [-retainobjectidentity]
        ///         [-allowduplicates]
        ///         [-defaultviewsortindex <field order index in the default view>]
        ///         [-sortindex <field order index>]]]>
        /// </param>
        /// <param name="output">The output to display.</param>
        /// <returns>The status code of the operation (zero means success).</returns>
        public override int Execute(String command, StringDictionary keyValues, out String output)
        {
            output = String.Empty;
            var list = Utilities.GetListFromViewUrl(Params["url"].Value);                                    

            foreach(XmlNode field in GetFields(Params["inputfile"].Value))
            {
                if (!Params["allowduplicates"].UserTypedIn && list.Fields.ContainsField(field.Attributes["DisplayName"].Value))
                {
                    output += "The field of the same DisplayName is already present. Use -allowduplicates option to allow that.";
                    return 1;
                }

                var xml = field.OuterXml;
                var id = new Guid(field.Attributes["ID"].Value);
                if (!Params["retainobjectidentity"].UserTypedIn)
                {
                    id = Guid.NewGuid();
                    field.Attributes["ID"].Value = id.ToString();
                    xml = field.OuterXml;
                }

                list.Fields.AddFieldAsXml(xml, Params["addtodefaultview"].UserTypedIn, GetFieldOptions());
                
                if (Params["sortindex"].UserTypedIn)
                {
                    var sortIndex = int.Parse(Params["sortindex"].Value);
                    ReorderField(list, list.Fields[id], sortIndex);
                }

                AddFieldToView(list, Params["addtodefaultview"].UserTypedIn, list.Fields[id]);
            }
            return OUTPUT_SUCCESS;
        }

        private SPAddFieldOptions GetFieldOptions()
        {
            var fieldOptions = SPAddFieldOptions.Default;
            if (Params["addfieldoptions"].UserTypedIn)
            {
                var init = false;
                foreach (var option in Params["addfieldoptions"].Value.Split(','))
                {
                    if (!init)
                    {
                        fieldOptions = (SPAddFieldOptions)Enum.Parse(typeof(SPAddFieldOptions), option.Trim(), true);
                        init = true;
                        continue;
                    }
                    fieldOptions = fieldOptions | (SPAddFieldOptions)Enum.Parse(typeof(SPAddFieldOptions), option.Trim(), true);
                }
            }
            return fieldOptions;
        }

        private void AddFieldToView(SPList list, bool addToView, SPField field)
        {
            if (addToView && Params["defaultviewsortindex"].UserTypedIn) //reorder view fields
            {
                var sortIndex = int.Parse(Params["defaultviewsortindex"].Value);
                var fieldNames = new List<String>();
                var viewFields = list.DefaultView.ViewFields;
                var fieldsCount = viewFields.Count;
                var i = 0;
                var inserted = false;
                while (i < fieldsCount)
                {
                    if ((i == sortIndex) && (!inserted))
                    {
                        fieldNames.Add(field.InternalName);
                        inserted = true;
                    }
                    else
                    {
                        if (field.InternalName != list.DefaultView.ViewFields[i])
                            fieldNames.Add(list.DefaultView.ViewFields[i]);
                        i++;
                    }
                }

                viewFields.DeleteAll();
                foreach (var fieldName in fieldNames)
                    viewFields.Add(fieldName);
                list.DefaultView.Update();
            }
        }

        #endregion

        private XmlNodeList GetFields(String fileName)
        {
            var document = new XmlDocument();
            document.LoadXml(File.ReadAllText(fileName));
            return document.SelectNodes("/Fields/Field");
        }        

        public override bool IsSyntaxValid(StringDictionary keyValues, bool validateUrl, out string message)
        {
            if(!base.IsSyntaxValid(keyValues, validateUrl, out message))
                return false;
            if (Params["addfieldoptions"].UserTypedIn)
            {
                var fieldOptions = SPAddFieldOptions.Default;
                foreach (var option in Params["addfieldoptions"].Value.Split(','))
                {
                    try
                    {
                        fieldOptions = fieldOptions | (SPAddFieldOptions)Enum.Parse(typeof(SPAddFieldOptions), option.Trim(), true);
                    }
                    catch (ArgumentException)
                    {
                        message = String.Format("The addfieldoptions parameter contains an invalid value: {0}", option);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Reorders the field.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="field">The field.</param>
        /// <param name="sortIndex">The sort index.</param>
        internal static void ReorderField(SPList list, SPField field, int sortIndex)
        {
            if (field.Reorderable)
            {
                List<SPField> fields = new List<SPField>();
                int count = 0;
                bool added = false;
                // First add the reorderable fields
                for (int i = 0; i < list.Fields.Count; i++)
                {
                    if (list.Fields[i].Reorderable)
                    {
                        if (count == sortIndex)
                        {
                            added = true;
                            fields.Add(field);
                            count++;
                        }

                        if (list.Fields[i].Id == field.Id)
                            continue;

                        fields.Add(list.Fields[i]);
                        count++;
                    }
                }
                if (!added)
                    fields.Add(field);

                // Now add the non-reorderable fields
                for (int i = 0; i < list.Fields.Count; i++)
                {
                    if (!list.Fields[i].Reorderable)
                    {
                        fields.Add(list.Fields[i]);
                    }
                }

                StringBuilder sb = new StringBuilder();

                XmlTextWriter xmlWriter = new XmlTextWriter(new StringWriter(sb));
                xmlWriter.Formatting = Formatting.Indented;

                xmlWriter.WriteStartElement("Fields");

                for (int i = 0; i < fields.Count; i++)
                {
                    xmlWriter.WriteStartElement("Field");
                    xmlWriter.WriteAttributeString("Name", fields[i].InternalName);
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
                xmlWriter.Flush();

                using (SPWeb web = list.ParentWeb)
                {
                    ReorderFields(web, list, sb.ToString());
                }
            }
        }

        /// <summary>
        /// This function reorders the fields in the specified list programmatically as specified by the xmlFieldsOrdered parameter
        /// </summary>
        /// <param name="web">The SPWeb object containing the list</param>
        /// <param name="list">The SPList object to update</param>
        /// <param name="xmlFieldsOrdered">A string in XML-format specifying the field order by the location within a xml-tree</param>
        private static void ReorderFields(SPWeb web, SPList list, string xmlFieldsOrdered)
        {
            try
            {
                string fpRPCMethod = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                 <Method ID=""0,REORDERFIELDS"">
                 <SetList Scope=""Request"">{0}</SetList>
                 <SetVar Name=""Cmd"">REORDERFIELDS</SetVar>
                 <SetVar Name=""ReorderedFields"">{1}</SetVar>
                 <SetVar Name=""owshiddenversion"">{2}</SetVar>
                 </Method>";

                // relookup list version in order to be able to update it
                list = web.Lists[list.ID];

                int currentVersion = list.Version;

                string version = currentVersion.ToString();
                string RpcCall = string.Format(fpRPCMethod, list.ID, SPHttpUtility.HtmlEncode(xmlFieldsOrdered), version);

                web.AllowUnsafeUpdates = true;

                Utilities.ProcessRpcResults(web.ProcessBatchData(RpcCall));
            }
            catch (System.Net.WebException err)
            {
                Console.WriteLine("WARNING:" + err.Message);
            }
        }

    }
}
