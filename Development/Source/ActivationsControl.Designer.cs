namespace CodePlex.SharePointInstaller
{
    partial class ActivationsControl
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
            this.ActivationsList = new System.Windows.Forms.ListView();
            this.ActivationCount = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ActivationsList
            // 
            this.ActivationsList.Location = new System.Drawing.Point(23, 54);
            this.ActivationsList.Name = "ActivationsList";
            this.ActivationsList.Size = new System.Drawing.Size(427, 194);
            this.ActivationsList.TabIndex = 12;
            this.ActivationsList.UseCompatibleStateImageBehavior = false;
            // 
            // ActivationCount
            // 
            this.ActivationCount.AutoSize = true;
            this.ActivationCount.Location = new System.Drawing.Point(52, 35);
            this.ActivationCount.Name = "ActivationCount";
            this.ActivationCount.Size = new System.Drawing.Size(100, 13);
            this.ActivationCount.TabIndex = 13;
            this.ActivationCount.Text = "(# active instances)";
            // 
            // ActivationsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ActivationCount);
            this.Controls.Add(this.ActivationsList);
            this.Name = "ActivationsControl";
            this.Size = new System.Drawing.Size(474, 324);
            this.Load += new System.EventHandler(this.ActivationsControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView ActivationsList;
        private System.Windows.Forms.Label ActivationCount;
    }
}