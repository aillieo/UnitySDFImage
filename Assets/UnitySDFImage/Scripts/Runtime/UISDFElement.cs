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
        }

        internal void PopulateSDFData(List<float> buffer)
        {
            switch (this.shape)
            {
                case Shape.Circle:
                    if (this.transform.parent == null)
                    {
                        throw new InvalidOperationException();
                    }

                    Transform trans = this.transform;
                    RectTransform rect = trans as RectTransform;
                    RectTransform parent = trans.parent as RectTransform;
                    var bounds = RectTransformUtils.CalculateNormalizedRelativeRectTransformBounds(parent, rect);
                    buffer.Add(bounds.center.x);
                    buffer.Add(bounds.center.y);
                    buffer.Add(bounds.size.x);
                    buffer.Add(bounds.size.y);
                    buffer.Add((int)this.operation);
                    break;
            }
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
