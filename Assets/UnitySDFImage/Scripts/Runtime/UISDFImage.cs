namespace AillieoUtils.UI.SDFImage
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    [ExecuteAlways]
    public class UISDFImage : MaskableGraphic
    {
        internal bool childrenDirty = true;

        private static readonly int sdfDataBufferId = Shader.PropertyToID("_SDFDataBuffer");
        private static readonly int sdfDataLengthId = Shader.PropertyToID("_SDFDataLength");
        private static readonly int blendRadiusId = Shader.PropertyToID("_BlendRadius");
        private static readonly int softnessId = Shader.PropertyToID("_Softness");

        private static Shader sdfImageShaderValue;
        private readonly List<UISDFElement> managedChildren = new List<UISDFElement>();
        private readonly Vector4[] packedSDFDataBuffer = new Vector4[16];

        private readonly List<float> rawSDFDataBuffer = new List<float>();

        [SerializeField]
        [Range(0.0001f, 1.0f)]
        private float blendRadiusValue = 0.1f;

        private Material materialSDFImage;

        [SerializeField]
        [Range(0, 0.1f)]
        private float softnessValue;

        public static Shader sdfImageShader
        {
            get
            {
                if (sdfImageShaderValue == null)
                {
                    sdfImageShaderValue = Shader.Find("AillieoUtils/SDFImage");
                }

                return sdfImageShaderValue;
            }

            set
            {
                sdfImageShaderValue = value;
            }
        }

        public float blendRadius
        {
            get
            {
                return this.blendRadiusValue;
            }

            set
            {
                if (this.blendRadiusValue != value)
                {
                    this.blendRadiusValue = value;
                    this.material.SetFloat(blendRadiusId, this.blendRadiusValue);
                }
            }
        }

        public float softness
        {
            get
            {
                return this.softnessValue;
            }

            set
            {
                if (this.softnessValue != value)
                {
                    this.softnessValue = value;
                    this.material.SetFloat(softnessId, this.softnessValue);
                }
            }
        }

        public override Material material
        {
            get
            {
                if (this.materialSDFImage == null)
                {
                    this.materialSDFImage = new Material(sdfImageShader);
                }

                return this.materialSDFImage;
            }
        }

        public override void SetAllDirty()
        {
            this.childrenDirty = true;
            base.SetAllDirty();
        }

        protected override void OnEnable()
        {
            this.childrenDirty = true;
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.childrenDirty = true;
            this.managedChildren.Clear();
        }

        protected override void UpdateMaterial()
        {
            base.UpdateMaterial();

            if (this.childrenDirty)
            {
                this.childrenDirty = false;
                this.FindChildren();
            }

            this.rawSDFDataBuffer.Clear();

            foreach (UISDFElement child in this.managedChildren)
            {
                child.PopulateSDFData(this.rawSDFDataBuffer);
            }

            this.PackFloatValues();

            this.material.SetVectorArray(sdfDataBufferId, this.packedSDFDataBuffer);
            this.material.SetInt(sdfDataLengthId, this.rawSDFDataBuffer.Count);

            this.material.SetFloat(blendRadiusId, this.blendRadius);
            this.material.SetFloat(softnessId, this.softness);

            this.rawSDFDataBuffer.Clear();
        }

        private void Update()
        {
            var willSetMaterialDirty = false;

            foreach (UISDFElement child in this.managedChildren)
            {
                if (child.isNotifyingParentDirty)
                {
                    this.childrenDirty = true;
                    willSetMaterialDirty = true;
                }

                child.isNotifyingParentDirty = false;

                if (child.transform.hasChanged)
                {
                    willSetMaterialDirty = true;
                }
            }

            if (willSetMaterialDirty)
            {
                this.SetMaterialDirty();
            }
        }

        private void PackFloatValues()
        {
            var count = this.rawSDFDataBuffer.Count;
            for (var i = 0; i < count; i += 4)
            {
                var x = i < count ? this.rawSDFDataBuffer[i] : 0f;
                var y = i + 1 < count ? this.rawSDFDataBuffer[i + 1] : 0f;
                var z = i + 2 < count ? this.rawSDFDataBuffer[i + 2] : 0f;
                var w = i + 3 < count ? this.rawSDFDataBuffer[i + 3] : 0f;
                this.packedSDFDataBuffer[i / 4] = new Vector4(x, y, z, w);
            }
        }

        private void FindChildren()
        {
            this.managedChildren.Clear();

            foreach (Transform child in this.transform)
            {
                if (child.TryGetComponent(out UISDFElement element))
                {
                    if (!element.isActiveAndEnabled)
                    {
                        continue;
                    }

                    this.managedChildren.Add(element);
                }
            }
        }

        private void OnTransformChildrenChanged()
        {
            this.childrenDirty = true;
            this.SetMaterialDirty();
        }
    }
}
