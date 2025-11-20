using UnityEngine;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BLINDED_AM_ME.Objects;

namespace BLINDED_AM_ME.Components
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(PortalCameraController))]
    public class PortalView : MonoBehaviour2
    {
        public enum TextureSize
        {
            [InspectorName("32")] _32 = 32,
            [InspectorName("64")] _64 = 64,
            [InspectorName("128")] _128 = 128,
            [InspectorName("256")] _256 = 256,
            [InspectorName("512")] _512 = 512,
            [InspectorName("1024")] _1024 = 1024,
            [InspectorName("2048")] _2048 = 2048
        }

        [SerializeField]
        private TextureSize _targetTextureSize = TextureSize._512;
        public TextureSize TargetTextureSize
        {
            get => _targetTextureSize;
            set => SetProperty(ref _targetTextureSize, value);
        }

        private RenderTexture _targetTexture;
        public RenderTexture TargetTexture
        {
            get => _targetTexture;
            set => SetProperty(ref _targetTexture, value);
        }

        [SerializeField, HideInInspector]
        private Material _targetMaterial;
        private Material TargetMaterial
        {
            get => _targetMaterial;
            set => SetProperty(ref _targetMaterial, value);
        }

        private Renderer _renderer;
        private PortalCameraController _cameraController;
        private WeakEventListener<PropertyChangedEventArgs> _cameraController_PropertyChangedListener;

        protected override void Awake()
        {
            _cameraController = GetComponent<PortalCameraController>();
            _renderer = GetComponent<Renderer>();
            _renderer.sharedMaterial = TargetMaterial;
            base.Awake();
        }

        protected override void OnEnable()
        {
            _cameraController_PropertyChangedListener?.OptOut();
            _cameraController_PropertyChangedListener = new WeakEventListener<PropertyChangedEventArgs>(CameraController_PropertyChanged);

            if (TryGetComponent(out PortalCameraController cameraController))
                cameraController.PropertyChanged += _cameraController_PropertyChangedListener.Handle;

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            if (TryGetComponent(out PortalCameraController cameraController))
                cameraController.PropertyChanged -= _cameraController_PropertyChangedListener.Handle;

            _cameraController_PropertyChangedListener.OptOut();
            base.OnDisable();
        }

        protected override void Start()
        {
            TargetTexture = CreateTexture((int)TargetTextureSize);
            base.Start();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            switch (propertyName)
            {
                case nameof(TargetTextureSize):
                    var old = TargetTexture;
                    TargetTexture = CreateTexture((int)TargetTextureSize);
                    old?.Release();
                    break;

                case nameof(TargetMaterial):
                case nameof(TargetTexture):
                    if (_cameraController?.Camera != null)
                        _cameraController.Camera.targetTexture = TargetTexture;

                    if (TargetMaterial != null)
                        TargetMaterial.mainTexture = TargetTexture;
                    break;
            }

            base.OnPropertyChanged(propertyName);
        }

        private void CameraController_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is PortalCameraController controller && e.PropertyName == nameof(PortalCameraController.Camera))
            {
                if (controller.Camera != null)
                    controller.Camera.targetTexture = TargetTexture;
            }
        }

        protected override void OnWillRenderObject()
        {
            // Ensure the renderer’s material matches our instance material
            if (_renderer.sharedMaterial != TargetMaterial)
            {
                var mat = _renderer.sharedMaterial;
                if (mat != null)
                {
                    TargetMaterial = new Material(mat);
                    TargetMaterial.name = $"{mat.name} ({TargetMaterial.GetInstanceID()})";
                    _renderer.sharedMaterial = TargetMaterial;
                }
                else
                {
                    TargetMaterial = null;
                }
            }
        }

        private RenderTexture CreateTexture(int size)
        {
            var x = new RenderTexture(size, size, 16, RenderTextureFormat.ARGB32)
            {
                name = "__PortalRenderTexture" + GetInstanceID(),
                hideFlags = HideFlags.DontSave
            };
            x.Create();
            return x;
        }
    }
}
