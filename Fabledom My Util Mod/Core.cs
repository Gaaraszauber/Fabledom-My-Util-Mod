using MelonLoader;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Nielsen;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(Fabledom_My_Util_Mod.Core), "Fabledom My Util Mod", "0.0.1", "Gaaraszauber", null)]
[assembly: MelonGame("Grenaa Games", "Fabledom")]

namespace Fabledom_My_Util_Mod
{
    public class Core : MelonMod
    {
        public bool open = false;
        private int selectedTab = 0;
        private string[] tabNames = new string[] { "General", "Give Cheats", "Extra" };
        private int windowHeight = 400;
        private int windowWidth = 600;
        private Rect windowRect = new Rect(500, 50, 600, 400);

        private GameObject __Singletons;
        private DataManager __myDataManager;
        private GameManager __myGameManager;
        private KingdomManager __myKingdomManager;
        private SeasonController __mySeasonController;

        private bool toggleDevMod = false;
        private bool toggleDevModOld = false;
        private bool toggleUnlockAll = false;
        private string inputItemAmount = "1";
        private Vector2 scrollPosition;
        private bool InitializeReferencesDone = false;
        private bool unlockALLDone = false;
        private int Amount = 0;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Fabledom Util Mod is Initialized.");
        }

        private void InitializeReferences()
        {
            LoggerInstance.Msg("Initialize References loading");
            __Singletons = GameObject.Find("Singletons");
            __myGameManager = GameObject.FindObjectOfType<GameManager>();
            __myDataManager = GameObject.FindObjectOfType<DataManager>();
            __myKingdomManager = GameObject.FindObjectOfType<KingdomManager>();
            __mySeasonController = GameObject.FindObjectOfType<SeasonController>();
            InitializeReferencesDone = true;
            LoggerInstance.Msg("Initialize References finished.");
        }

        public override void OnUpdate()
        {
            if (SceneManager.GetActiveScene().name == "Game - Standard")
            {
                if (!InitializeReferencesDone)
                {
                    InitializeReferences();
                }
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                ToggleMenu();
            }
            UpdateMyValues();
        }

        private void ToggleMenu()
        {
            open = !open;
            if (open)
            {
                MelonEvents.OnGUI.Subscribe(DrawMenu, 1);
            }
            else
            {
                MelonEvents.OnGUI.Unsubscribe(DrawMenu);
            }
        }

        private void UpdateMyValues()
        {
            if (__myGameManager != null)
            {
                if (toggleDevMod != toggleDevModOld)
                {
                    __myGameManager.isDevBuild = toggleDevMod;
                    LoggerInstance.Msg($"Dev mode set to: {__myGameManager.isDevBuild}");
                    toggleDevModOld = !toggleDevModOld;
                }
            }
            if (toggleUnlockAll)
            {
                if (!unlockALLDone)
                {
                    LoggerInstance.Msg("Starting UnlockALL coroutine");
                    MelonCoroutines.Start(UnlockALLCoroutine());
                    unlockALLDone = true;
                }
            }
        }

        private void DrawMenu()
        {
            GUI.skin.window.padding = new RectOffset(10, 10, 20, 10);
            windowRect = GUI.Window(0, windowRect, DrawWindowContents, "Fabledom Util Mod by Gaaraszauber");
        }

