using HarmonyLib;
using MV.WorldObject.SpawnRoles;
using System.Reflection;

namespace KogamaToolsEx.Plugin.Misc
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
        private static bool initialzed = false;

        // todo find something better to hook into, this is shit
        // none of the init methods worked
        [HarmonyPatch(typeof(SpawnRolesRuntimeData), "ToString")]
        [HarmonyPostfix]
        private static void Initialize_Postfix()
        {
            if (initialzed)
                return;

            KogamaTools.Logger.LogInfo("Invoking init methods");
            InvokeInitMethods();

            initialzed = true;
        }

        private static void InvokeInitMethods()
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

                    KogamaTools.Logger.LogInfo($"Invoking {method.Name}");

                    method.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    KogamaTools.Logger.LogError($"Failed to invoke {method.Name}: {ex.Message}");
                }
            }
        }
    }
}
