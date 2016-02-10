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
/**********************************************************************/
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Windows.Forms;

namespace CodePlex.SharePointInstaller
{
  public class InstallerControl : UserControl
  {
    private string title;
    private string subTitle;
    private string nextTitle;

    protected InstallerControl()
    {
    }

    public string Title
    {
      get { return title; }
      set { title = value; }
    }

    public string SubTitle
    {
      get { return subTitle; }
      set { subTitle = value; }
    }

    protected InstallerForm Form
    {
      get
      {
        return (InstallerForm)this.ParentForm;
      }
    }

    protected internal void OpenControl(InstallOptions options)
    {
        Form.DisplayNextTitle(this.nextTitle);
        Open(options); // pass to descendent class
    }
    protected internal virtual void Open(InstallOptions options) {  }

    protected internal void CloseControl(InstallOptions options)
    {
        Form.DisplayNextTitle("");
        Close(options); // pass to descendent class
    }

    protected internal virtual void Close(InstallOptions options) { }

    protected internal virtual void StoreNextTitle(string nextTitle) { this.nextTitle = nextTitle; }

    protected internal virtual void RequestCancel() 
    {
      Application.Exit();
    }

      private void InitializeComponent()
      {
          this.SuspendLayout();
          // 
          // InstallerControl
          // 
          this.Name = "InstallerControl";
          this.ResumeLayout(false);

      }

  }

  public class InstallerControlList : List<InstallerControl>
  {
  }
}
