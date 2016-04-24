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
/* KML: Created this to allow site collection targets                 */
/*                                                                    */
/**********************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;


namespace CodePlex.SharePointInstaller
{
  public partial class SiteCollectionDeploymentTargetsControl : InstallerControl
  {
    #region Constants

    public const string FormattedSiteCollectionCountWarningMessage =
      "There are {0} site collections within this web application.\n" +
      "Showing a lot of site collections may take significant time.\n" +
      "If you want to deploy to all site collections in this web application\n" +
      "you can simply check the web application checkbox without expanding it,\n" +
      "however this will still take considerable time during the install process.\n\n" +
      "Are you sure you want to show all {0} site collections for this web application?";

    public const int SiteCollectionCountWarning = 100;

    #endregion

    #region Constructor

    private ReadOnlyCollection<Guid?> _featureIds;
    public SiteCollectionDeploymentTargetsControl(ReadOnlyCollection<Guid?> featureIds)
    {
      _featureIds = featureIds;
      InitializeComponent();

      siteCollectionsTreeView.AfterCheck += new TreeViewEventHandler(SiteCollectionsTreeView_AfterCheck);

      this.Load += new EventHandler(Control_Load);
    }

    #endregion

    #region overrides

    protected internal override void Close(InstallOptions options)
    {
      foreach (TreeNode webAppTreeNode in siteCollectionsTreeView.Nodes)
      {
        // Add the web application as a target
        if (webAppTreeNode.Checked)
        {
          WebApplicationInfo webAppInfo = webAppTreeNode.Tag as WebApplicationInfo;
          if (webAppInfo != null)
          {
            options.WebApplicationTargets.Add(webAppInfo.Application);

            ExtendedTreeNode extendedWebAppTreeNode = webAppTreeNode as ExtendedTreeNode;
            if (extendedWebAppTreeNode != null)
            {
              if (!extendedWebAppTreeNode.Populated)
              {
                // Add ALL site collections within the web app as targets
                foreach (SPSite siteCollection in webAppInfo.Application.Sites)
                {
                    try
                    {
                        SiteLoc siteLoc = new SiteLoc(siteCollection);
                        siteLoc.featureList.AddRange(_featureIds); // add all known features for now (be nice to let user choose)
                        options.SiteCollectionTargets.Add(siteLoc);
                    }
                    finally
                    {
                        // guarantee SPSite is released ASAP even in face of exception
                        siteCollection.Dispose();
                    }
                }
              }
              else
              {
                // Add the checked site collections within this web application as targets 
                foreach (TreeNode siteCollTreeNode in webAppTreeNode.Nodes)
                {
                  if (siteCollTreeNode.Checked)
                  {
                    SiteCollectionInfo siteCollInfo = siteCollTreeNode.Tag as SiteCollectionInfo;
                    if (siteCollInfo != null)
                    {
                        SiteLoc siteLoc = new SiteLoc(siteCollInfo.SiteCollection);
                        siteLoc.featureList.AddRange(_featureIds); // add all known features for now (be nice to let user choose)
                        options.SiteCollectionTargets.Add(siteLoc);
                    }
                  }
                }
              }
            }
          }
        }
      }
    }

    #endregion

    #region Event Handlers

    private void Control_Load(object sender, EventArgs e)
    {
      ConfigureSiteCollectionTreeView();

      EnableDisableNextButton();
    }

    void SiteCollectionsTreeView_AfterCheck(object sender, TreeViewEventArgs e)
    {
      EnableDisableNextButton();
    }

    void WebAppTreeNode_TreeNodePopulate(object sender, TreeViewCancelEventArgs e)
    {
      WebApplicationInfo webAppInfo = e.Node.Tag as WebApplicationInfo;
      if (webAppInfo != null)
      {
        if (webAppInfo.Application.Sites.Count >= SiteCollectionCountWarning)
        {
          string msg = String.Format(FormattedSiteCollectionCountWarningMessage, webAppInfo.Application.Sites.Count);
          if (MessageBox.Show(msg, "Large Number of Site Collections", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
          {
            e.Cancel = true;
          }
        }
        if (!e.Cancel)
        {
          foreach (SPSite site in webAppInfo.Application.Sites)
          {
              try
              {
                  SiteCollectionInfo siteCollInfo = new SiteCollectionInfo(site, false);
                  ExtendedTreeNode.AddNewExtendedTreeNode(e.Node.Nodes, siteCollInfo.ToString(), siteCollInfo);
              }
              finally
              {
                  // guarantee SPSite is released ASAP even in face of exception
                  site.Dispose();
              }

          }
        }
      }
    }

    #endregion

    #region Private Methods

    private void EnableDisableNextButton()
    {
        bool enabled = true;
        // Original code only enabled Next button when some site collection was checked
        // But it is valid and useful for user to continue without activating on any 
        // site collection, so now we enable in all cases
      /*
      bool enabled = false;
      // Enable the next button on the form if we have any site collections or web apps checked
      // Note that a web app being checked doesn't count unless the tree node has not yet been populated
      // We do this to avoid race conditions that could occur
      foreach (TreeNode webAppTreeNode in siteCollectionsTreeView.Nodes)
      {
        ExtendedTreeNode extendedWebAppTreeNode = webAppTreeNode as ExtendedTreeNode;
        if (extendedWebAppTreeNode != null && extendedWebAppTreeNode.Checked && !extendedWebAppTreeNode.Populated)
        {
          enabled = true;
          break;
        }
        else
        {
          foreach (TreeNode siteCollTreeNode in webAppTreeNode.Nodes)
          {
            if (siteCollTreeNode.Checked)
            {
              enabled = true;
              break;
            }
          }
        }
      }
       * */

      Form.NextButton.Enabled = enabled;
    }

    private void ConfigureSiteCollectionTreeView()
    {
      PopulateTreeViewForWebApps(siteCollectionsTreeView.Nodes, SPWebService.AdministrationService.WebApplications);
      PopulateTreeViewForWebApps(siteCollectionsTreeView.Nodes, SPWebService.ContentService.WebApplications);
    }

    private void PopulateTreeViewForWebApps(TreeNodeCollection treeNodes, SPWebApplicationCollection webApps)
    {
      foreach (SPWebApplication application in webApps)
      {
        WebApplicationInfo webAppInfo = new WebApplicationInfo(application, false);
        ExtendedTreeNode webAppTreeNode = ExtendedTreeNode.AddNewExtendedTreeNode(treeNodes, webAppInfo.ToString(), true, webAppInfo);
        webAppTreeNode.TreeNodePopulate += new ExtendedTreeNode.TreeNodeEventHandler(WebAppTreeNode_TreeNodePopulate);
      }
    }

    #endregion

    #region Inner Classes

    // KML TODO - this is duplicated in another file - we should probably break it out into its own file along with SiteCollectionInfo
    //            However, this is not needed if we bundle DeploymentTargetsControl with SiteCollectionDeploymentTargetsControl
    private class WebApplicationInfo
    {
      private readonly SPWebApplication application;
      private readonly bool required;

      internal WebApplicationInfo(SPWebApplication application, bool required)
      {
        this.application = application;
        this.required = required;
      }

      internal SPWebApplication Application
      {
        get { return application; }
      }

      public bool Required
      {
        get { return required; }
      }

      public bool IsSRP
      {
        get { return application.Properties.ContainsKey("Microsoft.Office.Server.SharedResourceProvider"); }
      }

      public override string ToString()
      {
        string str = application.GetResponseUri(SPUrlZone.Default).ToString();

        if (application.IsAdministrationWebApplication)
        {
          str += "     (Central Administration)";
        } else if (IsSRP)
        {
          str += "     (Shared Resource Provider)";
        } else if (!String.IsNullOrEmpty(application.DisplayName))
        {
          str += "     (" + application.DisplayName + ")";
        } else if (!String.IsNullOrEmpty(application.Name))
        {
          str += "     (" + application.Name + ")";
        }

        int numSiteCollections = application.Sites.Count;
        str += String.Format("    ({0} site collection{1})", numSiteCollections, (numSiteCollections > 1) ? "s" : String.Empty);

        return str;
      }
    }

    private class SiteCollectionInfo
    {
      private readonly SPSite siteCollection;
      private readonly bool required;

      internal SiteCollectionInfo(SPSite siteCollection, bool required)
      {
        this.siteCollection = siteCollection;
        this.required = required;
      }

      internal SPSite SiteCollection
      {
        get { return siteCollection; }
      }

      internal bool Required
      {
        get { return required; }
      }

      public override string ToString()
      {
        string rootWebTitle = String.Empty;
        try
        {
          // Note that this does involve doing an OpenWeb on the root web
          // which does have a performance penalty
          // The thought is that the information is worth the performance cost
          rootWebTitle = siteCollection.RootWeb.Title;
        }
        catch (UnauthorizedAccessException)
        {
          rootWebTitle = "<cannot access root web - access denied>";
        }
        catch (Exception ex)
        {
          rootWebTitle = "<" + ex.Message + ">";
        }
        return siteCollection.Url + "    (" + rootWebTitle + ")";
      }
    }

    #endregion
  }
}
