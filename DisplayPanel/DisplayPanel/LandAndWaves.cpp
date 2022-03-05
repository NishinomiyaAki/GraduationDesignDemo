#include "pch.h"
#include "D3DApp.h"
#include "MathHelper.h"
#include "UploadBuffer.h"
#include "GeometryGenerator.h"
#include "FrameResource.h"
#include "Waves.h"

#define DLL_API extern "C" __declspec(dllexport)

using Microsoft::WRL::ComPtr;
using namespace DirectX;
using namespace DirectX::PackedVector;

const int gNumFrameResources = 3;

// Lightweight structure stores parameters to draw a shape.  This will
// vary from app-to-app.
struct RenderItem
{
	RenderItem() = default;

	// World matrix of the shape that describes the object's local space
	// relative to the world space, which defines the position, orientation,
	// and scale of the object in the world.
	XMFLOAT4X4 World = MathHelper::Identity4x4();

	// Dirty flag indicating the object data has changed and we need to update the constant buffer.
	// Because we have an object cbuffer for each FrameResource, we have to apply the
	// update to each FrameResource.  Thus, when we modify obect data we should set
	// NumFramesDirty = gNumFrameResources so that each frame resource gets the update.
	int NumFramesDirty = gNumFrameResources;

	// Index into GPU constant buffer corresponding to the ObjectCB for this render item.
	UINT ObjCBIndex = -1;

	MeshGeometry* Geo = nullptr;

	// Primitive topology.
	D3D12_PRIMITIVE_TOPOLOGY PrimitiveType = D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST;

	// DrawIndexedInstanced parameters.
	UINT IndexCount = 0;
	UINT StartIndexLocation = 0;
	int BaseVertexLocation = 0;
};

enum class RenderLayer : int
{
	Opaque = 0,
	Count
};

class ChaseExampleApp : public D3DApp
{
public:
	static ChaseExampleApp* GetInstance();

	ChaseExampleApp(HINSTANCE hInstance);
	ChaseExampleApp(HWND hWnd);
	ChaseExampleApp(const ChaseExampleApp& rhs) = delete;
	ChaseExampleApp& operator=(const ChaseExampleApp& rhs) = delete;
	~ChaseExampleApp();

	virtual bool Initialize()override;

	virtual void OnMouseDown(WPARAM btnState, int x, int y) override;
	virtual void OnMouseUp(WPARAM btnState, int x, int y) override;
	virtual void OnMouseMove(WPARAM btnState, int x, int y) override;

	void GetPosition(float& x, float& y, float& z);
	void GetPlayerPosition(float& x, float& y, float& z);
	void SetPosition(float x, float y, float z);
private:
	virtual void OnResize()override;
	virtual void Update(const GameTimer& gt)override;
	virtual void Draw(const GameTimer& gt)override;

	void OnKeyboardInput(const GameTimer& gt);
	void UpdateCamera(const GameTimer& gt);
	void UpdateGround(const GameTimer& gt);
	void UpdateMainPassCB(const GameTimer& gt);
	void UpdateWaves(const GameTimer& gt);
	void UpdateAIBox(const GameTimer& gt);
	void UpdateAlertZone(const GameTimer& gt);
	void UpdateControlBox(const GameTimer& gt);

	void BuildRootSignature();
	void BuildShadersAndInputLayout();
	void BuildLandGeometry();
	void BuildAIBox();
	void BuildAlertZone();
	void BuildControlBox();
	void BuildWavesGeometryBuffers();
	void BuildPSOs();
	void BuildFrameResources();
	void BuildRenderItems();
	void DrawRenderItems(ID3D12GraphicsCommandList* cmdList, const std::vector<RenderItem*>& ritems);

	float GetHillsHeight(float x, float z)const;
	XMFLOAT3 GetHillsNormal(float x, float z)const;

private:
	static ChaseExampleApp* Instance;

	std::vector<std::unique_ptr<FrameResource>> mFrameResources;
	FrameResource* mCurrFrameResource = nullptr;
	int mCurrFrameResourceIndex = 0;

	UINT mCbvSrvDescriptorSize = 0;

	ComPtr<ID3D12RootSignature> mRootSignature = nullptr;

	std::unordered_map<std::string, std::unique_ptr<MeshGeometry>> mGeometries;
	std::unordered_map<std::string, ComPtr<ID3DBlob>> mShaders;
	std::unordered_map<std::string, ComPtr<ID3D12PipelineState>> mPSOs;

	std::vector<D3D12_INPUT_ELEMENT_DESC> mInputLayout;

	RenderItem* mWavesRitem = nullptr;

	// List of all the render items.
	std::vector<std::unique_ptr<RenderItem>> mAllRitems;

	// Render items divided by PSO.
	std::vector<RenderItem*> mRitemLayer[(int)RenderLayer::Count];

	std::unique_ptr<Waves> mWaves;

	PassConstants mMainPassCB;

	bool mIsWireframe = false;

	XMFLOAT3 mEyePos = { 0.0f, 0.0f, 0.0f };
	XMFLOAT4X4 mView = MathHelper::Identity4x4();
	XMFLOAT4X4 mProj = MathHelper::Identity4x4();
	XMFLOAT3 mAIPos = { 0.0f, 4.0f, 0.0f };
	XMFLOAT3 mPos = { 0.0f, 4.0f, -30.0f };

	float mTheta = 1.5f * XM_PI;
	float mPhi = XM_PIDIV2 - 0.4f;
	float mRadius = 50.0f;

	float mSunTheta = 1.25f * XM_PI;
	float mSunPhi = XM_PIDIV4;

	POINT mLastMousePos;
};

DLL_API bool _stdcall Init(HWND hwnd)
{
	ChaseExampleApp* theApp = new ChaseExampleApp(hwnd);
	return theApp->Initialize();
}

DLL_API void _stdcall ResetTimer()
{
	ChaseExampleApp::GetApp()->GetTimer()->Reset();
}

DLL_API void _stdcall Run()
{
	ChaseExampleApp::GetApp()->RunStable();
}

DLL_API void _stdcall WrappedOnMouseDown(WPARAM btnState, int x, int y)
{
	ChaseExampleApp::GetApp()->OnMouseDown(btnState, x, y);
}

