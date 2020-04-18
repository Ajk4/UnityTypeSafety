using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

abstract class UnityTypeSafeCodegen<T> {
    protected abstract string Filename { get; }
    protected abstract HashSet<T> GetCurrentElements();

    protected virtual int GetConsistentHashCode(HashSet<T> elements) {
        return HashSet<T>.CreateSetComparer().GetHashCode(elements);
    }

    private int previousHashCode = 0;
    protected abstract void WriteFile(StreamWriter writer, HashSet<T> elements);

    public void Update() {
        var prefsKey = $"UnityTypeSafety.{Filename}";

        var current = GetCurrentElements();

        var currentHash = GetConsistentHashCode(current);
        if (currentHash != previousHashCode) { // in case class was reloaded and static data was cleared
            previousHashCode = EditorPrefs.GetInt(prefsKey, 0);
        }

        if (currentHash != previousHashCode) {
            var fullPath = UnityTypeSafeCodegens.GENERATION_DIR + Filename + ".cs";

            Debug.Log(string.Format("Regenerating {0} file...", fullPath));
            if (File.Exists(fullPath)) {
                File.Delete(fullPath);
            }

            Directory.CreateDirectory(UnityTypeSafeCodegens.GENERATION_DIR);
            var writer = File.CreateText(fullPath);

            WriteFile(writer, current);
            writer.Close();
            AssetDatabase.ImportAsset(fullPath);
            previousHashCode = currentHash;
            EditorPrefs.SetInt(prefsKey, currentHash);
        }
    }
}

[InitializeOnLoad]
class UnityTypeSafeCodegens {
    public const String GENERATION_DIR = "Assets/UnityTypeSafety-generated/";

    private static readonly ScenesUnityTypeSafeCodegen ScenesCodegen = new ScenesUnityTypeSafeCodegen();
    private static readonly LayersUnityTypeSafeCodegen LayersCodegen = new LayersUnityTypeSafeCodegen();

    private static readonly SortingLayersUnityTypeSafeCodegen SortingLayersCodegen =
        new SortingLayersUnityTypeSafeCodegen();

    private static readonly TagsUnityTypeSafeCodegen TagsCodegen = new TagsUnityTypeSafeCodegen();
    private static readonly InputsUnityTypeSafeCodegen InputsCodegen = new InputsUnityTypeSafeCodegen();

    static UnityTypeSafeCodegens() {
        Debug.Log("LOADED UNITY TYPE SAVE VERSION 0.1.11"); // remove it eventually
        EditorApplication.update += Update;
    }

    private static void Update() {
        ScenesCodegen.Update();
        LayersCodegen.Update();
        SortingLayersCodegen.Update();
        TagsCodegen.Update();
        InputsCodegen.Update();
    }
}