using UnityEngine;
using UnityEngine.EventSystems;

namespace FishCollection
{
    public class InventoryViewPanelChildScrollRect : MonoBehaviour, IPointerDownHandler, IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        public InventoryViewPanel inventoryViewPanel;

        public void OnPointerDown(PointerEventData eventData)
        {
            inventoryViewPanel.OnPointerDown(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            inventoryViewPanel.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            inventoryViewPanel.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            inventoryViewPanel.OnEndDrag(eventData);
        }
    }
}