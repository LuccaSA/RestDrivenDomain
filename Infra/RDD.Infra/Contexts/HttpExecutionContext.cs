using RDD.Domain;
using System.Threading;

namespace RDD.Infra.Contexts
{
    public class HttpExecutionContext : IExecutionContext
    {
        private IPrincipal _curPrincipal;
        public IPrincipal curPrincipal
        {
            get => _curPrincipal;
            set
            {
                _curPrincipal = value;
                if (_curPrincipal != null && _curPrincipal.Culture != null && _curPrincipal.Culture.RawCulture != null)
                {
                    Thread.CurrentThread.CurrentCulture = _curPrincipal.Culture.RawCulture;
                    Thread.CurrentThread.CurrentUICulture = _curPrincipal.Culture.RawCulture;
                }
            }
        }
    }
}
