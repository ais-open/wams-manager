using System;

namespace Ais.Internal.Dcm.Business
{
    public class Locator
    {
        public string Id { get; internal set; }
        public DateTime ExpirationDateTime { get; internal set; }
        public LocatorType Type { get; internal set; }
        public string Path { get; internal set; }
        public string BaseUri { get; internal set; }
        public string ContentAccessComponent { get; internal set; }
        public string AccessPolicyId { get; internal set; }
        public string AssetId { get; internal set; }

        MediaServiceContext _context = null;
        public Locator(MediaServiceContext context)
        {
            this._context = context;
        }
    }

    public enum LocatorType
    {
        None=0,
        SaS = 1,
        OnDemandOrigin =2
    }
}
