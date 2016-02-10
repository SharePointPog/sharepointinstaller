namespace CodePlex.SharePointInstaller.Controls
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
            this.installDetailsTextBox = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            resources.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
            this.tableLayoutPanel.Controls.Add(this.progressBar, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.descriptionLabel, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.panel1, 0, 4);
            this.tableLayoutPanel.Controls.Add(this.installDetailsTextBox, 0, 3);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            // 
            // progressBar
            // 
            resources.ApplyResources(this.progressBar, "progressBar");
            this.progressBar.Name = "progressBar";
            this.progressBar.Step = 1;
            // 
            // descriptionLabel
            // 
            resources.ApplyResources(this.descriptionLabel, "descriptionLabel");
            this.descriptionLabel.Name = "descriptionLabel";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.errorDetailsTextBox);
            this.panel1.Controls.Add(this.errorPictureBox);
            this.panel1.Name = "panel1";
            // 
            // errorDetailsTextBox
            // 
            resources.ApplyResources(this.errorDetailsTextBox, "errorDetailsTextBox");
            this.errorDetailsTextBox.BackColor = System.Drawing.Color.White;
            this.errorDetailsTextBox.ForeColor = System.Drawing.Color.Red;
            this.errorDetailsTextBox.Name = "errorDetailsTextBox";
            this.errorDetailsTextBox.ReadOnly = true;
            // 
            // errorPictureBox
            // 
            resources.ApplyResources(this.errorPictureBox, "errorPictureBox");
            this.errorPictureBox.Name = "errorPictureBox";
            this.errorPictureBox.TabStop = false;
            // 
            // installDetailsTextBox
            // 
            this.installDetailsTextBox.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.installDetailsTextBox, "installDetailsTextBox");
            this.installDetailsTextBox.HideSelection = false;
            this.installDetailsTextBox.Name = "installDetailsTextBox";
            this.installDetailsTextBox.ReadOnly = true;
            this.installDetailsTextBox.Text = global::CodePlex.SharePointInstaller.Resources.CommonUIStrings.ControlSubTitleOptions;
            // 
            // InstallProcessControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel);
            this.Name = "InstallProcessControl";
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.PictureBox errorPictureBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox errorDetailsTextBox;
        private System.Windows.Forms.RichTextBox installDetailsTextBox;

    }
}