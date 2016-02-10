using System;

namespace CodePlex.SharePointInstaller.Wrappers
{
    public interface IEntityInfo //for mocking in unit tests
    {
        string Url { get; }
    }

    public abstract class EntityInfo : IEntityInfo
    {
        protected EntityInfo(Guid id, String url)
        {
            Id = id;
            Url = url;
        }

        public Guid Id
        {
            get; 
            private set;
        }

        public String Url
        {
            get; 
            private set;
        }

        public String Description
        {
            get; 
            protected set;
        }

        public bool Corrupted { get; protected set; }

        public bool Equals(EntityInfo obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.Id.Equals(Id) && Equals(obj.Url, Url) && Equals(obj.Description, Description);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (EntityInfo)) return false;
            return Equals((EntityInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Id.GetHashCode();
                result = (result*397) ^ (Url != null ? Url.GetHashCode() : 0);
                result = (result*397) ^ (Description != null ? Description.GetHashCode() : 0);
                return result;
            }
        }

        public static bool operator ==(EntityInfo left, EntityInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EntityInfo left, EntityInfo right)
        {
            return !Equals(left, right);
        }
    }
}
