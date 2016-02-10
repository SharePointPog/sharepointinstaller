using System;
using System.Configuration;

namespace CodePlex.SharePointInstaller.Configuration
{
    public class Parameter : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsRequired = true)]
        public String Name
        {
            get
            {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("value", DefaultValue = "", IsRequired = true)]
        public String Value
        {
            get
            {
                return this["value"] as string;
            }
        }
    }
}
