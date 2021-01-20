using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace toio.Simulator
{
    [CustomEditor(typeof(CubeAvatar))]
    public class CubeAvatarEditor : Editor
    {

        string[] roleLabels = new string[3]{"開始時自滅", "シミュレータと連動", "リアルと連動"};

        public override void OnInspectorGUI()
        {
            var cube = target as CubeSimulator;


            serializedObject.Update();

            var roleNonEditor = serializedObject.FindProperty("roleNonEditor");
            var roleEditor = serializedObject.FindProperty("roleEditor");

            // UI

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("【非Editor環境でのロール】");

            roleNonEditor.intValue = (int)EditorGUILayout.Popup("",
                    (int)roleNonEditor.intValue, roleLabels
                );
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("【Editor環境でのロール】");

            roleEditor.intValue = (int)EditorGUILayout.Popup("",
                    (int)roleEditor.intValue, roleLabels
                );
            EditorGUILayout.EndVertical();

            // Save Properties
            serializedObject.ApplyModifiedProperties();
        }
    }

}
#endif
