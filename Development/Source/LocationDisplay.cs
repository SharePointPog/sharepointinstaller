using System;
using System.Collections.Generic;
using System.Text;

namespace CodePlex.SharePointInstaller
{
    class LocationDisplay
    {
        FeatureLocations featuredLocations;
        public LocationDisplay(FeatureLocations fLocs)
        {
            this.featuredLocations = fLocs;
        }
        public string GetLocationSummary()
        {
            StringBuilder text = new StringBuilder();
            text.AppendFormat("{0} Locations", featuredLocations.GetTotalFeatureLocations());
            Queue<string> locFrags = GetLocationFragments();
            bool first = true;
            while (locFrags.Count > 0)
            {
                string fragment = locFrags.Dequeue();
                if (first)
                {
                    text.Append(": ");
                }
                else
                {
                    text.Append(", ");
                }
                text.Append(fragment);
                first = false;
            }
            return text.ToString();
        }
        private Queue<string> GetLocationFragments()
        {
            Queue<string> fragments = new Queue<string>();
            if (featuredLocations.FarmLocations.Count > 0)
            {
                fragments.Enqueue(string.Format("{0} Farm", featuredLocations.FarmLocations.Count));
            }
            if (featuredLocations.WebAppLocations.Count > 0)
            {
                fragments.Enqueue(string.Format("{0} WebApp", featuredLocations.WebAppLocations.Count));
            }
            if (featuredLocations.SiteLocations.Count > 0)
            {
                fragments.Enqueue(string.Format("{0} SiteColl", featuredLocations.SiteLocations.Count));
            }
            if (featuredLocations.WebLocations.Count > 0)
            {
                fragments.Enqueue(string.Format("{0} Web", featuredLocations.WebLocations.Count));
            }
            return fragments;
        }

    }
}
