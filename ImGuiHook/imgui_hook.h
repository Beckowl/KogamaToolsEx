#pragma once
#include <dxgi.h>

struct ImGuiContext;

typedef void(__stdcall* DrawCallback)();
typedef void(__stdcall* ReadyCallback)();

#ifdef IMGUIHOOK_EXPORTS
#define EXPORT __declspec(dllexport)
#else
#define EXPORT
#endif

extern "C" {
    EXPORT void ImGuiHook_Start(ReadyCallback readyCb, DrawCallback drawCb);
    EXPORT void ImGuiHook_Shutdown();
}