using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace Fabledom_My_Util_Mod
{
    internal class Utils
    {
        private Core core;
        public Utils(Core core) { this.core = core; }
        public void UnlockAll()
        {
            GameManager.Instance.debugUnlocked = true;
            UIManager.Instance.UpdateUnlockableUi();
            TerritoryManager.Instance.UnlockAll();
            UIManager.Instance.kingdomInfoContainer.ShowFeastSuppliesContainer();
            UIManager.Instance.kingdomInfoContainer.ShowActiveMissionsContainer();
            UIManager.Instance.kingdomInfoContainer.ShowNobilityContainer();
            UIManager.Instance.kingdomInfoContainer.ShowFortificationContainer();

            PlaceConstructableButton[] buttons = UIManager.Instance.buildBarContent.GetComponentsInChildren<PlaceConstructableButton>(true);
            foreach (var button in buttons)
            {
                button.UpdateStatus();
            }
        }

        public void UnlockAllEquipment()
        {
            for (int i = 0; i < DataManager.Instance.equipmentSoData.Count; i++)
            {
                DataManager.Instance.ReceiveEquipment(DataManager.Instance.equipmentSoData[i].key);
            }
        }
        public void ChangeSeason(int seasonID) { DateTimeManager.Instance.SetSeason((Season)seasonID); }
        public void UnlockAllRulers() { WorldMapGameplayManager.Instance.DebugShowAllRulers(); }
        public void SpawnVillagers(FablingClass fablingClass, int amount) { KingdomManager.Instance.ForceImmigration(fablingClass, amount); }
        public void SpawnVillagers(int amount) { KingdomManager.Instance.ForceImmigration(amount); }
        public void AlterNobility(int amount) { KingdomManager.Instance.AlterNobility(amount); }
        public void AlterFortification(int amount) { KingdomManager.Instance.AlterFortification(amount); }
        public void SpawnTrollCamp() { KingdomManager.Instance.SpawnTrollCamp(); }
        public void SpawnDragon() { KingdomManager.Instance.SpawnDragon(); }
        public void SpawnWickedWitches() { KingdomManager.Instance.SpawnWickedWitches(); }
        public void SpawnFish() { KingdomManager.Instance.SpawnFish(); }

        public void GiveItem(string itemName, string amount)
        {
            if (int.TryParse(amount, out int itemAmount))
            {
                if (itemName == "coin")
                {
                    Utils_Resources.AddCoins(itemAmount);
                }
                else if (itemName == "feastSupplies")
                {
                    Utils_Resources.AddFeastSupplies(itemAmount);
                }
                else
                {
                    DebugWindow.ADD_RESOURCE.Invoke(itemName, itemAmount);
                }
            }
            else
            {
                core.LoggerInstance.Error("Invalid amount entered");
            }
        }
    }
}
