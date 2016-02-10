using System.Collections;
using System.Collections.Generic;

namespace CodePlex.SharePointInstaller.Utils
{
    public class ExplicitlyCovariantList<TClass, TInterface> : IEnumerable<TInterface>
        where TClass : TInterface
    {
        private IList<TClass> list;

        public ExplicitlyCovariantList(IList<TClass> list)
        {
            this.list = list;
        }

        public IEnumerator<TInterface> GetEnumerator()
        {
            foreach (TClass item in list)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}