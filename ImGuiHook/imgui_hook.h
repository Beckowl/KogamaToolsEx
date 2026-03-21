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

typedef ID3D11ShaderResourceView ImGuiTexture;

extern "C" {
    EXPORT void ImGuiHook_Start(ReadyCallback readyCb, DrawCallback drawCb);
    EXPORT void ImGuiHook_Shutdown();

    EXPORT ImGuiTexture* ImGuiHook_TexFromMemory(const void* mem, int width, int height);
    EXPORT ImGuiTexture* ImGuiHook_TexFromFile(const char* path);
    EXPORT void ImGuiHook_FreeTexture(ImGuiTexture* tex);
}