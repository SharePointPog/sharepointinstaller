/***************************************************************************/
/*                                                                         */
/*                      SharePoint Solution Installer                      */
/*              http://www.codeplex.com/sharepointinstaller                */
/*                                                                         */
/*             (c) Copyright 2007-2009 Lars Fastrup Nielsen.               */
/*                                                                         */
/*  This source is subject to the Microsoft Permissive License.            */
/*  http://www.codeplex.com/sharepointinstaller/Project/License.aspx       */
/*                                                                         */
/*                                                                         */
/* KML: Added WebApplicationTargets and SitecollectionTargets              */
/* LFN 20090201: Removed generic targets                                   */
/*                                                                         */
/***************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;


namespace CodePlex.SharePointInstaller
{
  public sealed class InstallOptions
  {
    private readonly IList<SPWebApplication> webApplicationTargets;
    private readonly IList<SiteLoc> siteCollectionTargets;

    public InstallOptions()
    {
      this.webApplicationTargets = new List<SPWebApplication>();
      this.siteCollectionTargets = new List<SiteLoc>();
    }

    public IList<SPWebApplication> WebApplicationTargets
    {
      get { return webApplicationTargets; }
    }

    public IList<SiteLoc> SiteCollectionTargets
    {
      get { return siteCollectionTargets; }
    }
  }
}
