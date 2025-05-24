using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public enum SceneType
{
    IsMerchant  = 0,
    IsDog  = 1,
    IsNPC = 2
}
    
public class InteractionManager : Manager, ISelectable
{


    public SceneType sceneType;

    [SerializeField] public Canvas MerchantInventoryCanvas;
    [SerializeField] public Canvas ItemToDoCanvas;
    [SerializeField] public Canvas InitialMenuCanvas;
    [SerializeField] public Canvas DialogueCanvas;
    [SerializeField] private DialogueManager _dialogueManager;
    [SerializeField] private GameObject _merchant;

    IEnumerator Start()
    {
        ToggleCanvas(MerchantInventoryCanvas, false);
        ToggleCanvas(ItemToDoCanvas, false);
        ToggleCanvas(InitialMenuCanvas, false);
        ToggleCanvas(DialogueCanvas, false);

        _merchant.SetActive(false);

        Canvas statsCanvas = _playerHealthbarSection.GetComponentInParent<Canvas>();
        statsCanvas.enabled = false;

        _textBox.enabled = true;
        _promptSkip.enabled = false;
        _promptContinue.enabled = false;

        if (EvaluateVampire())
        {
            InitializePlayerStats();

            _currentLine = UIDialogueStorage.VampireSunDamageLines[0];
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            statsCanvas.enabled = true;

            StartCoroutine(UpdateUI(GameConfig.VampireSunDamage, PlayerManager.Instance.HealthPoints));

            PlayerManager.Instance.HealthPoints -= GameConfig.VampireSunDamage;

            _currentLine = UIDialogueStorage.VampireSunDamageLines[1];
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            statsCanvas.enabled = false;
        }

        SetScene();

        if (sceneType == SceneType.IsMerchant)
        {
            _currentLine = "Feel free to take a look at my merchandise, dear knight.";
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }

        _textBox.text = "";

        ToggleCanvas(InitialMenuCanvas, true);
    }

    private void OnEnable()
    {

    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    private void SetScene()
    {
        int randomIndex = Random.Range(0, 1);

        switch (randomIndex)
        {
            case 0:
                sceneType = SceneType.IsMerchant;

                _merchant.SetActive(false);

                break;

            case 1:
                sceneType = SceneType.IsDog;

                break;

            case 2:
                sceneType = SceneType.IsNPC;

                break;
        }
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
                ToggleCanvas(MerchantInventoryCanvas, true);
                ToggleCanvas(InitialMenuCanvas, false);

                break;

            case 1:
                ToggleCanvas(DialogueCanvas, true);
                ToggleCanvas(InitialMenuCanvas, false);

                _dialogueManager.StartDialogue();

                break;

            case 2:
                SceneManager.LoadScene(2);

                break;
        }
    }
}
