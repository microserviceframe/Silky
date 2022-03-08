using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Silky.Core.Runtime.Rpc;
using Silky.Core.Serialization;
using Silky.HealthChecks.Rpc.ServerCheck;
using Silky.Http.Core.Handlers;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Security;

namespace Silky.HealthChecks.Rpc
{
    public class SilkyRpcHealthCheck : SilkyHealthCheckBase
    {
        public SilkyRpcHealthCheck(IServerManager serverManager, IServerHealthCheck serverHealthCheck,
            ICurrentRpcToken currentRpcToken, ISerializer serializer,
            IHttpHandleDiagnosticListener httpHandleDiagnosticListener, IHttpContextAccessor httpContextAccessor,
            IServiceEntryLocator serviceEntryLocator, IRpcEndpointMonitor rpcEndpointMonitor,
            IOptions<GovernanceOptions> governanceOptions) : base(serverManager, serverHealthCheck, currentRpcToken,
            serializer, httpHandleDiagnosticListener, httpContextAccessor, serviceEntryLocator, rpcEndpointMonitor,
            governanceOptions)
        {
            HealthCheckType = HealthCheckType.Rpc;
        }

        protected override ICollection<IServer> GetServers()
        {
            return _serverManager.Servers.Where(p => p.Endpoints.Any(e => e.ServiceProtocol == ServiceProtocol.Tcp))
                .ToArray();
        }

        private string GetMessageId(HttpContext httpContext)
        {
            httpContext.SetHttpMessageId();
            return httpContext.TraceIdentifier;
        }
    }
}