namespace CodePlex.SharePointInstaller
{
  partial class UpgradeControl
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpgradeControl));
            this.upgradeRadioButton = new System.Windows.Forms.RadioButton();
            this.removeRadioButton = new System.Windows.Forms.RadioButton();
            this.messageLabel = new System.Windows.Forms.Label();
            this.hintLabel = new System.Windows.Forms.Label();
            this.upgradeDescriptionLabel = new System.Windows.Forms.Label();
            this.removeDescriptionLabel = new System.Windows.Forms.Label();
            this.doactivateFeaturesChoice = new System.Windows.Forms.CheckBox();
            this.dodeactivateFeaturesChoice = new System.Windows.Forms.CheckBox();
            this.featureLocationSummaryLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // upgradeRadioButton
            // 
            resources.ApplyResources(this.upgradeRadioButton, "upgradeRadioButton");
            this.upgradeRadioButton.Checked = true;
            this.upgradeRadioButton.Name = "upgradeRadioButton";
            this.upgradeRadioButton.TabStop = true;
            this.upgradeRadioButton.UseVisualStyleBackColor = true;
            this.upgradeRadioButton.CheckedChanged += new System.EventHandler(this.upgradeRadioButton_CheckedChanged);
            // 
            // removeRadioButton
            // 
            resources.ApplyResources(this.removeRadioButton, "removeRadioButton");
            this.removeRadioButton.Name = "removeRadioButton";
            this.removeRadioButton.UseVisualStyleBackColor = true;
            this.removeRadioButton.CheckedChanged += new System.EventHandler(this.removeRadioButton_CheckedChanged);
            // 
            // messageLabel
            // 
            resources.ApplyResources(this.messageLabel, "messageLabel");
            this.messageLabel.Name = "messageLabel";
            // 
            // hintLabel
            // 
            resources.ApplyResources(this.hintLabel, "hintLabel");
            this.hintLabel.Name = "hintLabel";
            // 
            // upgradeDescriptionLabel
            // 
            resources.ApplyResources(this.upgradeDescriptionLabel, "upgradeDescriptionLabel");
            this.upgradeDescriptionLabel.Name = "upgradeDescriptionLabel";
            // 
            // removeDescriptionLabel
            // 
            resources.ApplyResources(this.removeDescriptionLabel, "removeDescriptionLabel");
            this.removeDescriptionLabel.Name = "removeDescriptionLabel";
            // 
            // doactivateFeaturesChoice
            // 
            resources.ApplyResources(this.doactivateFeaturesChoice, "doactivateFeaturesChoice");
            this.doactivateFeaturesChoice.Name = "doactivateFeaturesChoice";
            this.doactivateFeaturesChoice.UseVisualStyleBackColor = true;
            this.doactivateFeaturesChoice.CheckedChanged += new System.EventHandler(this.doactivateFeaturesChoice_CheckedChanged);
            // 
            // dodeactivateFeaturesChoice
            // 
            resources.ApplyResources(this.dodeactivateFeaturesChoice, "dodeactivateFeaturesChoice");
            this.dodeactivateFeaturesChoice.Name = "dodeactivateFeaturesChoice";
            this.dodeactivateFeaturesChoice.UseVisualStyleBackColor = true;
            this.dodeactivateFeaturesChoice.CheckedChanged += new System.EventHandler(this.dodeactivateFeaturesChoice_CheckedChanged);
            // 
            // featureLocationSummaryLabel
            // 
            resources.ApplyResources(this.featureLocationSummaryLabel, "featureLocationSummaryLabel");
            this.featureLocationSummaryLabel.Name = "featureLocationSummaryLabel";
            // 
            // UpgradeControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.featureLocationSummaryLabel);
            this.Controls.Add(this.doactivateFeaturesChoice);
            this.Controls.Add(this.dodeactivateFeaturesChoice);
            this.Controls.Add(this.removeDescriptionLabel);
            this.Controls.Add(this.upgradeDescriptionLabel);
            this.Controls.Add(this.hintLabel);
            this.Controls.Add(this.messageLabel);
            this.Controls.Add(this.removeRadioButton);
            this.Controls.Add(this.upgradeRadioButton);
            this.Name = "UpgradeControl";
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.RadioButton upgradeRadioButton;
    private System.Windows.Forms.RadioButton removeRadioButton;
    private System.Windows.Forms.Label messageLabel;
    private System.Windows.Forms.Label hintLabel;
    private System.Windows.Forms.Label upgradeDescriptionLabel;
    private System.Windows.Forms.Label removeDescriptionLabel;
    private System.Windows.Forms.CheckBox doactivateFeaturesChoice;
    private System.Windows.Forms.CheckBox dodeactivateFeaturesChoice;
    private System.Windows.Forms.Label featureLocationSummaryLabel;
  }
}
