using UnityEngine;

namespace UnityTypeSafe {
	public static class Layers {
		 public static LayerMask DEFAULT = LayerMask.NameToLayer("Default");
		 public static LayerMask TRANSPARENTFX = LayerMask.NameToLayer("TransparentFX");
		 public static LayerMask IGNORE_RAYCAST = LayerMask.NameToLayer("Ignore Raycast");
		 public static LayerMask WATER = LayerMask.NameToLayer("Water");
		 public static LayerMask UI = LayerMask.NameToLayer("UI");
	}
}
