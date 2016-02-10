using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebPartPages;

namespace CodePlex.SharePointInstaller.CommandsApi.OperationHelpers
{
    public class Utilities
    {
        public const string ENCODED_SPACE = "_x0020_";

        /// <summary>
        /// Gets all bindings.
        /// </summary>
        /// <value>All bindings.</value>
        public static BindingFlags AllBindings
        {
            get
            {
                return BindingFlags.CreateInstance |
                BindingFlags.FlattenHierarchy |
                BindingFlags.GetField |
                BindingFlags.GetProperty |
                BindingFlags.IgnoreCase |
                BindingFlags.Instance |
                BindingFlags.InvokeMethod |
                BindingFlags.NonPublic |
                BindingFlags.Public |
                BindingFlags.SetField |
                BindingFlags.SetProperty |
                BindingFlags.Static;
            }
        }

#if MOSS
        /// <summary>
        /// Gets the shared resource provider.
        /// </summary>
        /// <param name="sspname">The sspname.</param>
        /// <returns></returns>
        public static object GetSharedResourceProvider(string sspname)
        {
            ServerContext current = ServerContext.GetContext(sspname);

            return GetSharedResourceProvider(current);
        }

        /// <summary>
        /// Gets the shared resource provider.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <returns></returns>
        public static object GetSharedResourceProvider(ServerContext current)
        {
            // return current.SharedResourceProvider;
            return GetPropertyValue(current, "SharedResourceProvider");
        }
#endif
        /// <summary>
        /// Gets the default shared resource provider.
        /// </summary>
        /// <returns></returns>
        public static object GetDefaultSharedResourceProvider()
        {
            Type srpCollectionType = Type.GetType("Microsoft.Office.Server.Administration.SharedResourceProviderCollection, Microsoft.Office.Server, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");
            ConstructorInfo srpCollectionConstructor = srpCollectionType.GetConstructor(
                AllBindings,
                null, new Type[] { typeof(SPFarm) }, null);
            object srpCollection = srpCollectionConstructor.Invoke(new object[] { SPFarm.Local });
            object srp = GetPropertyValue(srpCollection, "Default");
            return srp;
        }

        
        /// <summary>
        /// Gets the SPRequest object.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <returns></returns>
        public static object GetSPRequestObject(SPWeb web)
        {
            return GetPropertyValue(web, "Request");
        }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="o">The object whose property is to be retrieved.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static object GetPropertyValue(object o, string propertyName)
        {
            return o.GetType().GetProperty(propertyName, AllBindings).GetValue(o, null);
        }

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="o">The object whose property is to be set.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value to set the property to.</param>
        public static void SetPropertyValue(object o, string propertyName, object value)
        {
            try
            {
                o.GetType().GetProperty(propertyName, AllBindings).SetValue(o, value, null);
            }
            catch (AmbiguousMatchException)
            {
                Type t = o.GetType();
                while (true)
                {
                    try
                    {
                        t.GetProperty(propertyName, AllBindings | BindingFlags.DeclaredOnly).SetValue(o, value, null);
                        break;
                    }
                    catch (NullReferenceException)
                    {
                        if (t.BaseType == null)
                            return;

                        t = t.BaseType;
                    }
                }
            }
        }

        public static void SetPropertyValue(object o, Type type, string propertyName, object value)
        {
            try
            {
                type.GetProperty(propertyName, AllBindings).SetValue(o, value, null);
            }
            catch (AmbiguousMatchException)
            {
                Type t = type;
                while (true)
                {
                    try
                    {
                        t.GetProperty(propertyName, AllBindings | BindingFlags.DeclaredOnly).SetValue(o, value, null);
                        break;
                    }
                    catch (NullReferenceException)
                    {
                        if (t.BaseType == null)
                            return;

                        t = t.BaseType;
                    }
                }
            }
        }

        public static object GetFieldValue(object o, string fieldName)
        {
            return o.GetType().GetField(fieldName, AllBindings).GetValue(o);
        }

