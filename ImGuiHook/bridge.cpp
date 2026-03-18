#include <imgui.h>

#include "imgui_hook.h"

typedef void(__stdcall* DrawCallback)();
typedef void(__stdcall* OnInitedCallback)(ImGuiContext*);

static DrawCallback drawCallback = nullptr;
static OnInitedCallback readyCallback = nullptr;
static bool ready = false;

extern "C" {
    __declspec(dllexport) void ImGuiHook_RegisterDrawCallback(DrawCallback cb) {
        drawCallback = cb;
    }

    __declspec(dllexport) void ImGuiHook_RegisterReadyCallback(OnInitedCallback cb) {
        readyCallback = cb;

        if (ready)
            cb(ImGui::GetCurrentContext());
    }

    __declspec(dllexport) void ImGuiHook_Deinit() {
        ImGuiHook_Shutdown();
    }
}

void Bridge_SetReady() {
    ready = true;

    if (readyCallback)
        readyCallback(ImGui::GetCurrentContext());
}

void Bridge_InvokeDrawCallback() {
    if (drawCallback)
        drawCallback();
}
