using UnityEngine;
using UnityEngine.UI;

public class TextController : MonoBehaviour
{
    [SerializeField] private Text _quantityPlayers;

    private void Start()
    {
        GameLogic.UpdateQuantityPlayers += UpdateQuantity;
    }
    public void UpdateQuantity(int quantity)
    {
        _quantityPlayers.text = quantity.ToString();
    }
}
