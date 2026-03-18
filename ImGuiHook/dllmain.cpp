#include <windows.h>
#include "imgui_hook.h"

BOOL APIENTRY DllMain(HINSTANCE hInstance, DWORD fdwReason, LPVOID)
{
    DisableThreadLibraryCalls(hInstance);

    if (fdwReason == DLL_PROCESS_ATTACH)
        CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)ImGuiHook_Start, NULL, 0, NULL);

    return TRUE;
}