using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    [SerializeField] protected string[] _texts;
    [SerializeField] protected TextMeshProUGUI _textBox;
    [SerializeField] protected TextMeshProUGUI _promptSkip;
    [SerializeField] protected TextMeshProUGUI _promptContinue;
    [SerializeField] protected TextMeshProUGUI _playerUIHealth;
    [SerializeField] protected TextMeshProUGUI _playerUIEgo;
    [SerializeField] protected Canvas _statsCanvas;
    [SerializeField] protected Slider _playerHealthBarBelow;
    [SerializeField] protected Slider _playerEgoBarBelow;
    [SerializeField] protected RectTransform _playerHealthbarSection;
    protected Coroutine _textCoroutine;
    protected Coroutine _waitForContinueCoroutine;
    protected int _currentStringIndex = 0;
    protected string _currentLine;

    void Update()
    {
        ListenForSkip();
    }

    protected void ListenForSkip()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_textCoroutine != null)
            {
                _promptSkip.enabled = false;
                StopCoroutine(_textCoroutine);
                _textCoroutine = null;
                DialogueUtil.ShowFullLine(_currentLine, _textBox, _promptSkip);
            }
        }
    }

    // Umfasst mehrere Methoden der Dalogue.Util-Klasse, händelt z.B. auch das Beenden eines Abschnitts wenn isLastLine == true
    protected IEnumerator HandleTextOutput(string line, bool isLastLine)
    {
        _textCoroutine = StartCoroutine(DialogueUtil.DisplayTextOverTime(line, _textBox, _promptSkip, _promptContinue));

        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitUntil.html
        yield return new WaitUntil(() => line == _textBox.text);

        if (isLastLine)
        {
            yield break;
        }

        // Einen Frame warten, damit Input nicht beide GetKeyDown-Events triggert
        yield return null;

        yield return StartCoroutine(DialogueUtil.WaitForContinue(_promptContinue));
    }

    public void ToggleCanvas(Canvas canvas, bool isActive)
    {
        canvas.enabled = isActive;

        // Zb im Falle der Dialoge trägt der Canvas hier selbst kein Selection Menu
        // https://docs.unity3d.com/ScriptReference/Component.TryGetComponent.html
        if (canvas.TryGetComponent(out SelectionMenu selectionMenu))
        {
            selectionMenu.SetInitialPointer();
            selectionMenu.enabled = isActive;

            if (isActive)
            {
                selectionMenu.InitializeMenu();
            }
        }
    }

    public void TogglePlayerStatsPosition(bool isDefaultPosition)
    {
        if (isDefaultPosition)
        {
            _playerHealthbarSection.SetLocalPositionAndRotation(GameConfig.HealthbarDefaultPosition, quaternion.identity);
        }
        else
        {
            _playerHealthbarSection.SetLocalPositionAndRotation(GameConfig.HealthbarAlternativePosition, quaternion.identity);
        }
    }

    protected IEnumerator PrintMultipleLines(string[] lines)
    {
        _textBox.enabled = true;

        foreach (string line in lines)
        {
            _currentLine = line;
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }
    }

    protected IEnumerator EvaluateVampire()
    {
        if (ConditionManager.Instance.IsVampire && MainManager.Instance.IsDay)
        {
            InitializePlayerStats();

            _currentLine = UIDialogueStorage.VampireSunDamageLines[0];
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            _statsCanvas.enabled = true;

            StartCoroutine(UpdateUI(GameConfig.VampireSunDamage, PlayerManager.Instance.HealthPoints));

            PlayerManager.Instance.HealthPoints -= GameConfig.VampireSunDamage;

            _currentLine = UIDialogueStorage.VampireSunDamageLines[1];
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            _statsCanvas.enabled = false;

            if (PlayerManager.Instance.HealthPoints <= 0)
            {
                _currentLine = "You have died. Your quest has ended.";
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));

                StopAllCoroutines();

                SceneManager.LoadScene("StartMenu");
                yield break;
            }
        }

        yield break;
    }

    public virtual IEnumerator UpdateUI(int damage, int currentHealth)
    {
        float hitValue = (float)damage / (float)PlayerManager.Instance.GetStartingHealth();

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

    protected void InitializePlayerStats()
    {
        _playerUIHealth.text = $"{PlayerManager.Instance.HealthPoints}/{PlayerManager.Instance.GetStartingHealth()}";
        _playerUIEgo.text = $"{PlayerManager.Instance.EgoPoints}/{PlayerManager.Instance.GetStartingEgo()}";

        // Weiße Healthbar setzen
        _playerHealthBarBelow.value = (float)PlayerManager.Instance.HealthPoints / (float)PlayerManager.Instance.GetStartingHealth();
        var childSlider = UnityUtil.GetFirstComponentInChildren<Slider>(_playerHealthBarBelow.gameObject);
        childSlider.GetComponent<Slider>().value = (float)PlayerManager.Instance.HealthPoints / (float)PlayerManager.Instance.GetStartingHealth();

        _playerEgoBarBelow.value = (float)PlayerManager.Instance.EgoPoints / (float)PlayerManager.Instance.GetStartingEgo();
        childSlider = UnityUtil.GetFirstComponentInChildren<Slider>(_playerEgoBarBelow.gameObject);
        childSlider.GetComponent<Slider>().value = (float)PlayerManager.Instance.EgoPoints / (float)PlayerManager.Instance.GetStartingEgo();
    }

    protected IEnumerator AnticipationTextCoroutine()
    {
        float counter = 0f;
        string singleChar = ". ";
        string finalLine = ". . . ";

        while (counter < 3.5f) // Hier Animationslänge einfügen
        {
            if (_textBox.text != finalLine)
            {
                _textBox.text += singleChar;
            }
            else
            {
                _textBox.text = "";
            }

            yield return new WaitForSeconds(GameConfig.AnticipationCharsSpeed);

            counter += GameConfig.AnticipationCharsSpeed;
        }

        _textBox.text = "!!!";

        yield return new WaitForSeconds(GameConfig.TimeAfterAnticipation);
    }

    protected IEnumerator EvaluateWerewolfCondition(bool turnsToNight)
    {
        if (turnsToNight)
        {
            ConditionManager.Instance.ToggleWerewolfStats(true);

            yield return StartCoroutine(PrintMultipleLines(UIDialogueStorage.WerewolfNightLines));
        }
        else
        {
            ConditionManager.Instance.ToggleWerewolfStats(false);

            yield return StartCoroutine(PrintMultipleLines(UIDialogueStorage.WerewolfDayLines));
        }
    }

    // Setzt die Variablen im MainManager zurück, damit diese mit den Daten des nächsten Tages befüllt werden können
    protected void SetUpNextDay(bool isDay)
    {
        MainManager.Instance.CurrentDay++;
        MainManager.Instance.WayPoints.Clear();
        MainManager.Instance.WayPointTypes.Clear();
        MainManager.Instance.LastWayPoint = "";
        MainManager.Instance.IsDay = isDay;
    }

    protected void ToggleCursorState(bool isLocked)
    {
        if (isLocked)
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }
    }
}
