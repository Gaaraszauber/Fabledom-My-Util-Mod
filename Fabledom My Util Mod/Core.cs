using MelonLoader;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Nielsen;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(Fabledom_My_Util_Mod.Core), "Fabledom My Util Mod", "0.0.2", "Gaaraszauber", null)]
[assembly: MelonGame("Grenaa Games", "Fabledom")]

namespace Fabledom_My_Util_Mod
{
    public class Core : MelonMod
    {
        private const int WINDOW_HEIGHT = 400;
        private const int WINDOW_WIDTH = 600;
        private const int BUTTONS_PER_ROW = 5;

        private bool _open = false;
        private int _selectedTab = 0;
        private string[] _tabNames = new string[] { "General", "Give Cheats", "Extra" };
        private Rect _windowRect = new Rect(500, 50, WINDOW_WIDTH, WINDOW_HEIGHT);
        private GameObject _singletons;
        private DataManager _dataManager;
        private GameManager _gameManager;
        private KingdomManager _kingdomManager;
        private SeasonController _seasonController;
        private bool _toggleDevMod = false;
        private bool _toggleDevModOld = false;
        private bool _toggleUnlockAll = false;
        private string _inputItemAmount = "1";
        private Vector2 _scrollPosition;
        private bool _initializeReferencesDone = false;
        private bool _unlockAllDone = false;
        private int _amount = 0;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Fabledom Util Mod is Initialized.");
        }

        private void InitializeReferences()
        {
            LoggerInstance.Msg("Initialize References loading");
            _singletons = GameObject.Find("Singletons");
            _gameManager = GameObject.FindObjectOfType<GameManager>();
            _dataManager = GameObject.FindObjectOfType<DataManager>();
            _kingdomManager = GameObject.FindObjectOfType<KingdomManager>();
            _seasonController = GameObject.FindObjectOfType<SeasonController>();
            _initializeReferencesDone = true;
            LoggerInstance.Msg("Initialize References finished.");
        }

        public override void OnUpdate()
        {
            if (SceneManager.GetActiveScene().name == "Game - Standard")
            {
                if (!_initializeReferencesDone)
                {
                    InitializeReferences();
                }
                if (Input.GetKeyDown(KeyCode.F4))
                {
                    ToggleMenu();
                }
                UpdateMyValues();
            }
        }

        private void ToggleMenu()
        {
            _open = !_open;
            if (_open)
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
            if (_gameManager != null && _toggleDevMod != _toggleDevModOld)
            {
                _gameManager.isDevBuild = _toggleDevMod;
                LoggerInstance.Msg($"Dev mode set to: {_gameManager.isDevBuild}");
                _toggleDevModOld = !_toggleDevModOld;
            }

            if (_toggleUnlockAll && !_unlockAllDone)
            {
                LoggerInstance.Msg("Starting UnlockALL coroutine");
                MelonCoroutines.Start(UnlockAllCoroutine());
                _unlockAllDone = true;
            }
        }

        private void DrawMenu()
        {
            GUI.skin.window.padding = new RectOffset(10, 10, 20, 10);
            _windowRect = GUI.Window(0, _windowRect, DrawWindowContents, "Fabledom Util Mod by Gaaraszauber");
        }

        private void DrawWindowContents(int windowID)
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            switch (_selectedTab)
            {
                case 0:
                    DrawGeneralTab();
                    break;
                case 1:
                    DrawGiveCheatsTab();
                    break;
                case 2:
                    DrawExtraTab();
                    break;
            }
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void DrawGeneralTab()
        {
            GUILayout.Label("Generally Cheats", GUI.skin.box);
            _toggleDevMod = GUILayout.Toggle(_toggleDevMod, "Dev Mode | Status: " + _gameManager.isDevBuild);

            DrawCenteredButtons(new string[] { "Unlock All", "Unlock all Ruler", "Unlock all Equipment" },
                new System.Action[] { UnlockAll, UnlockAllRulers, UnlockAllEquipment }, 450);

            GUILayout.Label("Change Seasons", GUI.skin.box);
            DrawCenteredButtons(new string[] { "Spring", "Summer", "Fall", "Winter" },
                new System.Action[] { () => ChangeSeason(0), () => ChangeSeason(1), () => ChangeSeason(2), () => ChangeSeason(3) }, 400);
        }

