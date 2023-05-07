namespace AillieoUtils.UI.SDFImage
{
    using UnityEngine.UI;

    public class UISDFElement : Graphic
    {
        // todo shape : circle, rect, triangle

        public enum SDFOperation
        {
            Union,
            Intersection,
            Subtraction
        }

        public float radius = 0.5f;
        public SDFOperation operation = SDFOperation.Union;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }

        public override bool raycastTarget
        {
            get
            {
                return false;
            }
        }
    }
}
