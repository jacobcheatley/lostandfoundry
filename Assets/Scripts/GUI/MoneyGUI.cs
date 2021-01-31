using UnityEngine;
using TMPro;

public class MoneyGUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI moneyText;

    void Start()
    {
        moneyText.text = $"{ResourceTracker.Money}";
        ResourceTracker.OnMoneyChange += (_, newMoney) => moneyText.text = $"{newMoney}";
    }
}
