using BridgeWE;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using HarmonyLib;
using StationEntranceVisuals.BridgeWE;
using StationEntranceVisuals.Systems;
using StationEntranceVisuals.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using static StationEntranceVisuals.Settings;

namespace StationEntranceVisuals
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(StationEntranceVisuals)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        public static readonly BindingFlags allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.GetProperty;
        public static Settings m_Setting;

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");
            m_Setting = new Settings(this);
            m_Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEn(m_Setting));
            AssetDatabase.global.LoadSettings(nameof(StationEntranceVisuals), m_Setting, new Settings(this));

            var bw = new BackgroundWorker();
            bw.DoWork += DeleteOldFiles;
            bw.RunWorkerAsync();

            GameManager.instance.RegisterUpdater(DoWhenLoaded);
        }

        private static void DeleteOldFiles(object sender, DoWorkEventArgs e)
        {
            FileUtils.DeleteOldFiles();
        }

        private void DoWhenLoaded()
        {
            log.Info($"Loading patches");
            DoPatches();
            RegisterFilesToWe();
        }

        private static void RegisterFilesToWe()
        {
            string modPath = Path.GetDirectoryName(GameManager.instance.modManager.FirstOrDefault(x => x.asset.assembly == typeof(Mod).Assembly).asset.path);

            var imagesDirectory = Path.Combine(modPath, "atlases");
            var atlases = Directory.GetDirectories(imagesDirectory, "*", SearchOption.TopDirectoryOnly);
            foreach (var atlasFolder in atlases)
            {
                WEImageManagementBridge.RegisterImageAtlas(typeof(Mod).Assembly, Path.GetFileName(atlasFolder), Directory.GetFiles(atlasFolder, "*.png"));
            }
            var localLayoutsDirectory = Path.Combine(modPath, "weLayouts");
            WETemplatesManagementBridge.RegisterCustomTemplates(typeof(Mod).Assembly, localLayoutsDirectory);
            WETemplatesManagementBridge.RegisterLoadableTemplatesFolder(typeof(Mod).Assembly, localLayoutsDirectory);

            var dataSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SEV_SettingSystem>();
            WEModuleOptionsBridge.CreateBuilder(typeof(Mod).Assembly, "StationEntranceVisuals.weOptions")
                .Dropdown("SubwayLineIndicatorShape",
                    () => dataSystem.SubwayLineIndicatorShape.ToString(),
                    x =>
                    {
                        log.DebugFormat("Setting SubwayLineIndicatorShape to {0}", x);
                        dataSystem.SubwayLineIndicatorShape = Enum.TryParse<LineIndicatorShapeOptions>(x, out var result) ? result : LineIndicatorShapeOptions.Square;
                    },
                    () => Enum.GetNames(typeof(LineIndicatorShapeOptions)).ToDictionary(x => x, x => $"StationEntranceVisuals.weOptions[LineIndicatorShapeOptions.{x}]"))
                .Dropdown("TrainLineIndicatorShape",
                    () => dataSystem.TrainLineIndicatorShape.ToString(),
                    x =>
                    {
                        log.DebugFormat("Setting TrainLineIndicatorShape to {0}", x);
                        dataSystem.TrainLineIndicatorShape = Enum.TryParse<LineIndicatorShapeOptions>(x, out var result) ? result : LineIndicatorShapeOptions.Square;
                    },
                    () => Enum.GetNames(typeof(LineIndicatorShapeOptions)).ToDictionary(x => x, x => $"StationEntranceVisuals.weOptions[LineIndicatorShapeOptions.{x}]"))
                .Dropdown("BusLineIndicatorShape",
                    () => dataSystem.BusLineIndicatorShape.ToString(),
                    x =>
                    {
                        log.DebugFormat("Setting BusLineIndicatorShape to {0}", x);
                        dataSystem.BusLineIndicatorShape = Enum.TryParse<LineIndicatorShapeOptions>(x, out var result) ? result : LineIndicatorShapeOptions.Diamond;
                    },
                    () => Enum.GetNames(typeof(LineIndicatorShapeOptions)).ToDictionary(x => x, x => $"StationEntranceVisuals.weOptions[LineIndicatorShapeOptions.{x}]"))
                .Dropdown("TramLineIndicatorShape",
                    () => dataSystem.TramLineIndicatorShape.ToString(),
                    x =>
                    {
                        log.DebugFormat("Setting TramLineIndicatorShape to {0}", x);
                        dataSystem.TramLineIndicatorShape = Enum.TryParse<LineIndicatorShapeOptions>(x, out var result) ? result : LineIndicatorShapeOptions.Pentagon;
                    },
                    () => Enum.GetNames(typeof(LineIndicatorShapeOptions)).ToDictionary(x => x, x => $"StationEntranceVisuals.weOptions[LineIndicatorShapeOptions.{x}]"))
                .Spacer("______")
                .Dropdown("LineOperatorCity",
                    () => dataSystem.LineOperatorCity.ToString(),
                    x =>
                    {
                        log.DebugFormat("Setting LineOperatorCity to {0}", x);
                        dataSystem.LineOperatorCity = Enum.TryParse<LineOperatorCityOptions>(x, out var result) ? result : LineOperatorCityOptions.Generic;
                    },
                    () => Enum.GetNames(typeof(LineOperatorCityOptions)).ToDictionary(x => x, x => $"StationEntranceVisuals.weOptions[LineOperatorCityOptions.{x}]"))
                .Dropdown("LineDisplayName",
                    () => dataSystem.LineDisplayName.ToString(),
                    x =>
                    {
                        log.DebugFormat("Setting LineDisplayName to {0}", x);
                        dataSystem.LineDisplayName = Enum.TryParse<LineDisplayNameOptions>(x, out var result) ? result : LineDisplayNameOptions.Generated;
                    },
                    () => Enum.GetNames(typeof(LineDisplayNameOptions)).ToDictionary(x => x, x => $"StationEntranceVisuals.weOptions[LineDisplayNameOptions.{x}]"))
                .Register();
        }

        private bool DoPatches()
        {
            try
            {
                if (AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "BelzontWE") is Assembly weAssembly)
                {
                    var exportedTypes = weAssembly.ExportedTypes;
                    foreach (var (type, sourceClassName) in new List<(Type, string)>() {
                    (typeof(WEFontManagementBridge), "FontManagementBridge"),
                    (typeof(WEImageManagementBridge), "ImageManagementBridge"),
                    (typeof(WETemplatesManagementBridge), "TemplatesManagementBridge"),
                    (typeof(WERouteFn), "WERouteFn"),
                    (typeof(WEModuleOptionsBridge), "ModuleOptionsBridge")
                })
                    {
                        var targetType = exportedTypes.First(x => x.Name == sourceClassName);
                        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                        {
                            var srcMethod = targetType.GetMethod(method.Name, allFlags, null, method.GetParameters().Select(x => x.ParameterType).ToArray(), null);
                            if (srcMethod != null)
                            {
                                Harmony.ReversePatch(srcMethod, new HarmonyMethod(method));
                            }
                            else
                            {
                                log.Warn($"Method not found while patching WE: {targetType.FullName} {srcMethod.Name}({string.Join(", ", method.GetParameters().Select(x => $"{x.ParameterType}"))})");
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("No WE Found!");
                }
            }
            catch
            {
                log.Error("Write Everywhere dll file required for using this mod! Check if it's enabled.");
                return false;
            }
            return true;
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
        }

    }
}
