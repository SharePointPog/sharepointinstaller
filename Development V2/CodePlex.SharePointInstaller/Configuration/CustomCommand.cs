using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.StsAdmin;
using Microsoft.SharePoint.Utilities;

namespace CodePlex.SharePointInstaller.Configuration
{
    public class CustomCommand : CommandConfigurationElement
    {
        [ConfigurationProperty("class", IsRequired = true)]
        public string CommandType
        {
            get
            {
                return this["class"].ToString();
            }
        }
    }
}
