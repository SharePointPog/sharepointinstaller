using System;
using CodePlex.SharePointInstaller.Commands;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Controls;
using CodePlex.SharePointInstaller.Resources;

namespace CodePlex.SharePointInstaller.Controls
{
    public partial class WelcomeControl : Step
    {
        public WelcomeControl(Installer form, Step previous) : base(form, previous)
        {
            InitializeComponent();            
        }

        public override void Initialize(InstallationContext context)
        {
            Title = InstallationUtility.ReplaceSolutionTitle(CommonUIStrings.ControlTitleWelcome, GetConfiguration(context).InstallationName);
            SubTitle = InstallationUtility.ReplaceSolutionTitle(CommonUIStrings.ControlSubTitleWelcome, GetConfiguration(context).InstallationName);
            Form.SetTitle(Title);
            Form.SetSubTitle(SubTitle);
            Form.NextButton.Enabled = true;
            Form.SkipButton.Enabled = false;
            Form.NextButton.Focus();
            TabStop = false;
        }

        public override void Execute(InstallationContext context)
        {
            messageLabel.TabStop = false;
            messageLabel.Text = InstallationUtility.ReplaceWelcomeVersion(messageLabel.Text);
            messageLabel.Text = InstallationUtility.ReplaceSolutionTitles(messageLabel.Text, Form.Configuration.Solutions);
        }

        public override Step CreateNextStep(InstallationContext context)
        {
            return new SystemPrerequisitesValidatorControl(Form, this);
        }
        }
}