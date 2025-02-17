using UnityEngine;

public enum BonusType
{
    Shield
}

[CreateAssetMenu(fileName = "NewBonus", menuName = "Bonus")]
public class BonusSO : ScriptableObject
{
    #region Public References
    public BonusType Type => type;
    public Sprite Icon => icon;
    public int Price => price;
    public string Caption => caption;
    #endregion

    [SerializeField] private BonusType type;
    [SerializeField] private Sprite icon;
    [SerializeField] private int price;
    [SerializeField] [TextArea] private string caption;
}
