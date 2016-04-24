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
/* KML: Minor update to usage of EULA config property and error text  */
/*                                                                    */
/**********************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Security;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

using Microsoft.Win32;

using Microsoft.SharePoint.Administration;
using System.Configuration;
using System.IO;
using CodePlex.SharePointInstaller.Resources;


namespace CodePlex.SharePointInstaller
{
  public partial class SystemCheckControl : InstallerControl
  {
    private static readonly ILog log = LogManager.GetLogger();
    private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

    private bool requireMOSS;
    private string minSharePointVersion;
    private string maxSharePointVersion;
    private bool requireSearchSKU;
    private SystemCheckList checks;
    private int nextCheckIndex;
    private int errors;

    #region Constructors

    public SystemCheckControl()
    {
      InitializeComponent();

      this.Load += new EventHandler(SystemCheckControl_Load);
    }

    #endregion

    #region Public Properties

    public static bool CanContactFarm()
    {
        try
        {
            return SPFarm.Local != null;
        }
        catch
        {
            return false;
        }
    }
    public bool RequireMOSS
    {
      get { return requireMOSS; }
      set { requireMOSS = value; }
    }

    public string MinSharePointVersion
    {
        get { return minSharePointVersion; }
        set { minSharePointVersion = value; }
    }

    public string MaxSharePointVersion
    {
        get { return maxSharePointVersion; }
        set { maxSharePointVersion = value; }
    }

    public bool RequireSearchSKU
    {
      get { return requireSearchSKU; }
      set { requireSearchSKU = value; }
    }

    #endregion

    #region Event Handlers

    private void SystemCheckControl_Load(object sender, EventArgs e)
    {
    }

    private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
    {
      timer.Stop();

      if (nextCheckIndex < checks.Count)
      {
        if (ExecuteCheck(nextCheckIndex))
        {
          nextCheckIndex++;
          timer.Start();
          return;
        } 
      }

      FinalizeChecks();
      CheckSolutionVersionsAndSetCaption();

    }

    #endregion

    #region Protected Methods

    protected internal override void Open(InstallOptions options)
    {
      if (checks == null)
      {
        Form.NextButton.Enabled = false;
        Form.PrevButton.Enabled = false;

        checks = new SystemCheckList();
        InitializeChecks();

        timer.Interval = 100;
        timer.Tick += new EventHandler(TimerEventProcessor);
        timer.Start();
      }
    }

    protected void CheckSolutionVersionsAndSetCaption()
    {
        SolutionCheck check = (SolutionCheck)checks["SolutionCheck"];
        SPSolution solution = check.Solution;

        Version newVersion = InstallConfiguration.SolutionVersion;
        string solutionTitle = InstallConfiguration.SolutionTitle;
        string caption;

        if (solution == null)
        {
            caption = string.Format("Install {0} {1}", solutionTitle, newVersion);
        }
        else
        {
            Version installedVersion = InstallConfiguration.InstalledVersion;
            if (newVersion != installedVersion)
            {
                string upgradePrompt = string.Format(
                    "Upgrade {0} from {1} to {2}",
                    solutionTitle, installedVersion, newVersion);
                caption = upgradePrompt;
            }
            else
            {
                caption = string.Format("Repair/Remove {0} {1}", solutionTitle, newVersion);
            }
        }
        Form.SetSolutionInfo(caption);
    }

    protected internal override void Close(InstallOptions options)
    {
    }

    #endregion

    #region Private Methods

    private void InitializeChecks()
    {
        this.tableLayoutPanel.SuspendLayout();

        //
        // WSS Installed Check
        //
        WSSInstalledCheck wssCheck = new WSSInstalledCheck();
        wssCheck.QuestionText = CommonUIStrings.wssCheckQuestionText;
        wssCheck.OkText = CommonUIStrings.wssCheckOkText;
        wssCheck.ErrorText = CommonUIStrings.wssCheckErrorText;
        AddCheck(wssCheck);

        //
        // WSS Version Check
        //
        if (MinSharePointVersion != "" || MaxSharePointVersion != "")
        {
            SharePointVersionCheck versionCheck = new SharePointVersionCheck(MinSharePointVersion, MaxSharePointVersion);
            versionCheck.QuestionText = string.Format(CommonUIStrings.versionCheckQuestionText, MinSharePointVersion, MaxSharePointVersion);
            string versionEquation = string.Format("{0} <= {1} <= {2}", MinSharePointVersion, SPFarm.Local.BuildVersion.ToString(), MaxSharePointVersion);
            if (MinSharePointVersion == "")
            {
                versionEquation = string.Format("{0} <= {1}", SPFarm.Local.BuildVersion.ToString(), MaxSharePointVersion);
            }
            else if (MaxSharePointVersion == "")
            {
                versionEquation = string.Format("{0} <= {1}", MinSharePointVersion, SPFarm.Local.BuildVersion.ToString());
            }
            versionCheck.OkText = string.Format(CommonUIStrings.versionCheckOkText, versionEquation);
            versionCheck.ErrorText = string.Format(CommonUIStrings.versionCheckErrorText, versionEquation);
            AddCheck(versionCheck);
        }

        //
        // MOSS Installed Check
        //
        if (requireMOSS)
        {
            MOSSInstalledCheck mossCheck = new MOSSInstalledCheck();
            mossCheck.QuestionText = CommonUIStrings.mossCheckQuestionText;
            mossCheck.OkText = CommonUIStrings.mossCheckOkText;
            mossCheck.ErrorText = CommonUIStrings.mossCheckErrorText;
            AddCheck(mossCheck);
        }

        //
        // Admin Rights Check
        //
        AdminRightsCheck adminRightsCheck = new AdminRightsCheck();
        adminRightsCheck.QuestionText = CommonUIStrings.adminRightsCheckQuestionText;
        adminRightsCheck.OkText = CommonUIStrings.adminRightsCheckOkText;
        adminRightsCheck.ErrorText = CommonUIStrings.adminRightsCheckErrorText;
        AddCheck(adminRightsCheck);

        //
        // Admin Service Check
        //
        AdminServiceCheck adminServiceCheck = new AdminServiceCheck();
        adminServiceCheck.QuestionText = CommonUIStrings.adminServiceCheckQuestionText;
        adminServiceCheck.OkText = CommonUIStrings.adminServiceCheckOkText;
        adminServiceCheck.ErrorText = CommonUIStrings.adminServiceCheckErrorText;
        AddCheck(adminServiceCheck);

        //
        // Timer Service Check
        //
        TimerServiceCheck timerServiceCheck = new TimerServiceCheck();
        timerServiceCheck.QuestionText = CommonUIStrings.timerServiceCheckQuestionText;
        timerServiceCheck.OkText = CommonUIStrings.timerServiceCheckOkText;
        timerServiceCheck.ErrorText = CommonUIStrings.timerServiceCheckErrorText;
        AddCheck(timerServiceCheck);

        //
        // Solution File Check
        //
        SolutionFileCheck solutionFileCheck = new SolutionFileCheck();
        solutionFileCheck.QuestionText = CommonUIStrings.solutionFileCheckQuestionText;
        solutionFileCheck.OkText = CommonUIStrings.solutionFileCheckOkText;
        AddCheck(solutionFileCheck);

        //
        // Solution Check
        //
        SolutionCheck solutionCheck = new SolutionCheck();
        solutionCheck.QuestionText = InstallConfiguration.FormatString(CommonUIStrings.solutionCheckQuestionText);
        solutionCheck.OkText = InstallConfiguration.FormatString(CommonUIStrings.solutionFileCheckOkText);
        solutionCheck.ErrorText = InstallConfiguration.FormatString(CommonUIStrings.solutionCheckErrorText);
        AddCheck(solutionCheck);

        //
        // Features Check
        //
        {
            FeaturesCheck check = new FeaturesCheck();
            check.QuestionText = InstallConfiguration.FormatString(CommonUIStrings.featuresCheckQuestionText);
            check.OkText = "Finished checking features"; // TODO - shouldn't be used
            check.ErrorText = "?"; // not used
            AddCheck(check);
        }

        //
        // Add empty row that will eat up the rest of the 
        // row space in the layout table.
        //
        this.tableLayoutPanel.RowCount++;
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

        this.tableLayoutPanel.ResumeLayout(false);
        this.tableLayoutPanel.PerformLayout();
    }

    private bool ExecuteCheck(int index)
    {
      SystemCheck check = checks[index];
      string imageLabelName = "imageLabel" + index;
      string textLabelName = "textLabel" + index;
      Label imageLabel = (Label)tableLayoutPanel.Controls[imageLabelName];
      Label textLabel = (Label)tableLayoutPanel.Controls[textLabelName];

      try
      {
        SystemCheckResult result = check.Execute();
        if (result == SystemCheckResult.Success)
        {
          imageLabel.Image = global::CodePlex.SharePointInstaller.Properties.Resources.CheckOk;
          textLabel.Text = check.OkText;
        } else if (result == SystemCheckResult.Error)
        {
          errors++;
          imageLabel.Image = global::CodePlex.SharePointInstaller.Properties.Resources.CheckFail;
          textLabel.Text = check.ErrorText;
        }

        //
        // Show play icon on next check that will run.
        //
        int nextIndex = index + 1;
        string nextImageLabelName = "imageLabel" + nextIndex;
        Label nextImageLabel = (Label)tableLayoutPanel.Controls[nextImageLabelName];
        if (nextImageLabel != null)
        {
          nextImageLabel.Image = global::CodePlex.SharePointInstaller.Properties.Resources.CheckPlay;
        }

        return true;
      }

      catch (InstallException ex)
      {
        errors++;
        imageLabel.Image = global::CodePlex.SharePointInstaller.Properties.Resources.CheckFail;
        textLabel.Text = ex.Message;
      }

      return false;
    }

    private void FinalizeChecks()
    {
      if (errors == 0)
      {
        ConfigureControls();
        Form.NextButton.Enabled = true;
        messageLabel.Text = CommonUIStrings.messageLabelTextSuccess;
      } else
      {
        messageLabel.Text = InstallConfiguration.FormatString(CommonUIStrings.messageLabelTextError);
      }

      Form.PrevButton.Enabled = true;
    }

    private void AddCheck(SystemCheck check)
    {
      int row = tableLayoutPanel.RowCount;

      Label imageLabel = new Label();
      imageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
      imageLabel.Image = global::CodePlex.SharePointInstaller.Properties.Resources.CheckWait;
      imageLabel.Location = new System.Drawing.Point(3, 0);
      imageLabel.Name = "imageLabel" + row;
      imageLabel.Size = new System.Drawing.Size(24, 20);

      Label textLabel = new Label();
      textLabel.AutoSize = true;
      textLabel.Dock = System.Windows.Forms.DockStyle.Fill;
      textLabel.Location = new System.Drawing.Point(33, 0);
      textLabel.Name = "textLabel" + row;
      textLabel.Size = new System.Drawing.Size(390, 20);
      textLabel.Text = check.QuestionText;
      textLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

      this.tableLayoutPanel.Controls.Add(imageLabel, 0, row);
      this.tableLayoutPanel.Controls.Add(textLabel, 1, row);
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel.RowCount++;

      checks.Add(check);
    }

    private void ConfigureControls()
    {
        SolutionCheck check = (SolutionCheck)checks["SolutionCheck"];
        SPSolution solution = check.Solution;

        Version newVersion = InstallConfiguration.SolutionVersion;

        if (solution == null)
        {
            AddInstallControls();
        } 
        else
        {
            Version installedVersion = InstallConfiguration.InstalledVersion;

            if (newVersion != installedVersion)
            {
                string upgradePrompt = string.Format(
                    "Upgrade from version '{0}' to version '{1}'",
                    installedVersion, newVersion);
                Form.StoreNextTitle(Resources.CommonUIStrings.controlTitleUpgradeRemove);
                Form.ContentControls.Add(Program.CreateUpgradeControl(upgradePrompt));
            }
            else
            {
                Form.StoreNextTitle(Resources.CommonUIStrings.controlTitleRepairRemove);
                Form.ContentControls.Add(Program.CreateRepairControl());
            }

            if (InstallConfiguration.HasFeatureScope(Microsoft.SharePoint.SPFeatureScope.WebApplication)
                || InstallConfiguration.HasFeatureScope(Microsoft.SharePoint.SPFeatureScope.Site)
                || InstallConfiguration.HasFeatureScope(Microsoft.SharePoint.SPFeatureScope.Web)
                )
            {
                Form.StoreNextTitle(Resources.CommonUIStrings.controlSummaryActivations);
                Form.ContentControls.Add(Program.CreateActivationsControl());
            }

            Form.StoreNextTitle(Resources.CommonUIStrings.controlSummaryInstalling);
            Form.ContentControls.Add(Program.CreateProcessControl());
        }
    }

    private void AddInstallControls()
    {
      //
      // Add EULA control if an EULA file was specified.
      //
      string filename = InstallConfiguration.EULA;
      if (!String.IsNullOrEmpty(filename))
      {
          Form.StoreNextTitle(Resources.CommonUIStrings.controlTitleLicense);
          Form.ContentControls.Add(Program.CreateEULAControl());
      }


      // Allow user to choose target web applications if either
      // (1) scope = Web Application
      // (2) option PromptForWebApplications was set
      //       This is for web parts, which may be site or web scope
      //       but which typically need SafeControl entries at the web app level
      //       and we don't have access to the installed solution yet, to query
      //       whether it contains web app resources, so we depend on the option
      bool webappDeploy = (InstallConfiguration.FeatureScope == Microsoft.SharePoint.SPFeatureScope.WebApplication)
        || InstallConfiguration.PromptForWebApplications;
      if (webappDeploy)
      {
          Form.StoreNextTitle(Resources.CommonUIStrings.controlTitleWebApplicationDeployment);
          Form.ContentControls.Add(Program.CreateWebAppDeploymentTargetsControl());
      }
      if (InstallConfiguration.FeatureScope == Microsoft.SharePoint.SPFeatureScope.Site)
      {
          ReadOnlyCollection<Guid?> featureIds = InstallConfiguration.FeatureIdList;
          if (featureIds == null || featureIds.Count == 0)
          {
              LogManager.GetLogger().Warn(Resources.CommonUIStrings.skippingSiteSelectionNoFeature);
          }
          else
          {
              Form.StoreNextTitle(Resources.CommonUIStrings.controlTitleSiteDeployment);
              Form.ContentControls.Add(Program.CreateSiteCollectionDeploymentTargetsControl());
          }
      }

      //Form.ContentControls.Add(Program.CreateOptionsControl());
      Form.StoreNextTitle(Resources.CommonUIStrings.controlTitleInstalling);
      Form.ContentControls.Add(Program.CreateProcessControl());
    }

    #endregion

    #region Check Classes

    private enum SystemCheckResult
    {
      Inconclusive,
      Success,
      Error
    }

    /// <summary>
    /// Base class for all system checks.
    /// </summary>
    private abstract class SystemCheck
    {
      private readonly string id;
      private string questionText;
      private string okText;
      private string errorText;

      protected SystemCheck(string id)
      {
        this.id = id;
      }

      public string Id
      {
        get { return id; }
      } 

      public string QuestionText
      {
        get { return questionText; }
        set { questionText = value; }
      }

      public string OkText
      {
        get { return okText; }
        set { okText = value; }
      }

      public string ErrorText
      {
        get { return errorText; }
        set { errorText = value; }
      }

      internal SystemCheckResult Execute()
      {
        if (CanRun)
        {
          return DoExecute();
        }
        return SystemCheckResult.Inconclusive;
      }

      protected abstract SystemCheckResult DoExecute();

      protected virtual bool CanRun
      {
        get { return true; }
      }

      protected static bool IsWSSInstalled
      {
          // registry key
          // HKLM\SOFTWARE\Microsoft\Shared Tools\Web Server Extensions\12.0
          // (or 14.0 for SharePoint 2010, or 15.0 for SharePoint 2013)
          // SharePoint=Installed
          // Rather than looking at various registry keys, simply try referencing SPFarm.Local
        get
        {
            try
            {
                Version installedVersion = SPFarm.Local.BuildVersion;
                return true;
            }
            catch
            {
                return false;
            }
        }
      }

      protected static bool IsMOSSInstalled
      {
        get
        {
            int hive = spver.GetHive();
            string name = string.Format(@"SOFTWARE\Microsoft\Office Server\{0}.0", hive);

          try
          {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(name);
            if (key != null)
            {
              string versionStr = key.GetValue("BuildVersion") as string;
              if (versionStr != null)
              {
                Version buildVersion = new Version(versionStr);
                if (buildVersion.Major == hive)
                {
                  return true;
                }
              }
            }
            return false;
          }

          catch (SecurityException ex)
          {
            throw new InstallException(string.Format(CommonUIStrings.installExceptionAccessDenied, name), ex);
          }
        }
      }
    }
    
    private class SystemCheckList : List<SystemCheck>
    {
      internal SystemCheck this[string id]
      {
        get
        {
          foreach (SystemCheck check in this)
          {
            if (check.Id == id) return check;
          }
          return null;
        }
      }
    }

    /// <summary>
    /// Checks if WSS / SPF is installed.
    /// </summary>
    private class WSSInstalledCheck : SystemCheck
    {
      internal WSSInstalledCheck() : base("WSSInstalledCheck") { }

      protected override SystemCheckResult DoExecute()
      {
        if (IsWSSInstalled) return SystemCheckResult.Success;
        return SystemCheckResult.Error;
      }
    }

    /// <summary>
    /// Checks if compatible Share Point version is installed
    /// </summary>
    private class SharePointVersionCheck : SystemCheck
    {
      private Version _minVersion;
      private Version _maxVersion;
      internal SharePointVersionCheck(string minVersion, string maxVersion) : base("SharePointVersionCheck")
      {
        if (minVersion == "") minVersion = "0.0.0.0";
        if (maxVersion == "") maxVersion = "99.99.99.99";
        _minVersion = new Version(minVersion);
        _maxVersion = new Version(maxVersion);
      }

      protected override SystemCheckResult DoExecute()
      {
        Version installedVersion = SPFarm.Local.BuildVersion;
        if (_minVersion > installedVersion) return SystemCheckResult.Error;
        if (_maxVersion < installedVersion) return SystemCheckResult.Error;
        return SystemCheckResult.Success;
      }
    }

    /// <summary>
    /// Checks if Microsoft Office SharePoint Server is installed.
    /// </summary>
    private class MOSSInstalledCheck : SystemCheck
    {
      internal MOSSInstalledCheck() : base("MOSSInstalledCheck") { }

      protected override SystemCheckResult DoExecute()
      {
        if (IsMOSSInstalled) return SystemCheckResult.Success;
        return SystemCheckResult.Error;
      }
    }

    private static string GetSPAdminServiceName()
    {
      int hive = spver.GetHive();
      switch (hive)
      {
        case 12: return "SPAdmin";
        case 14: return "SPAdminV4";
        case 15: return "SPAdminV4";
        default: throw new Exception("Unknown SharePoint hive number: " + hive.ToString());
      }
    }

    /// <summary>
    /// Checks if the Windows SharePoint Services Administration service is started.
    /// </summary>
    private class AdminServiceCheck : SystemCheck
    {
      internal AdminServiceCheck() : base("AdminServiceCheck") { }

      protected override SystemCheckResult DoExecute()
      {
        try
        {
          string adminServiceName = GetSPAdminServiceName();
          ServiceController sc = new ServiceController(adminServiceName);
          if (sc.Status == ServiceControllerStatus.Running)
          {
            return SystemCheckResult.Success;
          }
          return SystemCheckResult.Error;
        }

        catch (Win32Exception ex)
        {
          log.Error(ex.Message, ex);
        }

        catch (InvalidOperationException ex)
        {
          log.Error(ex.Message, ex);
        }

        return SystemCheckResult.Inconclusive;
      }

      protected override bool CanRun
      {
        get { return IsWSSInstalled; }
      }
    }

    private static string GetSPTimerServiceName()
    {
        int hive = spver.GetHive();
        switch (hive)
        {
            case 12: return "SPTimerV3";
            case 14: return "SPTimerV4";
            case 15: return "SPTimerV4";
            default: throw new Exception("Unknown SharePoint hive number: " + hive.ToString());
        }
    }

    /// <summary>
    /// Checks if the Windows SharePoint Services Timer service is started.
    /// </summary>
    private class TimerServiceCheck : SystemCheck
    {
      internal TimerServiceCheck() : base("TimerServiceCheck") { }

      protected override SystemCheckResult DoExecute()
      {
        try
        {
          ServiceController sc = new ServiceController(GetSPTimerServiceName());
          if (sc.Status == ServiceControllerStatus.Running)
          {
            return SystemCheckResult.Success;
          }
          return SystemCheckResult.Error;

          //
          // LFN 2009-06-21: Do not restart the time service anymore. First it does
          // not always work with Windows Server 2008 where it seems a local 
          // admin may not necessarily be allowed to start and stop the service.
          // Secondly, the timer service has become more stable with WSS SP1 and SP2.
          //
          /*TimeSpan timeout = new TimeSpan(0, 0, 60);
          ServiceController sc = new ServiceController("SPTimerV3");
          if (sc.Status == ServiceControllerStatus.Running)
          {
            sc.Stop();
            sc.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
          }

          sc.Start();
          sc.WaitForStatus(ServiceControllerStatus.Running, timeout);

          return SystemCheckResult.Success;*/
        }

        catch (System.ServiceProcess.TimeoutException ex)
        {
          log.Error(ex.Message, ex);
        }

        catch (Win32Exception ex)
        {
          log.Error(ex.Message, ex);
        }

        catch (InvalidOperationException ex)
        {
          log.Error(ex.Message, ex);
        }

        return SystemCheckResult.Inconclusive;
      }

      protected override bool CanRun
      {
        get { return IsWSSInstalled; }
      }
    }

    /// <summary>
    /// Checks if the current user is an administrator.
    /// </summary>
    private class AdminRightsCheck : SystemCheck
    {
      internal AdminRightsCheck() : base("AdminRightsCheck") { }

      protected override SystemCheckResult DoExecute()
      {
        try
        {
          if (SPFarm.Local.CurrentUserIsAdministrator())
          {
            return SystemCheckResult.Success;
          }
          return SystemCheckResult.Error;
        }

        catch (NullReferenceException)
        {
          throw new InstallException(CommonUIStrings.installExceptionDatabase);
        }

        catch (Exception ex)
        {
          throw new InstallException(ex.Message, ex);
        }
      }

      protected override bool CanRun
      {
        get { return IsWSSInstalled; }
      }
    }

    private class SolutionFileCheck : SystemCheck
    {
      internal SolutionFileCheck() : base("SolutionFileCheck") { }

      protected override SystemCheckResult DoExecute()
      {
        string filename = InstallConfiguration.SolutionFile;
        if (!String.IsNullOrEmpty(filename))
        {
          FileInfo solutionFileInfo = new FileInfo(filename);
          if (!solutionFileInfo.Exists)
          {
            throw new InstallException(string.Format(CommonUIStrings.installExceptionFileNotFound, filename));
          }
        } else
        {
          throw new InstallException(CommonUIStrings.installExceptionConfigurationNoWsp);
        }

        return SystemCheckResult.Success;
      }
    }

    private class SolutionCheck : SystemCheck
    {
      private SPSolution solution;

      internal SolutionCheck() : base("SolutionCheck") { }

      protected override SystemCheckResult DoExecute()
      {
        Guid solutionId = Guid.Empty;
        try
        {
          solutionId = InstallConfiguration.SolutionId;
        }
        catch (ArgumentNullException)
        {
          throw new InstallException(CommonUIStrings.installExceptionConfigurationNoId);
        }
        catch (FormatException)
        {
          throw new InstallException(CommonUIStrings.installExceptionConfigurationInvalidId);
        }

        try
        {
          solution = SPFarm.Local.Solutions[solutionId];
          if (solution != null)
          {
            this.OkText = InstallConfiguration.FormatString(CommonUIStrings.solutionCheckOkTextInstalled);
            InstallConfiguration.SolutionInstalled = true;
          }
          else
          {
            this.OkText = InstallConfiguration.FormatString(CommonUIStrings.solutionCheckOkTextNotInstalled);
            InstallConfiguration.SolutionInstalled = false;
          }
        }

        catch (NullReferenceException)
        {
            throw new InstallException(CommonUIStrings.installExceptionDatabase);
        }

        catch (Exception ex)
        {
          throw new InstallException(ex.Message, ex);
        }

        return SystemCheckResult.Success;
      }

      protected override bool CanRun
      {
        get { return IsWSSInstalled; }
      }

      internal SPSolution Solution
      {
        get { return solution; }
      }
    }

    /// <summary>
    /// Checks if config file features are valid
    /// </summary>
    private class FeaturesCheck : SystemCheck
    {
        internal FeaturesCheck() : base("FeaturesCheck") { }


        protected override SystemCheckResult DoExecute()
        {
            try
            {
                string checkResult = "?";
                if (InstallConfiguration.FeatureIdList == null)
                {
                    // TODO: Perry, 2010-10-06, l10n this
                    checkResult = "No features specified";
                    this.OkText = checkResult;
                    return SystemCheckResult.Success;
                }
                int newFeatures = 0, installedFeatures = 0, otherFeatures = 0, nosolFeatures = 0;
                foreach (Guid? guid in InstallConfiguration.FeatureIdList)
                {
                    if (guid == null) continue; // Perry, 2010-10-06: I don't know why we allow null GUIDs in this list anyway
                    try
                    {
                        SPFeatureDefinition fdef = SPFarm.Local.FeatureDefinitions[guid.Value];
                        if (fdef == null)
                        {
                            ++newFeatures;
                        }
                        else if (fdef.SolutionId == InstallConfiguration.SolutionId)
                        {
                            ++installedFeatures;
                        }
                        else if (fdef.SolutionId == Guid.Empty)
                        {
                            ++nosolFeatures;
                        }
                        else
                        {
                            ++otherFeatures;
                        }
                    }
                    catch (Exception exc)
                    {
                        throw new InstallException("Exception checking for feature: " + guid.Value.ToString() + ": " + exc.Message);
                    }
                }
                // TODO: Perry, 2010-10-06, l10n this
                if (newFeatures > 0 && installedFeatures == 0 && otherFeatures == 0 && nosolFeatures == 0)
                {
                    checkResult = InstallConfiguration.FormatString(CommonUIStrings.featuresNewText
                        , newFeatures
                        );
                }
                else if (newFeatures == 0 && installedFeatures > 0 && otherFeatures == 0 && nosolFeatures == 0)
                {
                    checkResult = InstallConfiguration.FormatString(CommonUIStrings.featuresInstalledText
                        , installedFeatures
                        );
                }
                else
                {
                    checkResult = InstallConfiguration.FormatString(CommonUIStrings.featuresMixedText
                        , InstallConfiguration.FeatureIdList.Count, newFeatures, installedFeatures, otherFeatures, nosolFeatures
                        );
                }
                this.OkText = checkResult;
                this.ErrorText = checkResult;
                if (InstallConfiguration.SolutionInstalled)
                {
                    if (newFeatures > 0 || otherFeatures > 0 || nosolFeatures > 0)
                        return SystemCheckResult.Error;
                }
                else
                {
                    if (installedFeatures > 0 || otherFeatures > 0 || nosolFeatures > 0)
                        return SystemCheckResult.Error;
                }
                return SystemCheckResult.Success;
            }
            catch (NullReferenceException)
            {
                throw new InstallException(CommonUIStrings.installExceptionDatabase);
            }
            catch (Exception ex)
            {
                throw new InstallException(ex.Message, ex);
            }
        }

        protected override bool CanRun
        {
            get { return IsWSSInstalled; }
        }
    }


    #endregion
  }
}
