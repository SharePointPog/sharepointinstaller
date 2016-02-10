namespace CodePlex.SharePointInstaller
{
    partial class FinishedControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FinishedControl));
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.FooterLabel = new System.Windows.Forms.Label();
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
            // FooterLabel
            // 
            this.FooterLabel.AccessibleDescription = null;
            this.FooterLabel.AccessibleName = null;
            resources.ApplyResources(this.FooterLabel, "FooterLabel");
            this.FooterLabel.Font = null;
            this.FooterLabel.Name = "FooterLabel";
            // 
            // FinishedControl
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.BackgroundImage = null;
            this.Controls.Add(this.FooterLabel);
            this.Controls.Add(this.tableLayoutPanel);
            this.Font = null;
            this.Name = "FinishedControl";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
      private System.Windows.Forms.Label FooterLabel;
    }
}
