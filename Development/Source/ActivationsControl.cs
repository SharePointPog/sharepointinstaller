using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CodePlex.SharePointInstaller
{
    public partial class ActivationsControl : InstallerControl
    {
        public ActivationsControl()
        {
            InitializeComponent();
        }

        private void ActivationsControl_Load(object sender, EventArgs e)
        {
            InstallOperation operation = this.Form.Operation;
            ActivationReporter.ReportActivationsToList(operation, ActivationsList);
            ActivationCount.Text = ActivationsList.Items.Count.ToString() + " Instances";
        }
    }
}
