namespace CodePlex.SharePointInstaller.Controls
{
    partial class DeploymentTargetsControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeploymentTargetsControl));
            this.webApplicationsLabel = new System.Windows.Forms.Label();
            this.hintLabel = new System.Windows.Forms.Label();
            this.treeView = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // webApplicationsLabel
            // 
            this.webApplicationsLabel.AccessibleDescription = null;
            this.webApplicationsLabel.AccessibleName = null;
            resources.ApplyResources(this.webApplicationsLabel, "webApplicationsLabel");
            this.webApplicationsLabel.Font = null;
            this.webApplicationsLabel.Name = "webApplicationsLabel";
            // 
            // hintLabel
            // 
            this.hintLabel.AccessibleDescription = null;
            this.hintLabel.AccessibleName = null;
            resources.ApplyResources(this.hintLabel, "hintLabel");
            this.hintLabel.Font = null;
            this.hintLabel.Name = "hintLabel";
            // 
            // treeView
            // 
            this.treeView.AccessibleDescription = null;
            this.treeView.AccessibleName = null;
            resources.ApplyResources(this.treeView, "treeView");
            this.treeView.BackgroundImage = null;
            this.treeView.CheckBoxes = true;
            this.treeView.Font = null;
            this.treeView.Name = "treeView";
            // 
            // DeploymentTargetsControl
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.webApplicationsLabel);
            this.Controls.Add(this.hintLabel);
            this.Font = null;
            this.Name = "DeploymentTargetsControl";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label webApplicationsLabel;
        private System.Windows.Forms.Label hintLabel;
        private System.Windows.Forms.TreeView treeView;
    }
}