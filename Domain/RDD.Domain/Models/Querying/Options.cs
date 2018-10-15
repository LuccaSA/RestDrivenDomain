namespace Rdd.Domain.Models.Querying
{
    public class Options
    {
        public bool NeedCount { get; set; }

        public bool NeedEnumeration { get; set; }

        public bool CheckRights { get; set; }

        public Options()
        {
            NeedEnumeration = true;
            CheckRights = true;
        }
    }
}