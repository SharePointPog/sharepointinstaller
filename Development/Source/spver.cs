using System;
using System.Text;
using Microsoft.SharePoint.Administration;

namespace CodePlex.SharePointInstaller
{
    class spver
    {
        static public int GetHive()
        {
            return SPFarm.Local.BuildVersion.Major;
        }
    }
}
