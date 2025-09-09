using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CommonBase;

namespace FishCollection
{
    public class MiniMapPanelBase : BaseUI
    {
        public Image Bg;
        public Image Mask;

        public override void Initialize()
        {
            Bg = transform.Find("Bg").GetComponent<Image>();
            Mask = transform.Find("Mask").GetComponent<Image>();
        }
    }
}
