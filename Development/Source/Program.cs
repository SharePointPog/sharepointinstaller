/**************************************************************************************/
/*                                                                                    */
/*                         SharePoint Solution Installer                              */
/*                 http://www.codeplex.com/sharepointinstaller                        */
/*                                                                                    */
/*                (c) Copyright 2007-2008 Lars Fastrup Nielsen.                       */
/*                                                                                    */
/*  This source is subject to the Microsoft Permissive License.                       */
/*  http://www.codeplex.com/sharepointinstaller/Project/License.aspx                  */
/*                                                                                    */
/* KML: Updated the instantiation of DeploymentTargets                                */
/* KML: Updated to use new FeatureScope and RequrieMoss config items                  */
/*                                                                                    */
/**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

using Microsoft.SharePoint;

namespace CodePlex.SharePointInstaller
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      try
      {
        DoMain();
      }
      catch (Exception exc)
      {
          MessageBox.Show("Error: " + exc.Message);
          return;
      }
    }
    static void DoMain()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      InstallerForm form = new InstallerForm();
      form.Text = InstallConfiguration.FormatString("{SolutionTitle}");
      form.SetProductLabel(GetApplicationTitle());

      form.StoreNextTitle(Resources.CommonUIStrings.controlSummaryWelcome);
      form.ContentControls.Add(CreateWelcomeControl());
      form.StoreNextTitle(Resources.CommonUIStrings.controlTitleSystemCheck);
      form.ContentControls.Add(CreateSystemCheckControl());
      if (!IsRunAsAdmin())
      {
          Elevate();
          Application.Exit();
      }
      else
      {
          if (SystemCheckControl.CanContactFarm())
          {
              Application.Run(form);
          }
          else
          {
              MessageBox.Show("Cannot contact farm", "Farm Failure");
          }
      }
    }

    /// <summary>
    /// E.g.,  SharePoint Installer 1.2.4.0
    /// </summary>
    private static string GetApplicationTitle()
    {
        string title = Application.ProductName;
#if SP2007
        title += " SP2007";
#elif SP2013
        title += " SP2013";
#endif
        title += " v" + Application.ProductVersion;
        return title;
    }

    private static InstallerControl CreateWelcomeControl()
    {
      WelcomeControl control = new WelcomeControl();
      control.Title = InstallConfiguration.FormatString(Resources.CommonUIStrings.controlTitleWelcome);
      control.SubTitle = InstallConfiguration.FormatString(Resources.CommonUIStrings.controlSubTitleWelcome);
      return control;
    }

    private static InstallerControl CreateSystemCheckControl()
    {
      SystemCheckControl control = new SystemCheckControl();
      control.Title = Resources.CommonUIStrings.controlTitleSystemCheck;
      control.SubTitle = InstallConfiguration.FormatString(Resources.CommonUIStrings.controlSubTitleSystemCheck);

      control.RequireMOSS = InstallConfiguration.RequireMoss;
      control.MinSharePointVersion = InstallConfiguration.MinSharePointVersion;
      control.MaxSharePointVersion = InstallConfiguration.MaxSharePointVersion;
      control.RequireSearchSKU = false;

      return control;
    }

    internal static InstallerControl CreateUpgradeControl(string prompt)
    {
      bool bDefaultUpgrade = CodePlex.SharePointInstaller.InstallConfiguration.DefaultUpgrade;
      UpgradeControl control = new UpgradeControl(bDefaultUpgrade);
      control.Title = Resources.CommonUIStrings.controlTitleUpgradeRemove;
      control.Title = prompt;
      control.SubTitle = Resources.CommonUIStrings.controlSubTitleSelectOperation;
      return control;
    }

    internal static InstallerControl CreateRepairControl()
    {
      RepairControl control = new RepairControl();
      control.Title = Resources.CommonUIStrings.controlTitleRepairRemove;
      control.SubTitle = Resources.CommonUIStrings.controlSubTitleSelectOperation;
      return control;
    }

    internal static InstallerControl CreateActivationsControl()
    {
        ActivationsControl control = new ActivationsControl();
        control.Title = Resources.CommonUIStrings.controlTitleActivations;
        control.SubTitle = Resources.CommonUIStrings.controlSubTitleActivations;
        return control;
    }

    internal static InstallerControl CreateEULAControl()
    {
      EULAControl control = new EULAControl();
      control.Title = Resources.CommonUIStrings.controlTitleLicense;
      control.SubTitle = Resources.CommonUIStrings.controlSubTitleLicense;
      return control;
    }

    internal static InstallerControl CreateWebAppDeploymentTargetsControl()
    {
        InstallerControl control = new DeploymentTargetsControl();
        control.Title = Resources.CommonUIStrings.controlTitleWebApplicationDeployment;
        control.SubTitle = Resources.CommonUIStrings.controlSubTitleWebApplicationDeployment;
        return control;
    }

    internal static InstallerControl CreateSiteCollectionDeploymentTargetsControl()
    {
        InstallerControl control = new SiteCollectionDeploymentTargetsControl();
        control.Title = Resources.CommonUIStrings.controlTitleSiteDeployment;
        control.SubTitle = Resources.CommonUIStrings.controlSubTitleSiteDeployment;
        return control;
    }

    internal static InstallProcessControl CreateProcessControl()
    {
      InstallProcessControl control = new InstallProcessControl();
      control.Title = Resources.CommonUIStrings.controlTitleInstalling;
      control.SubTitle = Resources.CommonUIStrings.controlSubTitleInstalling;
      return control;
    }

    /// <summary>
    /// The function checks whether the current process is run as administrator.
    /// In other words, it dictates whether the primary access token of the 
    /// process belongs to user account that is a member of the local 
    /// Administrators group and it is elevated.
    /// </summary>
    /// <returns>
    /// Returns true if the primary access token of the process belongs to user 
    /// account that is a member of the local Administrators group and it is 
    /// elevated. Returns false if the token does not.
    /// </returns>
    internal static bool IsRunAsAdmin()
    {
        var id = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(id);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    /// <summary>
    /// The function checks whether the current process is run as administrator
    /// and elevates privileges, if the user does not accept the elevation the 
    /// program exists.
    /// </summary>
    /// <returns>
    /// Returns true if the the user elevated, false if not.
    /// </returns>
    private static bool Elevate()
    {
        // Launch itself as administrator
        var proc = new ProcessStartInfo
        {
            UseShellExecute = true,
            WorkingDirectory = Environment.CurrentDirectory,
            FileName = Application.ExecutablePath,
            Verb = "runas"
        };
        try
        {
            Process.Start(proc);
            return true;
        }
        catch
        {
            // The user refused to allow privileges elevation.   
            //Logging.Log.Error("User did not elevate privileges...");
            return false;
        }

    }
  }
}