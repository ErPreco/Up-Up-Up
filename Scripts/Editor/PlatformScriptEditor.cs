using UnityEditor;

[CustomEditor(typeof(PlatformScript)), CanEditMultipleObjects]
public class PlatformScriptEditor : Editor
{
    private SerializedObject soTarget;

    #region Spinner
    private SerializedProperty spinStep;
    #endregion

    #region Zoomer
    private SerializedProperty zoomOutMultiplier;
    private SerializedProperty zoomTime;
    #endregion

    #region Sprinter
    private SerializedProperty sprintMultiplier;
    private SerializedProperty lookaheadMultiplier;
    #endregion

    #region Shield
    private SerializedProperty playerShieldPrefab;
    private SerializedProperty shieldRadius;
    private SerializedProperty shieldTime;
    #endregion

    private SerializedProperty disappearAfterUse;

    private void OnEnable()
    {
        soTarget = new SerializedObject(target);

        #region Spinner
        spinStep = soTarget.FindProperty("spinStep");
        #endregion

        #region Zoomer
        zoomOutMultiplier = soTarget.FindProperty("zoomOutMultiplier");
        zoomTime = soTarget.FindProperty("zoomTime");
        #endregion

        #region Sprinter
        sprintMultiplier = soTarget.FindProperty("sprintMultiplier");
        lookaheadMultiplier = soTarget.FindProperty("lookaheadMultiplier");
        #endregion

        #region Shield
        playerShieldPrefab = soTarget.FindProperty("playerShieldPrefab");
        shieldRadius = soTarget.FindProperty("shieldRadius");
        shieldTime = soTarget.FindProperty("shieldTime");
        #endregion

        disappearAfterUse = soTarget.FindProperty("disappearAfterUse");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PlatformScript platform = (PlatformScript)target;

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();

        if (platform.PlatformType == PlatformType.Spinner)
        {
            EditorGUILayout.Slider(spinStep, 0.1f, 1f);
        }
        else if (platform.PlatformType == PlatformType.Zoomer)
        {
            EditorGUILayout.Slider(zoomOutMultiplier, 1, 1.5f);
            EditorGUILayout.Slider(zoomTime, 0.01f, 1);
        }
        else if (platform.PlatformType == PlatformType.Sprinter)
        {
            EditorGUILayout.Slider(sprintMultiplier, 1, 1.5f);
            EditorGUILayout.Slider(lookaheadMultiplier, 1.2f, 2.5f);
        }
        else if (platform.PlatformType == PlatformType.Shield)
        {
            EditorGUILayout.PropertyField(playerShieldPrefab);
            EditorGUILayout.PropertyField(shieldRadius);
            EditorGUILayout.PropertyField(shieldTime);
        }

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(disappearAfterUse);

        if (EditorGUI.EndChangeCheck())
        {
            // If we have changed a property, save the modified property
            soTarget.ApplyModifiedProperties();
        }
    }
}