        private void DrawWindowContents(int windowID)
        {
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            switch (selectedTab)
            {
                case 0: DrawGeneralTab(); break;
                case 1: DrawGiveCheatsTab(); break;
                case 2: DrawExtraTab(); break;
            }
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void DrawGeneralTab()
        {
            GUILayout.Label("Generally Cheats", GUI.skin.box);
            toggleDevMod = GUILayout.Toggle(toggleDevMod, "Dev Mode | Status: " + __myGameManager.isDevBuild);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // Fügt horizontalen Platz links hinzu

            GUILayout.BeginVertical(GUILayout.Width(450)); // Gesamtbreite der Buttons

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Unlock All", GUILayout.Width(150)))
            {
                if (!unlockALLDone)
                {
                    LoggerInstance.Msg("Starting UnlockALL coroutine");
                    MelonCoroutines.Start(UnlockALLCoroutine());
                    unlockALLDone = true;
                }
            }
            if (GUILayout.Button("Unlock all Ruler", GUILayout.Width(150)))
            {
                WorldMapGameplayManager.Instance.DebugShowAllRulers();
            }
            if (GUILayout.Button("Unlock all Equipment", GUILayout.Width(150)))
            {
                for (int i = 0; i < DataManager.Instance.equipmentSoData.Count; i++)
                {
                    DataManager.Instance.ReceiveEquipment(DataManager.Instance.equipmentSoData[i].key);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace(); // Fügt horizontalen Platz rechts hinzu
            GUILayout.EndHorizontal();
            GUILayout.Label("Change Seasons", GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // Fügt horizontalen Platz links hinzu

            GUILayout.BeginVertical(GUILayout.Width(400)); // Gesamtbreite der Buttons

            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Spring", GUILayout.Width(100))) { ChangeSeason(0); }
            if (GUILayout.Button("Summer", GUILayout.Width(100))) { ChangeSeason(1); }
            if (GUILayout.Button("Fall", GUILayout.Width(100))) { ChangeSeason(2); }
            if (GUILayout.Button("Winter", GUILayout.Width(100))) { ChangeSeason(3); }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace(); // Fügt horizontalen Platz rechts hinzu
            GUILayout.EndHorizontal();

        }

        private void DrawGiveCheatsTab()
        {
            if (__myDataManager == null) return;

            GUILayout.Label("Give Cheats", GUI.skin.box);
            var dataList = __myDataManager.resourceSoData;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Amount:", GUILayout.Width(60));
            inputItemAmount = GUILayout.TextField(inputItemAmount, GUILayout.Width(50));
            GUILayout.EndHorizontal();
            GUILayout.Label("Villager", GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // Fügt horizontalen Platz links hinzu

            GUILayout.BeginVertical(GUILayout.Width(400)); // Gesamtbreite der Buttons

            
            GUILayout.BeginHorizontal();
            if (int.TryParse(inputItemAmount, out int itemAmount))
            {
                Amount = itemAmount;
            }
            if (GUILayout.Button("Fablings", GUILayout.Width(100)))
            {
                KingdomManager.Instance.ForceImmigration(Amount);
            }
            if (GUILayout.Button("Peasants", GUILayout.Width(100)))
            {
                KingdomManager.Instance.ForceImmigration(FablingClass.PEASANT, Amount);
            }
            if (GUILayout.Button("Commoners", GUILayout.Width(100)))
            {
                KingdomManager.Instance.ForceImmigration(FablingClass.COMMONER, Amount);
            }
            if (GUILayout.Button("Nobles", GUILayout.Width(100)))
            {
                KingdomManager.Instance.ForceImmigration(FablingClass.NOBLE, Amount);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace(); // Fügt horizontalen Platz rechts hinzu
            GUILayout.EndHorizontal();


            GUILayout.Label("Items", GUI.skin.box);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            int buttonsPerRow = 5;
            int buttonWidth = (windowWidth - 30) / buttonsPerRow - 4;
            for (int i = 0; i < dataList.Count; i++)
            {
                if (i % buttonsPerRow == 0)
                {
                    GUILayout.BeginHorizontal();
                }
                if (GUILayout.Button(dataList[i].name, GUILayout.Width(buttonWidth)))
                {
                    GiveItem(dataList[i].key, inputItemAmount);
                }
                if (i % buttonsPerRow == buttonsPerRow - 1 || i == dataList.Count - 1)
                {
                    if (i < 25) {
                        GUILayout.EndHorizontal();
                    }
                    
                }
            }
            if (GUILayout.Button("Nobility", GUILayout.Width(buttonWidth)))
            {
                KingdomManager.Instance.AlterNobility(Amount);
            }
            if (GUILayout.Button("Fort", GUILayout.Width(buttonWidth)))
            {
                KingdomManager.Instance.AlterFortification(Amount);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }

        private void GiveItem(string itemName, string amount)
        {
            if (int.TryParse(amount, out int itemAmount))
            {
                LoggerInstance.Msg($"Giving {itemAmount} of {itemName}");
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
                Debug.LogError("Invalid amount entered");
            }
        }

        private void DrawExtraTab()
        {
            GUILayout.Label("Extra Functions", GUI.skin.box);
            GUILayout.Label("Spawner", GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // Fügt horizontalen Platz links hinzu

            GUILayout.BeginVertical(GUILayout.Width(400)); // Gesamtbreite der Buttons

            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Troll Camp", GUILayout.Width(100)))
            {
                KingdomManager.Instance.SpawnTrollCamp();
            }
            if (GUILayout.Button("Dragon", GUILayout.Width(100)))
            {
                KingdomManager.Instance.SpawnDragon();
            }
            if (GUILayout.Button("Witches", GUILayout.Width(100)))
            {
                KingdomManager.Instance.SpawnWickedWitches();
            }
            if (GUILayout.Button("Fish", GUILayout.Width(100)))
            {
                KingdomManager.Instance.SpawnFish();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace(); // Fügt horizontalen Platz rechts hinzu
            GUILayout.EndHorizontal();

        }

        private IEnumerator UnlockALLCoroutine()
        {
            GameManager.Instance.debugUnlocked = true;
            UIManager.Instance.UpdateUnlockableUi();
            TerritoryManager.Instance.UnlockAll();
            UIManager.Instance.kingdomInfoContainer.ShowFeastSuppliesContainer();
            UIManager.Instance.kingdomInfoContainer.ShowActiveMissionsContainer();
            UIManager.Instance.kingdomInfoContainer.ShowNobilityContainer();
            UIManager.Instance.kingdomInfoContainer.ShowFortificationContainer();

            yield return null;

            PlaceConstructableButton[] buttons = UIManager.Instance.buildBarContent.GetComponentsInChildren<PlaceConstructableButton>(true);
            foreach (var button in buttons)
            {
                button.UpdateStatus();
                yield return null;
            }
            toggleUnlockAll = false;
            yield return null;
        }

        private void ChangeSeason(int SeasonID)
        {
            DateTimeManager.Instance.SetSeason((Season)SeasonID);
        }
    }
}
