using System;
using System.Configuration;
using Microsoft.SharePoint;

namespace CodePlex.SharePointInstaller.Configuration
{
    public class FeatureInfo : ConfigurationElement
    {
        [ConfigurationProperty("id", IsRequired = true)]
        public Guid Id
        {
            get
            {
                return (Guid)this["id"];
            }
        }

        [ConfigurationProperty("scope", IsRequired = true)]
        public SPFeatureScope Scope
        {
            get
            {
                return (SPFeatureScope)Enum.Parse(typeof(SPFeatureScope), this["scope"].ToString());
            }
        }

        [ConfigurationProperty("url", IsRequired = false)]
        public String Url
        {
            get
            {
                return (String) this["url"];
            }
        }

        [ConfigurationProperty("name", IsRequired = false)]
        public String Name
        {
            get
            {
                return (String)this["name"] ?? string.Empty;
            }
        }

        [ConfigurationProperty("deactivateonly", IsRequired = false)]
        public bool DeactivateOnly
        {
            get
            {
                bool deactivateOnly;
                Boolean.TryParse(this["deactivateonly"].ToString(), out deactivateOnly);
                return deactivateOnly;
            }
        }

    }
}