        public static object GetFieldValueLookup(SPListItem item, string fieldName)
        {
            if (item != null)
            {
                SPFieldLookupValue lookupValue =
                    new SPFieldLookupValue(item[fieldName] as string);
                return lookupValue.LookupValue;
            }
            else
            {
                return string.Empty;
            }
        }

        public static void SetFieldValue(object o, Type type, string fieldName, object value)
        {
            type.GetField(fieldName, AllBindings).SetValue(o, value);
        }

        /// <summary>
        /// Executes the method.
        /// </summary>
        /// <param name="objectType">The type.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <param name="parameterValues">The parameter values.</param>
        /// <returns></returns>
        public static object ExecuteMethod(Type objectType, string methodName, Type[] parameterTypes, object[] parameterValues)
        {
            return ExecuteMethod(objectType, null, methodName, parameterTypes, parameterValues);
        }

        /// <summary>
        /// Executes the method.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <param name="parameterValues">The parameter values.</param>
        /// <returns></returns>
        public static object ExecuteMethod(object obj, string methodName, Type[] parameterTypes, object[] parameterValues)
        {
            return ExecuteMethod(obj.GetType(), obj, methodName, parameterTypes, parameterValues);
        }

        /// <summary>
        /// Executes the method.
        /// </summary>
        /// <param name="objectType">The type.</param>
        /// <param name="obj">The obj.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <param name="parameterValues">The parameter values.</param>
        /// <returns></returns>
        public static object ExecuteMethod(Type objectType, object obj, string methodName, Type[] parameterTypes, object[] parameterValues)
        {
            MethodInfo methodInfo = objectType.GetMethod(methodName, AllBindings, null, parameterTypes, null);
            try
            {
                return methodInfo.Invoke(obj, parameterValues);
            }
            catch (TargetInvocationException ex)
            {
                // Get and throw the real exception.
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Processes the RPC results.
        /// </summary>
        /// <param name="results">The results.</param>
        public static void ProcessRpcResults(string results)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(results);
            XmlElement errorText = (XmlElement)xml.SelectSingleNode("//ErrorText");
            if (errorText != null)
            {
                throw new SPException(errorText.InnerText + "(" + xml.DocumentElement.GetAttribute("Code") + ")");
            }
        }

        /// <summary>
        /// Gets the formatted XML.
        /// </summary>
        /// <param name="xmlDoc">The XML doc.</param>
        /// <returns></returns>
        public static string GetFormattedXml(XmlDocument xmlDoc)
        {
            StringBuilder sb = new StringBuilder();

            XmlTextWriter xmlWriter = new XmlTextWriter(new StringWriter(sb));
            xmlWriter.Formatting = Formatting.Indented;
            xmlDoc.WriteContentTo(xmlWriter);
            xmlWriter.Flush();

            return sb.ToString();
        }

        /// <summary>
        /// The LookupAccountSid function accepts a security identifier (SID) as input. It retrieves the name of the account for this SID and the name of the first domain on which this SID is found.
        /// </summary>
        /// <param name="systemName">A pointer to a null-terminated character string that specifies the target computer. This string can be the name of a remote computer. If this string is NULL, the account name translation begins on the local system. If the name cannot be resolved on the local system, this function will try to resolve the name using domain controllers trusted by the local system. Generally, specify a value for systemName only when the account is in an untrusted domain and the name of a computer in that domain is known.</param>
        /// <param name="sid">The SID to look up in binary format</param>
        /// <param name="name">A pointer to a buffer that receives a null-terminated string that contains the account name that corresponds to the sid parameter.</param>
        /// <param name="nameBuffer">On input, specifies the size, in TCHARs, of the name buffer. If the function fails because the buffer is too small or if nameBuffer is zero, nameBuffer receives the required buffer size, including the terminating null character.</param>
        /// <param name="domainName">A pointer to a buffer that receives a null-terminated string that contains the name of the domain where the account name was found.</param>
        /// <param name="domainNameBuffer">On input, specifies the size, in TCHARs, of the domainName buffer. If the function fails because the buffer is too small or if domainNameBuffer is zero, domainNameBuffer receives the required buffer size, including the terminating null character.</param>
        /// <param name="accountType">A pointer to a variable that receives a SID_NAME_USE value that indicates the type of the account.</param>
        /// <returns></returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool LookupAccountSid(string systemName, byte[] sid, StringBuilder name, ref int nameBuffer, StringBuilder domainName, ref int domainNameBuffer, ref SID_NAME_USE accountType);

