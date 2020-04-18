﻿using System.Collections.Generic;
using UnityEditor;

namespace UnityTypeSafe {

    internal class InputsReflectionReader {
        
        public static HashSet<string> GetInputs() {
            var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];

            SerializedObject obj = new SerializedObject(inputManager);

            SerializedProperty axisArray = obj.FindProperty("m_Axes");

            var output = new HashSet<string>();

            for (int i = 0; i < axisArray.arraySize; ++i) {
                var axis = axisArray.GetArrayElementAtIndex(i);

                var name = axis.FindPropertyRelative("m_Name").stringValue;
                var axisVal = axis.FindPropertyRelative("axis").intValue;
                var inputType = (InputType) axis.FindPropertyRelative("type").intValue;

                output.Add(name);
            }

            return output;
        }

        public enum InputType {
            KeyOrMouseButton,
            MouseMovement,
            JoystickAxis,
        };

        // TODo Without this it doesnt work and list is empty. Have on load thing?
        [MenuItem("Assets/ReadInputManager")]
        public static void DoRead() {
            GetInputs();
        }
    }

}