using System;
using System.IO;
using CodePlex.SharePointInstaller.Commands;
using CodePlex.SharePointInstaller.Configuration;

namespace CodePlex.SharePointInstaller.Controls
{
    public partial class EULAControl : Step
    {
        public EULAControl(Installer form, Step previous) : base(form, previous)
        {
            InitializeComponent();
        }

        private void ShowLicenseText(InstallationContext context)
        {
            InstallConfiguration installConfiguration = GetConfiguration(context);
            if (!String.IsNullOrEmpty(installConfiguration.EULA))
            {
                try
                {
                    richTextBox.LoadFile(installConfiguration.EULA);
                    acceptCheckBox.Enabled = true;
                }
                catch (IOException)
                {
                    richTextBox.Lines = new[] { "Error! Could not load " + installConfiguration.EULA };
                }
            }
        }

        private void OnAcceptChecked(object sender, EventArgs e)
        {
            Form.NextButton.Enabled = acceptCheckBox.Checked;
        }

        public override void Initialize(InstallationContext context)
        {
            //Form.PrevButton.Enabled = true;
            Form.NextButton.Enabled = acceptCheckBox.Checked;
            Form.SkipButton.Enabled = false;
            richTextBox.TabStop = false;
        }

        public override void Execute(InstallationContext context)
        {
            Title = Resources.CommonUIStrings.ControlTitleLicense;
            SubTitle = Resources.CommonUIStrings.ControlSubTitleLicense;
            ShowLicenseText(context);
            acceptCheckBox.Focus();
        }

        public override Step CreateNextStep(InstallationContext context)
        {
            Form.Context.Action = InstallAction.Install;
            return new DeploymentTargetsControl(Form, this);
        }
    }
}