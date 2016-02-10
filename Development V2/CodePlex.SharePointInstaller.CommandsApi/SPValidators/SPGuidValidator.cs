using System;

namespace CodePlex.SharePointInstaller.CommandsApi.SPValidators
{
    public class SPGuidValidator : SPNonEmptyValidator
    {
        public override bool Validate(string str)
        {
            try
            {
                var guid = new Guid(str);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}