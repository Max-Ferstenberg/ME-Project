using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    private int countA;
    private int countB;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameManager gameManager = (GameManager)target;

        GUILayout.Space(10);

        GUILayout.Label("Simulate Responses", EditorStyles.boldLabel);

        countA = EditorGUILayout.IntField("Count A", countA);
        countB = EditorGUILayout.IntField("Count B", countB);

        if (GUILayout.Button("Simulate Responses"))
        {
            gameManager.SimulateResponses(countA, countB);
        }

        GUILayout.Space(10);

        GUILayout.Label("End Scenario Debug", EditorStyles.boldLabel);

        if (GUILayout.Button("Trigger End Scenario Debug"))
        {
            gameManager.TriggerEndScenarioDebug();
        }
    }
}
