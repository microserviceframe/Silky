using JetBrains.Annotations;

namespace Silky.Rpc.Configuration
{
    public class RpcOptions
    {
        internal static string Rpc = "Rpc";

        public RpcOptions()
        {
            Host = "0.0.0.0";
            Port = 2200;
            UseLibuv = true;
            IsSsl = false;
            SoBacklog = 8192;
            RemoveUnHealthServer = true;
            EnableHealthCheck = true;
            ConnectTimeout = 500;
            _healthCheckWatchInterval = 300;
        }

        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseLibuv { get; set; }
        public bool IsSsl { get; set; }
        public string SslCertificateName { get; set; }
        public string SslCertificatePassword { get; set; }
        public int SoBacklog { get; set; }
        public bool RemoveUnHealthServer { get; set; }

        public bool EnableHealthCheck { get; set; }
        [NotNull] public string Token { get; set; }
        public double ConnectTimeout { get; set; }

        private int _healthCheckWatchInterval;

        public int HealthCheckWatchInterval
        {
            get => _healthCheckWatchInterval;
            set => _healthCheckWatchInterval = value <= 60 ? 60 : value;
        }
    }
}