using System;

namespace CodePlex.SharePointInstaller.CommandsApi.SPValidators
{
    public class SPTrueFalseValidator : SPValidator
    {
        /// <summary>
        /// Validates the specified string.
        /// </summary>
        /// <param name="str">The string to validate.</param>
        /// <returns></returns>
        public override bool Validate(string str)
        {
            bool result;
            return Boolean.TryParse(str, out result);
        }
    }
}
