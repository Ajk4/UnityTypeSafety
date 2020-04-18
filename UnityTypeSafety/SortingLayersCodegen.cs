using System;
using System.Collections.Generic;
using System.IO;
using UnityEditorInternal;

namespace UnityTypeSafety {
class LayersUnityTypeSafeCodegen : UnityTypeSafeCodegen<string> {
    protected override string Filename => "Layers";

    protected override HashSet<string> GetCurrentElements() {
        return new HashSet<String>(InternalEditorUtility.layers);
    }

    protected override void WriteFile(StreamWriter writer, HashSet<string> elements) {
        writer.WriteLine("using UnityEngine;");
        writer.WriteLine("");
        writer.WriteLine("namespace UnityTypeSafety {");
        writer.WriteLine("\tpublic static class Layers {");

        foreach (var layer in elements) {
            // TODO Proper escaping
            // TODO DRY
            // TODO Underscore if starts with digit
            var escapedName = layer.Replace(" ", "_").ToUpper();

            writer.WriteLine("\t\t public static LayerMask " + escapedName + " = LayerMask.NameToLayer(\"" +
                             layer + "\");");
        }

        writer.WriteLine("\t}");
        writer.WriteLine("}");
    }
}
}