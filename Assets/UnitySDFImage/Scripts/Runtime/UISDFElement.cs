namespace AillieoUtils.UI.SDFImage
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class UISDFElement : MonoBehaviour
    {
        public SDFOperation operation;

        public Shape shape;

        internal bool isNotifyingParentDirty;

        [Range(3, 20)]
        public int n = 3;

        [Range(0, Mathf.PI)]
        public float startAngle = Mathf.PI / 2f;

        public enum SDFOperation
        {
            Union,
            Intersection,
            Subtraction,
            ShapeBlending,
        }

        public enum Shape
        {
            Circle,
            Rect,
            RegularPolygon,
        }

        internal void PopulateSDFData(List<float> buffer)
        {
            if (this.transform.parent == null)
            {
                throw new InvalidOperationException();
            }

            Transform trans = this.transform;
            RectTransform rect = trans as RectTransform;
            RectTransform parent = trans.parent as RectTransform;
            var bounds = RectTransformUtils.CalculateNormalizedRelativeRectTransformBounds(parent, rect);

            buffer.Add((int)this.shape);

            switch (this.shape)
            {
                case Shape.Circle:
                    buffer.Add(bounds.center.x);
                    buffer.Add(bounds.center.y);
                    buffer.Add(bounds.size.x);
                    buffer.Add(bounds.size.y);
                    break;
                case Shape.Rect:
                    buffer.Add(bounds.center.x);
                    buffer.Add(bounds.center.y);
                    buffer.Add(bounds.size.x);
                    buffer.Add(bounds.size.y);
                    break;
                case Shape.RegularPolygon:
                    buffer.Add(bounds.center.x);
                    buffer.Add(bounds.center.y);
                    buffer.Add(bounds.size.x);
                    buffer.Add(bounds.size.y);
                    buffer.Add(this.n);
                    buffer.Add(this.startAngle);
                    break;
                default:
                    throw new NotImplementedException();
            }

            buffer.Add((int)this.operation);
        }

        private void OnTransformParentChanged()
        {
            this.NotifyOldParentDirty();
            this.NotifyNewParentDirty();
        }

        private void OnEnable()
        {
            this.NotifyNewParentDirty();
        }

        private void OnDisable()
        {
            this.NotifyOldParentDirty();
        }

        private void NotifyOldParentDirty()
        {
            this.isNotifyingParentDirty = true;
        }

        private void NotifyNewParentDirty()
        {
            if (this.transform.parent == null)
            {
                return;
            }

            if (this.transform.parent.TryGetComponent(out UISDFImage image))
            {
                image.childrenDirty = true;
            }
        }
    }
}
