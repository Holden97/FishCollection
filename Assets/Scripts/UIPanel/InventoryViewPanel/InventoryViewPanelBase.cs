using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CommonBase;

namespace FishCollection
{
    public class InventoryViewPanelBase : BaseUI
    {
        public Image InventoryView;
        public Image Viewport;
        public TextMeshProUGUI Capcity;
        public TextMeshProUGUI Capcity_1;
        public TextMeshProUGUI Occupy;
        public TextMeshProUGUI Occupy_2;
        public Button OrganizeBtn;
        public virtual void OnOrganizeBtnClicked() { }
        public TextMeshProUGUI Text__TMP_;
        public Button CloseBtn;
        public virtual void OnCloseBtnClicked() { }
        public TextMeshProUGUI Text__TMP__2;

        public override void Initialize()
        {
            InventoryView = transform.Find("InventoryView").GetComponent<Image>();
            Viewport = transform.Find("InventoryView/Viewport").GetComponent<Image>();
            Capcity = transform.Find("Capcity").GetComponent<TextMeshProUGUI>();
            Capcity_1 = transform.Find("Capcity/Capcity_1").GetComponent<TextMeshProUGUI>();
            Occupy = transform.Find("Occupy").GetComponent<TextMeshProUGUI>();
            Occupy_2 = transform.Find("Occupy/Occupy").GetComponent<TextMeshProUGUI>();
            OrganizeBtn = transform.Find("OrganizeBtn").GetComponent<Button>();
            OrganizeBtn.onClick.AddListener(OnOrganizeBtnClicked);
            Text__TMP_ = transform.Find("OrganizeBtn/Text (TMP)").GetComponent<TextMeshProUGUI>();
            CloseBtn = transform.Find("CloseBtn").GetComponent<Button>();
            CloseBtn.onClick.AddListener(OnCloseBtnClicked);
            Text__TMP__2 = transform.Find("CloseBtn/Text (TMP)").GetComponent<TextMeshProUGUI>();
        }
    }
}
