using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SIS
{
    [InitializeOnLoad]
    public class Setup : EditorWindow
    {
        private static Config currentConfig;
        private static string settingsPath = "/Resources/";
        private static string packagesPath;

        private Packages selectedPackage = Packages.OpenIAB;
        private enum Packages
        {
            OpenIAB,
            Prime31,
            StansAssets,
            Unibill,
            NoBilling
        }


        static Setup()
        {
            EditorApplication.hierarchyWindowChanged += EditorUpdate;
        }


        [MenuItem("Window/Simple IAP System/Plugin Setup")]
        static void Init()
        {
			packagesPath = "/Packages/";
            EditorWindow window = EditorWindow.GetWindowWithRect(typeof(Setup), new Rect(0, 0, 340, 250), false, "Plugin Setup");
			
			var script = MonoScript.FromScriptableObject(window);
			string thisPath = AssetDatabase.GetAssetPath(script);
			packagesPath = thisPath.Replace("/Setup.cs", packagesPath);
        }


        private static void EditorUpdate()
        {
            if (Setup.Current == null)
                return;

            if (Setup.Current.autoOpen)
                Init();
        }


        void OnGUI()
        {			
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Simple IAP System - Billing Plugin Setup", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Please choose the billing plugin you are using for SIS:");

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            selectedPackage = (Packages)EditorGUILayout.EnumPopup(selectedPackage);

            if (GUILayout.Button("?", GUILayout.Width(20)))
            {
                switch(selectedPackage)
                {
                    case Packages.OpenIAB:
                        Application.OpenURL("https://github.com/onepf/OpenIAB-Unity-Plugin");
                        break;
                    case Packages.Prime31:
                        Application.OpenURL("https://www.assetstore.unity3d.com/#/publisher/270");
                        break;
                    case Packages.StansAssets:
                        Application.OpenURL("https://www.assetstore.unity3d.com/#/publisher/2256");
                        break;
                    case Packages.Unibill:
                        Application.OpenURL("https://www.assetstore.unity3d.com/#/content/5767");
                        break;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button("Import/Save"))
            {
                if(selectedPackage != Packages.NoBilling)
                    AssetDatabase.ImportPackage(packagesPath + selectedPackage.ToString() + ".unitypackage", true);
                DisableAutoOpen();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Note:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("OpenIAB is a free alternative to the paid plugins.");
            EditorGUILayout.LabelField("Selecting 'No Billing' disables purchases for real money,");
            EditorGUILayout.LabelField("but you can still sell items for virtual currency earned");
            EditorGUILayout.LabelField("in your game (In Game Content).");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Please read the PDF documentation for further details.");
            EditorGUILayout.LabelField("Support links: Window > Simple IAP System > About.");
        }


        void DisableAutoOpen()
        {
            Setup.Current.autoOpen = false;
            Setup.Save();
            this.Close();
        }


        public static Config Current
        {
            get
            {
                if (currentConfig == null)
                {
                    currentConfig = Resources.Load("PluginSetup", typeof(Config)) as Config;

                    if (currentConfig == null)
                    {
                        string dir = Path.GetDirectoryName(settingsPath);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                            AssetDatabase.ImportAsset(dir);
                        }

                        currentConfig = (Config)ScriptableObject.CreateInstance(typeof(Config));
                        if (currentConfig != null)
                        {
							var script = MonoScript.FromScriptableObject(currentConfig);
							string thisPath = AssetDatabase.GetAssetPath(script);
							thisPath = thisPath.Replace("/Config.cs", settingsPath);
                            AssetDatabase.CreateAsset(currentConfig, thisPath + "PluginSetup.asset");
                        }
                    }
                }

                return currentConfig;
            }
            set
            {
                currentConfig = value;
            }
        }


        public static void Save()
        {
            EditorUtility.SetDirty(Setup.Current);
        }
    }
}