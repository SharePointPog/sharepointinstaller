using System.IO;

namespace CodePlex.SharePointInstaller.CommandsApi.SPValidators
{
    public class SPDirectoryExistsValidator : SPNonEmptyValidator
    {
        public override bool Validate(string str)
        {
            if (base.Validate(str))
            {
                try
                {
                    if (Directory.Exists(str))
                    {
                        return true;
                    }
                    else
                    {
                        ErrorInfo = "Directory not found";
                        throw new DirectoryNotFoundException();
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }


}
