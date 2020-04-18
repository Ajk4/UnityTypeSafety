using System.Collections.Generic;
using System.IO;
using UnityTypeSafe;

namespace UnityTypeSafety {
class InputsUnityTypeSafeCodegen : UnityTypeSafeCodegen<string> {
    // TODO Enrich input data with informations like input type

    protected override string Filename => "Inputs";

    protected override HashSet<string> GetCurrentElements() {
        return InputsReflectionReader.GetInputs();
    }

    protected override void WriteFile(StreamWriter writer, HashSet<string> elements) {
        writer.WriteLine("using UnityEngine;");
        writer.WriteLine("");
        writer.WriteLine("namespace UnityTypeSafety {");
        writer.WriteLine("\tpublic static class Inputs {");

        foreach (var input in elements) {
            // TODO Proper escaping
            // TODO DRY
            // TODO Underscore if starts with digit
            var escapedName = input.Replace(" ", "_").ToUpper();

            writer.WriteLine("\t\t public static string " + escapedName + " = \"" + input + "\";");
        }

        writer.WriteLine("\t}");
        writer.WriteLine("}");
    }
}
}