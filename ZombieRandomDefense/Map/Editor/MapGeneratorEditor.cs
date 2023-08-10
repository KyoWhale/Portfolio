using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator generator = (MapGenerator)target;

        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck() && generator.autoUpdate)
        {
            if (generator.mapSize.x >= 5 && generator.mapSize.y >= 10)
            {
                generator.GenerateAllWithoutNavMesh();
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Ground and Obstacles"))
        {
            if (generator.mapSize.x >= 5 && generator.mapSize.y >= 10)
            {
                generator.GenerateAllWithoutNavMesh();
            }
        }
        if (GUILayout.Button("Generate NavMesh"))
        {
            generator.GenerateNavMesh();
        }
    }
}
