using System;
using System.Collections;
using FishCollection;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace FishCollection
{
    public class HpBar : MonoBehaviour
    {
        public FishBoid fishBoid;
        public Image hpBarImg;
        public CanvasGroup hpCanvasGroup;
        public Coroutine hpCoroutine;

        private void OnEnable()
        {
            hpCanvasGroup.alpha = 0;
        }

        public Coroutine ShowHpBar()
        {
            return StartCoroutine(Fade(1, 1));
        }

        private void Update()
        {
            transform.rotation = Camera.main.transform.rotation;
            hpBarImg.fillAmount = fishBoid.hpPercent;
            if (hpBarImg.fillAmount < 1 && hpBarImg.fillAmount > 0 && hpCoroutine == null && !Mathf.Approximately(hpCanvasGroup.alpha, 1f))
            {
                hpCoroutine = ShowHpBar();
            }

            if (hpBarImg.fillAmount < 0.3f)
            {
                hpBarImg.color = Color.red;
            }
            else if (hpBarImg.fillAmount < 0.5f)
            {
                hpBarImg.color = Color.yellow;
            }
            else
            {
                hpBarImg.color = Color.green;
            }
        }


        // 渐显方法
        private IEnumerator Fade(float targetAlpha, float duration)
        {
            if (hpCanvasGroup == null)
            {
                yield break;
            }

            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0, targetAlpha, elapsedTime / duration);
                hpCanvasGroup.alpha = alpha;
                yield return null;
            }

            hpCoroutine = null;
        }
    }
}