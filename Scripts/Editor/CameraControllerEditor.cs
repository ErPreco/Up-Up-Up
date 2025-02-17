using UnityEditor;

[CustomEditor(typeof(CameraController)), CanEditMultipleObjects]
public class CameraControllerEditor : Editor
{
    private SerializedObject soTarget;

    private SerializedProperty useZoomOut;
    private SerializedProperty tracker;
    private SerializedProperty zoomOutMultiplier;

    private void OnEnable()
    {
        soTarget = new SerializedObject(target);

        useZoomOut = soTarget.FindProperty("useZoomOut");
        tracker = soTarget.FindProperty("tracker");
        zoomOutMultiplier = soTarget.FindProperty("zoomOutMultiplier");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CameraController controller = (CameraController)target;

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(useZoomOut);
        if (controller.useZoomOut)
        {
            EditorGUILayout.PropertyField(tracker);
            EditorGUILayout.Slider(zoomOutMultiplier, 1, 1.15f);
        }

        if (EditorGUI.EndChangeCheck())
        {
            // If we have changed a property, save the modified property
            soTarget.ApplyModifiedProperties();
        }
    }
}
