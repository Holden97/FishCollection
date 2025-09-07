using CommonBase;
using UnityEngine;

namespace FishCollection
{
    public class GamingState : BaseState
    {
        KeyCode toggleKey = KeyCode.Tab;

        public GamingState(string stateName, FiniteStateMachine fsm) : base(stateName, fsm)
        {
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();
            if (Input.GetKeyDown(toggleKey))
            {
                bool openWindow = UIManager.Instance.SwitchPanel<InventoryViewPanel>();
                Time.timeScale = openWindow ? 0f : 1f;
            }
        }

        // public void ToggleInventory()
        // {
        //     !isInventoryOpen;
        //
        //     if (inventoryPanel != null)
        //     {
        //         inventoryPanel.SetActive(isInventoryOpen);
        //     }
        //
        //     // Pause/Resume game
        //     Time.timeScale = isInventoryOpen ? 0f : 1f;
        // }
        //
        // public void OpenInventory()
        // {
        //     isInventoryOpen = true;
        //     if (inventoryPanel != null)
        //     {
        //         inventoryPanel.SetActive(true);
        //     }
        //
        //     Time.timeScale = 0f;
        // }
        //
        // public void CloseInventory()
        // {
        //     isInventoryOpen = false;
        //     if (inventoryPanel != null)
        //     {
        //         inventoryPanel.SetActive(false);
        //     }
        //
        //     Time.timeScale = 1f;
        // }
    }
}