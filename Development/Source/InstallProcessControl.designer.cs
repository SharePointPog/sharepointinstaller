namespace CodePlex.SharePointInstaller
{
  partial class InstallProcessControl
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallProcessControl));
        this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
        this.progressBar = new System.Windows.Forms.ProgressBar();
        this.descriptionLabel = new System.Windows.Forms.Label();
        this.panel1 = new System.Windows.Forms.Panel();
        this.errorDetailsTextBox = new System.Windows.Forms.TextBox();
        this.errorPictureBox = new System.Windows.Forms.PictureBox();
        this.tableLayoutPanel.SuspendLayout();
        this.panel1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.errorPictureBox)).BeginInit();
        this.SuspendLayout();
        // 
        // tableLayoutPanel
        // 
        this.tableLayoutPanel.AccessibleDescription = null;
        this.tableLayoutPanel.AccessibleName = null;
        resources.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
        this.tableLayoutPanel.BackgroundImage = null;
        this.tableLayoutPanel.Controls.Add(this.progressBar, 0, 1);
        this.tableLayoutPanel.Controls.Add(this.descriptionLabel, 0, 2);
        this.tableLayoutPanel.Controls.Add(this.panel1, 0, 3);
        this.tableLayoutPanel.Font = null;
        this.tableLayoutPanel.Name = "tableLayoutPanel";
        // 
        // progressBar
        // 
        this.progressBar.AccessibleDescription = null;
        this.progressBar.AccessibleName = null;
        resources.ApplyResources(this.progressBar, "progressBar");
        this.progressBar.BackgroundImage = null;
        this.progressBar.Font = null;
        this.progressBar.Name = "progressBar";
        this.progressBar.Step = 1;
        // 
        // descriptionLabel
        // 
        this.descriptionLabel.AccessibleDescription = null;
        this.descriptionLabel.AccessibleName = null;
        resources.ApplyResources(this.descriptionLabel, "descriptionLabel");
        this.descriptionLabel.Font = null;
        this.descriptionLabel.Name = "descriptionLabel";
        // 
        // panel1
        // 
        this.panel1.AccessibleDescription = null;
        this.panel1.AccessibleName = null;
        resources.ApplyResources(this.panel1, "panel1");
        this.panel1.BackgroundImage = null;
        this.panel1.Controls.Add(this.errorDetailsTextBox);
        this.panel1.Controls.Add(this.errorPictureBox);
        this.panel1.Font = null;
        this.panel1.Name = "panel1";
        // 
        // errorDetailsTextBox
        // 
        this.errorDetailsTextBox.AccessibleDescription = null;
        this.errorDetailsTextBox.AccessibleName = null;
        resources.ApplyResources(this.errorDetailsTextBox, "errorDetailsTextBox");
        this.errorDetailsTextBox.BackColor = System.Drawing.Color.White;
        this.errorDetailsTextBox.BackgroundImage = null;
        this.errorDetailsTextBox.Font = null;
        this.errorDetailsTextBox.ForeColor = System.Drawing.Color.Red;
        this.errorDetailsTextBox.Name = "errorDetailsTextBox";
        this.errorDetailsTextBox.ReadOnly = true;
        // 
        // errorPictureBox
        // 
        this.errorPictureBox.AccessibleDescription = null;
        this.errorPictureBox.AccessibleName = null;
        resources.ApplyResources(this.errorPictureBox, "errorPictureBox");
        this.errorPictureBox.Font = null;
        this.errorPictureBox.ImageLocation = null;
        this.errorPictureBox.Name = "errorPictureBox";
        this.errorPictureBox.TabStop = false;
        // 
        // InstallProcessControl
        // 
        this.AccessibleDescription = null;
        this.AccessibleName = null;
        resources.ApplyResources(this, "$this");
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackgroundImage = null;
        this.Controls.Add(this.tableLayoutPanel);
        this.Font = null;
        this.Name = "InstallProcessControl";
        this.tableLayoutPanel.ResumeLayout(false);
        this.tableLayoutPanel.PerformLayout();
        this.panel1.ResumeLayout(false);
        this.panel1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.errorPictureBox)).EndInit();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.ProgressBar progressBar;
    private System.Windows.Forms.Label descriptionLabel;
    private System.Windows.Forms.PictureBox errorPictureBox;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.TextBox errorDetailsTextBox;

  }
}
