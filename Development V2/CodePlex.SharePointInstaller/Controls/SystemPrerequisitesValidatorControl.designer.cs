namespace CodePlex.SharePointInstaller.Controls
{
    partial class SystemPrerequisitesValidatorControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SystemPrerequisitesValidatorControl));
            this.table = new System.Windows.Forms.TableLayoutPanel();
            this.messageLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // table
            // 
            this.table.AccessibleDescription = null;
            this.table.AccessibleName = null;
            resources.ApplyResources(this.table, "tableLayoutPanel");
            this.table.BackgroundImage = null;
            this.table.Font = null;
            this.table.Name = "table";
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
            this.Controls.Add(this.table);
            this.Controls.Add(this.messageLabel);
            this.Font = null;
            this.Name = "SystemCheckControl";
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.TableLayoutPanel table;
        private System.Windows.Forms.Label messageLabel;
    }
}