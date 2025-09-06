using UnityEngine;

namespace UI
{
    public class InventoryUIController : MonoBehaviour
    {
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
        
        private bool isInventoryOpen = false;
        
        void Start()
        {
            if (inventoryPanel == null)
            {
                CreateDefaultInventoryPanel();
            }
            
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
        }
        
        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleInventory();
            }
        }
        
        public void ToggleInventory()
        {
            isInventoryOpen = !isInventoryOpen;
            
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(isInventoryOpen);
            }
            
            // Pause/Resume game
            Time.timeScale = isInventoryOpen ? 0f : 1f;
        }
        
        public void OpenInventory()
        {
            isInventoryOpen = true;
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(true);
            }
            Time.timeScale = 0f;
        }
        
        public void CloseInventory()
        {
            isInventoryOpen = false;
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
            Time.timeScale = 1f;
        }
        
        void CreateDefaultInventoryPanel()
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                canvas = new GameObject("Canvas");
                canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            
            inventoryPanel = new GameObject("InventoryPanel");
            inventoryPanel.transform.SetParent(canvas.transform, false);
            
            var rectTransform = inventoryPanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(600, 400);
            rectTransform.anchoredPosition = Vector2.zero;
            
            var image = inventoryPanel.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
        }
    }
}