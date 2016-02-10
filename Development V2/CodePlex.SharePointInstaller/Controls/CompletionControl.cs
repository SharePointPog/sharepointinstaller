using System;
using CodePlex.SharePointInstaller.Commands;
using CodePlex.SharePointInstaller.Controls;
using CodePlex.SharePointInstaller.Logging;

namespace CodePlex.SharePointInstaller.Controls
{
    public partial class CompletionControl : Step
    {
        public CompletionControl(Installer form, Step previous) : base(form, previous)
        {
            InitializeComponent();
            Title = "Setup completed.";
        }

        public String Details
        {
            get
            {
                return detailsTextBox.Text;
            }
            set
            {
                detailsTextBox.Text = value;
            }
        }
      
        public override void Initialize(InstallationContext context)
        {
            //Form.PrevButton.Enabled = false;
            Form.NextButton.Enabled = false;
            Form.SkipButton.Enabled = false;
            Form.HandleCompletion();
        }

        public override void Execute(InstallationContext context)
        {
            foreach (var record in Log.Records)
            {
                Details += record + Environment.NewLine;
            }

            if (String.IsNullOrEmpty(Details))
            {
                Details += "All the solutions were skipped.";
            }
        }

        public override Step CreateNextStep(InstallationContext context)
        {
            return null;
        }
    }
}