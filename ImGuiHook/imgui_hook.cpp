#include <d3d11.h>
#include <dxgi.h>
#include <imgui.h>
#include <imgui_impl_dx11.h>
#include <imgui_impl_win32.h>

#include "imgui_hook.h"
#include "kiero.h"

#define STB_IMAGE_IMPLEMENTATION
#include "stb_image.h"

extern LRESULT ImGui_ImplWin32_WndProcHandler(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);

typedef HRESULT(__stdcall* Present)(IDXGISwapChain*, UINT, UINT);
typedef HRESULT(__stdcall* ResizeBuffers)(IDXGISwapChain*, UINT, UINT, UINT, DXGI_FORMAT, UINT);

#define RELEASE(p) do { if (p) { (p)->Release(); (p) = NULL; } } while(0)

static ID3D11Device* pDevice = NULL;
static ID3D11DeviceContext* pContext = NULL;
static ID3D11RenderTargetView* mainRTV = NULL;

static Present oPresent = NULL;
static ResizeBuffers oResizeBuffers = NULL;

static HWND window = NULL;
static WNDPROC oWndProc = NULL;

static ReadyCallback readyCallback = NULL;
static DrawCallback drawCallback = NULL;

static bool initialized = false;

static LRESULT __stdcall WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    if (ImGui::GetCurrentContext() && ImGui_ImplWin32_WndProcHandler(hWnd, msg, wParam, lParam))
        return true;

    const ImGuiIO& io = ImGui::GetIO();

    // block mouse input (so it doesn't go through the ImGui window)
    if (io.WantCaptureMouse)
    {
        switch (msg)
        {
        case WM_LBUTTONDOWN: case WM_LBUTTONUP:
        case WM_RBUTTONDOWN: case WM_RBUTTONUP:
        case WM_MBUTTONDOWN: case WM_MBUTTONUP:
        case WM_XBUTTONDOWN: case WM_XBUTTONUP:
            return 0;
        }
    }

    // block keyboard if typing
    if (io.WantCaptureKeyboard || io.WantTextInput)
    {
        switch (msg)
        {
        case WM_KEYDOWN: case WM_KEYUP:
        case WM_SYSKEYDOWN: case WM_SYSKEYUP:
        case WM_CHAR:
            return 0;
        }
    }

    return CallWindowProc(oWndProc, hWnd, msg, wParam, lParam);
}

static ID3D11RenderTargetView* CreateRTV(IDXGISwapChain* pSwapChain)
{
    ID3D11Texture2D* pBackBuffer = NULL;
    ID3D11RenderTargetView* rtv = NULL;

    if (FAILED(pSwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), (void**)&pBackBuffer)))
        return NULL;

    pDevice->CreateRenderTargetView(pBackBuffer, NULL, &rtv);
    pBackBuffer->Release();

    return rtv;
}

static void Initialize(IDXGISwapChain* pSwapChain)
{
    if (FAILED(pSwapChain->GetDevice(__uuidof(ID3D11Device), (void**)&pDevice)))
        return;

    pDevice->GetImmediateContext(&pContext);

    DXGI_SWAP_CHAIN_DESC sd;
    pSwapChain->GetDesc(&sd);
    window = sd.OutputWindow;

    mainRTV = CreateRTV(pSwapChain);
    oWndProc = (WNDPROC)SetWindowLongPtr(window, GWLP_WNDPROC, (LONG_PTR)WndProc);

    ImGui::CreateContext();
    ImGuiIO& io = ImGui::GetIO();

    io.ConfigFlags |= ImGuiConfigFlags_NoMouseCursorChange;

    ImGui::StyleColorsDark();

    ImGui_ImplWin32_Init(window);
    ImGui_ImplDX11_Init(pDevice, pContext);

    initialized = true;

    if (readyCallback != NULL)
        readyCallback();
}

static void RenderFrame()
{
    if (!pContext || !mainRTV)
        return;

    ImGui_ImplDX11_NewFrame();
    ImGui_ImplWin32_NewFrame();
    ImGui::NewFrame();

    if (drawCallback != NULL)
        drawCallback();

    ImGui::Render();
    pContext->OMSetRenderTargets(1, &mainRTV, NULL);
    ImGui_ImplDX11_RenderDrawData(ImGui::GetDrawData());
}

