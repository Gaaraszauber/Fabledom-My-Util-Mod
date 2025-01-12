using MelonLoader;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Nielsen;
using UnityEngine.InputSystem;
using HarmonyLib;
using System.Reflection;
using Harmony;
using static Febucci.UI.TextAnimator;


[assembly: MelonInfo(typeof(Fabledom_My_Util_Mod.Core), "Fabledom My Util Mod", "0.0.3", "Gaaraszauber", null)]
[assembly: MelonGame("Grenaa Games", "Fabledom")]

namespace Fabledom_My_Util_Mod
{
    public class Core : MelonMod
    {
        //Wichtig bitte immer anpassen
        private string CURRENT_VERSION = "";

        private const int WINDOW_HEIGHT = 400;
        private const int WINDOW_WIDTH = 600;
        private const int BUTTONS_PER_ROW = 5;

        private bool _open = false;
        private int _selectedTab = 0;
        private string[] _tabNames = new string[] { "General", "Give Cheats", "Extra" };
        private Rect _windowRect = new Rect(500, 50, WINDOW_WIDTH, WINDOW_HEIGHT);
        private DataManager _dataManager;
        private GameManager _gameManager;
        private bool _toggleDevMod = false;
        private bool _toggleDevModOld = false;
        private string _inputItemAmount = "1";
        private Vector2 _scrollPosition;
        private bool _initializeReferencesDone = false;
        private bool _unlockAllDone = false;
        private int _amount = 0;

        private Utils utils;


        private float _resourceMultiplier = 1f;
        private float _timeScale = 1;

        private UpdateChecker updateChecker = new UpdateChecker();


        public static bool InstantBuildNoMaterialsToggle = false;
        public static float constructionSpeed = 1;


        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Fabledom Util Mod is Initialized.");
            CURRENT_VERSION = Info.Version;

            CheckForUpdates();
            utils = new Utils(this);
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.gaaraszauber.fabledommod");
            harmony.PatchAll(typeof(MyHarmonyPatches));
        }

        private async void CheckForUpdates()
        {
            LoggerInstance.Msg("Checking for Updates....");
            try
            {
                string latestVersion = await updateChecker.GetLatestVersion();
                LoggerInstance.Msg($"Installed Version:  {CURRENT_VERSION}");
                LoggerInstance.Msg($"Latest Version:  {latestVersion}");
                if (CompareVersions(CURRENT_VERSION, latestVersion) < 0)
                {
                    LoggerInstance.Warning($"A new version is available: {latestVersion}\nDownloads:\nNexusmods: https://www.nexusmods.com/fabledom/mods/1\nGithub: https://github.com/Gaaraszauber/Fabledom-My-Util-Mod/\n");                    
                }
                else
                {
                    LoggerInstance.Msg("You already have the current version installed.");

                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"Error retrieving the latest version: {ex.Message}");
            }
        }

        private int CompareVersions(string version1, string version2)
        {
            var v1 = new Version(version1);
            var v2 = new Version(version2);
            return v1.CompareTo(v2);
        }



        private void InitializeReferences()
        {
            LoggerInstance.Msg("Initialize References loading");
            _gameManager = GameObject.FindObjectOfType<GameManager>();
            _dataManager = GameObject.FindObjectOfType<DataManager>();
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
                LoggerInstance.Msg($"Dev mode set to: {_gameManager.isDevBuild}" );
                _toggleDevModOld = !_toggleDevModOld;
            }
        }

        private void DrawMenu()
        {
            GUI.skin.window.padding = new RectOffset(10, 10, 20, 10);
            // Dunklerer Hintergrund
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.98f);

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
            _toggleDevMod = GUILayout.Toggle(_toggleDevMod, "Dev Mode | Status: " + _gameManager.isDevBuild + " | When active, you will not receive any achievements!");

            DrawCenteredButtons(new string[] { "Unlock All", "Unlock all Ruler", "Unlock all Equipment" },
                new System.Action[] { utils.UnlockAll, utils.UnlockAllRulers, utils.UnlockAllEquipment }, 450);

            GUILayout.Label("Change Seasons", GUI.skin.box);
            DrawCenteredButtons(new string[] { "Spring", "Summer", "Fall", "Winter" },
                new System.Action[] { () => utils.ChangeSeason(0), () => utils.ChangeSeason(1), () => utils.ChangeSeason(2), () => utils.ChangeSeason(3) }, 400);

            GUILayout.Label("Change Weather", GUI.skin.box);
            DrawCenteredButtons(new string[] { "Sunny", "Rainy" },
                new System.Action[] {
                    () => DateTimeManager.Instance.weatherController.SetWeatherState(Weather.SUNNY),
                    () => DateTimeManager.Instance.weatherController.SetWeatherState(Weather.RAIN)
                }, 400);

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
            if (int.TryParse(_inputItemAmount, out int amount)) { _amount = amount;  }

            DrawCenteredButtons(new string[] { "Fablings", "Peasants", "Commoners", "Nobles" },
            new System.Action[] { () => utils.SpawnVillagers(_amount), () => utils.SpawnVillagers(FablingClass.PEASANT,_amount),
                                    () => utils.SpawnVillagers(FablingClass.COMMONER,_amount), () => utils.SpawnVillagers(FablingClass.NOBLE,_amount) }, 400);

            GUILayout.Label("Items and More", GUI.skin.box);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            int buttonWidth = (WINDOW_WIDTH - 30) / BUTTONS_PER_ROW - 4;

            for (int i = 0; i < dataList.Count; i++)
            {
                if (i % BUTTONS_PER_ROW == 0) GUILayout.BeginHorizontal();

                if (GUILayout.Button(dataList[i].name, GUILayout.Width(buttonWidth)))
                {
                    utils.GiveItem(dataList[i].key, _inputItemAmount);
                }

                if (i % BUTTONS_PER_ROW == BUTTONS_PER_ROW - 1 || i == dataList.Count - 1)
                {
                    if(i < 25) GUILayout.EndHorizontal();
                }
            }
  
            if (GUILayout.Button("Nobility", GUILayout.Width(buttonWidth)))
            {
                utils.AlterNobility(_amount);
            }
            if (GUILayout.Button("Fort", GUILayout.Width(buttonWidth)))
            {
                utils.AlterFortification(_amount);
            }

            if (GUILayout.Button("Happiness", GUILayout.Width(buttonWidth)))
            {
                KingdomManager.Instance.debugHappiness = _amount;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }

        private void DrawExtraTab()
        {
            GUILayout.Label("Extra Functions", GUI.skin.box);
            Core.InstantBuildNoMaterialsToggle = GUILayout.Toggle(Core.InstantBuildNoMaterialsToggle, "Instant Build (No Materials)");
            GUILayout.Label("Spawner", GUI.skin.box);
            DrawCenteredButtons(new string[] { "Troll Camp", "Dragon", "Witches", "Fish" },
                new System.Action[] { utils.SpawnTrollCamp, utils.SpawnDragon, utils.SpawnWickedWitches, utils.SpawnFish }, 400);
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

        private static bool InstantBuildPrefix(Nielsen.Constructable __instance)
        {
            var healthField = HarmonyLib.AccessTools.Field(typeof(Nielsen.Constructable), "health");
            var health = (Health)healthField.GetValue(__instance);
            health.Heal(health.totalMaxHealth);

            var buildCompleteMethod = HarmonyLib.AccessTools.Method(typeof(Nielsen.Constructable), "BuildComplete");
            buildCompleteMethod.Invoke(__instance, null);

            return false;
        }
    }
}
