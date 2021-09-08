using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TransferObjects;

namespace OSRSWalkThroughListener
{
    public class ConnectionHandler
    {
        Services _services;
        AssemblyName _transferAssembly;

        public ConnectionHandler()
        {
            _services = new();

            foreach(AssemblyName assembly in typeof(ConnectionHandler).Assembly.GetReferencedAssemblies())
            {
                if (assembly.FullName.StartsWith("TransferObject"))
                {
                    _transferAssembly = assembly;
                }
            }
        }

        public object HandleRequest(string request)
        {
            ServiceCallInfo info = ParseRequest(request);

            if (info == null) return GetErrorResult();

            return SendRequest(info);
        }

        private ServiceCallInfo ParseRequest(string request)
        {
            string[] parts = request.Split("|");

            if (parts.Length != 2) return null;

            string dtoTypeString = parts[0];
            string parameter = parts[1];

            Type dtoType = Type.GetType("TransferObjects." + dtoTypeString + ", " + _transferAssembly.FullName);

            MethodInfo servicetoCall = _services.GetType().GetMethod("ExecuteService", new Type[] { dtoType });

            return new ServiceCallInfo
            {
                MethodInfo = servicetoCall,
                Parameters = JsonSerializer.Deserialize(parameter, dtoType)
            };
        }

        private object SendRequest(ServiceCallInfo scInfo)
        {
            object result = scInfo.MethodInfo.Invoke(_services, new object[] { scInfo.Parameters });

            return result;
        }

        private ServiceResult GetErrorResult()
        {
            return new ServiceResult()
            {
                Result = false,
                Data = "An error occured"
            };
        }
    }

    public class ServiceCallInfo
    {
        public MethodInfo MethodInfo { get; set; }
        public object Parameters { get; set; }
    }
}
