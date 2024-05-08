using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ParameterTable : EditorWindow
{
    float objectScale = 1f;
    float objectXPosition = 0f;
    float objectYPosition = 1f;
    float objectZPosition = 0f;
    GameObject objectToHandle;

    [MenuItem("Tools/Object Handler")]
    public static void ShowWindow()
    {
        GetWindow(typeof(ParameterTable));
    }

    private void Start()
    {
        objectToHandle = GameObject.Find("Player");
    }

    private void OnGUI()
    {
        objectScale = EditorGUILayout.FloatField("Object Scale", objectScale);

        GUILayout.Label("Position");
        objectXPosition = EditorGUILayout.FloatField("X", objectXPosition);
        objectYPosition = EditorGUILayout.FloatField("Y", objectYPosition);
        objectZPosition = EditorGUILayout.FloatField("Z", objectZPosition);

        //objectToHandle = EditorGUILayout.ObjectField("Object to handle", objectToHandle, typeof(GameObject), true) as GameObject;

        if(GUILayout.Button("Apply Changes"))
        {
            Apply();
        }
    }

    private void Apply()
    {
        objectToHandle.transform.localScale = new Vector3(objectScale, objectScale, objectScale);
        objectToHandle.transform.position = new Vector3(objectXPosition, objectYPosition, objectZPosition);
    }
}
