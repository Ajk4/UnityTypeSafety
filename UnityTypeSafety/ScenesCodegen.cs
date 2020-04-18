using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

class ScenesUnityTypeSafeCodegen : UnityTypeSafeCodegen<string> {
    protected override string Filename => "Scenes";

    protected override HashSet<string> GetCurrentElements() {
        return new HashSet<string>(EditorBuildSettings.scenes.Select(scene => scene.path)
            .Where(p => p != null && p.Length > 0));
    }

    protected override void WriteFile(StreamWriter writer, HashSet<string> elements) {
        writer.WriteLine("using UnityEngine;");
        writer.WriteLine("using System.Linq;");
        writer.WriteLine("using System;");
        writer.WriteLine("");
        writer.WriteLine("namespace UnityTypeSafety {");

        writer.WriteLine("\tpublic enum Scenes {");
        // TODO Not available scene names here. Only in runtime? Have only build index here?
        foreach (var path in elements) {
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

        foreach (var path in elements) {
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