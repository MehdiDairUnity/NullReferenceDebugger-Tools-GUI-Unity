using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

public class NullReferenceDebugger : EditorWindow
{
    private List<(GameObject obj, string scriptName, string fieldName)> nullReferences = new List<(GameObject, string, string)>();

    [MenuItem("Mehdi Tools/Null Reference Checker")]
    public static void ShowWindow()
    {
        GetWindow<NullReferenceDebugger>("Null Reference Checker");
    }

    private void OnGUI()
    {
        GUILayout.Label("🔍 Null Reference Checker", EditorStyles.boldLabel);
        GUILayout.Label("Click the button to scan for missing references.", EditorStyles.helpBox);

        if (GUILayout.Button("🔎 Scan for Null References"))
        {
            ScanForNullReferences();
        }

        GUILayout.Space(10);

        if (nullReferences.Count > 0)
        {
            GUILayout.Label($"🚨 {nullReferences.Count} Null References Found:", EditorStyles.boldLabel);

            foreach (var entry in nullReferences)
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button($"🔴 {entry.obj.name} → {entry.scriptName}.{entry.fieldName}", GUILayout.Width(300)))
                {
                    Selection.activeGameObject = entry.obj;
                    EditorGUIUtility.PingObject(entry.obj);
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            GUILayout.Label("✅ No null references detected!", EditorStyles.boldLabel);
        }
    }

    private void ScanForNullReferences()
    {
        nullReferences.Clear();
        MonoBehaviour[] scripts = FindObjectsOfType<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            if (script == null) continue;
            CheckScriptForNulls(script);
        }

        if (nullReferences.Count > 0)
        {
            //Debug.LogError($"🚨 Found {nullReferences.Count} null references! Open 'Tools > Null Reference Checker' to review them.");
        }
        else
        {
            //Debug.Log("✅ No null references found in the active scene.");
        }
    }

    private void CheckScriptForNulls(MonoBehaviour script)
    {
        FieldInfo[] fields = script.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            if (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
            {
                object value = field.GetValue(script);
                if (value == null)
                {
                    nullReferences.Add((script.gameObject, script.GetType().Name, field.Name));
                }
            }
        }
    }
}
