#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityTypeSafe;

using UnityEditor;
using UnityEditor.UI;
using UnityEditorInternal;

abstract class UnityTypeSafeCodegen<T> {
    protected abstract string Filename { get; }
    protected abstract HashSet<T> GetCurrentList();
    private HashSet<T> previous = null;
    protected abstract void WriteFile(StreamWriter writer, HashSet<T> list);

    public void ForceRefresh() {
        previous = null;
        Update();
    }

    public void Update() {
        var current = GetCurrentList();
        if (previous == null || !previous.SetEquals(current)) {
            var fullPath = UnityTypeSafeCodegens.GENERATION_DIR + Filename + ".cs";

            Debug.Log(string.Format("Regenerating {0} file...", fullPath));
            if (File.Exists(fullPath)) {
                File.Delete(fullPath);
            }

            Directory.CreateDirectory(UnityTypeSafeCodegens.GENERATION_DIR);
            var writer = File.CreateText(fullPath);

            WriteFile(writer, current);

            writer.Close();

            AssetDatabase.Refresh();
        }
        previous = current;
    }
}

// TODO Refactor other codegens
class ScenesUnityTypeSafeCodegen : UnityTypeSafeCodegen<string> {
    protected override string Filename {
        get { return "Scenes"; }
    }

    protected override HashSet<string> GetCurrentList() {
        return new HashSet<string>(EditorBuildSettings.scenes.Select(scene => scene.path).Where(p => p != null && p.Length > 0));
    }

    protected override void WriteFile(StreamWriter writer, HashSet<string> list) {
        writer.WriteLine("using UnityEngine;");
        writer.WriteLine("using System.Linq;");
        writer.WriteLine("using System;");
        writer.WriteLine("");
        writer.WriteLine("namespace UnityTypeSafety {");

        writer.WriteLine("\tpublic enum Scenes {");
        // TODO Not available scene names here. Only in runtime? Have only build index here?
        foreach (var path in list) {
            // This should be in getter?
            var sceneName = Path.GetFileNameWithoutExtension(path);
            // TODO Proper escaping
            // TODO DRY escaping
            var escapedName = sceneName.Replace(" ", "_").ToUpper();
            // public static Scene DEFAULT = new Scene("default");
            writer.WriteLine("\t\t " + escapedName + ",");
        }
        writer.WriteLine("\t}");

        writer.WriteLine("\tpublic static class ScenesExtension {");
        writer.WriteLine("");
        writer.WriteLine("\t\tpublic static string GetName(this Scenes scene) {");
        writer.WriteLine("\t\t\tswitch(scene){");

        foreach (var path in list) {
            // This should be in getter?
            var sceneName = Path.GetFileNameWithoutExtension(path);
            // TODO Proper escaping
            // TODO DRY escaping
            var escapedName = sceneName.Replace(" ", "_").ToUpper();
            writer.WriteLine("\t\t\t\tcase Scenes." + escapedName + ":");
            writer.WriteLine("\t\t\t\t\treturn \"" + sceneName + "\";");
        }
        writer.WriteLine("\t\t\t\tdefault:");
        writer.WriteLine("\t\t\t\t\tthrow new Exception(\"Uknown \" + scene);");

        writer.WriteLine("\t\t\t}");
        writer.WriteLine("\t\t}");
        writer.WriteLine("\t}");

        writer.WriteLine("}");
    }
}

[InitializeOnLoad]
class UnityTypeSafeCodegens {
    // IDEAS:
    // Annotation 'prefab'

    public const String GENERATION_DIR = "Assets/UnityTypeSafety-generated/";

    private const String SORTING_LAYER_ENUM_FILE = GENERATION_DIR + "SortingLayers.cs";
    private const String LAYERS_FILE = GENERATION_DIR + "Layers.cs";
    private const String TAGS_FILE = GENERATION_DIR + "Tags.cs";
    private const String INPUTS_FILE = GENERATION_DIR + "Inputs.cs";

    private static HashSet<SortingLayer> PreviousSortingLayerNames = null;
    private static HashSet<String> PreviousLayersNames = null;
    private static HashSet<String> PreviousTags = null;
    private static HashSet<String> PreviousInputs = null;

    private static readonly ScenesUnityTypeSafeCodegen ScenesCodegen = new ScenesUnityTypeSafeCodegen();
    private static Action DebouncedRefreshAssets;

    static UnityTypeSafeCodegens() {
        Debug.Log("LOADED UNITY TYPE SAVE VERSION 0.05"); // remove it eventually
        
        DebouncedRefreshAssets = Debounce(() => {
            Debug.Log("UnityTypeSafe::Observed changes. Reloading assets");
            AssetDatabase.Refresh();
        });
        EditorApplication.update += Update;
    }

    [MenuItem("Tools/UnityTypeSafe/Force refresh")]
    static void ForceRefresh() {
        File.Delete(SORTING_LAYER_ENUM_FILE);
        PreviousSortingLayerNames = null;
        File.Delete(LAYERS_FILE);
        PreviousLayersNames = null;
        File.Delete(TAGS_FILE);
        PreviousLayersNames = null;
        File.Delete(INPUTS_FILE);
        PreviousInputs = null;
        Update();
        ScenesCodegen.ForceRefresh();
    }

