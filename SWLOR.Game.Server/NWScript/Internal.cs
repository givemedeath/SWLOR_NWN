using System;
using System.Collections.Generic;

namespace SWLOR.Game.Server.NWScript
{
    partial class Internal
    {
        public const uint OBJECT_INVALID = 0x7F000000;
        public static NWGameObject OBJECT_SELF { get; private set; } = OBJECT_INVALID;
        public static event EventHandler<MainLoopEventArgs> OnMainLoopEvent;
        private static readonly ScriptDispatcher _scriptDispatcher = new ScriptDispatcher();

        public static void OnMainLoop(ulong frame)
        {
            try
            {
                OnMainLoopEvent?.Invoke(null, new MainLoopEventArgs(frame));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private struct ScriptContext
        {
            public NWGameObject OwnerObject;
            public string ScriptName;
        }
        private static readonly Stack<ScriptContext> ScriptContexts = new Stack<ScriptContext>();
        public static int OnRunScript(string script, uint oidSelf)
        {
            int ret = 0;
            OBJECT_SELF = oidSelf;
            ScriptContexts.Push(new ScriptContext { OwnerObject = oidSelf, ScriptName = script });
            try
            {
                ret = _scriptDispatcher.RunScript(script);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            ScriptContexts.Pop();
            OBJECT_SELF = ScriptContexts.Count == 0 ? OBJECT_INVALID : ScriptContexts.Peek().OwnerObject;
            return ret;
        }

        private struct Closure
        {
            public NWGameObject OwnerObject;
            public ActionDelegate Run;
        }
        private static ulong NextEventId = 0;
        private static readonly Dictionary<ulong, Closure> Closures = new Dictionary<ulong, Closure>();

        public static void OnClosure(ulong eid, uint oidSelf)
        {
            NWGameObject old = OBJECT_SELF;
            OBJECT_SELF = oidSelf;
            try
            {
                Closures[eid].Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Closures.Remove(eid);
            OBJECT_SELF = old;
        }

        public static void ClosureAssignCommand(uint obj, ActionDelegate func)
        {
            if (NativeFunctions.ClosureAssignCommand(obj, NextEventId) != 0)
            {
                Closures.Add(NextEventId++, new Closure { OwnerObject = obj, Run = func });
            }
        }

        public static void ClosureDelayCommand(uint obj, float duration, ActionDelegate func)
        {
            if (NativeFunctions.ClosureDelayCommand(obj, duration, NextEventId) != 0)
            {
                Closures.Add(NextEventId++, new Closure { OwnerObject = obj, Run = func });
            }
        }

        public static void ClosureActionDoCommand(uint obj, ActionDelegate func)
        {
            if (NativeFunctions.ClosureActionDoCommand(obj, NextEventId) != 0)
            {
                Closures.Add(NextEventId++, new Closure { OwnerObject = obj, Run = func });
            }
        }
    }
}
