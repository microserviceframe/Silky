using Microsoft.AspNetCore.Http;
using Silky.Core.Exceptions;
using Silky.Core.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Extensions
{
    public static class HttpContextExtensions
    {
        public static ServiceEntry GetServiceEntry(this HttpContext context)
        {
            var serviceEntry = context.GetEndpoint()?.Metadata.GetMetadata<ServiceEntry>();
            return serviceEntry;
        }

        public static void SignoutToSwagger(this HttpContext httpContext)
        {
            httpContext.Response.Headers["access-token"] = "invalid_token";
        }

        public static void SetHttpMessageId(this HttpContext httpContext)
        {
            RpcContext.Context.SetAttachment(AttachmentKeys.MessageId, httpContext.TraceIdentifier);
        }

        public static void SetUserClaims(this HttpContext httpContext)
        {
            var isAuthenticated = httpContext.User.Identity?.IsAuthenticated;
            if (isAuthenticated == true)
            {
                foreach (var userClaim in httpContext.User.Claims)
                {
                    RpcContext.Context.SetAttachment(userClaim.Type, userClaim.Value);
                }
            }
        }

        public static void SetHttpHandleAddressInfo(this HttpContext httpContext)
        {
            RpcContext.Context.SetAttachment(AttachmentKeys.IsGateway, true);

            var localWebEndpointDescriptor = RpcEndpointHelper.GetLocalWebEndpoint();
            var clientHost = httpContext.Connection.RemoteIpAddress;
            var clientPort = httpContext.Connection.RemotePort;

            RpcContext.Context.SetAttachment(AttachmentKeys.ClientHost, clientHost.MapToIPv4().ToString());
            RpcContext.Context.SetAttachment(AttachmentKeys.ClientServiceProtocol,
                localWebEndpointDescriptor.ServiceProtocol.ToString());
            RpcContext.Context.SetAttachment(AttachmentKeys.RpcRequestPort, clientPort.ToString());
            RpcContext.Context.SetAttachment(AttachmentKeys.ClientPort, clientPort.ToString());

            var localRpcEndpoint = RpcEndpointHelper.GetLocalWebEndpointDescriptor();
            RpcContext.Context.SetAttachment(AttachmentKeys.LocalAddress, localRpcEndpoint.Host);
            RpcContext.Context.SetAttachment(AttachmentKeys.LocalPort, localRpcEndpoint.Port);
            RpcContext.Context.SetAttachment(AttachmentKeys.LocalServiceProtocol, localRpcEndpoint.ServiceProtocol);
        }
        
        public static void SetResultCode(this HttpResponse httpResponse, StatusCode statusCode)
        {
            httpResponse.Headers["SilkyResultCode"] = statusCode.ToString();
        }
        
    }
}