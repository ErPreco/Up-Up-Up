using UnityEngine;

[CreateAssetMenu(fileName = "NewObstacle", menuName = "Obstacle")]
public class ObstacleSO : ScriptableObject
{
    #region Public Reference
    public GameObject Prefab => prefab;
    public ObstacleType Type => prefab.GetComponent<ObstacleScript>().ObstacleType;
    public int StartHeight => startHeight;
    public int SameTypeSpawnThreshold => sameTypeSpawnThreshold;

    public Rarity Rarity => rarity;
    public Sprite Icon => icon;
    public string Caption => caption;
    #endregion

    [SerializeField] private GameObject prefab;
    [Tooltip("The height in meters from which start spawning")]
    [SerializeField] private int startHeight;
    [Tooltip("The minimum distance in unity unit between two obstacles of the same type")]
    [SerializeField] private int sameTypeSpawnThreshold;

    [Space]
    [SerializeField] private Rarity rarity;
    [SerializeField] private Sprite icon;
    [SerializeField] [TextArea] private string caption;
}