        /// <summary>
        /// The SID_NAME_USE enumeration type contains values that specify the type of a security identifier (SID).
        /// Needed for TryGetNT4StyleAccountName and the unmanaged LookupAccountSid method.
        /// </summary>
        public enum SID_NAME_USE
        {
            SidTypeAlias = 4,
            SidTypeComputer = 9,
            SidTypeDeletedAccount = 6,
            SidTypeDomain = 3,
            SidTypeGroup = 2,
            SidTypeInvalid = 7,
            SidTypeUnknown = 8,
            SidTypeUser = 1,
            SidTypeWellKnownGroup = 5
        }

        /// <summary>
        /// Determines whether [is login valid] [the specified STR login name].
        /// </summary>
        /// <param name="strLoginName">Name of the STR login.</param>
        /// <param name="bIsUserAccount">if set to <c>true</c> [b is user account].</param>
        /// <returns>
        /// 	<c>true</c> if [is login valid] [the specified STR login name]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLoginValid(string strLoginName, out bool bIsUserAccount)
        {
            bIsUserAccount = false;
            object request = GetPropertyValue(SPFarm.Local, "Request");

            MethodInfo methodInfo = typeof(SPUtility).GetMethod("IsLoginValid", AllBindings, null, new Type[] { request.GetType(), typeof(string), typeof(bool).MakeByRefType() }, null);
            try
            {
                object[] args = new object[] {request, strLoginName, bIsUserAccount};
                bool isLoginValid = (bool)methodInfo.Invoke(null, args);
                bIsUserAccount = (bool)args[2];
                return isLoginValid;
            }
            catch (TargetInvocationException ex)
            {
                // Get and throw the real exception.
                throw ex.InnerException;
            }

        }



        /// <summary>
        /// Gets the field schema.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="featureSafe">if set to <c>true</c> [feature safe].</param>
        /// <param name="removeEncodedSpaces">if set to <c>true</c> [remove encoded spaces].</param>
        /// <returns></returns>
        public static string GetFieldSchema(SPField field, bool featureSafe, bool removeEncodedSpaces)
        {
            string schema = field.SchemaXml;
            if (field.InternalName.Contains(ENCODED_SPACE) && removeEncodedSpaces)
            {
                schema = schema.Replace(string.Format("Name=\"{0}\"", field.InternalName),
                                        string.Format("Name=\"{0}\"", field.InternalName.Replace(ENCODED_SPACE, string.Empty)));
            }
            if (featureSafe)
            {
                XmlDocument schemaDoc = new XmlDocument();
                schemaDoc.LoadXml(schema);
                XmlElement fieldElement = schemaDoc.DocumentElement;

                // Remove the Version attribute
                if (fieldElement.HasAttribute("Version"))
                    fieldElement.RemoveAttribute("Version");

                // Remove the Aggregation attribute
                if (fieldElement.HasAttribute("Aggregation"))
                    fieldElement.RemoveAttribute("Aggregation");

                // Remove the Customization attribute
                if (fieldElement.HasAttribute("Customization"))
                    fieldElement.RemoveAttribute("Customization");

                // Fix the UserSelectionMode attribute
                if (fieldElement.HasAttribute("UserSelectionMode"))
                {
                    if (fieldElement.GetAttribute("UserSelectionMode") == "PeopleAndGroups")
                        fieldElement.SetAttribute("UserSelectionMode", "1");
                    else if (fieldElement.GetAttribute("UserSelectionMode") == "PeopleOnly")
                        fieldElement.SetAttribute("UserSelectionMode", "0");
                }
                schema = schemaDoc.OuterXml;
            }
            return schema;
        }

