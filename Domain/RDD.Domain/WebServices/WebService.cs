using RDD.Domain.Helpers;
using RDD.Domain.Models;
using System.Collections.Generic;
using System.Globalization;

namespace RDD.Domain.WebServices
{
    public class WebService : EntityBase<WebService, int>, IPrincipal
    {
        public override int Id { get; set; }
        public override string Name { get; set; }
        public string Token { get; set; }

        public PrincipalType Type => PrincipalType.WebService;

        public Culture Culture => new Culture(CultureInfo.GetCultureInfo("en-US"));

        public ICollection<int> AppOperations { get; set; }

        public WebService()
        {
            AppOperations = new HashSet<int>();
        }
    }
}
