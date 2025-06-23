using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PondManager : Manager, ISelectable
{
    [SerializeField] private Canvas _selectionMenuCanvas;
    [SerializeField] private Canvas _statsCanvas;
    [SerializeField] private ConditionIconsManager _conditionIconsManager;

    IEnumerator Start()
    {
        ToggleCanvas(_selectionMenuCanvas, false);
        ToggleCanvas(_statsCanvas, false);

        InitializePlayerStats();

        yield return PrintMultipleLines(UIDialogueStorage.PondReachedLines);

        _textBox.text = "";

        ToggleCanvas(_selectionMenuCanvas, true);
    }

    // Update is called once per frame
    void Update()
    {
        ListenForSkip();
    }

    public void HandleSelectedMenuPoint(int index)
    {
        switch (index)
        {
            case 0:

                ToggleCanvas(_selectionMenuCanvas, false);
                StartCoroutine(PondCoroutine());

                break;

            case 1:

                SceneManager.LoadScene(2);

                break;

            default:
                throw new IndexOutOfRangeException();
        }
    }

    private IEnumerator PondCoroutine()
    {
        bool wasHurt = false;

        yield return PrintMultipleLines(UIDialogueStorage.PondEntryLines);

        ToggleCanvas(_statsCanvas, true);

        ConditionManager.Instance.ResetStats();

        _conditionIconsManager.SetIcons();
        
        if (PlayerManager.Instance.HealthPoints < PlayerManager.Instance.GetStartingHealth())
        {
            StartCoroutine(UpdateUI(PlayerManager.Instance.GetStartingHealth() - PlayerManager.Instance.HealthPoints, true, PlayerManager.Instance.HealthPoints));

            PlayerManager.Instance.HealthPoints = PlayerManager.Instance.GetStartingHealth();

            wasHurt = true;
        }

        if (PlayerManager.Instance.EgoPoints < PlayerManager.Instance.GetStartingEgo())
        {
            StartCoroutine(UpdateUI(PlayerManager.Instance.GetStartingEgo() - PlayerManager.Instance.EgoPoints, false, PlayerManager.Instance.EgoPoints));

            PlayerManager.Instance.EgoPoints = PlayerManager.Instance.GetStartingEgo();

            wasHurt = true;
        }


        if (wasHurt)
        {
            _currentLine = "Both your body and mind are now fully recovered!";
        }
        else
        {
            _currentLine = "The water has cleansed you of your unfortunate conditions.";
        }

        yield return HandleTextOutput(_currentLine, false);

        SceneManager.LoadScene(2);
    }

    // Visualisiert Heilung von Health oder Ego in den Leisten
    private IEnumerator UpdateUI(int healAmount, bool isHealthChange, int initialAmount)
    {
        float healValue = 0;
        Slider slider = null;

        if (isHealthChange)
        {
            slider = _playerHealthBarBelow;
            healValue = (float)healAmount / (float)PlayerManager.Instance.GetStartingHealth();
            _playerUIHealth.text = $"{initialAmount + healAmount}/{PlayerManager.Instance.GetStartingHealth()}";
        }
        else
        {
            slider = _playerEgoBarBelow;
            healValue = (float)healAmount / (float)PlayerManager.Instance.GetStartingEgo();
            _playerUIEgo.text = $"{initialAmount + healAmount}/{PlayerManager.Instance.GetStartingEgo()}";
        }

        float currentValue = slider.value;
        float nextValue = currentValue + healValue;
        float lerpValue = 0;

        // Untere Healthbar setzen
        slider.value = nextValue;

        yield return new WaitForSeconds(GameConfig.TimeBeforeHealthbarUpdate);

        var childSlider = UnityUtil.GetFirstComponentInChildren<Slider>(slider.gameObject);
        childSlider.GetComponent<Slider>().value = nextValue;

        while (lerpValue <= 1 && lerpValue >= 0)
        {
            lerpValue += GameConfig.BarsLerpSpeed * Time.deltaTime;
            childSlider.value = Mathf.Lerp(currentValue, nextValue, lerpValue / healValue);
            yield return null;
        }

        childSlider.value = nextValue;
    }
}
