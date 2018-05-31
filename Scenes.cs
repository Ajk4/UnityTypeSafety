using UnityEngine;
using System.Linq;
using System;

namespace UnityTypeSafe {
	public enum Scenes {
	}
	public static class ScenesExtension {

		public static string Name(this Scenes scene) {
			switch(scene){
				default:
					throw new Exception("Uknown " + scene);
			}
		}
	}
}
