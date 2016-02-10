using System;
using CodePlex.SharePointInstaller.Commands;
using CodePlex.SharePointInstaller.Configuration;

namespace CodePlex.SharePointInstaller.Controls
{
    public partial class UpgradeControl : Step
    {
        public UpgradeControl(Installer form, Step previous) : base(form, previous)
        {
            InitializeComponent();
        }

        public UpgradeControl(Installer form, Step previous, bool enableUpgrade, bool enableRepair, bool enableRemove): this(form, previous)
        {
            upgradeRadioButton.Enabled = enableUpgrade;
            upgradeDescriptionLabel.Enabled = enableUpgrade;
            repairRadioButton.Enabled = enableRepair;
            repairDescriptionLabel.Enabled = enableRepair;
            removeRadioButton.Enabled = enableRemove;
            removeDescriptionLabel.Enabled = enableRemove;
        }
        
        private void UpgradeSelected(object sender, EventArgs e)
        {
            if (upgradeRadioButton.Checked)
            {
                Form.Context.Action = InstallAction.Upgrade;
                Form.NextButton.Enabled = true;
            }
        }

        private void RemoveSelected(object sender, EventArgs e)
        {
            if (removeRadioButton.Checked)
            {
                Form.Context.Action = InstallAction.Remove;
                Form.NextButton.Enabled = true;
            }
        }

        private void RepairSelected(object sender, EventArgs e)
        {
            if (repairRadioButton.Checked)
            {
                Form.Context.Action = InstallAction.Repair;
                Form.NextButton.Enabled = true;
            }
        }

        public override void Initialize(InstallationContext context)
        {            
            if (GetConfiguration(context).UpgradeDescription != null)
            {
                upgradeDescriptionLabel.Text = InstallationUtility.FormatString(context.SolutionInfo, GetConfiguration(context).UpgradeDescription);
            }

            if (repairRadioButton.Enabled)
                repairRadioButton.Checked = true;
            else
                if (upgradeRadioButton.Enabled)
                    upgradeRadioButton.Checked = true;
                else
                    removeRadioButton.Checked = true;

            Form.NextButton.Enabled = upgradeRadioButton.Checked || removeRadioButton.Checked || repairRadioButton.Checked;
            Form.SkipButton.Enabled = false;
        }

        public override void Execute(InstallationContext context)
        {
            Title = Resources.CommonUIStrings.ControlTitleUpgradeRemove;
            SubTitle = Resources.CommonUIStrings.ControlSubTitleSelectOperation;
            messageLabel.Text = InstallationUtility.FormatString(Form.Context.SolutionInfo, messageLabel.Text);
        }

        public override Step CreateNextStep(InstallationContext context)
        {
            return new InstallProcessControl(Form, this);
        }
    }
}