using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CodePlex.SharePointInstaller.Configuration
{
    public class CommandConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
            set
            {
                this["name"] = value;
            }
        }

        public virtual object Key
        {
            get
            {
                return Name;
            }
        }
    }
}
