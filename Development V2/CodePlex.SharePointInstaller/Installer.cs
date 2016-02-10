using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using CodePlex.SharePointInstaller.Commands;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Controls;
using CodePlex.SharePointInstaller.Resources;

namespace CodePlex.SharePointInstaller
{
    public partial class Installer : Form
    {
        private Step currentStep;

        private InstallConfiguration configuration;

        public Installer()
        {
            InitializeComponent();
            Load += OnLoad;
            CurrentStep = new WelcomeControl(this, null);  
            CurrentStep.Initialize(Context);
            CurrentStep.Execute(Context);            
        }

        public InstallationContext CreateInstallationContext(IList<Guid> solutions)
        {
            return (Context = new InstallationContext(solutions));            
        }

        private void OnLoad(object sender, EventArgs e)
        {
            var bannerImageFile = Configuration.BannerImage;
            if (!String.IsNullOrEmpty(bannerImageFile))
            {
                titlePanel.BackgroundImageLayout = ImageLayout.Stretch;
                if (bannerImageFile != "Default")
                {
                    titlePanel.BackgroundImage = LoadImage(bannerImageFile);
                }
                else
                {
                    titlePanel.BackgroundImage = Properties.Resources.Banner;
                }
            }

            var logoImageFile = Configuration.LogoImage;
            if (!String.IsNullOrEmpty(logoImageFile))
            {
                if (logoImageFile != "Default")
                {
                    logoPicture.BackgroundImage = LoadImage(logoImageFile);
                }
            }

            var vendor = Configuration.Vendor;
            if (!String.IsNullOrEmpty(vendor))
            {
                vendorLabel.Text = vendor;
            }
            else
            {
                // For some reason I could not get the update in Installer.resx to stick - forcing value here
                vendorLabel.Text = "www.codeplex.com/sharepointinstaller";
            }

        }

        private void Skip(object sender, EventArgs e)
        {
            CurrentStep = CurrentStep.SkipAndGoToOtherStep(Context);
        }

        private void GoNext(object sender, EventArgs e)
        {
            CurrentStep = CurrentStep.GoNext();            
        }

        private void GoPrevious(object sender, EventArgs e)
        {
            CurrentStep = CurrentStep.GoPrevious();            
        }
        
        private void Cancel(object sender, EventArgs e)
        {
            if (CurrentStep != null)
            {
                CurrentStep.CancelInstallation();
            }
            else
            {
                Application.Exit();
            }
        }
       
        public Step CurrentStep
        {
            get
            {
                return currentStep;
            }
            private set
            {                
                value.Dock = DockStyle.Fill;                
                contentPanel.Controls.Clear();
                contentPanel.Controls.Add(value);
                currentStep = value;
                titleLabel.Text = value.Title;
                subTitleLabel.Text = value.SubTitle;
            }
        }

        public InstallConfiguration Configuration
        {
            get
            {
                if (configuration == null)
                {
                    configuration = InstallConfiguration.GetConfiguration();
                }
                return configuration;
            }
        }

        public InstallationContext Context
        {
            get; 
            private set;
        }

        public Button SkipButton
        {
            get
            {
                return skipButton;
            }
        }

        public Button AbortButton
        {
            get
            {
                return cancelButton;
            }
        }

        public Button PreviousButton
        {
            get
            {
                return previousButton;
            }
        }

        public Button NextButton
        {
            get
            {
                return nextButton;
            }
        }        
       
        public void SetTitle(string title)
        {
            titleLabel.Text = title;
        }

        public void HandleCompletion()
        {
            AbortButton.Text = CommonUIStrings.AbortButtonText;
        }

        public void SetSubTitle(string title)
        {
            subTitleLabel.Text = title;
        }        

        private Image LoadImage(String filename)
        {
            try
            {
                return Image.FromFile(filename);
            }

            catch (IOException)
            {
                return null;
            }
        }     
    }
}