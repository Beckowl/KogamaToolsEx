#include <imgui.h>

typedef void(__stdcall* DrawCallback)();
typedef void(__stdcall* OnInitedCallback)(ImGuiContext*);

static DrawCallback g_drawCallback = nullptr;
static OnInitedCallback g_initedCallback = nullptr;
static bool g_imguiReady = false;

extern "C" {
    __declspec(dllexport) void RegisterDrawCallback(DrawCallback cb) {
        g_drawCallback = cb;
    }

    __declspec(dllexport) void RegisterInitedCallback(OnInitedCallback cb) {
        g_initedCallback = cb;
        if (g_imguiReady)
            cb(ImGui::GetCurrentContext());
    }
}

void Bridge_SetReady() {
    g_imguiReady = true;
    if (g_initedCallback)
        g_initedCallback(ImGui::GetCurrentContext());
}

void Bridge_InvokeDrawCallback() {
    if (g_drawCallback)
        g_drawCallback();
}