using System;
using UnityEngine;
using CommonBase;
using DG.Tweening;
using UnityEngine.UI;

namespace FishCollection
{
    public class MainGameHudPanel : MainGameHudPanelBase
    {
        public GameObject fishImg;

        public override void Initialize()
        {
            base.Initialize();
            this.Button.onClick.AddListener(OnClickBag);
        }

        private void OnEnable()
        {
            this.EventRegister<SpecialFish>(GameEvent.SpecialFishEaten, OnFishEaten);
        }

        private void OnFishEaten(SpecialFish obj)
        {
            if (obj != null)
                FlyToUI(obj.gameObject);
        }

        private void OnClickBag()
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(this.Button.transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad)) // 放大超过1
                .Append(this.Button.transform.DOScale(1.2f, 0.1f).SetEase(Ease.InQuad));
            UIManager.Instance.ShowPanel<InventoryViewPanel>();
        }

        public void FlyToUI(GameObject worldObj)
        {
            // 1. 获取世界坐标对应屏幕位置
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldObj.transform.position);

            // 2. 转换为 Canvas 本地坐标
            RectTransform canvasRect = this.GetComponent<RectTransform>();
            Vector2 uiPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                null,
                out uiPos
            );

            // 3. 创建 UI 图标克隆
            GameObject flyIcon = GameObject.Instantiate(fishImg, transform, false);
            var img = flyIcon.GetComponent<Image>();
            RectTransform rt = flyIcon.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(50, 50); // 大小可调
            rt.localPosition = uiPos;
            rt.localScale = Vector3.one * 0.5f;
            // 4. 飞行动画 + 缩放 + 渐隐
            Sequence seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(rt.DOScale(1.2f, .1f).SetEase(Ease.OutBack)) // 放大动画
                .Append(rt.DOScale(0.8f, .1f)) // 缩小
                .Join(rt.DOLocalMove(this.Button.transform.localPosition, .5f).SetEase(Ease.InQuad))
                // .Join(img.DOFade(0f, moveDuration))
                .OnComplete(() =>
                {
                    Destroy(flyIcon);

                    // 5. targetUI 弹动效果
                    this.Button.transform.DOPunchScale(Vector3.one * 0.3f, .5f, 1, 0.5f);
                });
        }
    }
}