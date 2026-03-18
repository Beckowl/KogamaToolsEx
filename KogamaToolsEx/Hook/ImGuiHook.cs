using BepInEx;
using System.Runtime.InteropServices;

namespace KogamaToolsEx.Hook
{
    internal static class ImGuiHook
    {
        public static event Action OnRender;
        public static event Action OnInitialized;
        public static event Action OnDestroy;

        private static IntPtr moduleHandle = IntPtr.Zero;

        private static DrawCallbackDelegate drawDelegate;
        private static InitedCallbackDelegate initDelegate;

        delegate void DrawCallbackDelegate();
        delegate void InitedCallbackDelegate(IntPtr context);

        public static void Initialize()
        {
            if (moduleHandle != IntPtr.Zero)
            {
                KogamaTools.Logger.LogWarning("ImGuiHook already initialized");
                return;
            }

            string path = Path.Combine(Paths.PluginPath, "ImGuiHook.dll");

            try
            {
                moduleHandle = NativeLibrary.Load(path);
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
            drawDelegate = RenderCallback;

            ImGuiHook_RegisterReadyCallback(Marshal.GetFunctionPointerForDelegate(initDelegate));
            ImGuiHook_RegisterDrawCallback(Marshal.GetFunctionPointerForDelegate(drawDelegate));
        }

        public static void Shutdown()
        {
            if (moduleHandle != IntPtr.Zero)
            {
                OnDestroy?.Invoke();
                ImGuiHook_Deinit();

                NativeLibrary.Free(moduleHandle);
                moduleHandle = IntPtr.Zero;
            }
        }

        private static void OnImGuiReady(IntPtr context)
        {
            KogamaTools.Logger.LogInfo($"ImGui ready, context ptr: {context}");
            ImGuiNET.ImGui.SetCurrentContext(context);

            OnInitialized?.Invoke();
        }

        private static void RenderCallback()
        {
            OnRender?.Invoke();
        }

        [DllImport("ImGuiHook.dll")]
        private static extern void ImGuiHook_RegisterDrawCallback(IntPtr callback);

        [DllImport("ImGuiHook.dll")]
        private static extern void ImGuiHook_RegisterReadyCallback(IntPtr callback);

        [DllImport("ImGuiHook.dll")]
        private static extern void ImGuiHook_Deinit();
    }
}