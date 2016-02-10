
namespace CodePlex.SharePointInstaller.CommandsApi.SPValidators
{
    public class SPIntRangeValidator : SPNonEmptyValidator
    {
        private int m_nLower;
        private int m_nUpper;

        public SPIntRangeValidator(int nLower, int nUpper)
        {
            m_nLower = nLower;
            m_nUpper = nUpper;
        }

        public override bool Validate(string strParam)
        {
            if (!base.Validate(strParam))
            {
                return false;
            }
            int num = int.Parse(strParam);
            if (m_nLower > num)
            {
                return false;
            }
            return (num <= m_nUpper);
        }
    }

 

}