static HRESULT __stdcall HookPresent(IDXGISwapChain* pSwapChain, UINT syncInterval, UINT flags)
{
    if (!initialized)
        Initialize(pSwapChain);

    RenderFrame();
            
    return oPresent(pSwapChain, syncInterval, flags);
}

static HRESULT __stdcall HookResizeBuffers(IDXGISwapChain* pSwapChain, UINT bufferCount, UINT width, UINT height, DXGI_FORMAT newFormat, UINT swapChainFlags)
{
    if (pContext)
        pContext->OMSetRenderTargets(0, NULL, NULL);

    RELEASE(mainRTV);

    HRESULT hr = oResizeBuffers(pSwapChain, bufferCount, width, height, newFormat, swapChainFlags);

    if (SUCCEEDED(hr) && width > 0 && height > 0)
    {
        mainRTV = CreateRTV(pSwapChain);

        if (mainRTV == NULL)
            return hr;

        pContext->OMSetRenderTargets(1, &mainRTV, NULL);

        D3D11_VIEWPORT vp = { 0, 0, (float)width, (float)height, 0, 1 };
        pContext->RSSetViewports(1, &vp);
    }

    return hr;
}

void ImGuiHook_Start(ReadyCallback readyCb, DrawCallback drawCb)
{
    drawCallback = drawCb;
    readyCallback = readyCb;

    while (kiero::init(kiero::RenderType::D3D11) != kiero::Status::Success)
        Sleep(1);

    kiero::bind(8, (void**)&oPresent, HookPresent);
    kiero::bind(13, (void**)&oResizeBuffers, HookResizeBuffers);
}

void ImGuiHook_Shutdown()
{
    if (!initialized)
        return;

    if (oWndProc)
        SetWindowLongPtr(window, GWLP_WNDPROC, (LONG_PTR)oWndProc);

    ImGui_ImplDX11_Shutdown();
    ImGui_ImplWin32_Shutdown();
    ImGui::DestroyContext();

    RELEASE(mainRTV);
    RELEASE(pContext);
    RELEASE(pDevice);

    kiero::unbind(8);
    kiero::unbind(13);
    kiero::shutdown();

    initialized = false;
}

ImGuiTexture* ImGuiHook_TexFromMemory(const void* mem, int width, int height)
{
    if (!pDevice || !mem)
        return NULL;

    D3D11_TEXTURE2D_DESC desc = {};
    desc.Width = (UINT)width;
    desc.Height = (UINT)height;
    desc.MipLevels = 1;
    desc.ArraySize = 1;
    desc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    desc.SampleDesc.Count = 1;
    desc.Usage = D3D11_USAGE_DEFAULT;
    desc.BindFlags = D3D11_BIND_SHADER_RESOURCE;

    D3D11_SUBRESOURCE_DATA data = {};
    data.pSysMem = mem;
    data.SysMemPitch = (UINT)(width * 4);

    ID3D11Texture2D* pTex = NULL;

    if (FAILED(pDevice->CreateTexture2D(&desc, &data, &pTex)))
        return NULL;

    D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc = {};
    srvDesc.Format = desc.Format;
    srvDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D;
    srvDesc.Texture2D.MipLevels = 1;
    srvDesc.Texture2D.MostDetailedMip = 0;

    ID3D11ShaderResourceView* srv = NULL;
    HRESULT hr = pDevice->CreateShaderResourceView(pTex, &srvDesc, &srv);
    pTex->Release();

    if (FAILED(hr))
        return NULL;

    return srv;
}

ImGuiTexture* ImGuiHook_TexFromFile(const char* path)
{
    int width, height, channels;
    unsigned char* data = stbi_load(path, &width, &height, &channels, 4);

    if (!data)
        return NULL;

    ImGuiTexture* tex = ImGuiHook_TexFromMemory(data, width, height);
    stbi_image_free(data);

    return tex;
}

void ImGuiHook_FreeTexture(ImGuiTexture* tex) {
    if (tex)
        tex->Release();
}