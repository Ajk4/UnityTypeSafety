using System;
using System.Collections.Generic;
using System.IO;
using UnityEditorInternal;

class TagsUnityTypeSafeCodegen : UnityTypeSafeCodegen<string> {
    protected override string Filename => "Tags";

    protected override HashSet<string> GetCurrentElements() {
        return new HashSet<String>(InternalEditorUtility.tags);
    }

    protected override void WriteFile(StreamWriter writer, HashSet<string> elements) {
        writer.WriteLine("using UnityEngine;");
        writer.WriteLine("");
        writer.WriteLine("namespace UnityTypeSafety {");
        writer.WriteLine("\tpublic static class Tags {");

        foreach (var layer in elements) {
            // TODO Proper escaping
            // TODO DRY escaping
            var escapedName = layer.Replace(" ", "_").ToUpper();
            writer.WriteLine("\t\t public static string " + escapedName + " = \"" + layer + "\";");
        }

        writer.WriteLine("\t}");
        writer.WriteLine("}");
    }
}