    /*
    [MenuItem("Tools/UnityTypeSafe/Test")]
    static void TestScene() {
        ScenesCodegen.Update();
    }
    */

    private static void Update() {
        ScenesCodegen.Update();

        // TODO dry
        var refeshAssets = false;
        {
            var currentSortingLayerNames = GetSortingLayerNames();
            if (PreviousSortingLayerNames == null || !currentSortingLayerNames.SetEquals(PreviousSortingLayerNames)) {
                GenerateFile(SORTING_LAYER_ENUM_FILE, writer => {
                    writer.WriteLine("using UnityEngine;");
                    writer.WriteLine("using System.Linq;");
                    writer.WriteLine("");
                    writer.WriteLine("namespace UnityTypeSafety {");
                    writer.WriteLine("\tpublic static class SortingLayers {");

                    foreach (var layer in currentSortingLayerNames) {
                        // TODO Proper escaping
                        // TODO DRY escaping
                        var escapedName = layer.name.Replace(" ", "_").ToUpper();
                        writer.WriteLine("\t\t public static SortingLayer " + escapedName +
                                         " = SortingLayer.layers.First(l => l.name == \"" + layer.name + "\");");
                    }

                    writer.WriteLine("\t}");
                    writer.WriteLine("}");
                });
                refeshAssets = true;
            }
            PreviousSortingLayerNames = currentSortingLayerNames;
        }
        {
            var currentLayersNames = GetLayersNames();
            if (PreviousLayersNames == null || !currentLayersNames.SetEquals(PreviousLayersNames)) {
                GenerateFile(LAYERS_FILE, writer => {
                    writer.WriteLine("using UnityEngine;");
                    writer.WriteLine("");
                    writer.WriteLine("namespace UnityTypeSafety {");
                    writer.WriteLine("\tpublic static class Layers {");

                    foreach (var layer in currentLayersNames) {
                        // TODO Proper escaping
                        // TODO DRY
                        // TODO Underscore if starts with digit
                        var escapedName = layer.Replace(" ", "_").ToUpper();

                        writer.WriteLine("\t\t public static LayerMask " + escapedName + " = LayerMask.NameToLayer(\"" +
                                         layer + "\");");
                    }

                    writer.WriteLine("\t}");
                    writer.WriteLine("}");

                    refeshAssets = true;
                });
            }
            PreviousLayersNames = currentLayersNames;
        }
        {
            var currentTags = GetTags();
            if (PreviousTags == null || !currentTags.SetEquals(PreviousTags)) {
                GenerateFile(TAGS_FILE, writer => {
                    writer.WriteLine("using UnityEngine;");
                    writer.WriteLine("");
                    writer.WriteLine("namespace UnityTypeSafety {");
                    writer.WriteLine("\tpublic static class Tags {");

                    foreach (var layer in currentTags) {
                        // TODO Proper escaping
                        // TODO DRY escaping
                        var escapedName = layer.Replace(" ", "_").ToUpper();
                        writer.WriteLine("\t\t public static string " + escapedName + " = \"" + layer + "\";");
                    }

                    writer.WriteLine("\t}");
                    writer.WriteLine("}");

                    refeshAssets = true;
                });
            }
            PreviousTags = currentTags;
        }
        {
            // TODO DRY MORE
            var current = InputsReflectionReader.GetInputs();
            if (PreviousInputs == null || !current.SetEquals(PreviousInputs)) {
                // TODO More typesafety (have different lists for different input types)
                GenerateFile(INPUTS_FILE, writer => {
                    writer.WriteLine("using UnityEngine;");
                    writer.WriteLine("");
                    writer.WriteLine("namespace UnityTypeSafety {");
                    writer.WriteLine("\tpublic static class Inputs {");

                    foreach (var input in current) {
                        // TODO Proper escaping
                        // TODO DRY
                        // TODO Underscore if starts with digit
                        var escapedName = input.Replace(" ", "_").ToUpper();

                        writer.WriteLine("\t\t public static string " + escapedName + " = \"" + input + "\";");
                    }

                    writer.WriteLine("\t}");
                    writer.WriteLine("}");

                    refeshAssets = true;
                });
            }
            PreviousInputs = current;
        }

        if (refeshAssets) {
            DebouncedRefreshAssets();
        }
    }

    private static void GenerateFile(string filename, Action<StreamWriter> writeFileFunction) {
        // Debug.Log(string.Format("Regenerating {0} file...", filename));
        File.Delete(filename);

        Directory.CreateDirectory("Assets/UnityTypeSafety");
        var writer = File.CreateText(filename);

        writeFileFunction(writer);

        writer.Close();
    }

    private static HashSet<SortingLayer> GetSortingLayerNames() {
        return new HashSet<SortingLayer>(SortingLayer.layers);
    }

    private static HashSet<String> GetLayersNames() {
        return new HashSet<String>(InternalEditorUtility.layers);
    }

    private static HashSet<String> GetTags() {
        return new HashSet<String>(InternalEditorUtility.tags);
    }
    
    // Move somewhere
    public static Action Debounce(Action func, int milliseconds = 300)
    {
        var last = 0;
        return () =>
        {
            var current = Interlocked.Increment(ref last);
            Task.Delay(milliseconds).ContinueWith(task =>
            {
                if (current == last) {
                    func();
                }
                task.Dispose();
            });
        };
    }
    
}

#endif