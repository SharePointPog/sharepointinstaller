namespace CodePlex.SharePointInstaller.Controls
{
    partial class EULAControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EULAControl));
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.leftPanel = new System.Windows.Forms.Panel();
            this.rightPanel = new System.Windows.Forms.Panel();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.acceptCheckBox = new System.Windows.Forms.CheckBox();
            this.topPanel = new System.Windows.Forms.Panel();
            this.bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox
            // 
            this.richTextBox.AccessibleDescription = null;
            this.richTextBox.AccessibleName = null;
            resources.ApplyResources(this.richTextBox, "richTextBox");
            this.richTextBox.BackColor = System.Drawing.Color.White;
            this.richTextBox.BackgroundImage = null;
            this.richTextBox.Font = null;
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.ReadOnly = true;
            // 
            // leftPanel
            // 
            this.leftPanel.AccessibleDescription = null;
            this.leftPanel.AccessibleName = null;
            resources.ApplyResources(this.leftPanel, "leftPanel");
            this.leftPanel.BackgroundImage = null;
            this.leftPanel.Font = null;
            this.leftPanel.Name = "leftPanel";
            // 
            // rightPanel
            // 
            this.rightPanel.AccessibleDescription = null;
            this.rightPanel.AccessibleName = null;
            resources.ApplyResources(this.rightPanel, "rightPanel");
            this.rightPanel.BackgroundImage = null;
            this.rightPanel.Font = null;
            this.rightPanel.Name = "rightPanel";
            // 
            // bottomPanel
            // 
            this.bottomPanel.AccessibleDescription = null;
            this.bottomPanel.AccessibleName = null;
            resources.ApplyResources(this.bottomPanel, "bottomPanel");
            this.bottomPanel.BackgroundImage = null;
            this.bottomPanel.Controls.Add(this.acceptCheckBox);
            this.bottomPanel.Font = null;
            this.bottomPanel.Name = "bottomPanel";
            // 
            // acceptCheckBox
            // 
            this.acceptCheckBox.AccessibleDescription = null;
            this.acceptCheckBox.AccessibleName = null;
            resources.ApplyResources(this.acceptCheckBox, "acceptCheckBox");
            this.acceptCheckBox.BackgroundImage = null;
            this.acceptCheckBox.Font = null;
            this.acceptCheckBox.Name = "acceptCheckBox";
            this.acceptCheckBox.UseVisualStyleBackColor = true;
            this.acceptCheckBox.CheckedChanged += new System.EventHandler(this.OnAcceptChecked);
            // 
            // topPanel
            // 
            this.topPanel.AccessibleDescription = null;
            this.topPanel.AccessibleName = null;
            resources.ApplyResources(this.topPanel, "topPanel");
            this.topPanel.BackgroundImage = null;
            this.topPanel.Font = null;
            this.topPanel.Name = "topPanel";
            // 
            // EULAControl
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.richTextBox);
            this.Controls.Add(this.rightPanel);
            this.Controls.Add(this.leftPanel);
            this.Controls.Add(this.topPanel);
            this.Controls.Add(this.bottomPanel);
            this.Font = null;
            this.Name = "EULAControl";
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.CheckBox acceptCheckBox;
        private System.Windows.Forms.Panel topPanel;

    }
}