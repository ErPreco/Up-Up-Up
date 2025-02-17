using UnityEngine;

public class ShopManager : MonoBehaviour
{
    #region Public References
    public bool IsBonusBought => isBonusBought;
    #endregion

    [SerializeField] private BonusSO[] shopItemsSO;

    private BonusType bonus;
    private bool isBonusBought;

    public void BuyBonus(BonusType type)
    {
        bonus = type;

        isBonusBought = true;
    }

    public void UseBonus()
    {

    }
}