DLL_API void _stdcall WrappedOnMouseUp(WPARAM btnState, int x, int y)
{
	ChaseExampleApp::GetApp()->OnMouseUp(btnState, x, y);
}

DLL_API void _stdcall WrappedOnMouseMove(WPARAM btnState, int x, int y) {
	ChaseExampleApp::GetApp()->OnMouseMove(btnState, x, y);
}

DLL_API void _stdcall GetBoxPosition(float& x, float& y, float& z) 
{
	ChaseExampleApp::GetInstance()->GetPosition(x, y, z);
}

DLL_API void _stdcall GetPlayerPosition(float& x, float& y, float& z) 
{
	ChaseExampleApp::GetInstance()->GetPlayerPosition(x, y, z);
}

DLL_API void _stdcall MoveBoxTo(float x, float y, float z) 
{
	ChaseExampleApp::GetInstance()->SetPosition(x, y, z);
}

DLL_API void _stdcall GetMovementArea(float& xMin, float& yMin, float& xMax, float& yMax) 
{
	xMin = yMin = -50;
	xMax = yMax = 50;
}

ChaseExampleApp* ChaseExampleApp::Instance = nullptr;
ChaseExampleApp* ChaseExampleApp::GetInstance()
{
	return Instance;
}


ChaseExampleApp::ChaseExampleApp(HINSTANCE hInstance)
	: D3DApp(hInstance)
{
	Instance = this;
}

ChaseExampleApp::ChaseExampleApp(HWND hWnd) : D3DApp(hWnd)
{
	Instance = this;
}

ChaseExampleApp::~ChaseExampleApp()
{
	if (md3dDevice != nullptr)
		FlushCommandQueue();
}

bool ChaseExampleApp::Initialize()
{
	if (!D3DApp::Initialize())
		return false;

	// Reset the command list to prep for initialization commands.
	ThrowIfFailed(mCommandList->Reset(mDirectCmdListAlloc.Get(), nullptr));

	//mWaves = std::make_unique<Waves>(128, 128, 1.0f, 0.03f, 4.0f, 0.2f);

	BuildRootSignature();
	BuildShadersAndInputLayout();
	BuildLandGeometry();
	BuildAIBox();
	BuildControlBox();
	BuildAlertZone();
	//BuildWavesGeometryBuffers();
	BuildRenderItems();
	// BuildRenderItems();
	BuildFrameResources();
	BuildPSOs();

	// Execute the initialization commands.
	ThrowIfFailed(mCommandList->Close());
	ID3D12CommandList* cmdsLists[] = { mCommandList.Get() };
	mCommandQueue->ExecuteCommandLists(_countof(cmdsLists), cmdsLists);

	// Wait until initialization is complete.
	FlushCommandQueue();

	return true;
}

void ChaseExampleApp::OnResize()
{
	D3DApp::OnResize();

	// The window resized, so update the aspect ratio and recompute the projection matrix.
	XMMATRIX P = DirectX::XMMatrixPerspectiveFovLH(0.25f * MathHelper::Pi, AspectRatio(), 1.0f, 1000.0f);
	XMStoreFloat4x4(&mProj, P);
}

void ChaseExampleApp::Update(const GameTimer& gt)
{
	OnKeyboardInput(gt);
	UpdateCamera(gt);

	// Cycle through the circular frame resource array.
	mCurrFrameResourceIndex = (mCurrFrameResourceIndex + 1) % gNumFrameResources;
	mCurrFrameResource = mFrameResources[mCurrFrameResourceIndex].get();

	// Has the GPU finished processing the commands of the current frame resource?
	// If not, wait until the GPU has completed commands up to this fence point.
	if (mCurrFrameResource->Fence != 0 && mFence->GetCompletedValue() < mCurrFrameResource->Fence)
	{
		HANDLE eventHandle = CreateEventEx(nullptr, nullptr, false, EVENT_ALL_ACCESS);
		ThrowIfFailed(mFence->SetEventOnCompletion(mCurrFrameResource->Fence, eventHandle));
		WaitForSingleObject(eventHandle, INFINITE);
		CloseHandle(eventHandle);
	}

	UpdateGround(gt);
	UpdateMainPassCB(gt);
	//UpdateWaves(gt);
	UpdateControlBox(gt);
	UpdateAIBox(gt);
	UpdateAlertZone(gt);
}

void ChaseExampleApp::Draw(const GameTimer& gt)
{
	auto cmdListAlloc = mCurrFrameResource->CmdListAlloc;

	// Reuse the memory associated with command recording.
	// We can only reset when the associated command lists have finished execution on the GPU.
	ThrowIfFailed(cmdListAlloc->Reset());

	// A command list can be reset after it has been added to the command queue via ExecuteCommandList.
	// Reusing the command list reuses memory.
	if (mIsWireframe)
	{
		ThrowIfFailed(mCommandList->Reset(cmdListAlloc.Get(), mPSOs["opaque_wireframe"].Get()));
	}
	else
	{
		ThrowIfFailed(mCommandList->Reset(cmdListAlloc.Get(), mPSOs["opaque"].Get()));
	}

	mCommandList->RSSetViewports(1, &mScreenViewport);
	mCommandList->RSSetScissorRects(1, &mScissorRect);

	// Indicate a state transition on the resource usage.
	CD3DX12_RESOURCE_BARRIER presentBarrier = CD3DX12_RESOURCE_BARRIER::Transition(CurrentBackBuffer(),
		D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATE_RENDER_TARGET);
	mCommandList->ResourceBarrier(1, &presentBarrier);

	// Clear the back buffer and depth buffer.
	mCommandList->ClearRenderTargetView(CurrentBackBufferView(), Colors::LightSteelBlue, 0, nullptr);
	mCommandList->ClearDepthStencilView(DepthStencilView(), D3D12_CLEAR_FLAG_DEPTH | D3D12_CLEAR_FLAG_STENCIL, 1.0f, 0, 0, nullptr);

	// Specify the buffers we are going to render to.
	D3D12_CPU_DESCRIPTOR_HANDLE currentBackBufferView = CurrentBackBufferView();
	D3D12_CPU_DESCRIPTOR_HANDLE depthStencilView = DepthStencilView();
	mCommandList->OMSetRenderTargets(1, &currentBackBufferView, true, &depthStencilView);

	mCommandList->SetGraphicsRootSignature(mRootSignature.Get());

	// Bind per-pass constant buffer.  We only need to do this once per-pass.
	auto passCB = mCurrFrameResource->PassCB->Resource();
	mCommandList->SetGraphicsRootConstantBufferView(1, passCB->GetGPUVirtualAddress());

	DrawRenderItems(mCommandList.Get(), mRitemLayer[(int)RenderLayer::Opaque]);

	// Indicate a state transition on the resource usage.
	CD3DX12_RESOURCE_BARRIER renderTargetBarrier = CD3DX12_RESOURCE_BARRIER::Transition(CurrentBackBuffer(),
		D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATE_PRESENT);
	mCommandList->ResourceBarrier(1, &renderTargetBarrier);

	// Done recording commands.
	ThrowIfFailed(mCommandList->Close());

	// Add the command list to the queue for execution.
	ID3D12CommandList* cmdsLists[] = { mCommandList.Get() };
	mCommandQueue->ExecuteCommandLists(_countof(cmdsLists), cmdsLists);

	// Swap the back and front buffers
	ThrowIfFailed(mSwapChain->Present(0, 0));
	mCurrBackBuffer = (mCurrBackBuffer + 1) % SwapChainBufferCount;

	// Advance the fence value to mark commands up to this fence point.
	mCurrFrameResource->Fence = ++mCurrentFence;

	// Add an instruction to the command queue to set a new fence point.
	// Because we are on the GPU timeline, the new fence point won't be
	// set until the GPU finishes processing all the commands prior to this Signal().
	mCommandQueue->Signal(mFence.Get(), mCurrentFence);
}

