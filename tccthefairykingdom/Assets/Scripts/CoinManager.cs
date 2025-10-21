using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
   public static CoinManager Instance { get; private set; }

    [Header("UI")]
    public Image coinImage;                // arraste aqui o Image (CoinImage)
    public TextMeshProUGUI coinText;       // arraste aqui o TextMeshProUGUI (CoinText)

    [Header("Dados")]
    public int startCoins = 0;
    private int coins;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        coins = startCoins;
        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateUI();
        // PlayerPrefs.SetInt("Coins", coins); // opcional: salvar
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = coins.ToString(); // só o número
        // Se quiser "Moedas: 5" use: coinText.text = $"Moedas: {coins}";
    }

    // opcional: para setar o sprite por script (se necessário)
    public void SetCoinSprite(Sprite s)
    {
        if (coinImage != null)
        {
            coinImage.sprite = s;
            coinImage.SetNativeSize();
            coinImage.preserveAspect = true;
        }
    }
}