﻿using System;
using System.Diagnostics;
using NWN;
using SWLOR.Game.Server.Data.Entity;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.Extension;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service.Contracts;
using SWLOR.Game.Server.ValueObject;

namespace SWLOR.Game.Server.Service
{
    public class ErrorService: IErrorService
    {
        private readonly INWScript _;
        private readonly IDataService _data;

        public ErrorService(INWScript script, IDataService data)
        {
            _ = script;
            _data = data;
        }

        public void LogError(Exception ex, string @event = "")
        {
            string stackTrace = ex.ToMessageAndCompleteStacktrace();

            stackTrace = "*****************" + Environment.NewLine +
                      "EVENT ERROR (C#)" + Environment.NewLine +
                      (string.IsNullOrWhiteSpace(@event) ? string.Empty : @event + Environment.NewLine) +
                      "*****************" + Environment.NewLine +
                      " EXCEPTION:" + Environment.NewLine + Environment.NewLine + stackTrace;
            Console.WriteLine(stackTrace);

            Error log = new Error
            {
                DateCreated = DateTime.UtcNow, 
                Caller = @event, 
                Message = ex.Message,
                StackTrace = stackTrace
            };
            DatabaseAction action = new DatabaseAction(log, DatabaseActionType.Insert);
            // Bypass the caching logic and directly enqueue the insert.
            _data.DataQueue.Enqueue(action);
        }

        public void Trace(TraceComponent component, string log)
        {
            // Check the global environment variable named "DEBUGGING_ENABLED" to see if it's set.
            string env = Environment.GetEnvironmentVariable("DEBUGGING_ENABLED");
            bool isDebuggingEnabled = env == "y" || env == "true" || env == "yes";
            
            if (!isDebuggingEnabled)
            {
                // Trace disabled globally.
                return;
            }

            // Trace components can be individually enabled or disabled.
            // Based on the capitalized enumeration name, check to see if that environment variable is enabled.
            // If the trace isn't attached to a specific component, it'll be displayed every time (so long as the global setting is on)
            bool componentEnabled = true;
            if(component != TraceComponent.None)
            {
                string componentName = Enum.GetName(typeof(TraceComponent), component)?.ToUpper();
                env = Environment.GetEnvironmentVariable("DEBUGGING_COMPONENT_ENABLED_" + componentName);
                componentEnabled = env == "y" || env == "true" || env == "yes";
            }
            
            // If the component is enabled, output the trace.
            if (componentEnabled)
            {
                string componentName = component == TraceComponent.None ? string.Empty : component.ToString();
                Console.WriteLine(componentName + " -- " + log);
            }
        }
    }
}
