namespace AillieoUtils.UI.SDFImage
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    [ExecuteAlways]
    public class UISDFImage : MaskableGraphic
    {
        public static Shader sdfImageShader;

        public float blendRadius
        {
            get { return blendRadiusValue; }
            set { if (blendRadiusValue != value) { blendRadiusValue = value; material.SetFloat("_BlendRadius", blendRadiusValue); } }
        }

        public float softness
        {
            get { return softnessValue; }
            set { if (softnessValue != value) { softnessValue = value; material.SetFloat("_Softness", softnessValue); } }
        }

        [SerializeField]
        private float blendRadiusValue = 0.1f;

        [SerializeField]
        private float softnessValue = 0f;

        private Material materialSDFImage;

        public override Material material
        {
            get
            {
                if (materialSDFImage == null)
                {
                    if (sdfImageShader == null)
                    {
                        sdfImageShader = Shader.Find("AillieoUtils/SDFImage");
                    }

                    materialSDFImage = new Material(sdfImageShader);
                }

                return materialSDFImage;
            }
        }

        // mode: ShapeBlending Union Intersection Subtraction
        
        protected override void UpdateMaterial()
        {
            base.UpdateMaterial();

            List<Vector4> sdfData = new List<Vector4>();
            foreach (Transform child in transform)
            {
                UISDFElement config = child.GetComponent<UISDFElement>();
                if (config != null)
                {
                    RectTransform childRect = child.transform as RectTransform;

                    var bounds = RectTransformUtils.CalculateNormalizedRelativeRectTransformBounds(this.rectTransform, childRect);
                    sdfData.Add(new Vector4(bounds.center.x, bounds.center.y, Mathf.Min(bounds.size.x, bounds.size.y), (int)config.operation));
                }
            }

            material.SetVectorArray("_CircleDataArray", sdfData);
            material.SetInt("_NumCircles", sdfData.Count);

            material.SetFloat("_BlendRadius", blendRadius);
            material.SetFloat("_Softness", softness);
        }

        private void Update()
        {
            foreach (Transform child in transform)
            {
                if (child.hasChanged && child.gameObject.GetComponent<UISDFElement>() != null)
                {
                    this.SetMaterialDirty();
                    break;
                }
            }
        }
    }
}
