using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InteractionManager : Manager
{
    IEnumerator Start()
    {
        _textBox.enabled = true;
        _promptSkip.enabled = true;
        _promptContinue.enabled = false;

        InitializePlayerStats();

        if(EvaluateVampire())
        {
            Canvas statsCanvas =  _playerHealthbarSection.GetComponentInParent<Canvas>();
            statsCanvas.enabled = false;
            
            _currentLine = DialogueManager.VampireSunDamageLines[0]; 
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            statsCanvas.enabled = true;

            StartCoroutine(UpdateUI(GameConfig.VampireSunDamage, PlayerManager.Instance.HealthPoints));

            PlayerManager.Instance.HealthPoints -= GameConfig.VampireSunDamage;

            _currentLine = DialogueManager.VampireSunDamageLines[1];
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            statsCanvas.enabled = false;
        }

        yield return StartCoroutine(PrintMultipleLines(_texts));

        SceneManager.LoadScene(2);
    }

    private IEnumerator UpdateUI(int damage, int currentHealth)
    {
        float hitValue = 0;

        hitValue = (float)damage / (float)PlayerManager.Instance.GetStartingHealth();

        if(currentHealth <= 0)
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
}
