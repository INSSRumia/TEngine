using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Launcher.Rendering
{
    /// <summary>
    /// 挂在 UIRoot/UICamera；与 UI 一起持久化，不依赖热更或 Camera.main 缓存。
    /// 只拥有本 UI Camera 在选定主相机堆栈中的条目，不管理其他 Overlay。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(10000)]
    public sealed class PersistentUICameraStack : MonoBehaviour
    {
        private Camera _uiCamera;
        private Camera[] _cameras = new Camera[8];
        // 保存实际列表，而不是在清理时重新访问 cameraStack：旧 Base 可能已禁用、
        // 销毁 AdditionalData 或变成 Overlay，getter 在这些情况下不可安全使用。
        private List<Camera> _boundStack;

        private void OnEnable()
        {
            _uiCamera = GetComponent<Camera>();
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            RefreshBinding();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            Unbind();
        }

        private void OnDestroy() => Unbind();

        // sceneLoaded 不覆盖 Camera.enabled / GameObject.SetActive / 运行时新建相机。
        // LateUpdate 在常规相机控制之后、渲染之前校正，缓冲数组仅在容量不足时增长。
        private void LateUpdate() => RefreshBinding();
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => RefreshBinding();
        private void OnActiveSceneChanged(Scene previous, Scene current) => RefreshBinding();

        /// <summary>显式校正入口；正常运行无需调用。也可供无渲染的 EditMode 测试使用。</summary>
        public void RefreshBinding()
        {
            if (_uiCamera == null)
            {
                Unbind();
                _uiCamera = GetComponent<Camera>();
            }

            if (!isActiveAndEnabled || _uiCamera == null || !_uiCamera.isActiveAndEnabled ||
                !(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))
            {
                Unbind();
                return;
            }

            // 本组件显式拥有的 UI Camera 可以转 Overlay；绝不把其他相机强制转 Base。
            var uiData = _uiCamera.GetUniversalAdditionalCameraData();
            uiData.renderType = CameraRenderType.Overlay;
            var uiRenderer = uiData.scriptableRenderer;
            if (!(uiRenderer is UniversalRenderer) ||
                !uiRenderer.SupportsCameraStackingType(CameraRenderType.Overlay))
            {
                Unbind();
                return;
            }

            var main = FindBaseCamera();
            if (main == null)
            {
                Unbind();
                return;
            }

            // 只为最终选中的游戏主相机补充官方数据，不对枚举到的其他相机增加组件。
            var baseData = main.GetUniversalAdditionalCameraData();
            var renderer = baseData.scriptableRenderer;
            if (baseData.renderType != CameraRenderType.Base || !(renderer is UniversalRenderer) ||
                !renderer.SupportsCameraStackingType(CameraRenderType.Base))
            {
                Unbind();
                return;
            }

            var stack = baseData.cameraStack;
            if (!ReferenceEquals(_boundStack, stack))
            {
                Unbind();
                _boundStack = stack;
            }
            if (_boundStack == null)
                return;

            // 保留第一次出现的位置；只移除自己的重复引用，其他 Overlay 顺序完全不变。
            var first = _boundStack.IndexOf(_uiCamera);
            if (first < 0)
                _boundStack.Add(_uiCamera);
            else
                for (var i = _boundStack.Count - 1; i > first; --i)
                    if (ReferenceEquals(_boundStack[i], _uiCamera))
                        _boundStack.RemoveAt(i);
        }

        private Camera FindBaseCamera()
        {
            var count = Camera.allCamerasCount;
            if (_cameras.Length < count)
                _cameras = new Camera[Mathf.NextPowerOfTwo(count)];
            count = Camera.GetAllCameras(_cameras);
            var activeScene = SceneManager.GetActiveScene();
            Camera selected = null;
            for (var i = 0; i < count; ++i)
            {
                var candidate = _cameras[i];
                if (candidate == null || candidate == _uiCamera || !candidate.isActiveAndEnabled ||
                    candidate.cameraType != CameraType.Game || !candidate.CompareTag("MainCamera") ||
                    !candidate.gameObject.scene.isLoaded || candidate.targetTexture != null ||
                    _uiCamera.targetTexture != null || candidate.targetDisplay != _uiCamera.targetDisplay)
                    continue;

                if (candidate.TryGetComponent<UniversalAdditionalCameraData>(out var data))
                {
                    var renderer = data.scriptableRenderer;
                    if (data.renderType != CameraRenderType.Base || !(renderer is UniversalRenderer) ||
                        !renderer.SupportsCameraStackingType(CameraRenderType.Base))
                        continue;
                }

                // 多场景时优先活动场景，再按 depth 和 instance id 稳定决策。
                // 不把任意无标签相机/离屏相机当成兜底 Base。
                if (selected == null || IsPreferred(candidate, selected, activeScene))
                    selected = candidate;
            }
            // 不让缓冲数组跨帧持有已卸载场景里的相机托管引用。
            for (var i = 0; i < count; ++i)
                _cameras[i] = null;
            return selected;
        }

        private static bool IsPreferred(Camera candidate, Camera current, Scene activeScene)
        {
            var candidateActive = candidate.gameObject.scene == activeScene;
            var currentActive = current.gameObject.scene == activeScene;
            if (candidateActive != currentActive)
                return candidateActive;
            if (candidate.depth != current.depth)
                return candidate.depth > current.depth;
            return candidate.GetInstanceID() < current.GetInstanceID();
        }

        private void Unbind()
        {
            if (_boundStack == null)
                return;
            for (var i = _boundStack.Count - 1; i >= 0; --i)
                if (ReferenceEquals(_boundStack[i], _uiCamera))
                    _boundStack.RemoveAt(i);
            _boundStack = null;
        }
    }
}
