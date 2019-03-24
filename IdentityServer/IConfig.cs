using System.Collections.Generic;
using IdentityServer4.Models;

namespace IdentityServer
{
    public interface IConfig
    {
        IEnumerable<IdentityResource> GetIdentityResources();
        IEnumerable<ApiResource> GetApis();
        IEnumerable<Client> GetClients();
    }
}
