using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CommonBase;

namespace FishCollection
{
    public class InventoryViewPanelBase : BaseUI
    {
        public Image Viewport;

        public void Start()
        {
            Viewport = transform.Find("Viewport").GetComponent<Image>();
        }
    }
}
