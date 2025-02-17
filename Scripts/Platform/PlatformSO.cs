using UnityEngine;

[CreateAssetMenu(fileName = "NewPlatform", menuName = "Platform")]
public class PlatformSO : ScriptableObject
{
    #region Public References
    public GameObject Prefab => prefab;
    public PlatformType Type => prefab.GetComponent<PlatformScript>().PlatformType;
    public int MaxTilt => maxTilt;
    public int StartHeight => startHeight;
    public bool IsHelper => isHelper;
    public int SameTypeSpawnThreshold => sameTypeSpawnThreshold;

    public Rarity Rarity => rarity;
    public Sprite Icon => icon;
    public string Caption => caption;
    #endregion

    [SerializeField] private GameObject prefab;
    [SerializeField] private int maxTilt;
    [SerializeField] private bool isHelper;

    [Tooltip("The height in meters from which start spawning")]
    [HideInInspector] public int startHeight;
    [Tooltip("The minimum distance in unity unit between two platforms of the same type")]
    [HideInInspector] public int sameTypeSpawnThreshold;

    [HideInInspector] public Rarity rarity;
    [HideInInspector] public Sprite icon;
    [HideInInspector] [TextArea] public string caption;
}
