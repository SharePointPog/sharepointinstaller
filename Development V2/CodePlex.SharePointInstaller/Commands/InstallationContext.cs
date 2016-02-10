using System;
using System.Collections.Generic;
using CodePlex.SharePointInstaller.Configuration;
using CodePlex.SharePointInstaller.Utils;
using CodePlex.SharePointInstaller.Wrappers;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller.Commands
{
    public interface IContext //for mocking in unit tests
    {
        InstallConfiguration Configuration { get; }
        SolutionInfo SolutionInfo { get; }
        InstallAction Action { get; set; }
        IList<SPWebApplication> WebApps { get; }
        IList<IEntityInfo> SiteCollections { get; }
        IList<IEntityInfo> Sites { get; }
    }

    public class InstallationContext : IContext
    {
        private IList<SPWebApplication> webApps = new List<SPWebApplication>();

        private IList<SiteCollectionInfo> siteCollections = new List<SiteCollectionInfo>();

        private IList<SiteInfo> sites = new List<SiteInfo>();

        private InstallConfiguration  configuration;

        private Queue<SolutionInfo> solutions = new Queue<SolutionInfo>();

        private bool isEmpty;

        public InstallationContext(IList<Guid> selectedSolutions)
        {
            foreach(var id in selectedSolutions)
                solutions.Enqueue(Configuration.Solutions[id]);
        }

        public InstallationContext(bool isEmpty)
        {
            this.isEmpty = isEmpty;
        }

        public void BeginInstallation()
        {
        }

        public void EndInstallation()
        {
            solutions.Dequeue();
            Clear();
        }

        public void AddSelectedWebApp(SPWebApplication webApp)
        {
            if(webApp != null && ((List<SPWebApplication>)webApps).Find(app => app.Id == webApp.Id) == null)
                webApps.Add(webApp);
        }

        public void AddSelectedSiteCollection(SiteCollectionInfo siteCollection)
        {
            if (siteCollection != null && FindSiteCollection(spSite => spSite.Id == siteCollection.Id) == null)
            {
                siteCollections.Add(siteCollection);
            }
        }

        public void AddSelectedSite(SiteInfo site)
        {
            if (site != null && FindSite(web => web.Id == site.Id) == null)
            {
                sites.Add(site);
            }
        }        
       
        private SiteInfo FindSite(Predicate<SiteInfo> match)
        {
            return ((List<SiteInfo>) sites).Find(match);       
        }

        private SiteCollectionInfo FindSiteCollection(Predicate<SiteCollectionInfo> match)
        {
            return ((List<SiteCollectionInfo>)siteCollections).Find(match);            
        }

        public void Clear()
        {
            webApps.Clear();
            siteCollections.Clear();
            sites.Clear();
        }

        public static InstallationContext Empty
        {
            get
            {
                return new InstallationContext(true);
            }
        }

        public IList<SPWebApplication> WebApps
        {
            get
            {
                return webApps;
            }            
        }

        public IList<SiteCollectionInfo> SiteCollections
        {
            get
            {
                return siteCollections;
            }            
        }

        public IList<SiteInfo> Sites
        {
            get
            {
                return sites;
            }
        }

        IList<IEntityInfo> IContext.SiteCollections
        {
            get
            {
                return
                    new List<IEntityInfo>(new ExplicitlyCovariantList<SiteCollectionInfo, IEntityInfo>(siteCollections));
            }
        }

        IList<IEntityInfo> IContext.Sites
        {
            get
            {
                return
                    new List<IEntityInfo>(new ExplicitlyCovariantList<SiteInfo, IEntityInfo>(sites));
            }
        }

        public InstallConfiguration Configuration
        {
            get
            {
                if(configuration == null)
                {
                    configuration = InstallConfiguration.GetConfiguration();
                }
                return configuration;
            }
        }

        public SolutionInfo SolutionInfo
        {
            get
            {
                if (solutions.Count > 0)
                    return solutions.Peek();
                return null;
            }            
        }

        public InstallAction Action
        {
            get; 
            set;
        }
    }
}
