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
        public float duration = 1f;
        public FishBoid fishBoid;
        public Image hpBarImg;
        public CanvasGroup hpCanvasGroup;

        private void OnEnable()
        {
            hpCanvasGroup.alpha = 0;
            StartCoroutine(FadeIn());
        }

        private void Update()
        {
            transform.rotation = Camera.main.transform.rotation;
            hpBarImg.fillAmount = fishBoid.hpPercent;
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
        private IEnumerator FadeIn()
        {
            if (hpCanvasGroup == null)
            {
                yield break;
            }

            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0, 1, elapsedTime / duration);
                hpCanvasGroup.alpha = alpha;
                yield return null;
            }

            // 确保最终透明度为1
            hpCanvasGroup.alpha = 1;
        }


        // 渐隐方法
        private IEnumerator FadeOut()
        {
            if (hpCanvasGroup == null)
            {
                yield break;
            }

            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, elapsedTime / duration);
                hpCanvasGroup.alpha = alpha;
                yield return null;
            }

            // 确保最终透明度为0
            hpCanvasGroup.alpha = 0;
        }
    }
}