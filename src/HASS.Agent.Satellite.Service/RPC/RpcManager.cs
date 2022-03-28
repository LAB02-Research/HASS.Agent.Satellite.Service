﻿using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using GrpcDotNetNamedPipes;
using Serilog;

namespace HASS.Agent.Satellite.Service.RPC
{
    internal static class RpcManager
    {
        /// <summary>
        /// Initializes the RPC server and starts listening
        /// </summary>
        internal static void Initialize()
        {
            try
            {
                // prepare security descriptors
                var pipeSecurity = new PipeSecurity();
                var usersSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                var systemSid = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
                pipeSecurity.AddAccessRule(new PipeAccessRule(usersSid, PipeAccessRights.ReadWrite, AccessControlType.Allow));
                pipeSecurity.AddAccessRule(new PipeAccessRule(systemSid, PipeAccessRights.FullControl, AccessControlType.Allow));
                
                // prepare server options
                var serverOptions = new NamedPipeServerOptions
                {
                    CurrentUserOnly = false,
                    PipeSecurity = pipeSecurity
                };

                // create a new instance
                Variables.RpcServer = new NamedPipeServer(Variables.RpcPipeName, serverOptions);

                // bind our service
                HassAgentSatelliteRpcCalls.BindService(Variables.RpcServer.ServiceBinder, new HassAgentSatelliteRpcCallsService());
                
                // start listening
                Variables.RpcServer.Start();

                // done
                Log.Information("[RPCMANAGER] Listening");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[RPCMANAGER] Error while initializing: {err}", ex.Message);
            }
        }
    }
}