void ChaseExampleApp::OnMouseDown(WPARAM btnState, int x, int y)
{
	mLastMousePos.x = x;
	mLastMousePos.y = y;

	SetCapture(mhMainWnd);
}

void ChaseExampleApp::OnMouseUp(WPARAM btnState, int x, int y)
{
	ReleaseCapture();
}

void ChaseExampleApp::OnMouseMove(WPARAM btnState, int x, int y)
{
	if ((btnState & MK_LBUTTON) != 0)
	{
		// Make each pixel correspond to a quarter of a degree.
		float dx = XMConvertToRadians(0.25f * static_cast<float>(x - mLastMousePos.x));
		float dy = XMConvertToRadians(0.25f * static_cast<float>(y - mLastMousePos.y));

		// Update angles based on input to orbit camera around box.
		mTheta -= dx;
		mPhi -= dy;

		// Restrict the angle mPhi.
		mPhi = MathHelper::Clamp(mPhi, 0.5f, XM_PIDIV2 - 0.3f);
	}
	else if ((btnState & MK_RBUTTON) != 0)
	{
		// Make each pixel correspond to 0.2 unit in the scene.
		float dx = 0.2f * static_cast<float>(x - mLastMousePos.x);
		float dy = 0.2f * static_cast<float>(y - mLastMousePos.y);

		// Update the camera radius based on input.
		mRadius += dx - dy;

		// Restrict the radius.
		mRadius = MathHelper::Clamp(mRadius, 30.0f, 70.0f);
	}

	mLastMousePos.x = x;
	mLastMousePos.y = y;
}

void ChaseExampleApp::GetPosition(float& x, float& y, float& z)
{
	x =	mAIPos.x;
	y = mAIPos.y;
	z = mAIPos.z;
}

void ChaseExampleApp::GetPlayerPosition(float& x, float& y, float& z)
{
	x = mPos.x;
	y = mPos.y;
	z = mPos.z;
}

void ChaseExampleApp::SetPosition(float x, float y, float z)
{
	mAIPos.x = x;
	mAIPos.y = y;
	mAIPos.z = z;
}

void ChaseExampleApp::OnKeyboardInput(const GameTimer& gt)
{
	if (GetAsyncKeyState('1') & 0x8000)
		mIsWireframe = true;
	else
		mIsWireframe = false;

	bool vertical = false, horizontal = false;
	float theta = mTheta;

	bool keyWState = GetAsyncKeyState('W') & 0x8000;
	bool keySState = GetAsyncKeyState('S') & 0x8000;
	bool keyAState = GetAsyncKeyState('A') & 0x8000;
	bool keyDState = GetAsyncKeyState('D') & 0x8000;

	if (keyWState ^ keySState) {
		vertical = true;
	}

	if (keyAState ^ keyDState) {
		horizontal = true;
	}

	if (vertical && horizontal) {
		if (keyWState) {
			theta += XM_PI;
			if (keyAState) {
				theta += XM_PIDIV4;
			}
			if (keyDState) {
				theta -= XM_PIDIV4;
			}
		}
			
		if (keySState) {
			theta += 0.0f;
			if (keyAState) {
				theta -= XM_PIDIV4;
			}
			if (keyDState) {
				theta += XM_PIDIV4;
			}
		}
	}
	else if (vertical) {
		if (keyWState)
			theta += XM_PI;

		if (keySState)
			theta += 0.0f;
	}
	else if (horizontal) {
		if (keyAState)
			theta += XM_PI + 1.5f;
		
		if (keyDState)
			theta += XM_PIDIV2;
	}
	else
	{
		return;
	}

	mPos.x += 0.01 * cosf(theta);
	mPos.z += 0.01 * sinf(theta);

	mPos.x = MathHelper::Clamp(mPos.x, -50.0f, 50.0f);
	mPos.z = MathHelper::Clamp(mPos.z, -50.0f, 50.0f);
}

void ChaseExampleApp::UpdateCamera(const GameTimer& gt)
{
	// Convert Spherical to Cartesian coordinates.
	mEyePos.x = mRadius * sinf(mPhi) * cosf(mTheta);
	mEyePos.z = mRadius * sinf(mPhi) * sinf(mTheta);
	mEyePos.y = mRadius * cosf(mPhi);

	// Build the view matrix.
	XMVECTOR pos = XMVectorSet(mEyePos.x + mPos.x, mEyePos.y + mPos.y, mEyePos.z + mPos.z, 1.0f);
	XMVECTOR target = XMVectorSet(mPos.x, mPos.y, mPos.z, 0.0f);
	XMVECTOR up = XMVectorSet(0.0f, 1.0f, 0.0f, 0.0f);

	XMMATRIX view = DirectX::XMMatrixLookAtLH(pos, target, up);
	XMStoreFloat4x4(&mView, view);
}

