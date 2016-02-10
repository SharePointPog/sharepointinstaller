using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CodePlex.SharePointInstaller.CommandsApi;
using Microsoft.SharePoint;
#if !SP2010
using Microsoft.SharePoint.StsAdmin;
#endif

namespace CodePlex.SharePointInstaller.CommandsApi.OperationHelpers
{
    public abstract class SPOperation : IPluggableCommand
    {
        protected const int OUTPUT_SUCCESS = 0;
        protected const int OUTPUT_FAILED = -1;
        private SPParamCollection m_params;
        private string m_helpMessage;
        private static bool m_Verbose;
        private static string m_LogFile;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SPOperation"/> is verbose.
        /// </summary>
        /// <value><c>true</c> if verbose; otherwise, <c>false</c>.</value>
        public static bool Verbose
        {
            get { return m_Verbose; }
            set { m_Verbose = value; }
        }

        /// <summary>
        /// Gets or sets the log file.
        /// </summary>
        /// <value>The log file.</value>
        public static string LogFile
        {
            get { return m_LogFile; }
            set { m_LogFile = value; }
        }



        // 98d3057cd9024c27b2007643c1 is a special hard coded name for a list that Microsoft uses to store the mapping
        // of URLs from v2 to v3 (maps the bucket urls to the new urls).
        protected const string UPGRADE_AREA_URL_LIST = "98d3057cd9024c27b2007643c1";


        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
        public static void Log(string message, params string[] args)
        {
            Log(message, EventLogEntryType.Information, args);
        }

        /// <summary>
        /// Logs the specified STR.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="msgType">Type of the MSG.</param>
        /// <param name="args">The args.</param>
        public static void Log(string message, EventLogEntryType msgType, params string[] args)
        {
            message = string.Format(message, args);
            if (msgType != EventLogEntryType.Information || Verbose)
            {
                Console.WriteLine(message);
            }
            if (!string.IsNullOrEmpty(m_LogFile))
                File.AppendAllText(m_LogFile, message + "\r\n");
        }

        /// <summary>
        /// Inits the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="helpMessage">The help message.</param>
        protected void Init(SPParamCollection parameters, string helpMessage)
        {
            helpMessage = helpMessage.TrimEnd(new char[] {'\r', '\n'});
#if DEBUG
            helpMessage += "\r\n\t[-debug]";
            parameters.Add(new SPParam("debug", "debug"));
#endif

            m_params = parameters;
            m_helpMessage = helpMessage;
        }

        /// <summary>
        /// Inits the parameters.
        /// </summary>
        /// <param name="keyValues">The key values.</param>
        public void InitParameters(StringDictionary keyValues)
        {
            foreach (SPParam param in Params)
            {
                param.InitValueFrom(keyValues);
            }
#if DEBUG
            if (Params["debug"].UserTypedIn)
                Debugger.Launch();
#endif
        }


        /// <summary>
        /// Validates the specified key values.
        /// </summary>
        /// <param name="keyValues">The key values.</param>
        public virtual void Validate(StringDictionary keyValues)
        {
            string message;
            if (!IsSyntaxValid(keyValues, out message))
                throw new SPSyntaxException(message);
        }

        public bool IsSyntaxValid(StringDictionary keyValues, out string message)
        {
            return IsSyntaxValid(keyValues, true, out message);
        }

        public virtual bool IsSyntaxValid(StringDictionary keyValues, bool validateUrl, out string message)
        {
            message = null;
            foreach (string current in keyValues.Keys)
            {
                if (current != "o" && Params[current] == null)
                {
                    message += string.Format("Command line error. Invalid parameter: {0}.\r\n", current);
                }
            }
            if (message != null)
                return false;

            InitParameters(keyValues);
            foreach (SPParam param in Params)
            {
                if (param.Enabled)
                {
                    if (param.IsRequired && !param.UserTypedIn)
                    {
                        message += SPResource.GetString("MissRequiredArg", new object[] { param.Name }) + "\r\n";
                    }
                }
            }
            if (message != null)
                return false;

            foreach (SPParam param in Params)
            {
                if (param.Enabled)
                {
                    if (param.UserTypedIn && !(param.Validate() || (param.Name.EndsWith("url") && !validateUrl)))
                    {
                        message += SPResource.GetString("InvalidArg", new object[] { param.Name });
                        if (!string.IsNullOrEmpty(param.ErrorInfo))
                            message += string.Format(" ({0})", param.ErrorInfo);

                        if (!string.IsNullOrEmpty(param.HelpMessage))
                        {
                            message += "\r\n\t" + param.HelpMessage + "\r\n";
                        }
                    }
                }
            }
            return message == null;
        }


        // Properties
        public virtual string DisplayNameId
        {
            get
            {
                return null;
            }
        }

       

        public string HelpMessage
        {
            get
            {
                return m_helpMessage;
            }
        }

        protected SPParamCollection Params
        {
            get
            {
                return m_params;
            }
        }

        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="keyValues">The key values.</param>
        /// <param name="output">The output.</param>
        /// <returns></returns>
        public abstract int Execute(string command, StringDictionary keyValues, out string output);

        #region ISPStsadmCommand Members

        /// <summary>
        /// Gets the help message.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public abstract string GetHelpMessage(string command);

        /// <summary>
        /// Runs the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="keyValues">The key values.</param>
        /// <param name="output">The output.</param>
        /// <returns></returns>
        public virtual int Run(string command, StringDictionary keyValues, out string output)
        {
            output = string.Empty;
            try
            {
                InitParameters(keyValues);
                Validate(keyValues);
                return Execute(command, keyValues, out output);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                else
                    throw;
            }
            catch (SPSyntaxException ex)
            {
                output += ex.Message;
                return (int) ErrorCodes.SyntaxError;
            }
        }

        #endregion
    }

    public class SPSyntaxException : ApplicationException
    {
        // Methods
        public SPSyntaxException(string strMessage)
            : base(strMessage)
        {
        }
    }

 

 
}
