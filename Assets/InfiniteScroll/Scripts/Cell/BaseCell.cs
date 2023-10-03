using UnityEngine;

namespace UI.InfiniteScroll
{
    [RequireComponent(typeof(RectTransform))]
    public class BaseCell : MonoBehaviour
    {
        public RectTransform RectTransform =>
            _rectTransform ? _rectTransform : (_rectTransform = GetComponent<RectTransform>());

        private RectTransform _rectTransform;
        public float Height => RectTransform.rect.height;
        public float Width => RectTransform.rect.width;


        public virtual void UpdateDisplay(int index)
        {
        }
    }
}