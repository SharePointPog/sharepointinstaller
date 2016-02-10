using System;
using System.Windows.Forms;

namespace CodePlex.SharePointInstaller.Controls
{
    public class InstallerControl : UserControl
    {
        protected InstallerControl()
        {
            
        }

        protected InstallerControl(Installer form)
        {
            Form = form;
        }

        public String Title
        {
            get; 
            set;
        }

        public String SubTitle
        {
            get; 
            set;
        }

        protected Installer Form
        {
            get; 
            private set;
        }

        protected internal virtual void CancelInstallation()
        {
            Application.Exit();
        }
    }
}