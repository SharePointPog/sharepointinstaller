namespace CodePlex.SharePointInstaller
{
  partial class SystemCheckControl
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SystemCheckControl));
        this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
        this.messageLabel = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // tableLayoutPanel
        // 
        this.tableLayoutPanel.AccessibleDescription = null;
        this.tableLayoutPanel.AccessibleName = null;
        resources.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
        this.tableLayoutPanel.BackgroundImage = null;
        this.tableLayoutPanel.Font = null;
        this.tableLayoutPanel.Name = "tableLayoutPanel";
        // 
        // messageLabel
        // 
        this.messageLabel.AccessibleDescription = null;
        this.messageLabel.AccessibleName = null;
        resources.ApplyResources(this.messageLabel, "messageLabel");
        this.messageLabel.Font = null;
        this.messageLabel.Name = "messageLabel";
        // 
        // SystemCheckControl
        // 
        this.AccessibleDescription = null;
        this.AccessibleName = null;
        resources.ApplyResources(this, "$this");
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackgroundImage = null;
        this.Controls.Add(this.tableLayoutPanel);
        this.Controls.Add(this.messageLabel);
        this.Font = null;
        this.Name = "SystemCheckControl";
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.Label messageLabel;
  }
}
