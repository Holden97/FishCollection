using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CommonBase;

namespace FishCollection
{
    public class MainGameHudPanelBase : BaseUI
    {
        public Button Button;
        public virtual void OnButtonClicked() { }
        public TextMeshProUGUI Text__TMP_;

        public override void Initialize()
        {
            Button = transform.Find("Button").GetComponent<Button>();
            Button.onClick.AddListener(OnButtonClicked);
            Text__TMP_ = transform.Find("Button/Text (TMP)").GetComponent<TextMeshProUGUI>();
        }
    }
}
