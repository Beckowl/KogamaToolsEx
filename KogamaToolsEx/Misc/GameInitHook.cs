using HarmonyLib;
using KogamaToolsX;
using MV.WorldObject.SpawnRoles;
using System.Reflection;

namespace KogamaToolsEx.Misc
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal class InvokeOnInitAttribute : Attribute
    {
        public int Priority { get; }

        public InvokeOnInitAttribute(int priority = int.MaxValue)
        {
            Priority = priority;
        }
    }


    [HarmonyPatch]
    internal static class GameInitHook
    {
        // todo find something better to hook into this is shit
        [HarmonyPatch(typeof(SpawnRolesRuntimeData), "ToString")]
        [HarmonyPostfix]
        private static void Initialize_Postfix()
        {
            Plugin.Logger.LogInfo("Invoking init methods");
            InvokeMethods();
        }

        private static void InvokeMethods()
        {
            var methods = Assembly.GetExecutingAssembly()
               .GetTypes()
               .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
               .Select(m => (Method: m, Attr: m.GetCustomAttribute<InvokeOnInitAttribute>()))
               .Where(x => x.Attr != null)
               .OrderBy(x => x.Attr!.Priority)
               .ToList();

            foreach (var (method, _) in methods)
            {
                try
                {
                    Plugin.Logger.LogInfo($"Invoking {method.Name}");

                    method.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError($"Failed to invoke {method.Name}: {ex.Message}");
                }
            }
        }
    }
}
