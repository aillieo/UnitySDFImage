namespace AillieoUtils.UI.SDFImage
{
    using UnityEngine;

    public static class RectTransformUtils
    {
        public static Bounds CalculateNormalizedRelativeRectTransformBounds(RectTransform root, RectTransform child)
        {
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(root, child);
            Rect rootRect = root.rect;
            float parentWidth = rootRect.width;
            float parentHeight = rootRect.height;
            Vector2 rootPivot = root.pivot;
            Vector2 normalizedPos = new Vector2(bounds.center.x / parentWidth + rootPivot.x, bounds.center.y / parentHeight + rootPivot.y);
            Vector2 normalizedSize = new Vector2(bounds.size.x / parentWidth, bounds.size.y / parentHeight);
            Bounds normalizedBounds = new Bounds(normalizedPos, normalizedSize);
            return normalizedBounds;
        }
    }
}
