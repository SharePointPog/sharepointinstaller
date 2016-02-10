using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using Microsoft.SharePoint.StsAdmin;

namespace CodePlex.SharePointInstaller.Configuration
{
    public class CommandToRun : CommandConfigurationElement
    {
        [ConfigurationProperty("commandline", IsRequired = false)]
        public string Parameters
        {
            get
            {
                return this["commandline"] as string;
            }
            set
            {
                this["commandline"] = value;
            }
        }

        public override object Key
        {
            get
            {
                return String.Concat(Name, Parameters);
            }
        }

        public ISPStsadmCommand CommandInstance
        {
            get;
            set;
        }

        public StringDictionary ParseCommandLine()
        {
            return ParseCommandLine(Name, Parameters);
        }

        public static StringDictionary ParseCommandLine(string commandName, string commandLine)
        {
            var result = new StringDictionary {{"o", commandName}};
            bool insideQuote = false;
            bool quoted = false;
            string key = null;
            int i = 0;
            const char separator = ' ';
            var parameters = new StringBuilder(commandLine);
            parameters.Append(separator); //another separator to get the final parameter
            do
            {
                if (parameters[i] == '"')
                {
                    insideQuote = !insideQuote;
                    if (insideQuote)
                    {
                        quoted = true;
                    }
                }
                if ((parameters[i] != separator) || insideQuote)
                {
                    i++;
                    continue;
                }
                //here we hit a separator unquotted
                if (i == 0) //false alert it's just a leading whitespace
                {
                    parameters.Remove(0, 1); //clean and start over
                    i = 0;
                    continue;
                }
                bool isKey = parameters[0] == '-';

                //get the current value without markup
                string val;
                if (isKey)
                {
                    val = parameters.ToString(1, i - 1); //without the minus sign and trailing space
                }
                else
                {
                    val = quoted ? parameters.ToString(1, i - 2) : parameters.ToString(0, i);
                }

                if (String.IsNullOrEmpty(key)) //no key created so far
                {
                    if (!isKey || quoted)
                    {
                        return null; //incorrect syntax
                    }
                    key = val;
                }
                else //it's most likely a value (but could be another key)
                {
                    if (isKey) //two keys in a row (perfectly legal)
                    {
                        result.Add(key, String.Empty);
                        key = val;
                    }
                    else
                    {
                        result.Add(key, val);
                        key = null;
                    }
                }
                parameters.Remove(0, i + 1);
                i = 0; //start over
                quoted = false;
            } while (i < parameters.Length);
            if (key != null)
                result.Add(key, String.Empty);
            return result;
        }
    }
}
