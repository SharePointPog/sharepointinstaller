using System.IO;
using System.Windows.Forms;

namespace CodePlex.SharePointInstaller.Utils
{
    public static class FileUtil
    {
        public static string GetRootedPath(string relativePath)
        {
            return Path.IsPathRooted(relativePath)
                       ? relativePath
                       : Path.Combine(Application.StartupPath, relativePath);
        }

        public static string GetFileName(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            return Path.IsPathRooted(relativePath)
                       ? Path.GetFileName(relativePath)
                       : Path.GetFileName(Path.Combine(Application.StartupPath, relativePath));
    }
    }
}
