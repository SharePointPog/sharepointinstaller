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
/* KML: Minor fix to configuration property access                    */
/*                                                                    */
/**********************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CodePlex.SharePointInstaller
{
  public partial class EULAControl : InstallerControl
  {
    public EULAControl()
    {
      InitializeComponent();

      this.Load += new EventHandler(EULAControl_Load);
    }

    protected internal override void Open(InstallOptions options)
    {
      Form.NextButton.Enabled = acceptCheckBox.Checked; 
    }

    private void EULAControl_Load(object sender, EventArgs e)
    {

      string filename = InstallConfiguration.EULA;
      if (!String.IsNullOrEmpty(filename))
      {
        try
        {
          this.richTextBox.LoadFile(filename);
          acceptCheckBox.Enabled = true;
        }

        catch (IOException)
        {
          this.richTextBox.Lines = new string[] { "Error! Could not load " + filename };
        }
      }
    }

    private void acceptCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      Form.NextButton.Enabled = acceptCheckBox.Checked;
    }
  }
}
