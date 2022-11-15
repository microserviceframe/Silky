﻿using System.Linq;
using Silky.Rpc.Runtime.Server;

namespace Silky.Http.Core.Routing.Builder.Internal;

internal class SilkyDashboardEndpointDataSource : SilkyServiceEntryEndpointDataSourceBase
{
    public SilkyDashboardEndpointDataSource(IServiceEntryManager serviceEntryManager,
        ServiceEntryEndpointFactory serviceEntryEndpointFactory) : base(serviceEntryManager,
        serviceEntryEndpointFactory)
    {
    }

    protected override ServiceEntry[] GetEntries()
    {
        return _serviceEntryManager.GetAllEntries().Where(p => p.IsSilkyAppService()).Distinct().ToArray();
    }
}