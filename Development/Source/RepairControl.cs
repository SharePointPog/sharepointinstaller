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
      doactivateFeaturesChoice.Checked = Form.WillActivateFeatures;
      dodeactivateFeaturesChoice.Checked = Form.WillDeactivateFeatures;
      UpdateButtons();
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
        doactivateFeaturesChoice.Checked = Form.WillActivateFeatures;
      }
      UpdateButtons();
    }

    private void removeRadioButton_CheckedChanged(object sender, EventArgs e)
    {
      if (removeRadioButton.Checked)
      {
        Form.Operation = InstallOperation.Uninstall;
        Form.NextButton.Enabled = true;
          // Clear visual check for activate, as it is not relevant now
          // but preserve underlying setting, in case user reenables it
        bool activpref = Form.WillActivateFeatures;
        doactivateFeaturesChoice.Checked = false;
        Form.WillActivateFeatures = activpref;
      }
      UpdateButtons();
    }

    private void dodeactivateFeaturesChoice_CheckedChanged(object sender, EventArgs e)
    {
        Form.WillDeactivateFeatures = dodeactivateFeaturesChoice.Checked;
        UpdateButtons();
    }

    private void doactivateFeaturesChoice_CheckedChanged(object sender, EventArgs e)
    {
        Form.WillActivateFeatures = doactivateFeaturesChoice.Checked;
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        doactivateFeaturesChoice.Enabled = repairRadioButton.Checked && dodeactivateFeaturesChoice.Checked;
    }
  }
}
