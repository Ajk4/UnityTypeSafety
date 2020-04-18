﻿using System.Collections.Generic;
 using System.IO;
 using System.Linq;
 using UnityEngine;

 class SortingLayersUnityTypeSafeCodegen : UnityTypeSafeCodegen<SortingLayer> {
    protected override string Filename => "SortingLayers";

    protected override HashSet<SortingLayer> GetCurrentElements() {
        return new HashSet<SortingLayer>(SortingLayer.layers);
    }

    protected override int GetConsistentHashCode(HashSet<SortingLayer> elements) {
        var names = new HashSet<string>(elements.Select(sl => sl.name));
        return HashSet<string>.CreateSetComparer().GetHashCode(names);
    }

    protected override void WriteFile(StreamWriter writer, HashSet<SortingLayer> elements) {
        writer.WriteLine("using UnityEngine;");
        writer.WriteLine("using System.Linq;");
        writer.WriteLine("");
        writer.WriteLine("namespace UnityTypeSafety {");
        writer.WriteLine("\tpublic static class SortingLayers {");
        
        foreach (var layer in elements) {
            // TODO Proper escaping
            // TODO DRY escaping
            var escapedName = layer.name.Replace(" ", "_").ToUpper();
            writer.WriteLine("\t\t public static SortingLayer " + escapedName +
                             " = SortingLayer.layers.First(l => l.name == \"" + layer.name + "\");");
        }
        
        writer.WriteLine("\t}");
        writer.WriteLine("}");
    }
}