void ChaseExampleApp::UpdateGround(const GameTimer& gt)
{
	auto currObjectCB = mCurrFrameResource->ObjectCB.get();
	auto e = mAllRitems[0].get();
	if (e->NumFramesDirty > 0) {
		XMMATRIX world = XMLoadFloat4x4(&e->World);

		ObjectConstants objConstants;
		XMStoreFloat4x4(&objConstants.World, DirectX::XMMatrixTranspose(world));

		currObjectCB->CopyData(e->ObjCBIndex, objConstants);

		// Next FrameResource need to be updated too.
		e->NumFramesDirty--;
	}
}

void ChaseExampleApp::UpdateMainPassCB(const GameTimer& gt)
{
	XMMATRIX view = XMLoadFloat4x4(&mView);
	XMMATRIX proj = XMLoadFloat4x4(&mProj);

	XMMATRIX viewProj = DirectX::XMMatrixMultiply(view, proj);

	XMVECTOR viewDeterminant = DirectX::XMMatrixDeterminant(view);
	XMMATRIX invView = DirectX::XMMatrixInverse(&viewDeterminant, view);

	XMVECTOR projDeterminant = DirectX::XMMatrixDeterminant(proj);
	XMMATRIX invProj = DirectX::XMMatrixInverse(&projDeterminant, proj);

	XMVECTOR viewProjDeterminant = DirectX::XMMatrixDeterminant(viewProj);
	XMMATRIX invViewProj = DirectX::XMMatrixInverse(&viewProjDeterminant, viewProj);

	XMStoreFloat4x4(&mMainPassCB.View, DirectX::XMMatrixTranspose(view));
	XMStoreFloat4x4(&mMainPassCB.InvView, DirectX::XMMatrixTranspose(invView));
	XMStoreFloat4x4(&mMainPassCB.Proj, DirectX::XMMatrixTranspose(proj));
	XMStoreFloat4x4(&mMainPassCB.InvProj, DirectX::XMMatrixTranspose(invProj));
	XMStoreFloat4x4(&mMainPassCB.ViewProj, DirectX::XMMatrixTranspose(viewProj));
	XMStoreFloat4x4(&mMainPassCB.InvViewProj, DirectX::XMMatrixTranspose(invViewProj));
	mMainPassCB.EyePosW = mEyePos;
	mMainPassCB.RenderTargetSize = XMFLOAT2((float)mClientWidth, (float)mClientHeight);
	mMainPassCB.InvRenderTargetSize = XMFLOAT2(1.0f / mClientWidth, 1.0f / mClientHeight);
	mMainPassCB.NearZ = 1.0f;
	mMainPassCB.FarZ = 1000.0f;
	mMainPassCB.TotalTime = gt.TotalTime();
	mMainPassCB.DeltaTime = gt.DeltaTime();

	auto currPassCB = mCurrFrameResource->PassCB.get();
	currPassCB->CopyData(0, mMainPassCB);
}

void ChaseExampleApp::UpdateWaves(const GameTimer& gt)
{
	// Every quarter second, generate a random wave.
	static float t_base = 0.0f;
	if ((mTimer.TotalTime() - t_base) >= 0.25f)
	{
		t_base += 0.25f;

		int i = MathHelper::Rand(4, mWaves->RowCount() - 5);
		int j = MathHelper::Rand(4, mWaves->ColumnCount() - 5);

		float r = MathHelper::RandF(0.2f, 0.5f);

		mWaves->Disturb(i, j, r);
	}

	// Update the wave simulation.
	mWaves->Update(gt.DeltaTime());

	// Update the wave vertex buffer with the new solution.
	auto currWavesVB = mCurrFrameResource->WavesVB.get();
	for (int i = 0; i < mWaves->VertexCount(); ++i)
	{
		Vertex v;

		v.Pos = mWaves->Position(i);
		v.Color = XMFLOAT4(DirectX::Colors::Blue);

		currWavesVB->CopyData(i, v);
	}

	// Set the dynamic VB of the wave renderitem to the current frame VB.
	mWavesRitem->Geo->VertexBufferGPU = currWavesVB->Resource();
}

void ChaseExampleApp::UpdateAIBox(const GameTimer& gt)
{
	auto currObjectCB = mCurrFrameResource->ObjectCB.get();
	auto e = mAllRitems[2].get();
	XMMATRIX world = XMMatrixMultiply(XMMatrixIdentity(), XMMatrixTranslation(mAIPos.x, mAIPos.y, mAIPos.z));

	ObjectConstants objConstants;
	XMStoreFloat4x4(&objConstants.World, DirectX::XMMatrixTranspose(world));

	currObjectCB->CopyData(e->ObjCBIndex, objConstants);

	e->NumFramesDirty--;
}

void ChaseExampleApp::UpdateAlertZone(const GameTimer& gt)
{
	auto currObjectCB = mCurrFrameResource->ObjectCB.get();
	auto e = mAllRitems[3].get();
	XMMATRIX world = XMMatrixMultiply(XMMatrixIdentity(), XMMatrixTranslation(mAIPos.x, mAIPos.y, mAIPos.z));

	ObjectConstants objConstants;
	XMStoreFloat4x4(&objConstants.World, DirectX::XMMatrixTranspose(world));

	currObjectCB->CopyData(e->ObjCBIndex, objConstants);

	e->NumFramesDirty--;
}

void ChaseExampleApp::UpdateControlBox(const GameTimer& gt)
{
	auto currObjectCB = mCurrFrameResource->ObjectCB.get();
	auto e = mAllRitems[1].get();
	XMMATRIX world = XMMatrixMultiply(XMMatrixIdentity(), XMMatrixRotationY(-(mTheta + XM_PI)));
	world = XMMatrixMultiply(world, XMMatrixTranslation(mPos.x, mPos.y, mPos.z));

	ObjectConstants objConstants;
	XMStoreFloat4x4(&objConstants.World, DirectX::XMMatrixTranspose(world));

	currObjectCB->CopyData(e->ObjCBIndex, objConstants);

	e->NumFramesDirty--;
}

