/**************************************************************************************/
/*                                                                                    */
/*                         SharePoint Solution Installer                              */
/*                 http://www.codeplex.com/sharepointinstaller                        */
/*                                                                                    */
/*            (c) Copyright 2007-2009 Lars Fastrup Nielsen.                           */
/*                                                                                    */
/*  This source is subject to the Microsoft Permissive License.                       */
/*  http://www.codeplex.com/sharepointinstaller/Project/License.aspx                  */
/*                                                                                    */
/* KML: Updated Open() to handle Site Collection Features                             */
/* KML: Added ActivateSiteCollectionFeatureCommand                                    */
/* KML: Added DeactivateSiteCollectionFeatureCommand                                  */
/* KML: Moved InstallOperation to be owned by the InstallerForm                       */
/* KML: Updated to new FeatureScope configuration property                            */
/* LFN 2008-06-15: Fixed bugs in the DeactivateSiteCollectionFeatureCommand class     */
/* PR 2010-03-03: Added DeactivateWebFeatureCommand                                   */
/*                                                                                    */
/**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading;

using Microsoft.SharePoint.Administration;

using Microsoft.SharePoint;
using System.IO;
using CodePlex.SharePointInstaller.Resources;
using System.Security;


namespace CodePlex.SharePointInstaller
{
  public partial class InstallProcessControl : InstallerControl
  {
    private static readonly ILog log = LogManager.GetLogger();

    private static readonly TimeSpan JobTimeout = TimeSpan.FromMinutes(15);

    private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
    private CommandList executeCommands;
    private CommandList rollbackCommands;
    private int nextCommand;
    private bool completed;
    private bool requestCancel;
    private int errors;
    private int rollbackErrors;

    public InstallProcessControl()
    {
      InitializeComponent();

      errorPictureBox.Visible = false;
      errorDetailsTextBox.Visible = false;

      this.Load += new EventHandler(InstallProcessControl_Load);
    }

    #region Event Handlers

    private void InstallProcessControl_Load(object sender, EventArgs e)
    {
      switch (Form.Operation)
      {
        case InstallOperation.Install:
          Form.SetTitle(CommonUIStrings.installTitle);
          Form.SetSubTitle(InstallConfiguration.FormatString(CommonUIStrings.installSubTitle));
          break;

        case InstallOperation.Upgrade:
          Form.SetTitle(CommonUIStrings.upgradeTitle);
          Form.SetSubTitle(InstallConfiguration.FormatString(CommonUIStrings.upgradeSubTitle));
          break;

        case InstallOperation.Repair:
          Form.SetTitle(CommonUIStrings.repairTitle);
          Form.SetSubTitle(InstallConfiguration.FormatString(CommonUIStrings.repairSubTitle));
          break;

        case InstallOperation.Uninstall:
          Form.SetTitle(CommonUIStrings.uninstallTitle);
          Form.SetSubTitle(InstallConfiguration.FormatString(CommonUIStrings.uninstallSubTitle));
          break;
      }

      Form.PrevButton.Enabled = false;
      Form.NextButton.Enabled = false;
    }

    private void TimerEventInstall(Object myObject, EventArgs myEventArgs)
    {
      timer.Stop();

      if (requestCancel)
      {
        descriptionLabel.Text = Resources.CommonUIStrings.descriptionLabelTextOperationCanceled;
        InitiateRollback();
      }

      else if (nextCommand < executeCommands.Count)
      {
        try
        {
          Command command = executeCommands[nextCommand];
          if (command.Execute())
          {
            nextCommand++;
            progressBar.PerformStep();

            if (nextCommand < executeCommands.Count)
            {
              descriptionLabel.Text = executeCommands[nextCommand].Description;
            }
          }
          timer.Start();
        }

        catch (Exception ex)
        {
          log.Error(CommonUIStrings.logError);
          log.Error(ex.Message, ex);

          errors++;
          errorPictureBox.Visible = true;
          errorDetailsTextBox.Visible = true;
          errorDetailsTextBox.Text = ex.Message;

          descriptionLabel.Text = Resources.CommonUIStrings.descriptionLabelTextErrorsDetected;
          InitiateRollback();
        }
      }

      else
      {
        descriptionLabel.Text = Resources.CommonUIStrings.descriptionLabelTextSuccess;
        HandleCompletion();
      }
    }

    private void TimerEventRollback(Object myObject, EventArgs myEventArgs)
    {
      timer.Stop();

      if (nextCommand < rollbackCommands.Count)
      {
        try
        {
          Command command = rollbackCommands[nextCommand];
          if (command.Rollback())
          {
            nextCommand++;
            progressBar.PerformStep();
          }
        }

        catch (Exception ex)
        {
            log.Error(CommonUIStrings.logError);
          log.Error(ex.Message, ex);

          rollbackErrors++;
          nextCommand++;
          progressBar.PerformStep();
        }

        timer.Start();
      }

      else
      {
        if (rollbackErrors == 0)
        {
          descriptionLabel.Text = Resources.CommonUIStrings.descriptionLabelTextRollbackSuccess;
        } 
        else
        {
          descriptionLabel.Text = string.Format(Resources.CommonUIStrings.descriptionLabelTextRollbackError, rollbackErrors);
        }

        HandleCompletion();
      }
    }

    #endregion

    #region Protected Methods

    protected internal override void RequestCancel()
    {
      if (completed)
      {
        base.RequestCancel();
      } else
      {
        requestCancel = true;
        Form.AbortButton.Enabled = false;
      }
    }

    protected internal override void Open(InstallOptions options)
    {
      executeCommands = new CommandList();
      rollbackCommands = new CommandList();
      nextCommand = 0;
      SPFeatureScope featureScope = InstallConfiguration.FeatureScope;
      IList<SiteLoc> siteCollectionLocs = null;

      switch (Form.Operation)
      {
          #region Install
          case InstallOperation.Install:
          executeCommands.Add(new AddSolutionCommand(this));
          executeCommands.Add(new CreateDeploymentJobCommand(this, options.WebApplicationTargets));
          executeCommands.Add(new WaitForJobCompletionCommand(this, CommonUIStrings.waitForSolutionDeployment));
          if (Form.WillActivateFeatures)
          {
              if (featureScope == SPFeatureScope.Farm)
              {
                  // TODO: synthesize FeatureLocs for the farm features
                  executeCommands.Add(new ActivateFarmFeatureCommand(this));
              }
              else if (featureScope == SPFeatureScope.Site)
              {
                  // TODO: Revise site collection choice to produce FeatureLocs structure
                  // and get rid of SiteCollectionLocs
                  siteCollectionLocs = options.SiteCollectionTargets;
                  if (siteCollectionLocs == null || siteCollectionLocs.Count == 0)
                  {
                      log.Info(CommonUIStrings.logNoSiteCollectionsSpecified);
                  }
                  else
                  {
                      executeCommands.Add(new ActivateSiteCollectionFeatureCommand(this, siteCollectionLocs));
                  }
              }
              // TODO - web selection for installation
          }
          executeCommands.Add(new RegisterVersionNumberCommand(this));

          for (int i = executeCommands.Count-1; i<=0; i--)
          {
            rollbackCommands.Add(executeCommands[i]);
          }
          break;
          #endregion
          #region Upgrade
          case InstallOperation.Upgrade:
          {
              FeatureLocations flocs = InstallProcessControl.GetFeaturedLocations(this.Form.Operation);
              if (Form.WillDeactivateFeatures && flocs.LocationsCount > 0)
              {
                  executeCommands.Add(FeaturesCommand.CreateDeactivatorCommand(this, Form.Operation));
              }
              if (!IsSolutionRenamed())
              {
                  executeCommands.Add(new CreateUpgradeJobCommand(this));
                  executeCommands.Add(new WaitForJobCompletionCommand(this, CommonUIStrings.waitForSolutionUpgrade));
              }
              else
              {
                  executeCommands.Add(new CreateRetractionJobCommand(this));
                  executeCommands.Add(new WaitForJobCompletionCommand(this, CommonUIStrings.waitForSolutionRetraction));
                  executeCommands.Add(new RemoveSolutionCommand(this));
                  executeCommands.Add(new AddSolutionCommand(this));
                  executeCommands.Add(new CreateDeploymentJobCommand(this, GetDeployedApplications()));
                  executeCommands.Add(new WaitForJobCompletionCommand(this, CommonUIStrings.waitForSolutionDeployment));
              }
              if (Form.WillActivateFeatures && flocs.LocationsCount > 0)
              {
                  executeCommands.Add(FeaturesCommand.CreateActivatorCommand(this, Form.Operation));
              }
              executeCommands.Add(new RegisterVersionNumberCommand(this));
          }
          break;
       	#endregion
        #region Repair
          case InstallOperation.Repair:
          {
              FeatureLocations flocs = InstallProcessControl.GetFeaturedLocations(this.Form.Operation);
              if (Form.WillDeactivateFeatures && flocs.LocationsCount > 0)
              {
                  executeCommands.Add(FeaturesCommand.CreateDeactivatorCommand(this, Form.Operation));
              }
              executeCommands.Add(new CreateRetractionJobCommand(this));
              executeCommands.Add(new WaitForJobCompletionCommand(this, CommonUIStrings.waitForSolutionRetraction));
              executeCommands.Add(new RemoveSolutionCommand(this));
              executeCommands.Add(new AddSolutionCommand(this));
              executeCommands.Add(new CreateDeploymentJobCommand(this, GetDeployedApplications()));
              executeCommands.Add(new WaitForJobCompletionCommand(this, CommonUIStrings.waitForSolutionDeployment));
              if (Form.WillActivateFeatures && flocs.LocationsCount > 0)
              {
                  executeCommands.Add(FeaturesCommand.CreateActivatorCommand(this, Form.Operation));
              }
          }
          break;
        #endregion
        #region Uninstall
        case InstallOperation.Uninstall:
          {
              FeatureLocations flocs = InstallProcessControl.GetFeaturedLocations(this.Form.Operation);
              if (Form.WillDeactivateFeatures && flocs.LocationsCount > 0)
              {
                  executeCommands.Add(FeaturesCommand.CreateDeactivatorCommand(this, Form.Operation));
              }
              executeCommands.Add(new CreateRetractionJobCommand(this));
              executeCommands.Add(new WaitForJobCompletionCommand(this, CommonUIStrings.waitForSolutionRetraction));
              executeCommands.Add(new RemoveSolutionCommand(this));
              executeCommands.Add(new UnregisterVersionNumberCommand(this));
          }
          break;
        #endregion
      }

      progressBar.Maximum = executeCommands.Count;

      descriptionLabel.Text = executeCommands[0].Description;

      timer.Interval = 1000;
      timer.Tick += new EventHandler(TimerEventInstall);
      timer.Start();
    }

    private static IList<SiteLoc> FeaturedSiteCollectionList = null;
    public static IList<SiteLoc> GetFeaturedSiteCollections()
    {
        if (FeaturedSiteCollectionList != null)
            return FeaturedSiteCollectionList;

        FeaturedSiteCollectionList = new List<SiteLoc>();
        ReadOnlyCollection<Guid?> featureIds = InstallConfiguration.FeatureId;
        if (featureIds == null || featureIds.Count == 0)
        {
            log.Warn(CommonUIStrings.logNoFeaturesSpecified);
            return FeaturedSiteCollectionList;
        }
        foreach (SPWebApplication webApp in SPWebService.AdministrationService.WebApplications)
        {
            RecordFeaturedSites(webApp, FeaturedSiteCollectionList, featureIds);
        }

        foreach (SPWebApplication webApp in SPWebService.ContentService.WebApplications)
        {
            RecordFeaturedSites(webApp, FeaturedSiteCollectionList, featureIds);
        }

        return FeaturedSiteCollectionList;
    }
    private static void RecordFeaturedSites(SPWebApplication webApp, IList<SiteLoc> res, ReadOnlyCollection<Guid?> featureIds)
    {
        foreach (SPSite siteCollection in webApp.Sites)
        {
            try
            {
                List<Guid?> featuresFound = null;

                foreach (Guid? featureId in featureIds)
                {
                    if (featureId == null) continue;
                    if (siteCollection.Features[featureId.Value] == null) continue;
                    if (featuresFound == null) { featuresFound = new List<Guid?>(); }
                    featuresFound.Add(featureId);
                }
                if (featuresFound != null)
                {
                    SiteLoc siteLoc = new SiteLoc(siteCollection);
                    siteLoc.featureList = featuresFound;
                    res.Add(siteLoc);
                }
            }
            finally
            {
                // guarantee SPSite is released ASAP even in face of exception
                siteCollection.Dispose();
            }
        }
    }

    private static FeatureLocations FeaturedLocationList = null;
    private static InstallOperation FeaturedOperation;

    /// <summary>
    /// Find all locations (farm, webapp, site, or web) which have any
    /// of our configured features (given by FeatureId) currently activated
    /// </summary>
    public static FeatureLocations GetFeaturedLocations(InstallOperation operation)
    {
        // TODO
        // Form.Operation tells us which operation
        // if we want to do override feature lists for different operations

        // cached
        if (FeaturedLocationList != null && operation == FeaturedOperation)
        {
            return FeaturedLocationList;
        }
        FeaturedOperation = operation;

        FeaturedLocationList = new FeatureLocations();
        // TODO: Perry, 2010-10-14: use FeaturedOperation (except then must remove feature count from system check)
        ReadOnlyCollection<Guid?> featureIds = InstallConfiguration.FeatureId;
        if (featureIds == null || featureIds.Count == 0)
        {
            log.Warn(CommonUIStrings.logNoFeaturesSpecified);
            return FeaturedLocationList;
        }

        SPFarm farm = SPFarm.Local;
        // Farm
        if (InstallConfiguration.HasFeatureScope(SPFeatureScope.Farm))
        {
            FeatureLoc floc = new FeatureLoc(farm);
            SPFeatureCollection activeFeatures = SPWebService.AdministrationService.Features;
            RecordActivatedFeatures(FeaturedLocationList, floc, activeFeatures);
        }
        // Web Application and the rest
        if (InstallConfiguration.HasFeatureScope(SPFeatureScope.WebApplication)
            || InstallConfiguration.HasFeatureScope(SPFeatureScope.Site)
            || InstallConfiguration.HasFeatureScope(SPFeatureScope.Web)
            )
        {
            foreach (SPWebApplication webapp in GetDeployedOrAllWebApplications(farm))
            {
                if (InstallConfiguration.HasFeatureScope(SPFeatureScope.WebApplication))
                {
                    FeatureLoc floc = new FeatureLoc(webapp);
                    SPFeatureCollection activeFeatures = webapp.Features;
                    RecordActivatedFeatures(FeaturedLocationList, floc, activeFeatures);
                }
                // Site Collection & Webs
                if (InstallConfiguration.HasFeatureScope(SPFeatureScope.Site)
                    || InstallConfiguration.HasFeatureScope(SPFeatureScope.Web)
                    )
                {
                    foreach (SPSite siteCollection in webapp.Sites)
                    {
                        try
                        {
                            if (InstallConfiguration.HasFeatureScope(SPFeatureScope.Site))
                            {
                                FeatureLoc floc = new FeatureLoc(siteCollection);
                                SPFeatureCollection activeFeatures = siteCollection.Features;
                                RecordActivatedFeatures(FeaturedLocationList, floc, activeFeatures);
                            }

                            // Webs
                            if (InstallConfiguration.HasFeatureScope(SPFeatureScope.Web))
                            {
                                foreach (SPWeb web in siteCollection.AllWebs)
                                {
                                    try
                                    {
                                        FeatureLoc floc = new FeatureLoc(web);
                                        SPFeatureCollection activeFeatures = web.Features;
                                        RecordActivatedFeatures(FeaturedLocationList, floc, activeFeatures);
                                    }
                                    finally
                                    {
                                        // guarantee SPWeb is released ASAP even in face of exception
                                        web.Dispose();
                                    }
                                }
                            }
                        }
                        catch (Exception exc)
                        {
                            string message = string.Format(CommonUIStrings.siteCollectionAccessError, siteCollection.Url);
                            log.Error(message, exc);
                        }
                        finally
                        {
                            // guarantee SPSite is released ASAP even in face of exception
                            siteCollection.Dispose();
                        }
                    }
                }

            }
        }
        return FeaturedLocationList;
    }

    /// <summary>
    /// Check all feature ids specified in config against passed feature collection
    /// If any found, add this feature location to the FeaturedLocationList
    static private void RecordActivatedFeatures(FeatureLocations FeaturedLocationList, FeatureLoc floc, SPFeatureCollection features)
    {
        ReadOnlyCollection<Guid?> featureIds = InstallConfiguration.FeatureId;
        List<Guid> featuresFound = null;

        foreach (Guid? featureId in featureIds)
        {
            if (featureId == null) continue;
            if (features[featureId.Value] == null) continue;
            if (featuresFound == null) { featuresFound = new List<Guid>(); }
            featuresFound.Add(featureId.Value);
        }
        if (featuresFound != null)
        {
            floc.featureList = featuresFound;
            FeaturedLocationList.AddFeatureLocation(floc);
        }
    }

    /// <summary>
    /// Return list of all web applications
    /// </summary>
    static private List<SPWebApplication> GetDeployedOrAllWebApplications(SPFarm farm)
    {
        List<SPWebApplication> webapps = TryGetDeployedWebApplications(farm);
        if (webapps != null) { return webapps; }
        log.Info("Failed to enumerate deployed webapps. Will fall back to traversing all webapps.");

        List<SPWebApplication> list = new List<SPWebApplication>();
        // Fall back to trying to enumerate every web application
        // The following way is problematic for permissions -- requires SCA on every
        // site collection on the entire farm, even if feature not deployed to some of them
        // e.g., individual my sites
        foreach (SPWebApplication webApp in SPWebService.AdministrationService.WebApplications)
        {
            list.Add(webApp);
        }
        foreach (SPWebApplication webApp in SPWebService.ContentService.WebApplications)
        {
            list.Add(webApp);
        }
        return list;
    }

    static private List<SPWebApplication> TryGetDeployedWebApplications(SPFarm farm)
    {
        SPSolution solution = null;
        try
        {
            solution = farm.Solutions[InstallConfiguration.SolutionId];
        }
        catch (Exception exc)
        {
            log.Error("Error accessing solution on farm", exc);
            return null;
        }
        try
        {
            if (!solution.ContainsWebApplicationResource)
            {
                log.Info("Solution contains no webapp resources.");
                return null;
            }
            if (solution.DeployedWebApplications == null)
            {
                log.Info("Solution.DeployedWebApplications is null.");
                return null;
            }
            List<SPWebApplication> list = new List<SPWebApplication>();
            foreach (SPWebApplication webapp in solution.DeployedWebApplications)
            {
                list.Add(webapp);
            }
            return list;
        }
        catch (Exception exc)
        {
            log.Error("Exception accessing DeployedWebApplications", exc);
            return null;
        }
    }

    private static IList<WebLoc> FeaturedWebsList = null;
    public static IList<WebLoc> GetFeaturedWebs()
    {
        if (FeaturedWebsList != null)
            return FeaturedWebsList;

        FeaturedWebsList = new List<WebLoc>();
        ReadOnlyCollection<Guid?> featureIds = InstallConfiguration.FeatureId;
        if (featureIds == null || featureIds.Count == 0)
        {
            log.Warn(CommonUIStrings.logNoFeaturesSpecified);
            return FeaturedWebsList;
        }

        SPFarm farm = SPFarm.Local;
        List<SPWebApplication> webapps = GetDeployedOrAllWebApplications(farm);
        foreach (SPWebApplication webApp in webapps)
        {
            RecordFeaturedWebs(webApp, FeaturedWebsList, featureIds);
        }
        return FeaturedWebsList;
    }
    private static void RecordFeaturedWebs(SPWebApplication webApp, IList<WebLoc> res, ReadOnlyCollection<Guid?> featureIds)
    {
        foreach (SPSite siteCollection in webApp.Sites)
        {
            try
            {
                foreach (SPWeb web in siteCollection.AllWebs)
                {
                    try
                    {
                        List<Guid?> featuresFound = null;

                        foreach (Guid? featureId in featureIds)
                        {
                            if (featureId == null) continue;
                            if (web.Features[featureId.Value] == null) continue;
                            if (featuresFound == null) { featuresFound = new List<Guid?>(); }
                            featuresFound.Add(featureId);
                        }
                        if (featuresFound != null)
                        {
                            WebLoc webLoc = new WebLoc(web);
                            webLoc.featureList = featuresFound;
                            res.Add(webLoc);
                        }
                    }
                    finally
                    {
                        // guarantee SPWeb is released ASAP even in face of exception
                        web.Dispose();
                    }
                }
            }
            catch (Exception exc)
            {
                string message = string.Format(CommonUIStrings.siteCollectionAccessError, siteCollection.Url);
                log.Error(message, exc);
            }
            finally
            {
                // guarantee SPSite is released ASAP even in face of exception
                siteCollection.Dispose();
            }
        }
    }

    #endregion

    #region Private Methods

    private string LogMessageToString(LogMessage msg)
    {
      string text = "";
      if (msg.MessageLevel != LogMessage.Level.Info)
      {
        text += "*";
      }
      text += msg.MessageLevel.ToString() + ": ";
      text += msg.Message;
      if (msg.Exception != null)
      {
          text += "\r\n Exception: " + msg.Exception.ToString();
          Exception excInner = msg.Exception.InnerException;
          while (excInner != null)
          {
              text += "\r\n\r\n\t Inner Exception: " + excInner.ToString();
              excInner = excInner.InnerException;
          }
      }
      return text;
    }
    private void HandleCompletion()
    {
      completed = true;

      Form.NextButton.Enabled = true;
      Form.AbortButton.Text = CommonUIStrings.abortButtonText;
      Form.AbortButton.Enabled = true;

      CompletionControl nextControl = new CompletionControl();

      foreach (LogMessage message in LogManager.GetMessages())
      {
          nextControl.Details += LogMessageToString(message) + "\r\n";
      }

      switch (Form.Operation)
      {
        case InstallOperation.Install:
            nextControl.Title = errors == 0 ? CommonUIStrings.installSuccess : CommonUIStrings.installError;
          break;

        case InstallOperation.Upgrade:
            nextControl.Title = errors == 0 ? CommonUIStrings.upgradeSuccess : CommonUIStrings.upgradeError;
          break;

        case InstallOperation.Repair:
            nextControl.Title = errors == 0 ? CommonUIStrings.repairSuccess : CommonUIStrings.repairError;
          break;

        case InstallOperation.Uninstall:
            nextControl.Title = errors == 0 ? CommonUIStrings.uninstallSuccess : CommonUIStrings.uninstallError;
          break;
      }

      Form.StoreNextTitle(Resources.CommonUIStrings.controlSummaryCompleted);
      Form.ContentControls.Add(nextControl);
    }

    private void InitiateRollback()
    {
      Form.AbortButton.Enabled = false;

      progressBar.Maximum = rollbackCommands.Count;
      progressBar.Value = rollbackCommands.Count;
      nextCommand = 0;
      rollbackErrors = 0;
      progressBar.Step = -1;
      
      //
      // Create and start new timer.
      //
      timer = new System.Windows.Forms.Timer();
      timer.Interval = 1000;
      timer.Tick += new EventHandler(TimerEventRollback);
      timer.Start();          
    }

    private bool IsSolutionRenamed()
    {
      SPFarm farm = SPFarm.Local;
      SPSolution solution = farm.Solutions[InstallConfiguration.SolutionId];
      if (solution == null) return false;

      FileInfo solutionFileInfo = new FileInfo(InstallConfiguration.SolutionFile);

      return !solution.Name.Equals(solutionFileInfo.Name, StringComparison.OrdinalIgnoreCase);
    }

    private Collection<SPWebApplication> GetDeployedApplications()
    {
      SPFarm farm = SPFarm.Local;
      SPSolution solution = farm.Solutions[InstallConfiguration.SolutionId];
      if (solution.ContainsWebApplicationResource)
      {
        return solution.DeployedWebApplications;
      }
      return null;
    }

    #endregion

    #region Command Classes

    /// <summary>
    /// The base class of all installation commands.
    /// </summary>
    private abstract class Command    
    {
      private readonly InstallProcessControl parent;

      protected Command(InstallProcessControl parent) 
      {
        this.parent = parent;
      }

      internal InstallProcessControl Parent 
      {
        get { return parent; }
      }

      internal abstract string Description { get; }

      protected internal virtual bool Execute() { return true;  }

      protected internal virtual bool Rollback() { return true; }
    }

    private class CommandList : List<Command>
    {
    }

    /// <summary>
    /// The base class of all SharePoint solution related commands.
    /// </summary>
    private abstract class SolutionCommand : Command
    {
      protected SolutionCommand(InstallProcessControl parent) : base(parent) { }

      protected void RemoveSolution()
      {
        try
        {
          SPFarm farm = SPFarm.Local;
          SPSolution solution = farm.Solutions[InstallConfiguration.SolutionId];
          if (solution != null)
          {
            if (!solution.Deployed)
            {
              solution.Delete();
            }
          }
        }

        catch (SqlException ex)
        {
          throw new InstallException(ex.Message, ex);
        }
      }
    }

    /// <summary>
    /// Command for adding the SharePoint solution.
    /// </summary>
    private class AddSolutionCommand : SolutionCommand
    {
      internal AddSolutionCommand(InstallProcessControl parent) : base(parent)
      {
      }

      internal override string Description 
      {
        get
        {
          return CommonUIStrings.addSolutionCommand;
        }
      }

      protected internal override bool Execute()
      {
        string filename = InstallConfiguration.SolutionFile;
        if (String.IsNullOrEmpty(filename))
        {
          throw new InstallException(CommonUIStrings.installExceptionConfigurationNoWsp);
        }

        try
        {
          SPFarm farm = SPFarm.Local;
          SPSolution solution = farm.Solutions.Add(filename);
          return true;
        }

        catch (SecurityException ex)
        {
          string message = CommonUIStrings.addSolutionAccessError;
          if (Environment.OSVersion.Version >= new Version("6.0"))
            message += " " + CommonUIStrings.addSolutionAccessErrorWinServer2008Solution;
          else
            message += " " + CommonUIStrings.addSolutionAccessErrorWinServer2003Solution;
          throw new InstallException(message, ex);
        }

        catch (IOException ex)
        {
          throw new InstallException(ex.Message, ex);
        }

        catch (ArgumentException ex)
        {
          throw new InstallException(ex.Message, ex);
        }

        catch (SqlException ex)
        {
          throw new InstallException(ex.Message, ex);
        }
      }

      protected internal override bool Rollback()
      {
        RemoveSolution();
        return true;
      }
    }

   /// <summary>
    /// Command for removing the SharePoint solution.
    /// </summary>
    private class RemoveSolutionCommand : SolutionCommand
    {
      internal RemoveSolutionCommand(InstallProcessControl parent) : base(parent) { }

      internal override string Description
      {
        get
        {
          return CommonUIStrings.removeSolutionCommand;
        }
      }

      protected internal override bool Execute()
      {
        RemoveSolution();
        return true;
      }
    }

    private abstract class JobCommand : Command
    {
      protected JobCommand(InstallProcessControl parent) : base(parent) { }

      protected static void RemoveExistingJob(SPSolution solution)
      {
        if (solution.JobStatus == SPRunningJobStatus.Initialized)
        {
          throw new InstallException(CommonUIStrings.installExceptionDuplicateJob);
        }

        SPJobDefinition jobDefinition = GetSolutionJob(solution);
        if (jobDefinition != null)
        {
          jobDefinition.Delete();
          Thread.Sleep(500);
        }
      }

      private static SPJobDefinition GetSolutionJob(SPSolution solution)
      {
        SPFarm localFarm = SPFarm.Local;
        SPTimerService service = localFarm.TimerService;
        foreach (SPJobDefinition definition in service.JobDefinitions)
        {
          if (definition.Title != null && definition.Title.Contains(solution.Name))
          {
            return definition;
          }
        }
        return null;
      }

      protected static DateTime GetImmediateJobTime()
      {
        return DateTime.Now - TimeSpan.FromDays(1);
      }
    }

    /// <summary>
    /// Command for creating a deployment job.
    /// </summary>
    private class CreateDeploymentJobCommand : JobCommand
    {
      private Collection<SPWebApplication> applications;

      internal CreateDeploymentJobCommand(InstallProcessControl parent, IList<SPWebApplication> applications) : base(parent)
      {
        if (applications != null)
        {
          this.applications = new Collection<SPWebApplication>();
          foreach (SPWebApplication application in applications)
          {
            this.applications.Add(application);
          }
        }
        else
        {
          this.applications = null;
        }
      }

      internal override string Description
      {
        get
        {
          return CommonUIStrings.createDeploymentJobCommand;
        }
      }

      protected internal override bool Execute()
      {
        try
        {
          SPSolution installedSolution = SPFarm.Local.Solutions[InstallConfiguration.SolutionId];

          if (installedSolution == null)
          {
            throw new InstallException("Expected solution {" + InstallConfiguration.SolutionId.ToString() + "} not found");
          }

          //
          // Remove existing job, if any. 
          //
          if (installedSolution.JobExists)
          {
            RemoveExistingJob(installedSolution);
          }

          log.Info("***** SOLUTION DEPLOYMENT *****");
          string webapps = "";
          int minCompat = -1, maxCompat = -1;
#if SP2013
          if (InstallConfiguration.CompatibilityLevel == "14"
              || InstallConfiguration.CompatibilityLevel.ToUpper() == "OLD"
              || InstallConfiguration.CompatibilityLevel.ToUpper() == "OLDVERSION"
              || InstallConfiguration.CompatibilityLevel.ToUpper() == "OLDVERSIONS"
              )
          {
              minCompat = 14;
              maxCompat = 14;
              log.Info("Installing solution at level 14: " + installedSolution.Name);
          }
          else if (InstallConfiguration.CompatibilityLevel == "15"
              || InstallConfiguration.CompatibilityLevel.ToUpper() == "NEW"
              || InstallConfiguration.CompatibilityLevel.ToUpper() == "NEWVERSION"
              || InstallConfiguration.CompatibilityLevel.ToUpper() == "NEWVERSIONS"
              )
          {
              minCompat = 15;
              maxCompat = 15;
              log.Info("Installing solution at level 15: " + installedSolution.Name);
          }
          else if (InstallConfiguration.CompatibilityLevel == "14,15"
              || InstallConfiguration.CompatibilityLevel.ToUpper() == "ALL"
              || InstallConfiguration.CompatibilityLevel.ToUpper() == "ALLVERSIONS"
              )
          {
              minCompat = 14;
              maxCompat = 15;
              log.Info("Installing solution at levels 14-15: " + installedSolution.Name);
          }
#endif
          log.Info("solution ContainsWebApplicationResource: "
              + installedSolution.ContainsWebApplicationResource.ToString()
              + ", applications: " + (applications == null ? "NULL" : applications.Count.ToString())
              );
          if (minCompat > 0)
          {
              if (applications == null)
              {
                  applications = new Collection<SPWebApplication>();
              }
              CompatibilityDeployer.Deploy(installedSolution, applications, minCompat, maxCompat, log);
              webapps = GetApplicationsList(applications);
          }
          else if (installedSolution.ContainsWebApplicationResource && applications != null && applications.Count > 0)
          {
            installedSolution.Deploy(GetImmediateJobTime(), true, applications, true);
            webapps = GetApplicationsList(applications);
          }
          else
          {
            installedSolution.Deploy(GetImmediateJobTime(), true, true);
            if (installedSolution.ContainsWebApplicationResource)
            {
                webapps = "All";
            }
            else
            {
                webapps = "All (No WebApplication Resources)";
            }
          }
          log.Info(string.Format(CommonUIStrings.WebApplicationDeploymentLogMessage, webapps));

          return true;
        }
        catch (SPException ex)
        {
          throw new InstallException(ex.Message, ex);
        }
        catch (SqlException ex)
        {
          throw new InstallException(ex.Message, ex);
        }
      }

      private Collection<SPWebApplication> GetAllWebApplications()
      {
        Collection<SPWebApplication> applications = new Collection<SPWebApplication>();
        foreach (SPWebApplication application in SPWebService.AdministrationService.WebApplications)
        {
            applications.Add(application);
        }
        foreach (SPWebApplication application in SPWebService.ContentService.WebApplications)
        {
            applications.Add(application);
        }
        return applications;
      }

      private string GetApplicationsList(Collection<SPWebApplication> applications)
      {
          Collection<SPWebApplication> allApplications = GetAllWebApplications();
          if (applications.Count == allApplications.Count)
          {
              return "All existing";
          }
          string apps = "";
          foreach (SPWebApplication app in applications)
          {
              if (apps.Length > 0)
              {
                  apps += ", ";
              }
              apps += app.Name;
          }
          return apps;
      }

      protected internal override bool Rollback()
      {
        SPSolution installedSolution = SPFarm.Local.Solutions[InstallConfiguration.SolutionId];

        if (installedSolution != null)
        {
          //
          // Remove existing job, if any. 
          //
          if (installedSolution.JobExists)
          {
            RemoveExistingJob(installedSolution);
          }

          log.Info("***** SOLUTION RETRACTION *****");
          if (installedSolution.ContainsWebApplicationResource)
          {
            installedSolution.Retract(GetImmediateJobTime(), applications);
          } else
          {
            installedSolution.Retract(GetImmediateJobTime());
          }
        }

        return true;
      }
    }

    /// <summary>
    /// Command for creating an upgrade job.
    /// </summary>
    private class CreateUpgradeJobCommand : JobCommand
    {
      internal CreateUpgradeJobCommand(InstallProcessControl parent)
        : base(parent)
      {
      }

      internal override string Description
      {
        get
        {
            return CommonUIStrings.createUpgradeJobCommand;
        }
      }

      protected internal override bool Execute()
      {
        try
        {
          string filename = InstallConfiguration.SolutionFile;
          if (String.IsNullOrEmpty(filename))
          {
            throw new InstallException(CommonUIStrings.installExceptionConfigurationNoWsp);
          }

          SPSolution installedSolution = SPFarm.Local.Solutions[InstallConfiguration.SolutionId];

          if (installedSolution == null)
          {
              throw new InstallException("Expected solution {" + InstallConfiguration.SolutionId.ToString() + "} not found");
          }

          //
          // Remove existing job, if any. 
          //
          if (installedSolution.JobExists)
          {
            RemoveExistingJob(installedSolution);
          }

          log.Info(CommonUIStrings.logUpgrade);
          installedSolution.Upgrade(filename, GetImmediateJobTime());
          return true;
        }

        catch (SqlException ex)
        {
          throw new InstallException(ex.Message, ex);
        }
      }
    }

    /// <summary>
    /// Command for creating a retraction job.
    /// </summary>
    private class CreateRetractionJobCommand : JobCommand
    {
      internal CreateRetractionJobCommand(InstallProcessControl parent) : base(parent)
      {
      }

      internal override string Description
      {
        get
        {
            return CommonUIStrings.createRetractionJobCommand;
        }
      }

      protected internal override bool Execute()
      {
        try
        {
          SPSolution installedSolution = SPFarm.Local.Solutions[InstallConfiguration.SolutionId];

          if (installedSolution == null)
          {
              throw new InstallException("Expected solution {" + InstallConfiguration.SolutionId.ToString() + "} not found");
          }

          //
          // Remove existing job, if any. 
          //
          if (installedSolution.JobExists)
          {
            RemoveExistingJob(installedSolution);
          }

          if (installedSolution.Deployed)
          {
            log.Info(CommonUIStrings.logRetract);
            if (installedSolution.ContainsWebApplicationResource)
            {
              Collection<SPWebApplication> applications = installedSolution.DeployedWebApplications;
              installedSolution.Retract(GetImmediateJobTime(), applications);
            } else
            {
              installedSolution.Retract(GetImmediateJobTime());
            }
          }
          return true;
        }

        catch (SqlException ex)
        {
          throw new InstallException(ex.Message, ex);
        }
      }
    }


    private class WaitForJobCompletionCommand : Command
    {
      private readonly string description;
      private DateTime startTime;
      private bool first = true;

      internal WaitForJobCompletionCommand(InstallProcessControl parent, string description) : base(parent)
      {
        this.description = description;
      }

      internal override string Description
      {
        get
        {
          return description;
        }
      }

      protected internal override bool Execute()
      {
        try
        {
          SPSolution installedSolution = SPFarm.Local.Solutions[InstallConfiguration.SolutionId];

          if (installedSolution == null)
          {
              throw new InstallException("Expected solution {" + InstallConfiguration.SolutionId.ToString() + "} not found");
          }

          if (first)
          {
            if (!installedSolution.JobExists) return true;
            startTime = DateTime.Now;
            first = false;
          }

          //
          // Wait for job to end
          //
          if (installedSolution.JobExists)
          {
            if (DateTime.Now > startTime.Add(JobTimeout))
            {
              throw new InstallException(CommonUIStrings.installExceptionTimeout);
            }

            return false;
          } else
          {
            log.Info(installedSolution.LastOperationDetails);

            SPSolutionOperationResult result = installedSolution.LastOperationResult;
            if (result != SPSolutionOperationResult.DeploymentSucceeded && result != SPSolutionOperationResult.RetractionSucceeded)
            {
              throw new InstallException(installedSolution.LastOperationDetails);
            }

            return true;
          }
        }

        catch (Exception ex)
        {
          throw new InstallException(ex.Message, ex);
        }
      }

      protected internal override bool Rollback()
      {
        SPSolution installedSolution = SPFarm.Local.Solutions[InstallConfiguration.SolutionId];
          
        //
        // Wait for job to end
        //
        if (installedSolution != null)
        {
          if (installedSolution.JobExists)
          {
            if (DateTime.Now > startTime.Add(JobTimeout))
            {
              throw new InstallException(CommonUIStrings.installExceptionTimeout);
            }
            return false;
          } else
          {
            log.Info(installedSolution.LastOperationDetails);
          }
        }

        return true;
      }
    }

    // All feature activations and deactivation commands moved to ActivationCommands.cs

    /// <summary>
    /// Command that registers the version number of a solution.
    /// </summary>
    private class RegisterVersionNumberCommand : Command
    {
      private Version oldVersion;

      internal RegisterVersionNumberCommand(InstallProcessControl parent) : base(parent) { }

      internal override string Description
      {
        get
        {
            return CommonUIStrings.registerVersionNumberCommand;
        }
      }

      protected internal override bool Execute()
      {
        oldVersion = InstallConfiguration.InstalledVersion;
        InstallConfiguration.InstalledVersion = InstallConfiguration.SolutionVersion;
        return true;
      }

      protected internal override bool Rollback()
      {
        InstallConfiguration.InstalledVersion = oldVersion;
        return true;
      }
    }

    /// <summary>
    /// Command that unregisters the version number of a solution.
    /// </summary>
    private class UnregisterVersionNumberCommand : Command
    {
      internal UnregisterVersionNumberCommand(InstallProcessControl parent) : base(parent) { }

      internal override string Description
      {
        get
        {
            return CommonUIStrings.unregisterVersionNumberCommand;
        }
      }

      protected internal override bool Execute()
      {
        InstallConfiguration.InstalledVersion = null;
        return true;
      }
    }

    #endregion
  }
}
