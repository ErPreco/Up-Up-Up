using UnityEngine;
using UnityEngine.UI;

public class BonusButton : MonoBehaviour
{
    [SerializeField] private BonusType type;

    [Space]
    [SerializeField] private ShopManager shopManager;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => OnClicked());
    }

    private void OnClicked()
    {
        if (shopManager.IsBonusBought)
        {
            // TODO: pop up an alert
            return;
        }

        shopManager.BuyBonus(type);
    }
}
