using UnityEngine;
using System.Linq;

namespace UnityTypeSafe {
	public static class SortingLayers {
		 public static SortingLayer DEFAULT = SortingLayer.layers.First(l => l.name == "Default");
	}
}
