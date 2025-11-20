using System.Collections;
using System.ComponentModel;
using UnityEngine;
using BLINDED_AM_ME.Objects;

namespace BLINDED_AM_ME.Components
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class PortalCameraController : MonoBehaviour2
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _linkedPortal;

        // Public properties (keeps PropertyChanged from base MonoBehaviour2)
        public Camera Camera
        {
            get => _camera;
            set
            {
                if (_camera == value) return;
                _camera = value;
                // notify base propertychanged if needed (MonoBehaviour2 likely provides it)
                OnPropertyChanged(nameof(Camera));
            }
        }

        public Transform LinkedPortal
        {
            get => _linkedPortal;
            set
            {
                if (_linkedPortal == value) return;
                _linkedPortal = value;
                OnPropertyChanged(nameof(LinkedPortal));
            }
        }

        // internal control
        private bool _needsRender;
        private Coroutine _renderCoroutine;

        protected override void Awake()
        {
            base.Awake();

            if (_camera == null)
                _camera = GetComponentInChildren<Camera>() ?? GetComponent<Camera>();

            if (_camera == null)
            {
                var go = new GameObject("PortalCamera");
                go.transform.SetParent(transform, false);
                _camera = go.AddComponent<Camera>();
            }

            // Prevent URP / engine from auto-rendering this camera.
            // We'll render it manually at end of frame to avoid nested URP render calls.
            _camera.enabled = false;
        }

        // Update the portal camera transform when the portal is being rendered
        protected override void OnWillRenderObject()
        {
            // `Camera.current` is the camera currently driving the render (the viewer camera)
            var viewer = Camera.current;
            if (viewer == null) return;
            if (_camera == null || _linkedPortal == null) return;
            if (viewer == _camera) return; // don't reflect the portal camera itself

            // compute the portal camera transform to match viewer through portal:
            // worldToLocal(viewer) into linked portal space -> apply to this portal
            // formula: m = transform.localToWorldMatrix * linkedPortal.worldToLocalMatrix * viewer.localToWorldMatrix
            Matrix4x4 localToWorld = transform.localToWorldMatrix;
            Matrix4x4 otherWorldToLocal = _linkedPortal.worldToLocalMatrix;
            Matrix4x4 viewerLocalToWorld = viewer.transform.localToWorldMatrix;
            Matrix4x4 m = localToWorld * otherWorldToLocal * viewerLocalToWorld;

            Vector3 pos = m.GetColumn(3);
            Quaternion rot = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));

            _camera.transform.SetPositionAndRotation(pos, rot);

            // copy projection settings if you want matching FOV, near/far etc.
            _camera.fieldOfView = viewer.fieldOfView;
            _camera.nearClipPlane = viewer.nearClipPlane;
            _camera.farClipPlane = viewer.farClipPlane;
            _camera.aspect = viewer.aspect;

            // make sure the portal camera is rendering into the assigned target texture
            // (PortalView sets camera.targetTexture when the PortalView property changes)
            // don't change enabled state here — we only schedule a render
            _needsRender = true;
        }

        private void LateUpdate()
        {
            // If we need a portal update, start a coroutine that will render it at end of frame.
            if (_needsRender && _renderCoroutine == null)
            {
                _renderCoroutine = StartCoroutine(RenderAtEndOfFrame());
            }
        }

        private IEnumerator RenderAtEndOfFrame()
        {
            // Wait until the frame's render is finished — this ensures we are outside URP's render loop.
            yield return new WaitForEndOfFrame();

            // double-check
            if (_camera == null)
            {
                _renderCoroutine = null;
                _needsRender = false;
                yield break;
            }

            // Only render if a targetTexture exists (PortalView should set this).
            if (_camera.targetTexture != null)
            {
                // Temporarily enable camera so Camera.Render will do work, then disable afterwards.
                // We do this because some pipeline code checks camera.enabled.
                bool prevEnabled = _camera.enabled;
                _camera.enabled = true;

                try
                {
                    // This call performs a manual render to the camera.targetTexture.
                    // Because it's run after WaitForEndOfFrame(), it won't be called while URP
                    // is mid-render and should avoid the 'UniversalCameraData has already been created' error.
                    _camera.Render();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"PortalCameraController: exception while rendering portal camera: {ex}");
                }
                finally
                {
                    // restore enabled state (we keep it false so URP doesn't auto render this camera)
                    _camera.enabled = prevEnabled;
                }
            }

            _needsRender = false;
            _renderCoroutine = null;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_renderCoroutine != null)
            {
                StopCoroutine(_renderCoroutine);
                _renderCoroutine = null;
            }
            _needsRender = false;
        }

        // Helper to raise PropertyChanged on the base MonoBehaviour2
        private void OnPropertyChanged(string name)
        {
            // If MonoBehaviour2 supplies protected OnPropertyChanged, call that
            // Otherwise, if it uses an event, adapt to it. We'll call protected virtual if present:
            var method = typeof(MonoBehaviour2).GetMethod("OnPropertyChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (method != null)
                method.Invoke(this, new object[] { name });
        }
    }
}
