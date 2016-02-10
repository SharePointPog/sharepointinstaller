using System;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Wrappers
{
    public class WebApplicationInfo : EntityInfo
    {
        private SPWebApplication application;
        
        public WebApplicationInfo(SPWebApplication application) : base(application.Id, application.GetResponseUri(SPUrlZone.Default).ToString())
        {
            this.application = application;
            IsSRP = application.Properties.ContainsKey("Microsoft.Office.Server.SharedResourceProvider");
            SetDescription();
        }

        public bool IsSRP
        {
            get; 
            private set;
        }

        public SPWebApplication Application
        {
            get
            {
                return application;
            }
        }

        protected void SetDescription()
        {
            Description = application.GetResponseUri(SPUrlZone.Default).ToString();

            if (application.IsAdministrationWebApplication)
                Description += "     (Central Administration)";
            else if (IsSRP)
                Description += "     (Shared Resource Provider)";
            else if (!String.IsNullOrEmpty(application.DisplayName))
                Description += "     (" + application.DisplayName + ")";
            else if (!String.IsNullOrEmpty(application.Name))
                Description += "     (" + application.Name + ")";
        }        
    }
}