void ChaseExampleApp::BuildRootSignature()
{
	// Root parameter can be a table, root descriptor or root constants.
	CD3DX12_ROOT_PARAMETER slotRootParameter[2];

	// Create root CBV.
	slotRootParameter[0].InitAsConstantBufferView(0);
	slotRootParameter[1].InitAsConstantBufferView(1);

	// A root signature is an array of root parameters.
	CD3DX12_ROOT_SIGNATURE_DESC rootSigDesc(2, slotRootParameter, 0, nullptr, D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT);

	// create a root signature with a single slot which points to a descriptor range consisting of a single constant buffer
	ComPtr<ID3DBlob> serializedRootSig = nullptr;
	ComPtr<ID3DBlob> errorBlob = nullptr;
	HRESULT hr = D3D12SerializeRootSignature(&rootSigDesc, D3D_ROOT_SIGNATURE_VERSION_1,
		serializedRootSig.GetAddressOf(), errorBlob.GetAddressOf());

	if (errorBlob != nullptr)
	{
		::OutputDebugStringA((char*)errorBlob->GetBufferPointer());
	}
	ThrowIfFailed(hr);

	ThrowIfFailed(md3dDevice->CreateRootSignature(
		0,
		serializedRootSig->GetBufferPointer(),
		serializedRootSig->GetBufferSize(),
		IID_PPV_ARGS(mRootSignature.GetAddressOf())));
}