        private void DrawGiveCheatsTab()
        {
            if (_dataManager == null) return;

            GUILayout.Label("Give Cheats", GUI.skin.box);
            var dataList = _dataManager.resourceSoData;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Amount:", GUILayout.Width(60));
            _inputItemAmount = GUILayout.TextField(_inputItemAmount, GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.Label("Villager", GUI.skin.box);
            DrawCenteredButtons(new string[] { "Fablings", "Peasants", "Commoners", "Nobles" },
                new System.Action[] { () => SpawnVillagers(), () => SpawnVillagers(FablingClass.PEASANT),
                                      () => SpawnVillagers(FablingClass.COMMONER), () => SpawnVillagers(FablingClass.NOBLE) }, 400);

            GUILayout.Label("Items", GUI.skin.box);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            int buttonWidth = (WINDOW_WIDTH - 30) / BUTTONS_PER_ROW - 4;

            for (int i = 0; i < dataList.Count; i++)
            {
                if (i % BUTTONS_PER_ROW == 0) GUILayout.BeginHorizontal();

                if (GUILayout.Button(dataList[i].name, GUILayout.Width(buttonWidth)))
                {
                    GiveItem(dataList[i].key, _inputItemAmount);
                }

                if (i % BUTTONS_PER_ROW == BUTTONS_PER_ROW - 1 || i == dataList.Count - 1)
                {
                    if (i < 25) GUILayout.EndHorizontal();
                }
            }


            if (GUILayout.Button("Nobility", GUILayout.Width(buttonWidth)))
            {
                AlterNobility();
            }
            if (GUILayout.Button("Fort", GUILayout.Width(buttonWidth)))
            {
                AlterFortification();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }

        private void DrawExtraTab()
        {
            GUILayout.Label("Extra Functions", GUI.skin.box);
            GUILayout.Label("Spawner", GUI.skin.box);
            DrawCenteredButtons(new string[] { "Troll Camp", "Dragon", "Witches", "Fish" },
                new System.Action[] { SpawnTrollCamp, SpawnDragon, SpawnWickedWitches, SpawnFish }, 400);
        }

        private void DrawCenteredButtons(string[] buttonTexts, System.Action[] actions, int totalWidth)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(GUILayout.Width(totalWidth));
            GUILayout.BeginHorizontal();

            for (int i = 0; i < buttonTexts.Length; i++)
            {
                if (GUILayout.Button(buttonTexts[i], GUILayout.Width(totalWidth / buttonTexts.Length)))
                {
                    actions[i]?.Invoke();
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void GiveItem(string itemName, string amount)
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
                LoggerInstance.Error("Invalid amount entered");
            }
        }

        private IEnumerator UnlockAllCoroutine()
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

            _toggleUnlockAll = false;
            yield return null;
        }

        private void ChangeSeason(int seasonID)
        {
            DateTimeManager.Instance.SetSeason((Season)seasonID);
        }

        private void UnlockAll()
        {
            if (!_unlockAllDone)
            {
                LoggerInstance.Msg("Starting UnlockALL coroutine");
                MelonCoroutines.Start(UnlockAllCoroutine());
                _unlockAllDone = true;
            }
        }

        private void UnlockAllRulers()
        {
            WorldMapGameplayManager.Instance.DebugShowAllRulers();
        }

        private void UnlockAllEquipment()
        {
            for (int i = 0; i < DataManager.Instance.equipmentSoData.Count; i++)
            {
                DataManager.Instance.ReceiveEquipment(DataManager.Instance.equipmentSoData[i].key);
            }
        }

        private void SpawnVillagers(FablingClass fablingClass)
        {
            if (int.TryParse(_inputItemAmount, out int amount))
            {
                KingdomManager.Instance.ForceImmigration(fablingClass, amount);
            }
        }

        private void SpawnVillagers()
        {
            if (int.TryParse(_inputItemAmount, out int amount))
            {
                KingdomManager.Instance.ForceImmigration(amount);
            }
        }



        private void AlterNobility()
        {
            if (int.TryParse(_inputItemAmount, out int amount))
            {
                KingdomManager.Instance.AlterNobility(amount);
            }
        }

        private void AlterFortification()
        {
            if (int.TryParse(_inputItemAmount, out int amount))
            {
                KingdomManager.Instance.AlterFortification(amount);
            }
        }

        private void SpawnTrollCamp()
        {
            KingdomManager.Instance.SpawnTrollCamp();
        }

        private void SpawnDragon()
        {
            KingdomManager.Instance.SpawnDragon();
        }

        private void SpawnWickedWitches()
        {
            KingdomManager.Instance.SpawnWickedWitches();
        }

        private void SpawnFish()
        {
            KingdomManager.Instance.SpawnFish();
        }
    }
}