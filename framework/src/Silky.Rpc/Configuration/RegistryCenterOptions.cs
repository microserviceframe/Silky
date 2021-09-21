﻿namespace Silky.Rpc.Configuration
{
    public class RegistryCenterOptions
    {
        internal static string RegistryCenter = "RegistryCenter";

        public RegistryCenterType RegistryCenterType { get; set; } = RegistryCenterType.Zookeeper;

        public double ConnectionTimeout { get; set; } = 50000;

        public double SessionTimeout { get; set; } = 80000;

        public double OperatingTimeout { get; set; } = 100000;

        public string ConnectionStrings { get; set; }

        public int FuseTimes { get; set; } = 10;

        public string RoutePath { get; set; } = "/silky/server";
        

    }
}