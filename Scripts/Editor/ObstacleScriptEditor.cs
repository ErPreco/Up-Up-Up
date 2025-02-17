using UnityEditor;

[CustomEditor(typeof(ObstacleScript)), CanEditMultipleObjects]
public class ObstacleScriptEditor : Editor
{
    private SerializedObject soTarget;

    #region Electrick Shock / Laser
    private SerializedProperty lightning;
    private SerializedProperty lightningCollider;

    private SerializedProperty activeTime;
    private SerializedProperty inactiveTime;

    private SerializedProperty stunTime;
    #endregion

    #region Shooter
    private SerializedProperty projectilePrefab;
    private SerializedProperty shootPoint;

    private SerializedProperty activationThreshold;
    private SerializedProperty reloadingTime;
    #endregion

    private void OnEnable()
    {
        soTarget = new SerializedObject(target);

        #region Electric Shock / Laser
        lightning = soTarget.FindProperty("lightning");
        lightningCollider = soTarget.FindProperty("lightningCollider");

        activeTime = soTarget.FindProperty("activeTime");
        inactiveTime = soTarget.FindProperty("inactiveTime");

        stunTime = soTarget.FindProperty("stunTime");
        #endregion

        #region Shooter
        projectilePrefab = soTarget.FindProperty("projectilePrefab");
        shootPoint = soTarget.FindProperty("shootPoint");

        activationThreshold = soTarget.FindProperty("activationThreshold");
        reloadingTime = soTarget.FindProperty("reloadingTime");
        #endregion
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ObstacleScript obstacle = (ObstacleScript)target;

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();

        if (obstacle.ObstacleType == ObstacleType.ElectricShock || obstacle.ObstacleType == ObstacleType.Laser)
        {
            EditorGUILayout.PropertyField(lightning);
            EditorGUILayout.PropertyField(lightningCollider);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(activeTime);
            EditorGUILayout.PropertyField(inactiveTime);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(stunTime);
        }

        if (obstacle.ObstacleType == ObstacleType.Shooter)
        {
            EditorGUILayout.PropertyField(projectilePrefab);
            EditorGUILayout.PropertyField(shootPoint);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(activationThreshold);
            EditorGUILayout.PropertyField(reloadingTime);
        }

        if (EditorGUI.EndChangeCheck())
        {
            // If we have changed a property, save the modified property
            soTarget.ApplyModifiedProperties();
        }
    }
}
