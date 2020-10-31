﻿using System;
using System.Reflection;
using SWLOR.Game.Server.Service.Legacy;
using SWLOR.Game.Server.ValueObject;

namespace SWLOR.Game.Server.Event.Legacy
{
    public static class ScriptItemEvent
    {
        public static void Run(string script)
        {
            if (!script.StartsWith("Item."))
            {
                script = "Item." + script;
            }

            var rootNamespace = Assembly.GetExecutingAssembly().GetName().Name;
            var scriptNamespace = rootNamespace + ".Scripts." + script;

            // Check the script cache first. If it exists, we run it.
            if (ScriptService.IsScriptRegisteredByNamespace(scriptNamespace))
            {
                ScriptService.RunScriptByNamespace(scriptNamespace);
            }
            else
            {
                Console.WriteLine("Unable to locate item script: " + script);
            }
        }
    }
}