        /// <summary>
        /// Gets the field.
        /// </summary>
        /// <param name="listViewUrl">The list view URL.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldTitle">The field title.</param>
        /// <param name="useFieldName">if set to <c>true</c> [use field name].</param>
        /// <param name="useFieldTitle">if set to <c>true</c> [use field title].</param>
        /// <returns></returns>
        public static SPField GetField(string listViewUrl, string fieldName, string fieldTitle, bool useFieldName, bool useFieldTitle)
        {
            SPList list = GetListFromViewUrl(listViewUrl);

            if (list == null)
            {
                throw new Exception("List not found.");
            }
            
            SPField field = null;
            try
            {
                // It's possible to have more than one field in the fields collection with the same display name.  The Fields collection
                // will merely return back the first item it finds with a name matching the one specified so it's really a rather useless
                // way of retrieving a field as it's extremely misleading and could lead to someone inadvertantly messing things up.
                // I provide the ability to use the display name for convienence but don't rely on it for anything.
                if (useFieldTitle)
                    field = list.Fields[fieldTitle];
            }
            catch (ArgumentException)
            {
            }

            if (field != null || useFieldName)
            {
                int count = 0;
                string foundFields = string.Empty;
                // If the user specified the display name we need to make sure that only one field exists matching that display name.
                // If they specified the public name then we need to loop until we find a match.
                foreach (SPField temp in list.Fields)
                {
                    if (useFieldName && (temp.InternalName.ToLowerInvariant() == fieldName.ToLowerInvariant() || temp.Id.ToString().Replace("{", "").Replace("}", "").ToLowerInvariant() == fieldName.ToLowerInvariant().Replace("{", "").Replace("}", "")))
                    {
                        field = temp;
                        break;
                    }
                    else if (useFieldTitle && temp.Title == fieldTitle)
                    {
                        count++;
                        foundFields += "\t" + temp.Title + " = " + temp.InternalName + "\r\n";
                    }
                }
                if (useFieldTitle && count > 1)
                {
                    throw new Exception("More than one field was found matching the display name specified:\r\n\r\n\tDisplay Name = public Name\r\n\t----------------------------\r\n" +
                                        foundFields +
                                        "\r\nUse \"-fieldpublicname\" to delete based on the public name of the field.");
                }
            }

            if (field == null)
                throw new Exception("Field not found.");
            return field;
        }




        /// <summary>
        /// Splits the path file.
        /// </summary>
        /// <param name="fullPathFile">The full path file.</param>
        /// <param name="path">The path.</param>
        /// <param name="filename">The filename.</param>
        public static void SplitPathFile(string fullPathFile, out string path, out string filename)
        {
            FileInfo info = new FileInfo(fullPathFile);
            path = info.Directory.FullName;
            filename = info.Name;
        }


        /// <summary>
        /// Gets the list from the view URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static SPList GetListFromViewUrl(string url)
        {
            using (SPSite site = new SPSite(url))
            using (SPWeb web = site.OpenWeb())
            {
                return GetListFromViewUrl(web, url);
            }
        }

        /// <summary>
        /// Gets the list from the view URL.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static SPList GetListFromViewUrl(SPWeb web, string url)
        {
            url = SPEncode.UrlDecodeAsUrl(url);

            SPList list = null;
            if (url.ToLowerInvariant().EndsWith(".aspx"))
            {
                try
                {
                    list = web.GetListFromWebPartPageUrl(url);
                }
                catch (SPException)
                {
                    // This block is redundant - if the above fails this should also fail - I left it here for legacy reasons only.
                    foreach (SPList tempList in web.Lists)
                    {
                        foreach (SPView view in tempList.Views)
                        {
                            if (url.ToLower() == SPEncode.UrlDecodeAsUrl(web.Site.MakeFullUrl(view.ServerRelativeUrl)).ToLower())
                            {
                                list = tempList;
                                break;
                            }
                        }
                        if (list != null)
                            break;
                    }
                }
            }
            else
            {
                try
                {
                    SPFolder folder = web.GetFolder(url);
                    if (folder == null)
                        return null;
                    list = web.Lists[folder.ParentListId];
                }
                catch (Exception)
                {
                    list = null;
                }
            }
            return list;
        }

