using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Logging;
using Silky.Core.MethodExecutor;
using Silky.Core.Rpc;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServerFallbackHandler : IServerFallbackHandler
    {
        public ILogger<DefaultServerFallbackHandler> Logger { get; set; }
        private readonly IServerDiagnosticListener _serverDiagnosticListener;

        public DefaultServerFallbackHandler(IServerDiagnosticListener serverDiagnosticListener)
        {
            _serverDiagnosticListener = serverDiagnosticListener;
            Logger = NullLogger<DefaultServerFallbackHandler>.Instance;
        }

        public async Task<RemoteResultMessage> Handle(RemoteInvokeMessage message, Context ctx,
            CancellationToken cancellationToken)
        {
            var tracingTimestamp = ctx[PollyContextNames.TracingTimestamp]?.To<long>();
            var remoteResultMessage = new RemoteResultMessage()
            {
                ServiceEntryId = message.ServiceEntryId,
                StatusCode = StatusCode.Success,
            };

            var exception = ctx[PollyContextNames.Exception] as Exception;
            Check.NotNull(exception, nameof(exception));
            _serverDiagnosticListener.TracingError(tracingTimestamp, RpcContext.Context.GetMessageId(),
                message.ServiceEntryId, exception.GetExceptionStatusCode(), exception);
            if (exception is RpcAuthenticationException || exception is NotFindLocalServiceEntryException)
            {
                remoteResultMessage.StatusCode = exception.GetExceptionStatusCode();
                remoteResultMessage.ExceptionMessage = exception.Message;
                return remoteResultMessage;
            }

            var serviceEntry = ctx[PollyContextNames.ServiceEntry] as ServiceEntry;
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            if (serviceEntry.FallbackMethodExecutor != null && serviceEntry.FallbackProvider != null)
            {
                object instance = EngineContext.Current.Resolve(serviceEntry.FallbackProvider.Type);
                if (instance == null)
                {
                    remoteResultMessage.StatusCode = StatusCode.NotFindFallbackInstance;
                    remoteResultMessage.ExceptionMessage =
                        $"Failed to instantiate the instance of the fallback service;{Environment.NewLine}" +
                        $"Type:{serviceEntry.FallbackProvider.Type.FullName}";
                    return remoteResultMessage;
                }

                object result = null;
                try
                {
                    var parameters = serviceEntry.ConvertParameters(message.Parameters);
                    result = await serviceEntry.FallbackMethodExecutor.ExecuteMethodWithDbContextAsync(instance,
                        parameters);
                    remoteResultMessage.StatusCode = StatusCode.Success;
                    remoteResultMessage.Result = result;
                    return remoteResultMessage;
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    remoteResultMessage.StatusCode = ex.GetExceptionStatusCode();
                    remoteResultMessage.ExceptionMessage = ex.GetExceptionMessage();
                    return remoteResultMessage;
                }
            }

            remoteResultMessage.StatusCode = exception.GetExceptionStatusCode();
            remoteResultMessage.ExceptionMessage = exception.GetExceptionMessage();

            return remoteResultMessage;
        }
    }
}