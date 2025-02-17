using UnityEditor;

[CustomEditor(typeof(PlatformSO)), CanEditMultipleObjects]
class PlatformSOEditor : Editor
{
    private SerializedObject soTarget;

    private SerializedProperty startHeight;
    private SerializedProperty sameTypeSpawnThreshold;
    
    private SerializedProperty icon;
    private SerializedProperty rarity;
    private SerializedProperty caption;

    private void OnEnable()
    {
        soTarget = new SerializedObject(target);

        startHeight = soTarget.FindProperty("startHeight");
        sameTypeSpawnThreshold = soTarget.FindProperty("sameTypeSpawnThreshold");

        icon = soTarget.FindProperty("icon");
        rarity = soTarget.FindProperty("rarity");
        caption = soTarget.FindProperty("caption");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PlatformSO platformSO = (PlatformSO)target;

        EditorGUI.BeginChangeCheck();

        if (platformSO.IsHelper)
        {
            EditorGUILayout.PropertyField(startHeight);
            EditorGUILayout.PropertyField(sameTypeSpawnThreshold);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(rarity);
        }
        else
        {
            EditorGUILayout.Space();
        }

        EditorGUILayout.PropertyField(icon);
        EditorGUILayout.PropertyField(caption);

        if (EditorGUI.EndChangeCheck())
        {
            // If we have changed a property, save the modified property
            soTarget.ApplyModifiedProperties();
        }
    }
}
