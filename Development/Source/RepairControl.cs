/**********************************************************************/
/*                                                                    */
/*                   SharePoint Solution Installer                    */
/*             http://www.codeplex.com/sharepointinstaller            */
/*                                                                    */
/*               (c) Copyright 2007 Lars Fastrup Nielsen.             */
/*                                                                    */
/*  This source is subject to the Microsoft Permissive License.       */
/*  http://www.codeplex.com/sharepointinstaller/Project/License.aspx  */
/*                                                                    */
/* KML: Updated to use Operation now owned by InstallerForm           */
/*                                                                    */
/**********************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace CodePlex.SharePointInstaller
{
  public partial class RepairControl : InstallerControl
  {

    public RepairControl()
    {
      InitializeComponent();


      messageLabel.Text = InstallConfiguration.FormatString(messageLabel.Text);
    }

    protected internal override void Open(InstallOptions options)
    {
      bool enable = repairRadioButton.Checked || removeRadioButton.Checked;
      Form.Operation = InstallOperation.Repair;
      Form.NextButton.Enabled = enable;
    }

    protected internal override void Close(InstallOptions options)
    {
    }

    private void repairRadioButton_CheckedChanged(object sender, EventArgs e)
    {
      if (repairRadioButton.Checked)
      {
        Form.Operation = InstallOperation.Repair;
        Form.NextButton.Enabled = true;
      }
    }

    private void removeRadioButton_CheckedChanged(object sender, EventArgs e)
    {
      if (removeRadioButton.Checked)
      {
        Form.Operation = InstallOperation.Uninstall;
        Form.NextButton.Enabled = true;
      }
    }

  }
}
