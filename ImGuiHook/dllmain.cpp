#include <d3d11.h>
#include <dxgi.h>
#include <imgui.h>
#include <imgui_impl_dx11.h>
#include <imgui_impl_win32.h>

#include "kiero.h"
#include "bridge.h"

#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "dxgi.lib")

extern LRESULT ImGui_ImplWin32_WndProcHandler(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);

typedef HRESULT(__stdcall* Present)(IDXGISwapChain*, UINT, UINT);
typedef HRESULT(__stdcall* ResizeBuffers)(IDXGISwapChain*, UINT, UINT, UINT, DXGI_FORMAT, UINT);

static Present oPresent = NULL;
static ResizeBuffers oResizeBuffers = NULL;

static ID3D11Device* pDevice = NULL;
static ID3D11DeviceContext* pContext = NULL;
static ID3D11RenderTargetView* mainRTV = NULL;
static HWND window = NULL;
static WNDPROC oWndProc = NULL;
static bool init = false;

LRESULT __stdcall WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    if (ImGui::GetCurrentContext() && ImGui_ImplWin32_WndProcHandler(hWnd, msg, wParam, lParam))
        return true;

    ImGuiIO& io = ImGui::GetIO();

    if (io.WantCaptureMouse) {
        switch (msg) {
        case WM_LBUTTONDOWN: case WM_LBUTTONUP: case WM_RBUTTONDOWN: case WM_RBUTTONUP:
        case WM_MBUTTONDOWN: case WM_MBUTTONUP: case WM_MOUSEMOVE:
        case WM_MOUSEWHEEL: case WM_MOUSEHWHEEL:
            return 0;
        }
    }

    return CallWindowProc(oWndProc, hWnd, msg, wParam, lParam);
}

HRESULT __stdcall hkResizeBuffers(IDXGISwapChain* pSwapChain, UINT BufferCount, UINT Width, UINT Height, DXGI_FORMAT NewFormat, UINT SwapChainFlags)
{
    if (mainRTV) {
        pContext->OMSetRenderTargets(0, NULL, NULL);
        mainRTV->Release();
        mainRTV = NULL;
    }

    HRESULT hr = oResizeBuffers(pSwapChain, BufferCount, Width, Height, NewFormat, SwapChainFlags);

    if (SUCCEEDED(hr) && Width > 0 && Height > 0) {
        ID3D11Texture2D* pBuffer = NULL;
        pSwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), (void**)&pBuffer);
        pDevice->CreateRenderTargetView(pBuffer, NULL, &mainRTV);
        pBuffer->Release();
        pContext->OMSetRenderTargets(1, &mainRTV, NULL);
        D3D11_VIEWPORT vp = { 0, 0, (float)Width, (float)Height, 0, 1 };
        pContext->RSSetViewports(1, &vp);
    }

    return hr;
}

HRESULT __stdcall hkPresent(IDXGISwapChain* pSwapChain, UINT SyncInterval, UINT Flags)
{
    if (!init)
    {
        if (FAILED(pSwapChain->GetDevice(__uuidof(ID3D11Device), (void**)&pDevice)))
            return oPresent(pSwapChain, SyncInterval, Flags);

        pDevice->GetImmediateContext(&pContext);

        DXGI_SWAP_CHAIN_DESC sd;
        pSwapChain->GetDesc(&sd);
        window = sd.OutputWindow;

        ID3D11Texture2D* pBackBuffer = NULL;
        pSwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), (void**)&pBackBuffer);
        pDevice->CreateRenderTargetView(pBackBuffer, NULL, &mainRTV);
        pBackBuffer->Release();

        oWndProc = (WNDPROC)SetWindowLongPtr(window, GWLP_WNDPROC, (LONG_PTR)WndProc);

        ImGui::CreateContext();
        ImGuiIO& io = ImGui::GetIO();
        io.ConfigFlags |= ImGuiConfigFlags_NoMouseCursorChange;
        ImGui::StyleColorsDark();

        ImGui_ImplWin32_Init(window);
        ImGui_ImplDX11_Init(pDevice, pContext);

        init = true;

        Bridge_SetReady();
    }

    ImGui_ImplDX11_NewFrame();
    ImGui_ImplWin32_NewFrame();
    ImGui::NewFrame();

    Bridge_InvokeDrawCallback();

    ImGui::Render();
    pContext->OMSetRenderTargets(1, &mainRTV, NULL);
    ImGui_ImplDX11_RenderDrawData(ImGui::GetDrawData());

    return oPresent(pSwapChain, SyncInterval, Flags);
}

int kieroThread()
{
    while (kiero::init(kiero::RenderType::D3D11) != kiero::Status::Success);
    kiero::bind(8, (void**)&oPresent, hkPresent);
    kiero::bind(13, (void**)&oResizeBuffers, hkResizeBuffers);
    return 0;
}

BOOL APIENTRY DllMain(HINSTANCE hInstance, DWORD fdwReason, LPVOID)
{
    DisableThreadLibraryCalls(hInstance);
    if (fdwReason == DLL_PROCESS_ATTACH)
        CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)kieroThread, NULL, 0, NULL);

    return TRUE;
}