        /// <summary>
        /// Gets the list by URL.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="listUrlName">Name of the list URL.</param>
        /// <param name="isDocLib">if set to <c>true</c> [is doc lib].</param>
        /// <returns></returns>
        public static SPList GetListByUrl(SPWeb web, string listUrlName, bool isDocLib)
        {
            string strUrl;
            if (!web.IsRootWeb)
            {
                strUrl = web.ServerRelativeUrl;
            }
            else
            {
                strUrl = string.Empty;
            }
            if (!isDocLib)
            {
                strUrl = strUrl + "/Lists/" + listUrlName;
            }
            else
            {
                strUrl = strUrl + "/" + listUrlName;
            }
            return web.GetList(strUrl);
        }


        /// <summary>
        /// Runs the operation.
        /// </summary>
        /// <param name="args">The arguments to pass into STSADM.</param>
        /// <param name="quiet">if set to <c>true</c> [quiet].</param>
        /// <returns></returns>
        public static int RunStsAdmOperation(string args, bool quiet)
        {
            string stsadmPath = Path.Combine(SPUtility.GetGenericSetupPath("BIN"), "stsadm.exe");

            return RunCommand(stsadmPath, args, quiet);
        }

        /// <summary>
        /// Runs the command.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="args">The args.</param>
        /// <param name="quiet">if set to <c>true</c> [quiet].</param>
        /// <returns></returns>
        public static int RunCommand(string fileName, string args, bool quiet)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.FileName = fileName;
            startInfo.Arguments = args;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;

            Process proc = new Process();
            try
            {
                proc.ErrorDataReceived += new DataReceivedEventHandler(Process_ErrorDataReceived);
                if (!quiet)
                    proc.OutputDataReceived += new DataReceivedEventHandler(Process_OutputDataReceived);

                proc.StartInfo = startInfo;
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();

                return proc.ExitCode;
            }
            finally
            {
                proc.Close();
            }
        }

        /// <summary>
        /// Handles the ErrorDataReceived event of the Process control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Diagnostics.DataReceivedEventArgs"/> instance containing the event data.</param>
        static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        /// <summary>
        /// Handles the OutputDataReceived event of the Process control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Diagnostics.DataReceivedEventArgs"/> instance containing the event data.</param>
        static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == "Operation completed successfully.")
                return;

