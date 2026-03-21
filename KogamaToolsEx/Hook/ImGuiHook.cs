using System.Reflection;
using System.Runtime.InteropServices;

namespace KogamaToolsEx.Hook
{
    internal static class ImGuiHook
    {
        public static event Action OnRender;
        public static event Action OnInitialized;
        public static event Action OnDestroy;

        private static IntPtr moduleHandle = IntPtr.Zero;

        delegate void DrawCallback();
        delegate void ReadyCallback();

        private static DrawCallback drawDelegate;
        private static ReadyCallback initDelegate;

        public static void Initialize()
        {
            if (moduleHandle != IntPtr.Zero)
            {
                KogamaTools.Logger.LogWarning("ImGuiHook already initialized");
                return;
            }

            try
            {
                var cd = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
  
                moduleHandle = NativeLibrary.Load(Path.Combine(cd, "ImGuiHook.dll"));
                KogamaTools.Logger.LogInfo($"ImGui module handle: {moduleHandle}");
            }
            catch (Exception ex)
            {
                KogamaTools.Logger.LogError($"Failed to load hook DLL: {ex.Message}");
                return;
            }

            // force imguiNET's p/invokes to resolve to the hook DLL instead of the original cimgui.dll
            // game will crash if i don't do this
            NativeLibrary.SetDllImportResolver(typeof(ImGuiNET.ImGui).Assembly, (name, assembly, path) =>
            {
                if (name == "cimgui")
                    return moduleHandle;

                return IntPtr.Zero;
            });

            initDelegate = OnImGuiReady;
            drawDelegate = OnDrawCallback;

            Task.Run(() => { ImGuiHook_Start(Marshal.GetFunctionPointerForDelegate(initDelegate), Marshal.GetFunctionPointerForDelegate(drawDelegate)); } );
        }

        public static void Shutdown()
        {
            if (moduleHandle != IntPtr.Zero)
            {
                OnDestroy?.Invoke();
                ImGuiHook_Shutdown();

                NativeLibrary.Free(moduleHandle);
                moduleHandle = IntPtr.Zero;
            }
        }

        private static void OnImGuiReady()
        {
            KogamaTools.Logger.LogInfo($"ImGui ready");
            OnInitialized?.Invoke();
        }

        private static void OnDrawCallback()
        {
            OnRender?.Invoke();
        }

        [DllImport("ImGuiHook.dll")]
        private static extern void ImGuiHook_Start(IntPtr readyCb, IntPtr drawCb);

        [DllImport("ImGuiHook.dll")]
        private static extern void ImGuiHook_Shutdown();
    }
}