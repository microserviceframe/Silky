using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Serialization;

namespace Silky.HealthChecks.Rpc
{
    public static class SilkyRpcHealthCheckEndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapSilkyRpcHealthChecks(this IEndpointRouteBuilder endpoints,
            string pattern = "/api/silkyrpc/health")
        {
            return endpoints.MapHealthChecks(pattern, new HealthCheckOptions()
            {
                Predicate = (check) => check.Name.Equals(SilkyRpcHealthCheckBuilderExtensions.SILKYRPC_NAME),
                ResponseWriter = SilkyRpcApiResponseWriter.ResponseWriter
            }).RequireAuthorization();
        }
        
        public static IEndpointConventionBuilder MapSilkyGatewayHealthChecks(this IEndpointRouteBuilder endpoints,
            string pattern = "/api/silkygateway/health")
        {
            return endpoints.MapHealthChecks(pattern, new HealthCheckOptions()
            {
                Predicate = (check) => check.Name.Equals(SilkyRpcHealthCheckBuilderExtensions.SILKYGATEWAT_NAME),
                ResponseWriter = SilkyRpcApiResponseWriter.ResponseWriter
            }).RequireAuthorization();
        }
    }
}