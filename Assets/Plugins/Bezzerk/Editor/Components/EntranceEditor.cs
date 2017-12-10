using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.Plugins.Hitt.Editor
{
    [CustomPropertyDrawer(typeof(Entrance))]
    public class EntranceEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 16 * 3 + 4;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.Box(position, GUIContent.none);

            var pos = property.FindPropertyRelative("position");
            var rot = property.FindPropertyRelative("rotation");
            var tag = property.FindPropertyRelative("tag");
            var r = position;
            r.y += 2;
            r.height = 16;
            EditorGUI.PropertyField(r, pos);
            r.y += r.height;
            {
                var v = rot.quaternionValue.eulerAngles;
                EditorGUI.showMixedValue = rot.hasMultipleDifferentValues;
                EditorGUI.BeginChangeCheck();
                var v2 = EditorGUI.Vector3Field(r, new GUIContent(rot.displayName), v);
                if (EditorGUI.EndChangeCheck())
                    rot.quaternionValue = Quaternion.Euler(v2);
            }
            r.y += r.height;
            EditorGUI.PropertyField(r, tag);

        }
    }
}
