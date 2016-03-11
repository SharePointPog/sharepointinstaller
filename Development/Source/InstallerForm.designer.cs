namespace CodePlex.SharePointInstaller
{
  partial class InstallerForm
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallerForm));
        this.contentPanel = new System.Windows.Forms.Panel();
        this.buttonPanel = new System.Windows.Forms.TableLayoutPanel();
        this.cancelButton = new System.Windows.Forms.Button();
        this.prevButton = new System.Windows.Forms.Button();
        this.nextButton = new System.Windows.Forms.Button();
        this.NextlayoutPanel = new System.Windows.Forms.TableLayoutPanel();
        this.solutionLabel = new System.Windows.Forms.Label();
        this.NextPanel = new System.Windows.Forms.Panel();
        this.NextTextLabel = new System.Windows.Forms.Label();
        this.NextLabel = new System.Windows.Forms.Label();
        this.vendorLabel = new System.Windows.Forms.Label();
        this.productLabel = new System.Windows.Forms.Label();
        this.topSeparatorPanel = new System.Windows.Forms.Panel();
        this.bottomSeparatorPanel = new System.Windows.Forms.Panel();
        this.titlePanel = new System.Windows.Forms.Panel();
        this.logoPicture = new System.Windows.Forms.PictureBox();
        this.subTitleLabel = new System.Windows.Forms.Label();
        this.titleLabel = new System.Windows.Forms.Label();
        this.buttonPanel.SuspendLayout();
        this.NextlayoutPanel.SuspendLayout();
        this.NextPanel.SuspendLayout();
        this.titlePanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.logoPicture)).BeginInit();
        this.SuspendLayout();
        // 
        // contentPanel
        // 
        resources.ApplyResources(this.contentPanel, "contentPanel");
        this.contentPanel.Name = "contentPanel";
        // 
        // buttonPanel
        // 
        resources.ApplyResources(this.buttonPanel, "buttonPanel");
        this.buttonPanel.Controls.Add(this.cancelButton, 3, 0);
        this.buttonPanel.Controls.Add(this.prevButton, 1, 0);
        this.buttonPanel.Controls.Add(this.nextButton, 2, 0);
        this.buttonPanel.Controls.Add(this.NextlayoutPanel, 0, 0);
        this.buttonPanel.Name = "buttonPanel";
        // 
        // cancelButton
        // 
        this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        resources.ApplyResources(this.cancelButton, "cancelButton");
        this.cancelButton.Name = "cancelButton";
        this.cancelButton.UseVisualStyleBackColor = true;
        this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
        // 
        // prevButton
        // 
        resources.ApplyResources(this.prevButton, "prevButton");
        this.prevButton.Name = "prevButton";
        this.prevButton.UseVisualStyleBackColor = true;
        this.prevButton.Click += new System.EventHandler(this.prevButton_Click);
        // 
        // nextButton
        // 
        resources.ApplyResources(this.nextButton, "nextButton");
        this.nextButton.Name = "nextButton";
        this.nextButton.UseVisualStyleBackColor = true;
        this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
        // 
        // NextlayoutPanel
        // 
        resources.ApplyResources(this.NextlayoutPanel, "NextlayoutPanel");
        this.NextlayoutPanel.Controls.Add(this.solutionLabel, 0, 0);
        this.NextlayoutPanel.Controls.Add(this.NextPanel, 0, 1);
        this.NextlayoutPanel.Controls.Add(this.vendorLabel, 0, 2);
        this.NextlayoutPanel.Controls.Add(this.productLabel, 0, 3);
        this.NextlayoutPanel.Name = "NextlayoutPanel";
        // 
        // solutionLabel
        // 
        resources.ApplyResources(this.solutionLabel, "solutionLabel");
        this.solutionLabel.ForeColor = System.Drawing.SystemColors.ActiveCaption;
        this.solutionLabel.Name = "solutionLabel";
        // 
        // NextPanel
        // 
        resources.ApplyResources(this.NextPanel, "NextPanel");
        this.NextPanel.Controls.Add(this.NextTextLabel);
        this.NextPanel.Controls.Add(this.NextLabel);
        this.NextPanel.Name = "NextPanel";
        // 
        // NextTextLabel
        // 
        resources.ApplyResources(this.NextTextLabel, "NextTextLabel");
        this.NextTextLabel.Name = "NextTextLabel";
        // 
        // NextLabel
        // 
        resources.ApplyResources(this.NextLabel, "NextLabel");
        this.NextLabel.Name = "NextLabel";
        // 
        // vendorLabel
        // 
        resources.ApplyResources(this.vendorLabel, "vendorLabel");
        this.vendorLabel.ForeColor = System.Drawing.SystemColors.GrayText;
        this.vendorLabel.Name = "vendorLabel";
        // 
        // productLabel
        // 
        resources.ApplyResources(this.productLabel, "productLabel");
        this.productLabel.ForeColor = System.Drawing.SystemColors.GrayText;
        this.productLabel.Name = "productLabel";
        // 
        // topSeparatorPanel
        // 
        this.topSeparatorPanel.BackColor = System.Drawing.SystemColors.ControlDark;
        resources.ApplyResources(this.topSeparatorPanel, "topSeparatorPanel");
        this.topSeparatorPanel.Name = "topSeparatorPanel";
        // 
        // bottomSeparatorPanel
        // 
        this.bottomSeparatorPanel.BackColor = System.Drawing.SystemColors.ControlDark;
        resources.ApplyResources(this.bottomSeparatorPanel, "bottomSeparatorPanel");
        this.bottomSeparatorPanel.Name = "bottomSeparatorPanel";
        // 
        // titlePanel
        // 
        this.titlePanel.BackColor = System.Drawing.Color.White;
        this.titlePanel.Controls.Add(this.logoPicture);
        this.titlePanel.Controls.Add(this.subTitleLabel);
        this.titlePanel.Controls.Add(this.titleLabel);
        resources.ApplyResources(this.titlePanel, "titlePanel");
        this.titlePanel.Name = "titlePanel";
        // 
        // logoPicture
        // 
        resources.ApplyResources(this.logoPicture, "logoPicture");
        this.logoPicture.BackColor = System.Drawing.Color.Transparent;
        this.logoPicture.Name = "logoPicture";
        this.logoPicture.TabStop = false;
        // 
        // subTitleLabel
        // 
        resources.ApplyResources(this.subTitleLabel, "subTitleLabel");
        this.subTitleLabel.BackColor = System.Drawing.Color.Transparent;
        this.subTitleLabel.Name = "subTitleLabel";
        // 
        // titleLabel
        // 
        resources.ApplyResources(this.titleLabel, "titleLabel");
        this.titleLabel.BackColor = System.Drawing.Color.Transparent;
        this.titleLabel.Name = "titleLabel";
        // 
        // InstallerForm
        // 
        resources.ApplyResources(this, "$this");
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.cancelButton;
        this.ControlBox = false;
        this.Controls.Add(this.topSeparatorPanel);
        this.Controls.Add(this.bottomSeparatorPanel);
        this.Controls.Add(this.contentPanel);
        this.Controls.Add(this.titlePanel);
        this.Controls.Add(this.buttonPanel);
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "InstallerForm";
        this.buttonPanel.ResumeLayout(false);
        this.NextlayoutPanel.ResumeLayout(false);
        this.NextlayoutPanel.PerformLayout();
        this.NextPanel.ResumeLayout(false);
        this.NextPanel.PerformLayout();
        this.titlePanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.logoPicture)).EndInit();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel titlePanel;
    private System.Windows.Forms.Panel contentPanel;
    private System.Windows.Forms.Panel topSeparatorPanel;
    private System.Windows.Forms.Panel bottomSeparatorPanel;
    private System.Windows.Forms.Button nextButton;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Button prevButton;
    private System.Windows.Forms.Label titleLabel;
    private System.Windows.Forms.Label subTitleLabel;
    private System.Windows.Forms.TableLayoutPanel buttonPanel;
    private System.Windows.Forms.PictureBox logoPicture;
    private System.Windows.Forms.TableLayoutPanel NextlayoutPanel;
    private System.Windows.Forms.Label solutionLabel;
    private System.Windows.Forms.Label vendorLabel;
    private System.Windows.Forms.Panel NextPanel;
    private System.Windows.Forms.Label NextTextLabel;
    private System.Windows.Forms.Label NextLabel;
    private System.Windows.Forms.Label productLabel;
  }
}