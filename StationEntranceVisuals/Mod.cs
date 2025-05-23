﻿using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Game.Simulation;
using HarmonyLib;
using StationEntranceVisuals.BridgeWE;
using StationEntranceVisuals.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Colossal.IO.AssetDatabase;

namespace StationEntranceVisuals
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(StationEntranceVisuals)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
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
        }

        private void DoPatches()
        {
            if (AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "BelzontWE") is Assembly weAssembly)
            {
                var exportedTypes = weAssembly.ExportedTypes;
                foreach (var (type, sourceClassName) in new List<(Type, string)>() {
                 (typeof(WEFontManagementBridge), "FontManagementBridge"),
                 (typeof(WEImageManagementBridge), "ImageManagementBridge"),
                 (typeof(WETemplatesManagementBridge), "TemplatesManagementBridge"),
                 (typeof(WERouteFn), "WERouteFn"),
             })
                {
                    var targetType = exportedTypes.FirstOrDefault(x => x.Name == sourceClassName);
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        var srcMethod = targetType.GetMethod(method.Name, ReflectionUtils.allFlags, null, method.GetParameters().Select(x => x.ParameterType).ToArray(), null);
                        if (srcMethod != null)
                        {
                            Harmony.ReversePatch(srcMethod, new HarmonyMethod(method));
                        }
                        else log.Warn($"Method not found while patching WE: {targetType.FullName} {srcMethod.Name}({string.Join(", ", method.GetParameters().Select(x => $"{x.ParameterType}"))})");
                    }
                }
            }
            else
            {
                throw new Exception("Write Everywhere dll file required for using this mod! Check if it's enabled.");
            }
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
        }

    }
}
