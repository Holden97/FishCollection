using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FishCollection
{
    public class CuteButton : Button
    {
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            this.transform.DOScale(1.2f, 0.25f).SetEase(Ease.OutQuad);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            this.transform.DOScale(1, 0.25f).SetEase(Ease.OutQuad);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            this.transform.DOScale(1, 0.1f).SetEase(Ease.OutQuad);
        }
    }
}