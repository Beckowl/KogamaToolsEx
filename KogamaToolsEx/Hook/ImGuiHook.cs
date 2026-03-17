using BepInEx;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace KogamaToolsEx.Hook
{
    internal class ImGuiHook : IDisposable
    {
        public event Action OnRender;

        private bool injected = false;
        private IntPtr moduleHandle = IntPtr.Zero;

        private static DrawCallbackDelegate drawDelegate;
        private static InitedCallbackDelegate initDelegate;

        delegate void DrawCallbackDelegate();
        delegate void InitedCallbackDelegate(IntPtr context);

        public void Initialize()
        {
            Inject();
            RedirectSymbols();
            SetupCallbacks();
        }

        private void Inject()
        {
            if (injected)
                return;

            KogamaTools.Logger.LogInfo("Injecting Dear ImGui hook...");

            string path = Path.Combine(Paths.PluginPath, "ImGuiHook.dll");
            IntPtr handle = LoadLibraryW(path);

            KogamaTools.Logger.LogInfo($"Handle: {handle}, Path: {path}");

            if (handle == IntPtr.Zero)
            {
                string msg = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                KogamaTools.Logger.LogError($"Failed to load hook DLL, error: {msg}");

                return;
            }

            injected = true;
            moduleHandle = handle;
        }

        private void RedirectSymbols()
        {
            NativeLibrary.SetDllImportResolver(typeof(ImGuiNET.ImGui).Assembly, (name, assembly, path) => {
                if (name == "cimgui")
                    return NativeLibrary.Load("ImGuiHook.dll");

                return IntPtr.Zero;
            });
        }

        private void SetupCallbacks()
        {
            if (!injected) 
                return;

            initDelegate = OnImGuiReady;
            drawDelegate = RenderCallback;

            RegisterInitedCallback(Marshal.GetFunctionPointerForDelegate(initDelegate));
            RegisterDrawCallback(Marshal.GetFunctionPointerForDelegate(drawDelegate));
        }

        private void OnImGuiReady(IntPtr context)
        {
            KogamaTools.Logger.LogInfo($"ImGui ready, context: {context}");
            ImGuiNET.ImGui.SetCurrentContext(context);
        }

        private void RenderCallback()
        {
            OnRender?.Invoke();
        }

        public void Dispose()
        {
            if (moduleHandle != IntPtr.Zero)
            {
                FreeLibrary(moduleHandle);
                moduleHandle = IntPtr.Zero;
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibraryW(string path);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("ImGuiHook.dll")]
        private static extern void RegisterDrawCallback(IntPtr callback);

        [DllImport("ImGuiHook.dll")]
        private static extern void RegisterInitedCallback(IntPtr callback);
    }
}