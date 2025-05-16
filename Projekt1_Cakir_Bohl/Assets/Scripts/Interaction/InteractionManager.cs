using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : Manager, ISelectable
{
    public enum SceneType
    {
        IsMerchant  = 0,
        IsDog  = 1,
        IsNPC = 2
    }

    public SceneType sceneType;

    [SerializeField] public Canvas MerchantInventoryCanvas;
    [SerializeField] public Canvas ItemToDoCanvas;
    private Item _currentItem;

    IEnumerator Start()
    {
        ToggleCanvas(MerchantInventoryCanvas, false);
        ToggleCanvas(ItemToDoCanvas, false);

        Canvas statsCanvas =  _playerHealthbarSection.GetComponentInParent<Canvas>();
        statsCanvas.enabled = false;
        
        if (EvaluateVampire())
        {
            InitializePlayerStats();

            _currentLine = DialogueManager.VampireSunDamageLines[0];
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            statsCanvas.enabled = true;

            StartCoroutine(UpdateUI(GameConfig.VampireSunDamage, PlayerManager.Instance.HealthPoints));

            PlayerManager.Instance.HealthPoints -= GameConfig.VampireSunDamage;

            _currentLine = DialogueManager.VampireSunDamageLines[1];
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            statsCanvas.enabled = false;
        }

        SetScene();

        _textBox.enabled = true;
        _promptSkip.enabled = true;
        _promptContinue.enabled = false;


        //yield return StartCoroutine(PrintMultipleLines(_texts));

        //SceneManager.LoadScene(2);
    }

    private void OnEnable()
    {
        InventoryDisplayer.itemSelection += OnItemSelected;
        //Merchant.onTryPurchase += OnTryPurchase;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        InventoryDisplayer.itemSelection -= OnItemSelected;
        //Merchant.onTryPurchase -= OnTryPurchase;
    }    

    private void SetScene()
    {
        int randomIndex = Random.Range(0, 1);

        switch (randomIndex)
        {
            case 0:
                sceneType = SceneType.IsMerchant;
                ToggleCanvas(MerchantInventoryCanvas, true);

                break;

            case 1:
                sceneType = SceneType.IsDog;

                break;

            case 2:
                sceneType = SceneType.IsNPC;

                break;
        }
    }

    private void OnItemSelected(Item item)
    {
        MerchantInventoryCanvas.GetComponent<SelectionMenu>().IsActive = false;

        ToggleCanvas(ItemToDoCanvas, true);

        _currentItem = item;
    }

    // private void OnTryPurchase()
    // {
    //     StartCoroutine(TryPurchase());
    // }

    private IEnumerator TryPurchase()
    {
        Item coin = new Coin();
        int currentCoins;

        // https://stackoverflow.com/questions/2829873/how-can-i-detect-if-this-dictionary-key-exists-in-c
        if (InventoryManager.Instance.Inventory.TryGetValue(coin, out currentCoins))
        {
            if (currentCoins >= _currentItem.StorePrice)
            {
                InventoryManager.Instance.Inventory[coin] = currentCoins - _currentItem.StorePrice;

                InventoryManager.Instance.ManageInventory(_currentItem, 1, true);

                _currentLine = $"You have purchased {_currentItem.Name} for {_currentItem.StorePrice}";
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));
            }
            else
            {
                _currentLine = "You don't have enough coins.";
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));
            }
        }
        else
        {
            _currentLine = "You don't have any coins.";
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }

        ToggleCanvas(ItemToDoCanvas, false);
        MerchantInventoryCanvas.GetComponent<SelectionMenu>().IsActive = true;
    }

    private IEnumerator UpdateUI(int damage, int currentHealth)
    {
        float hitValue = 0;

        hitValue = (float)damage / (float)PlayerManager.Instance.GetStartingHealth();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // GameOver Screen
        }

        _playerUIHealth.text = $"{currentHealth - damage}/{PlayerManager.Instance.GetStartingHealth()}";

        float currentValue = _playerHealthBarBelow.value;
        float nextValue = currentValue - hitValue;
        float lerpValue = 0;

        // Weiße Healthbar setzen
        var childSlider = UnityUtil.GetFirstComponentInChildren<Slider>(_playerHealthBarBelow.gameObject);
        childSlider.GetComponent<Slider>().value = nextValue;

        yield return new WaitForSeconds(GameConfig.TimeBeforeHealthbarUpdate);

        while (lerpValue <= 1 && lerpValue >= 0)
        {
            lerpValue += GameConfig.BarsLerpSpeed * Time.deltaTime;
            _playerHealthBarBelow.value = Mathf.Lerp(currentValue, nextValue, lerpValue / hitValue);
            yield return null;
        }

        _playerHealthBarBelow.value = nextValue;
    }

    public void HandleSelectedMenuPoint(int index)
    {
        switch (index)
        {
            case 0:
                StartCoroutine(TryPurchase());
                ToggleCanvas(ItemToDoCanvas, false);

                break;

            case 1:
                ToggleCanvas(ItemToDoCanvas, false);
                MerchantInventoryCanvas.GetComponent<SelectionMenu>().IsActive = true;
                break;
        }
    }
}