            Console.WriteLine(e.Data);
        }

        /// <summary>
        /// Determines whether a string starts with another string.  This is taken from Microsoft.SharePoint.Utilities.SPUtility
        /// and is needed by ConvertToServiceRelUrl.
        /// </summary>
        /// <param name="strMain">The STR main.</param>
        /// <param name="strBegining">The STR begining.</param>
        /// <returns></returns>
        public static bool StsStartsWith(string strMain, string strBegining)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IsPrefix(strMain, strBegining, CompareOptions.IgnoreCase);
        }


        /// <summary>
        /// compare strings.
        /// </summary>
        /// <param name="str1">The STR1.</param>
        /// <param name="str2">The STR2.</param>
        /// <returns></returns>
        public static bool StsCompareStrings(string str1, string str2)
        {
            CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
            return (0 == compareInfo.Compare(str1, str2, CompareOptions.IgnoreCase));
        }

 

        /// <summary>
        /// Splits the URL.
        /// </summary>
        /// <param name="fullOrRelativeUri">The full or relative URI.</param>
        /// <param name="dirName">Name of the dir.</param>
        /// <param name="leafName">Name of the leaf.</param>
        public static void SplitUrl(string fullOrRelativeUri, out string dirName, out string leafName)
        {
            if (fullOrRelativeUri != null)
            {
                if ((fullOrRelativeUri.Length > 0) && ('/' == fullOrRelativeUri[0]))
                {
                    fullOrRelativeUri = fullOrRelativeUri.Substring(1);
                }
            }
            else
            {
                dirName = string.Empty;
                leafName = string.Empty;
                return;
            }
            int length = fullOrRelativeUri.LastIndexOf('/');
            if (-1 != length)
            {
                dirName = fullOrRelativeUri.Substring(0, length);
                leafName = fullOrRelativeUri.Substring(length + 1);
            }
            else
            {
                dirName = string.Empty;
                if (fullOrRelativeUri.Length > 0)
                {
                    if ('/' == fullOrRelativeUri[0])
                    {
                    }
                    leafName = fullOrRelativeUri.Substring(1);
                }
                else
                {
                    leafName = string.Empty;
                }
            }
        }



        /// <summary>
        /// Converts to service rel URL.  This is taken from Microsoft.SharePoint.Utilities.SPUtility.
        /// </summary>
        /// <param name="strUrl">The STR URL.</param>
        /// <param name="strBaseUrl">The STR base URL.</param>
        /// <returns></returns>
        public static string ConvertToServiceRelUrl(string strUrl, string strBaseUrl)
        {
            if (((strBaseUrl == null) || !StsStartsWith(strBaseUrl, "/")) || ((strUrl == null) || !StsStartsWith(strUrl, "/")))
            {
                throw new ArgumentException();
            }
            if ((strUrl.Length > 1) && (strUrl[strUrl.Length - 1] == '/'))
            {
                strUrl = strUrl.Substring(0, strUrl.Length - 1);
            }
            if ((strBaseUrl.Length > 1) && (strBaseUrl[strBaseUrl.Length - 1] == '/'))
            {
                strBaseUrl = strBaseUrl.Substring(0, strBaseUrl.Length - 1);
            }
            if (!StsStartsWith(strUrl, strBaseUrl))
            {
                throw new ArgumentException();
            }
            if (strBaseUrl != "/")
            {
                if (strUrl.Length != strBaseUrl.Length)
                {
                    return strUrl.Substring(strBaseUrl.Length + 1);
                }
                return "";
            }
            return strUrl.Substring(1);
        }

        /// <summary>
        /// Concats the server relative urls.
        /// </summary>
        /// <param name="firstPart">The first part.</param>
        /// <param name="secondPart">The second part.</param>
        /// <returns></returns>
        public static string ConcatServerRelativeUrls(string firstPart, string secondPart)
        {
            firstPart = firstPart.TrimEnd('/');
            secondPart = secondPart.TrimStart('/');
            return (firstPart + "/" + secondPart);
        }

        /// <summary>
        /// Gets the server relative URL from full URL.
        /// </summary>
        /// <param name="url">The STR URL.</param>
        /// <returns></returns>
        public static string GetServerRelUrlFromFullUrl(string url)
        {
            int index = url.IndexOf("//");
            if ((index < 0) || (index == (url.Length - 2)))
            {
                throw new ArgumentException();
            }
            int startIndex = url.IndexOf('/', index + 2);
            if (startIndex < 0)
            {
                return "/";
            }
            string str = url.Substring(startIndex);
            if (str.IndexOf("?") >= 0)
                str = str.Substring(0, str.IndexOf("?"));

            if (str.IndexOf(".aspx") > 0)
                str = str.Substring(0, str.LastIndexOf("/"));

            if ((str.Length > 1) && (str[str.Length - 1] == '/'))
            {
                return str.Substring(0, str.Length - 1);
            }
            return str;
        }

        /// <summary>
        /// Gets the checked out user id.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static string GetCheckedOutUserId(SPItem item)
        {
            if (item is SPListItem)
            {
                SPListItem item2 = (SPListItem)item;
                if (item2.File != null)
                {
                    if (item2.File.CheckedOutBy == null)
                        return null;

                    return item2.File.CheckedOutBy.LoginName;
                }
                if (item2.ParentList.BaseType == SPBaseType.DocumentLibrary)
                {
                    return (string)item["CheckoutUser"];
                }
            }
            return null;
        }

        /// <summary>
        /// Determines whether [is checked out by current user] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if [is checked out by current user] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCheckedOutByCurrentUser(SPItem item)
        {
            string user = GetCheckedOutUserId(item);
            if (string.IsNullOrEmpty(user))
                return false;
            return ((Environment.UserDomainName + "\\" + Environment.UserName).ToLowerInvariant() == user.ToLowerInvariant());
        }

        /// <summary>
        /// Determines whether the list item is checked out..
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if is checked out; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCheckedOut(SPItem item)
        {
            return !string.IsNullOrEmpty(GetCheckedOutUserId(item));
        }

        /// <summary>
        /// Ensures the aspx.
        /// </summary>
        /// <param name="relativeUrl">The relative URL.</param>
        /// <param name="allowMasterPage">if set to <c>true</c> [allow master page].</param>
        /// <param name="throwException">if set to <c>true</c> [throw exception].</param>
        /// <returns></returns>
        public static bool EnsureAspx(string relativeUrl, bool allowMasterPage, bool throwException)
        {
            if (relativeUrl == null)
            {
                if (throwException)
                    throw new ArgumentNullException();
                else
                    return false;
            }
            string extension = Path.GetExtension(relativeUrl);
            if (!string.IsNullOrEmpty(extension))
            {
                extension = extension.Substring(1);
            }
            if (!string.IsNullOrEmpty(extension))
            {
                if (CompareStrings(extension, "aspx"))
                {
                    return true;
                }
                if (allowMasterPage)
                {
                    if (CompareStrings(extension, "master"))
                    {
                        return true;
                    }
                    if (CompareStrings(extension, "ascx"))
                    {
                        return true;
                    }
                }
            }
            if (throwException)
                throw new SPException(string.Format("Url is not a valid aspx or master page: {0}", relativeUrl));
            else
                return false;
        }

        /// <summary>
        /// Compares the strings.
        /// </summary>
        /// <param name="str1">The STR1.</param>
        /// <param name="str2">The STR2.</param>
        /// <returns></returns>
        public static bool CompareStrings(string str1, string str2)
        {
            CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
            return (0 == compareInfo.Compare(str1, str2, CompareOptions.IgnoreCase));
        }



        /// <summary>
        /// Determines whether this instance [can convert to from] the specified converter.
        /// </summary>
        /// <param name="converter">The converter.</param>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if this instance [can convert to from] the specified converter; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanConvertToFrom(TypeConverter converter, Type type)
        {
            return ((((converter != null) && converter.CanConvertTo(type)) && converter.CanConvertFrom(type)) && !(converter is ReferenceConverter));
        }

        /// <summary>
        /// Formats the exception.
        /// </summary>
        /// <param name="ex">The exception object.</param>
        /// <returns></returns>
        public static string FormatException(Exception ex)
        {
            if (ex == null) return "";

            string msg = "";

            msg += "\r\nError Type:      " + ex.GetType();
            msg += "\r\nError Message:   " + ex.Message; //Get the error message
            msg += "\r\nError Source:    " + ex.Source;  //Source of the message
            msg += "\r\nError TargetSite:" + ex.TargetSite; //Method where the error occurred
            foreach (DictionaryEntry de in ex.Data)
            {
                msg += "\r\n" + de.Key + ": " + de.Value;
            }
            msg += "\r\nError Stack Trace:\r\n" + ex.StackTrace; //Stack Trace of the error
            if (ex.InnerException != null)
                msg += "\r\nInner Exception:\r\n" + FormatException(ex.InnerException);

            return msg;
        }

        /// <summary>
        /// Creates the secure string.
        /// </summary>
        /// <param name="strIn">The string to convert.</param>
        /// <returns></returns>
        public static SecureString CreateSecureString(string strIn)
        {
            if (strIn != null)
            {
                SecureString str = new SecureString();
                foreach (char ch in strIn)
                {
                    str.AppendChar(ch);
                }
                str.MakeReadOnly();
                return str;
            }
            return null;
        }
    }
}
