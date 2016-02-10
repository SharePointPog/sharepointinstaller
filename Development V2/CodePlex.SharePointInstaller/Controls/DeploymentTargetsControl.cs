using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CodePlex.SharePointInstaller.Commands;
using CodePlex.SharePointInstaller.Controls;
using CodePlex.SharePointInstaller.Logging;
using CodePlex.SharePointInstaller.Wrappers;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Controls
{
    public partial class DeploymentTargetsControl : Step
    {
        public const string FormattedSiteCollectionCountWarningMessage =
            "There are {0} site collections within this web application.\n" +
            "Showing a lot of site collections may take significant time.\n" +
            "If you want to deploy to all site collections in this web application\n" +
            "you can simply check the web application checkbox without expanding it,\n" +
            "however this will still take considerable time during the install process.\n\n" +
            "Are you sure you want to show all {0} site collections for this web application?";
        public const int SiteCollectionCountWarning = 100;

        private bool nextEnabled;
        private InstallationContext context;

        private bool iterateThroughSites;
        private bool iterateThroughSiteCollections;

        public DeploymentTargetsControl(Installer form, Step previous) : base(form, previous)
        {
            InitializeComponent();
            treeView.AfterCheck += OnTreeViewAfterCheck;
            treeView.BeforeCheck += OnTreeViewBeforeCheck;
            treeView.BeforeExpand += OnTreeViewBeforeCheck;
        }

        public override void Initialize(InstallationContext context)
        {
            this.context = context;
            Title = Resources.CommonUIStrings.ControlTitleSiteDeployment;
            SubTitle = Resources.CommonUIStrings.ControlSubTitleSiteDeployment;
            Form.NextButton.Enabled = nextEnabled;
            Form.SkipButton.Enabled = false;

            iterateThroughSites = context.SolutionInfo.Features.SiteFeatures.Count > 0;
            iterateThroughSiteCollections = iterateThroughSites || context.SolutionInfo.Features.SiteCollectionFeatures.Count > 0;
        }

        public override void Execute(InstallationContext context)
        {
            PopulateTreeView(context);
            EnableDisableNextButton();
            treeView.Enabled = !VerifyIfanyNodeChecked();
        }

        private static void OnTreeViewBeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.ForeColor == System.Drawing.Color.Red)
                e.Cancel = true;
        }

        protected void FillContext(InstallationContext context)
        {
            foreach (TreeNode webAppNode in treeView.Nodes)
            {
                if (webAppNode.Checked)
                {
                    var webAppInfo = webAppNode.Tag as WebApplicationInfo;
                    if (webAppInfo != null)
                    {
                        context.AddSelectedWebApp(webAppInfo.Application);
                        var extendedWebAppTreeNode = webAppNode as ExtendedTreeNode;
                        if (extendedWebAppTreeNode != null)
                        {
                            if (extendedWebAppTreeNode.Populated)
                                FillContextWithPopulatedWebAppNode(webAppNode, context);
                            else
                                FillContextWithWebAppNode(webAppInfo, context);
                        }
                    }
                }
            }
        }

        private void FillContextWithSiteCollectionAndWebs(SPSite siteCollection, InstallationContext context)
        {
            context.AddSelectedSiteCollection(new SiteCollectionInfo(siteCollection));
            if (!iterateThroughSites) return;

            List<SiteInfo> siteCollectionWebs = new List<SiteInfo>(); //to store the list of successfully added webs
            bool? catchAccessDeniedException = null;
            try
            {
                if (!SiteInfo.IsAccessible(siteCollection.RootWeb))
                    return;
                var siteId = siteCollection.ID; //DirectoryNotFoundException here if SiteCollection is corrupted
                catchAccessDeniedException = siteCollection.CatchAccessDeniedException;
                siteCollection.CatchAccessDeniedException = false;

                foreach (SPWeb site in siteCollection.AllWebs)
                {
                    using (site)
                    {
                        if (!SiteInfo.IsAccessible(site))
                            continue;
                        var webId = site.ID; //DirectoryNotFoundException here if SiteCollection is corrupted
                        siteCollectionWebs.Add(new SiteInfo(site));
                    }
                }
            }
            catch (IOException e)
            {
                Log.Error(String.Format("Skipping corrupted site collection {0}.", siteCollection.Url), e);
                return;
            }
            catch (UnauthorizedAccessException e)
            {
                Log.Info(String.Format("Skipping restricted site collection {0}.", siteCollection.Url), e);
                return;
            }
            finally
            {
                if (catchAccessDeniedException.HasValue)
                    siteCollection.CatchAccessDeniedException = catchAccessDeniedException.Value;
            }

            if (siteCollectionWebs.Count > 0)
                foreach (SiteInfo web in siteCollectionWebs)
                    context.AddSelectedSite(web);
        }

        private void FillContextWithWebAppNode(WebApplicationInfo webAppInfo, InstallationContext context)
        {
            RunSeparateThread(delegate
                                  {
                                      foreach (SPSite siteCollection in webAppInfo.Application.Sites)
                                      {
                                          using (siteCollection)
                                          {
                                              FillContextWithSiteCollectionAndWebs(siteCollection, context);
                                          }
                                      }
                                  });
        }

        private void FillContextWithPopulatedWebAppNode(TreeNode webAppNode, InstallationContext context)
        {
            // Add the checked site collections within this web application as targets 
            foreach (TreeNode siteCollectionNode in webAppNode.Nodes)
            {
                var siteCollectionInfo = siteCollectionNode.Tag as SiteCollectionInfo;
                if (siteCollectionInfo != null)
                {
                    if (siteCollectionNode.Checked)
                    {
                        if (((ExtendedTreeNode)siteCollectionNode).Populated)
                        {
                            context.AddSelectedSiteCollection(siteCollectionInfo);
                            FillContextWithPopulatedSiteCollectionNode(siteCollectionNode, context);
                        }
                        else
                            RunSeparateThread(delegate
                                                {
                                                    using (var siteCollection = new SPSite(siteCollectionInfo.Id))
                                                    {
                                                        FillContextWithSiteCollectionAndWebs(siteCollection, context);
                                                    }
                                                });
                    }                   
                }
            }
        }

        private void FillContextWithPopulatedSiteCollectionNode(TreeNode siteCollectionNode, InstallationContext context)
        {
            foreach (TreeNode siteNode in siteCollectionNode.Nodes)
            {
                var siteInfo = siteNode.Tag as SiteInfo;
                if (siteInfo != null)
                {
                    if (siteNode.Checked)
                        context.AddSelectedSite(siteInfo);                    
                }                                                    
            }
        }

        private void OnTreeViewAfterCheck(object sender, TreeViewEventArgs e)
        {
            EnableDisableNextButton();
        }

        private void OnWebAppTreeNodePopulate(object sender, TreeViewCancelEventArgs e)
        {
            RunSeparateThread(
                delegate
                    {
                        var webAppInfo = e.Node.Tag as WebApplicationInfo;
                        if (webAppInfo != null)
                        {
                            if (webAppInfo.Application.Sites.Count >= SiteCollectionCountWarning)
                            {
                                var msg = String.Format(FormattedSiteCollectionCountWarningMessage,
                                                        webAppInfo.Application.Sites.Count);
                                if (String.IsNullOrEmpty(context.SolutionInfo.Url) && MessageBox.Show(msg, "Large Number of Site Collections", MessageBoxButtons.YesNo,MessageBoxIcon.Warning) != DialogResult.Yes)
                                {
                                    e.Cancel = true;
                                }
                            }
                            if (!e.Cancel)
                            {
                                UpdateUI(treeView, () => treeView.Enabled = false);                                
                                foreach (SPSite siteCollection in webAppInfo.Application.Sites)
                                {
                                    using (siteCollection)
                                    {
                                        InstallationUtility.WriteLog(String.Format("SCDTC: SPSite with ID: {0} is opened.", siteCollection.ID));
                                        var info = new SiteCollectionInfo(siteCollection);
                                        UpdateUI(treeView, () =>
                                                               {
                                                                   if (!e.Node.IsExpanded)
                                                                       e.Node.Expand();
                                                                   var siteCollectionNode = ExtendedTreeNode.AddNewExtendedTreeNode(e.Node.Nodes, info.Description, !info.Corrupted && iterateThroughSites, info);
                                                                   if (info.Corrupted)
                                                                   {
                                                                       siteCollectionNode.ForeColor = System.Drawing.Color.Red;
                                                                       siteCollectionNode.Checked = false;
                                                                   }
                                                                   else if (iterateThroughSites)
                                                                       siteCollectionNode.TreeNodePopulate += OnSiteCollectionTreeNodePopulate;
                                                                   siteCollectionNode.EnsureVisible();
                                                               });
                                    }
                                }
                                UpdateUI(treeView, () => MakePreSelection(e.Node.Nodes, delegate(TreeNode node)
                                                                                            {
                                                                                                var siteColInfo = node.Tag as SiteCollectionInfo;
                                                                                                var defaultTarget = context.SolutionInfo.DefaultTarget;
                                                                                                return siteColInfo != null && defaultTarget != null &&
                                                                                                       siteColInfo.Id == defaultTarget.SiteCollectionId;

                                                                                            }));
                            }
                        }
                    });
        }

        private void OnSiteCollectionTreeNodePopulate(object sender, TreeViewCancelEventArgs e)
        {
            RunSeparateThread(
                delegate
                    {
                        UpdateUI(treeView, () => treeView.Enabled = false);
                        var siteCollectionInfo = e.Node.Tag as SiteCollectionInfo;
                        try
                        {                            
                            if (siteCollectionInfo != null)
                            {
                                using (var siteCollection = new SPSite(siteCollectionInfo.Id))
                                {
                                    foreach (SPWeb site in siteCollection.AllWebs)
                                    {
                                        using (site)
                                        {
                                            if (
                                                !site.DoesUserHavePermissions(SPBasePermissions.ViewPages |
                                                                              SPBasePermissions.ManageWeb))
                                                continue;
                                            InstallationUtility.WriteLog(
                                                String.Format("SCDTC: SPWeb with ID: {0} is opened.", site.ID));
                                            var info = new SiteInfo(site);
                                            UpdateUI(treeView, () =>
                                                                   {
                                                                       if (!e.Node.IsExpanded)
                                                                           e.Node.Expand();

                                                                       var node =
                                                                           ExtendedTreeNode.AddNewExtendedTreeNode(
                                                                               e.Node.Nodes, info.Description,
                                                                               false, info);
                                                                       if (info.Corrupted)
                                                                       {
                                                                           node.ForeColor =
                                                                               System.Drawing.Color.Red;
                                                                           node.Checked = false;
                                                                       }
                                                                       node.EnsureVisible();
                                                                   });
                                        }
                                    }
                                    UpdateUI(treeView, () => MakePreSelection(e.Node.Nodes, delegate(TreeNode node)
                                                                                                {
                                                                                                    var siteInfo = node.Tag as SiteInfo;
                                                                                                    var defaultTarget = context.SolutionInfo.DefaultTarget;
                                                                                                    return siteInfo != null && defaultTarget != null &&
                                                                                                           siteInfo.Id == defaultTarget.Id;
                                                                                                }));
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            Log.Error(String.Format("Failed to populate site collection node in tree view. Url: {0}", siteCollectionInfo != null ? siteCollectionInfo.Url : ""), ex);
                        }                        
                    });
        }

        private delegate bool MostAdjusted(TreeNode node);

        private void MakePreSelection(TreeNodeCollection nodes, MostAdjusted mostAdjusted)
        {
            foreach (TreeNode node in nodes)
            {                         
                if (mostAdjusted(node))
                {
                    if (node.Tag is SiteInfo)
                        node.Checked = true;
                    node.Expand();
                    node.EnsureVisible();
                    return;
                }
            }
            treeView.Enabled = true;
        }

        private void UpdateUI(Control ctrl, MethodInvoker code)
        {
            try
            {
                if (ctrl.IsDisposed || !ctrl.IsHandleCreated || code == null)
                    return;
                if (ctrl.InvokeRequired)
                    ctrl.BeginInvoke(code);
                else
                    code();
            }
            catch
            {
            }
        }

        private void EnableDisableNextButton()
        {
            if(VerifyIfanyNodeChecked())
            {
                Form.NextButton.Focus();
                nextEnabled = Form.NextButton.Enabled = true;
            }
            else
                nextEnabled = Form.NextButton.Enabled = false;
        }

        private bool VerifyIfanyNodeChecked()
        {
            var enabled = false;

            // Enable the next button on the form if we have any site collections or web apps checked
            // Note that a web app being checked doesn't count unless the tree node has not yet been populated
            // We do this to avoid race conditions that could occur
            foreach (TreeNode node in treeView.Nodes)
            {
                var webAppNode = node as ExtendedTreeNode;
                if (webAppNode != null && webAppNode.Checked && !webAppNode.Populated)
                {
                    enabled = true;
                    break;
                }
                foreach (TreeNode siteCollectionNode in node.Nodes)
                {
                    if (siteCollectionNode.Checked)
                    {
                        enabled = true;
                        break;
                    }
                    foreach(TreeNode siteNode in siteCollectionNode.Nodes)
                    {
                        if(siteNode.Checked)
                        {
                            enabled = true;
                            break;
                        }
                    }
                }
            }
            return enabled;
        }
        
        private void RunSeparateThread(WaitCallback callback)
        {
            ThreadPool.QueueUserWorkItem(callback);
        }

        private void PopulateTreeView(InstallationContext context)
        {
            if (GetConfiguration(context).RequireDeploymentToCentralAdminWebApplication)
                PopulateWebAppNodes(treeView.Nodes, SPWebService.AdministrationService.WebApplications);
            PopulateWebAppNodes(treeView.Nodes, SPWebService.ContentService.WebApplications);
            MakePreSelection(treeView.Nodes, delegate(TreeNode node)
                                   {
                                       var webAppInfo = node.Tag as WebApplicationInfo;
                                       var defaultTarget = context.SolutionInfo.DefaultTarget;
                                       return webAppInfo != null && defaultTarget != null &&
                                              webAppInfo.Id == defaultTarget.WebApplicationId;
                                   });
        }

        private void PopulateWebAppNodes(TreeNodeCollection treeNodes, SPWebApplicationCollection webApps)
        {
            foreach (var webApp in webApps)
            {
                var webAppInfo = new WebApplicationInfo(webApp);
                var webAppTreeNode = ExtendedTreeNode.AddNewExtendedTreeNode(treeNodes, webAppInfo.Description, iterateThroughSiteCollections, webAppInfo);
                if (iterateThroughSiteCollections)
                    webAppTreeNode.TreeNodePopulate += OnWebAppTreeNodePopulate;
            }
        }

        public override Step CreateNextStep(InstallationContext context)
        {
            FillContext(context);
            treeView.Enabled = false;
            return new InstallProcessControl(Form, this);
        }
    }
}