void ChaseExampleApp::BuildShadersAndInputLayout()
{
	mShaders["standardVS"] = d3dUtil::CompileShader(L"Shaders\\color.hlsl", nullptr, "VS", "vs_5_0");
	mShaders["opaquePS"] = d3dUtil::CompileShader(L"Shaders\\color.hlsl", nullptr, "PS", "ps_5_0");

	mInputLayout =
	{
		{ "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 },
		{ "COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 12, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 }
	};
}

void ChaseExampleApp::BuildLandGeometry()
{
	GeometryGenerator geoGen;
	GeometryGenerator::MeshData grid = geoGen.CreateGrid(100.0f, 100.0f, 99, 99);

	//
	// Extract the vertex elements we are interested and apply the height function to
	// each vertex.  In addition, color the vertices based on their height so we have
	// sandy looking beaches, grassy low hills, and snow mountain peaks.
	//

	std::vector<Vertex> vertices(grid.Vertices.size());
	for (size_t i = 0; i < grid.Vertices.size(); ++i)
	{
		auto& p = grid.Vertices[i].Position;
		vertices[i].Pos = p;
		//vertices[i].Pos.y = GetHillsHeight(p.x, p.z);

		// Color the vertex based on its height.
		if (vertices[i].Pos.y < -10.0f)
		{
			// Sandy beach color.
			vertices[i].Color = XMFLOAT4(1.0f, 0.96f, 0.62f, 1.0f);
		}
		else if (vertices[i].Pos.y < 5.0f)
		{
			// Light yellow-green.
			vertices[i].Color = XMFLOAT4(0.48f, 0.77f, 0.46f, 1.0f);
		}
		else if (vertices[i].Pos.y < 12.0f)
		{
			// Dark yellow-green.
			vertices[i].Color = XMFLOAT4(0.1f, 0.48f, 0.19f, 1.0f);
		}
		else if (vertices[i].Pos.y < 20.0f)
		{
			// Dark brown.
			vertices[i].Color = XMFLOAT4(0.45f, 0.39f, 0.34f, 1.0f);
		}
		else
		{
			// White snow.
			vertices[i].Color = XMFLOAT4(1.0f, 1.0f, 1.0f, 1.0f);
		}
	}

	const UINT vbByteSize = (UINT)vertices.size() * sizeof(Vertex);

	std::vector<std::uint16_t> indices = grid.GetIndices16();
	const UINT ibByteSize = (UINT)indices.size() * sizeof(std::uint16_t);

	auto geo = std::make_unique<MeshGeometry>();
	geo->Name = "landGeo";

	ThrowIfFailed(D3DCreateBlob(vbByteSize, &geo->VertexBufferCPU));
	CopyMemory(geo->VertexBufferCPU->GetBufferPointer(), vertices.data(), vbByteSize);

	ThrowIfFailed(D3DCreateBlob(ibByteSize, &geo->IndexBufferCPU));
	CopyMemory(geo->IndexBufferCPU->GetBufferPointer(), indices.data(), ibByteSize);

	geo->VertexBufferGPU = d3dUtil::CreateDefaultBuffer(md3dDevice.Get(),
		mCommandList.Get(), vertices.data(), vbByteSize, geo->VertexBufferUploader);

	geo->IndexBufferGPU = d3dUtil::CreateDefaultBuffer(md3dDevice.Get(),
		mCommandList.Get(), indices.data(), ibByteSize, geo->IndexBufferUploader);

	geo->VertexByteStride = sizeof(Vertex);
	geo->VertexBufferByteSize = vbByteSize;
	geo->IndexFormat = DXGI_FORMAT_R16_UINT;
	geo->IndexBufferByteSize = ibByteSize;

	SubmeshGeometry submesh;
	submesh.IndexCount = (UINT)indices.size();
	submesh.StartIndexLocation = 0;
	submesh.BaseVertexLocation = 0;

	geo->DrawArgs["grid"] = submesh;

	mGeometries["landGeo"] = std::move(geo);
}

void ChaseExampleApp::BuildAIBox()
{
	GeometryGenerator geoGen;
	GeometryGenerator::MeshData box = geoGen.CreateCylinder(4.0f, 4.0f, 8.0f, 20, 20);

	std::vector<Vertex> vertices(box.Vertices.size());
	for (int i = 0; i < box.Vertices.size(); i++) 
	{
		vertices[i].Pos = box.Vertices[i].Position;

		vertices[i].Color = XMFLOAT4(1.0f, 0.3f, 0.0f, 1.0f);
	}

	const UINT vertexBufferByteSize = (UINT)vertices.size() * sizeof(Vertex);
	const UINT indiceBufferByteSize = (UINT)box.GetIndices16().size() * sizeof(std::uint16_t);

	auto geo = std::make_unique<MeshGeometry>();
	geo->Name = "boxGeo";

	ThrowIfFailed(D3DCreateBlob(vertexBufferByteSize, &geo->VertexBufferCPU));
	CopyMemory(geo->VertexBufferCPU->GetBufferPointer(), vertices.data(), vertexBufferByteSize);

	ThrowIfFailed(D3DCreateBlob(indiceBufferByteSize, &geo->IndexBufferCPU));
	CopyMemory(geo->IndexBufferCPU->GetBufferPointer(), box.GetIndices16().data(), indiceBufferByteSize);

	geo->VertexBufferGPU = d3dUtil::CreateDefaultBuffer(md3dDevice.Get(),
		mCommandList.Get(), vertices.data(), vertexBufferByteSize, geo->VertexBufferUploader);

	geo->IndexBufferGPU = d3dUtil::CreateDefaultBuffer(md3dDevice.Get(),
		mCommandList.Get(), box.GetIndices16().data(), indiceBufferByteSize, geo->IndexBufferUploader);

	geo->VertexByteStride = sizeof(Vertex);
	geo->VertexBufferByteSize = vertexBufferByteSize;
	geo->IndexFormat = DXGI_FORMAT_R16_UINT;
	geo->IndexBufferByteSize = indiceBufferByteSize;

	SubmeshGeometry submesh;
	submesh.IndexCount = (UINT)box.GetIndices16().size();
	submesh.StartIndexLocation = 0;
	submesh.BaseVertexLocation = 0;

	geo->DrawArgs["grid"] = submesh;

	mGeometries["boxGeo"] = std::move(geo);
}

void ChaseExampleApp::BuildAlertZone() 
{
	GeometryGenerator geoGen;
	GeometryGenerator::MeshData alertZone = geoGen.CreateSphere(20.0f, 20, 20);

	std::vector<Vertex> vertices(alertZone.Vertices.size());
	for (int i = 0; i < alertZone.Vertices.size(); i++)
	{
		vertices[i].Pos = alertZone.Vertices[i].Position;

		vertices[i].Color = XMFLOAT4(1.0f, 0.0f, 0.0f, 0.1f);
	}

	const UINT vbByteSize = (UINT)vertices.size() * sizeof(Vertex);
	const UINT ibByteSize = (UINT)alertZone.GetIndices16().size() * sizeof(std::uint16_t);

	auto geo = std::make_unique<MeshGeometry>();
	geo->Name = "alertGeo";

	ThrowIfFailed(D3DCreateBlob(vbByteSize, &geo->VertexBufferCPU));
	CopyMemory(geo->VertexBufferCPU->GetBufferPointer(), vertices.data(), vbByteSize);

	ThrowIfFailed(D3DCreateBlob(ibByteSize, &geo->IndexBufferCPU));
	CopyMemory(geo->IndexBufferCPU->GetBufferPointer(), alertZone.GetIndices16().data(), ibByteSize);

	geo->VertexBufferGPU = d3dUtil::CreateDefaultBuffer(md3dDevice.Get(),
		mCommandList.Get(), vertices.data(), vbByteSize, geo->VertexBufferUploader);

	geo->IndexBufferGPU = d3dUtil::CreateDefaultBuffer(md3dDevice.Get(),
		mCommandList.Get(), alertZone.GetIndices16().data(), ibByteSize, geo->IndexBufferUploader);

	geo->VertexByteStride = sizeof(Vertex);
	geo->VertexBufferByteSize = vbByteSize;
	geo->IndexFormat = DXGI_FORMAT_R16_UINT;
	geo->IndexBufferByteSize = ibByteSize;

	SubmeshGeometry submesh;
	submesh.IndexCount = (UINT)alertZone.GetIndices16().size();
	submesh.StartIndexLocation = 0;
	submesh.BaseVertexLocation = 0;

	geo->DrawArgs["grid"] = submesh;

	mGeometries["alertGeo"] = std::move(geo);
}

void ChaseExampleApp::BuildControlBox()
{
	GeometryGenerator geoGen;
	GeometryGenerator::MeshData box = geoGen.CreateSphere(4.0f, 30, 30);

	std::vector<Vertex> vertices(box.Vertices.size());
	for (int i = 0; i < box.Vertices.size(); i++)
	{
		vertices[i].Pos = box.Vertices[i].Position;

		vertices[i].Color = XMFLOAT4(0.0f, 1.0f, 1.0f, 1.0f);
	}

	const UINT vertexBufferByteSize = (UINT)vertices.size() * sizeof(Vertex);
	const UINT indiceBufferByteSize = (UINT)box.GetIndices16().size() * sizeof(std::uint16_t);

	auto geo = std::make_unique<MeshGeometry>();
	geo->Name = "myGeo";

	ThrowIfFailed(D3DCreateBlob(vertexBufferByteSize, &geo->VertexBufferCPU));
	CopyMemory(geo->VertexBufferCPU->GetBufferPointer(), vertices.data(), vertexBufferByteSize);

	ThrowIfFailed(D3DCreateBlob(indiceBufferByteSize, &geo->IndexBufferCPU));
	CopyMemory(geo->IndexBufferCPU->GetBufferPointer(), box.GetIndices16().data(), indiceBufferByteSize);

	geo->VertexBufferGPU = d3dUtil::CreateDefaultBuffer(md3dDevice.Get(),
		mCommandList.Get(), vertices.data(), vertexBufferByteSize, geo->VertexBufferUploader);

	geo->IndexBufferGPU = d3dUtil::CreateDefaultBuffer(md3dDevice.Get(),
		mCommandList.Get(), box.GetIndices16().data(), indiceBufferByteSize, geo->IndexBufferUploader);

	geo->VertexByteStride = sizeof(Vertex);
	geo->VertexBufferByteSize = vertexBufferByteSize;
	geo->IndexFormat = DXGI_FORMAT_R16_UINT;
	geo->IndexBufferByteSize = indiceBufferByteSize;

	SubmeshGeometry submesh;
	submesh.IndexCount = (UINT)box.GetIndices16().size();
	submesh.StartIndexLocation = 0;
	submesh.BaseVertexLocation = 0;

	geo->DrawArgs["grid"] = submesh;

	mGeometries["myGeo"] = std::move(geo);
}

void ChaseExampleApp::BuildWavesGeometryBuffers()
{
	std::vector<std::uint16_t> indices(3 * mWaves->TriangleCount()); // 3 indices per face
	assert(mWaves->VertexCount() < 0x0000ffff);

	// Iterate over each quad.
	int m = mWaves->RowCount();
	int n = mWaves->ColumnCount();
	int k = 0;
	for (int i = 0; i < m - 1; ++i)
	{
		for (int j = 0; j < n - 1; ++j)
		{
			indices[k] = i * n + j;
			indices[k + 1] = i * n + j + 1;
			indices[k + 2] = (i + 1) * n + j;

			indices[k + 3] = (i + 1) * n + j;
			indices[k + 4] = i * n + j + 1;
			indices[k + 5] = (i + 1) * n + j + 1;

			k += 6; // next quad
		}
	}

	UINT vbByteSize = mWaves->VertexCount() * sizeof(Vertex);
	UINT ibByteSize = (UINT)indices.size() * sizeof(std::uint16_t);

	auto geo = std::make_unique<MeshGeometry>();
	geo->Name = "waterGeo";

	// Set dynamically.
	geo->VertexBufferCPU = nullptr;
	geo->VertexBufferGPU = nullptr;

	ThrowIfFailed(D3DCreateBlob(ibByteSize, &geo->IndexBufferCPU));
	CopyMemory(geo->IndexBufferCPU->GetBufferPointer(), indices.data(), ibByteSize);

	geo->IndexBufferGPU = d3dUtil::CreateDefaultBuffer(md3dDevice.Get(),
		mCommandList.Get(), indices.data(), ibByteSize, geo->IndexBufferUploader);

	geo->VertexByteStride = sizeof(Vertex);
	geo->VertexBufferByteSize = vbByteSize;
	geo->IndexFormat = DXGI_FORMAT_R16_UINT;
	geo->IndexBufferByteSize = ibByteSize;

	SubmeshGeometry submesh;
	submesh.IndexCount = (UINT)indices.size();
	submesh.StartIndexLocation = 0;
	submesh.BaseVertexLocation = 0;

	geo->DrawArgs["grid"] = submesh;

	mGeometries["waterGeo"] = std::move(geo);
}

void ChaseExampleApp::BuildPSOs()
{
	D3D12_GRAPHICS_PIPELINE_STATE_DESC opaquePsoDesc;

	//
	// PSO for opaque objects.
	//
	ZeroMemory(&opaquePsoDesc, sizeof(D3D12_GRAPHICS_PIPELINE_STATE_DESC));
	opaquePsoDesc.InputLayout = { mInputLayout.data(), (UINT)mInputLayout.size() };
	opaquePsoDesc.pRootSignature = mRootSignature.Get();
	opaquePsoDesc.VS =
	{
		reinterpret_cast<BYTE*>(mShaders["standardVS"]->GetBufferPointer()),
		mShaders["standardVS"]->GetBufferSize()
	};
	opaquePsoDesc.PS =
	{
		reinterpret_cast<BYTE*>(mShaders["opaquePS"]->GetBufferPointer()),
		mShaders["opaquePS"]->GetBufferSize()
	};

	D3D12_RENDER_TARGET_BLEND_DESC blendDesc;
	blendDesc.BlendEnable = true;
	blendDesc.BlendOp = D3D12_BLEND_OP_ADD;
	blendDesc.BlendOpAlpha = D3D12_BLEND_OP_ADD;
	blendDesc.DestBlend = D3D12_BLEND_INV_SRC_ALPHA;
	blendDesc.DestBlendAlpha = D3D12_BLEND_ZERO;
	blendDesc.LogicOp = D3D12_LOGIC_OP_NOOP;
	blendDesc.LogicOpEnable = false;
	blendDesc.RenderTargetWriteMask = D3D12_COLOR_WRITE_ENABLE_ALL;
	blendDesc.SrcBlend = D3D12_BLEND_SRC_ALPHA;
	blendDesc.SrcBlendAlpha = D3D12_BLEND_ONE;

	opaquePsoDesc.RasterizerState = CD3DX12_RASTERIZER_DESC(D3D12_DEFAULT);
	opaquePsoDesc.BlendState.RenderTarget[0] = blendDesc;
	opaquePsoDesc.DepthStencilState = CD3DX12_DEPTH_STENCIL_DESC(D3D12_DEFAULT);
	opaquePsoDesc.SampleMask = UINT_MAX;
	opaquePsoDesc.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
	opaquePsoDesc.NumRenderTargets = 1;
	opaquePsoDesc.RTVFormats[0] = mBackBufferFormat;
	opaquePsoDesc.SampleDesc.Count = m4xMsaaState ? 4 : 1;
	opaquePsoDesc.SampleDesc.Quality = m4xMsaaState ? (m4xMsaaQuality - 1) : 0;
	opaquePsoDesc.DSVFormat = mDepthStencilFormat;

	ThrowIfFailed(md3dDevice->CreateGraphicsPipelineState(&opaquePsoDesc, IID_PPV_ARGS(&mPSOs["opaque"])));

	//
	// PSO for opaque wireframe objects.
	//

	D3D12_GRAPHICS_PIPELINE_STATE_DESC opaqueWireframePsoDesc = opaquePsoDesc;
	opaqueWireframePsoDesc.RasterizerState.FillMode = D3D12_FILL_MODE_WIREFRAME;
	ThrowIfFailed(md3dDevice->CreateGraphicsPipelineState(&opaqueWireframePsoDesc, IID_PPV_ARGS(&mPSOs["opaque_wireframe"])));
}

void ChaseExampleApp::BuildFrameResources()
{
	for (int i = 0; i < gNumFrameResources; ++i)
	{
		mFrameResources.push_back(std::make_unique<FrameResource>(md3dDevice.Get(),
			1, (UINT)mAllRitems.size(), mGeometries["landGeo"].get()->VertexBufferByteSize / sizeof(Vertex)));
	}
}

void ChaseExampleApp::BuildRenderItems()
{
	//auto wavesRitem = std::make_unique<RenderItem>();
	//wavesRitem->World = MathHelper::Identity4x4();
	//wavesRitem->ObjCBIndex = 0;
	//wavesRitem->Geo = mGeometries["waterGeo"].get();
	//wavesRitem->PrimitiveType = D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST;
	//wavesRitem->IndexCount = wavesRitem->Geo->DrawArgs["grid"].IndexCount;
	//wavesRitem->StartIndexLocation = wavesRitem->Geo->DrawArgs["grid"].StartIndexLocation;
	//wavesRitem->BaseVertexLocation = wavesRitem->Geo->DrawArgs["grid"].BaseVertexLocation;

	//mWavesRitem = wavesRitem.get();

	//mRitemLayer[(int)RenderLayer::Opaque].push_back(wavesRitem.get());

	auto gridRitem = std::make_unique<RenderItem>();
	gridRitem->World = MathHelper::Identity4x4();
	gridRitem->ObjCBIndex = 1;
	gridRitem->Geo = mGeometries["landGeo"].get();
	gridRitem->PrimitiveType = D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST;
	gridRitem->IndexCount = gridRitem->Geo->DrawArgs["grid"].IndexCount;
	gridRitem->StartIndexLocation = gridRitem->Geo->DrawArgs["grid"].StartIndexLocation;
	gridRitem->BaseVertexLocation = gridRitem->Geo->DrawArgs["grid"].BaseVertexLocation;

	mRitemLayer[(int)RenderLayer::Opaque].push_back(gridRitem.get());

	//mAllRitems.push_back(std::move(wavesRitem));
	mAllRitems.push_back(std::move(gridRitem));

	auto myRitem = std::make_unique<RenderItem>();
	myRitem->World = MathHelper::Identity4x4();
	myRitem->ObjCBIndex = 2;
	myRitem->Geo = mGeometries["myGeo"].get();
	myRitem->PrimitiveType = D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST;
	myRitem->IndexCount = myRitem->Geo->DrawArgs["grid"].IndexCount;
	myRitem->StartIndexLocation = myRitem->Geo->DrawArgs["grid"].StartIndexLocation;
	myRitem->BaseVertexLocation = myRitem->Geo->DrawArgs["grid"].BaseVertexLocation;

	mRitemLayer[(int)RenderLayer::Opaque].push_back(myRitem.get());

	mAllRitems.push_back(std::move(myRitem));

	auto boxRitem = std::make_unique<RenderItem>();
	boxRitem->World = MathHelper::Identity4x4();
	boxRitem->ObjCBIndex = 3;
	boxRitem->Geo = mGeometries["boxGeo"].get();
	boxRitem->PrimitiveType = D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST;
	boxRitem->IndexCount = boxRitem->Geo->DrawArgs["grid"].IndexCount;
	boxRitem->StartIndexLocation = boxRitem->Geo->DrawArgs["grid"].StartIndexLocation;
	boxRitem->BaseVertexLocation = boxRitem->Geo->DrawArgs["grid"].BaseVertexLocation;

	mRitemLayer[(int)RenderLayer::Opaque].push_back(boxRitem.get());

	mAllRitems.push_back(std::move(boxRitem));

	auto alertRitem = std::make_unique<RenderItem>();
	alertRitem->World = MathHelper::Identity4x4();
	alertRitem->ObjCBIndex = 4;
	alertRitem->Geo = mGeometries["alertGeo"].get();
	alertRitem->PrimitiveType = D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST;
	alertRitem->IndexCount = alertRitem->Geo->DrawArgs["grid"].IndexCount;
	alertRitem->StartIndexLocation = alertRitem->Geo->DrawArgs["grid"].StartIndexLocation;
	alertRitem->BaseVertexLocation = alertRitem->Geo->DrawArgs["grid"].BaseVertexLocation;

	mRitemLayer[(int)RenderLayer::Opaque].push_back(alertRitem.get());

	mAllRitems.push_back(std::move(alertRitem));
}

void ChaseExampleApp::DrawRenderItems(ID3D12GraphicsCommandList* cmdList, const std::vector<RenderItem*>& ritems)
{
	UINT objCBByteSize = d3dUtil::CalcConstantBufferByteSize(sizeof(ObjectConstants));

	auto objectCB = mCurrFrameResource->ObjectCB->Resource();

	// For each render item...
	for (size_t i = 0; i < ritems.size(); ++i)
	{
		auto ri = ritems[i];

		D3D12_VERTEX_BUFFER_VIEW vertexBufferView = ri->Geo->VertexBufferView();
		cmdList->IASetVertexBuffers(0, 1, &vertexBufferView);
		D3D12_INDEX_BUFFER_VIEW indexBufferView = ri->Geo->IndexBufferView();
		cmdList->IASetIndexBuffer(&indexBufferView);
		cmdList->IASetPrimitiveTopology(ri->PrimitiveType);

		D3D12_GPU_VIRTUAL_ADDRESS objCBAddress = objectCB->GetGPUVirtualAddress();
		objCBAddress += ri->ObjCBIndex * objCBByteSize;

		cmdList->SetGraphicsRootConstantBufferView(0, objCBAddress);

		cmdList->DrawIndexedInstanced(ri->IndexCount, 1, ri->StartIndexLocation, ri->BaseVertexLocation, 0);
	}
}

float ChaseExampleApp::GetHillsHeight(float x, float z)const
{
	return 0.3f * (z * sinf(0.1f * x) + x * cosf(0.1f * z));
}

XMFLOAT3 ChaseExampleApp::GetHillsNormal(float x, float z)const
{
	// n = (-df/dx, 1, -df/dz)
	XMFLOAT3 n(
		-0.03f * z * cosf(0.1f * x) - 0.3f * cosf(0.1f * z),
		1.0f,
		-0.3f * sinf(0.1f * x) + 0.03f * x * sinf(0.1f * z));

	XMVECTOR unitNormal = XMVector3Normalize(XMLoadFloat3(&n));
	XMStoreFloat3(&n, unitNormal);

	return n;
}