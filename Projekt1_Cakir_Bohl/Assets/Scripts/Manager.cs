using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI _textBox;
    [SerializeField] protected TextMeshProUGUI _promptSkip;
    [SerializeField] protected TextMeshProUGUI _promptContinue;
    [SerializeField] protected TextMeshProUGUI _playerUIHealth;
    [SerializeField] protected TextMeshProUGUI _playerUIEgo;
    [SerializeField] protected Canvas _statsCanvas;
    [SerializeField] protected Slider _playerHealthBarBelow;
    [SerializeField] protected Slider _playerEgoBarBelow;
    [SerializeField] protected AudioSource _mainEffectsAudioSource;
    [SerializeField] protected AudioSource _dialogueAudioSource;
    [SerializeField] protected AudioClip _playerHealthHit;
    [SerializeField] protected AudioClip _onHeal;
    [SerializeField] protected SpriteRenderer _autoArrows;
    protected Coroutine _textCoroutine;
    protected Coroutine _waitForContinueCoroutine;
    protected Item _currentItem;
    protected int _currentStringIndex = 0;
    protected string _currentLine;

    void Update()
    {
        ListenForSkipOrAuto();
    }

    // Hört darauf, ob die aktuelle Zeile übersprungen wird oder ob der Auto-Modus aktiviert/deaktiviert wurde
    public virtual void ListenForSkipOrAuto()
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

            if (PlayerManager.Instance.IsAuto)
            {
                _autoArrows.enabled = false;

                PlayerManager.Instance.IsAuto = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            if (_textCoroutine == null)
            {
                return;
            }

            if (PlayerManager.Instance.IsAuto == true)
            {
                _autoArrows.enabled = false;

                PlayerManager.Instance.IsAuto = false;

                if (_currentLine != "")
                {
                    _promptSkip.enabled = true;
                }
            }
            else
            {
                _autoArrows.enabled = true;

                PlayerManager.Instance.IsAuto = true;

                _promptSkip.enabled = false;
                _promptContinue.enabled = false;
            }
        }
    }

    // Umfasst mehrere Methoden der Dalogue.Util-Klasse, händelt z.B. auch das Beenden eines Abschnitts wenn isLastLine == true
    protected IEnumerator HandleTextOutput(string line, bool isLastLine, bool isDialogue = false)
    {
        if (isDialogue)
        {
            _dialogueAudioSource.Play();        
        }

        _textCoroutine = StartCoroutine(DialogueUtil.DisplayTextOverTime(line, _textBox, _promptSkip, _promptContinue));

        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitUntil.html
        yield return new WaitUntil(() => line == _textBox.text);

        if (isDialogue)
        {
            _dialogueAudioSource.Stop();        
        }

        if (!PlayerManager.Instance.IsAuto)
        {
            // Einen Frame warten, damit Input nicht beide GetKeyDown-Events triggert
            yield return null;

            yield return StartCoroutine(DialogueUtil.WaitForContinue(_promptContinue));
        }
        else
        {
            yield return new WaitForSeconds(GameConfig.TimeBeforeNextLine);
        }

        _currentLine = "";
    }

    // Schaltet einen Canvas ein und aus und setzt das darauf gelegende Menü zurück
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

    // Erweiterung von HandleTextOutput für ein ganzes String-Array
    protected IEnumerator PrintMultipleLines(string[] lines)
    {
        _textBox.enabled = true;

        foreach (string line in lines)
        {
            _currentLine = line;
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            _textBox.text = "";
        }
    }

    // Überprüft zu Anfang einer Szene, ob der Player ein Vampir ist und wendet den enstsprechenden Schaden an
    protected IEnumerator EvaluateVampire()
    {
        if (ConditionManager.Instance.IsVampire && MainManager.Instance.IsDay)
        {
            InitializePlayerStats();

            _currentLine = UIDialogueStorage.VampireSunDamageLines[0];
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            _mainEffectsAudioSource.PlayOneShot(_playerHealthHit);

            StartCoroutine(UpdateUIDamage(GameConfig.VampireSunDamage, PlayerManager.Instance.HealthPoints));

            PlayerManager.Instance.HealthPoints -= GameConfig.VampireSunDamage;

            _currentLine = UIDialogueStorage.VampireSunDamageLines[1];
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

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

    // Druckt die entsprechenden Zeilen, wenn der Player als Zombie nicht eine Konversation anfangen kann
    public virtual IEnumerator ZombieConversationAttempt()
    {
        yield return PrintMultipleLines(UIDialogueStorage.ZombieCantSpeakLines);

        _textBox.text = "";
    }

    // Visualisiert Schaden von Health in den Leisten
    public virtual IEnumerator UpdateUIDamage(int damage, int currentHealth)
    {
        float hitValue = (float)damage / (float)PlayerManager.Instance.GetStartingHealth();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
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

    // Visualisiert Heilung von Health oder Ego in den Leisten
    protected IEnumerator UpdateUIHeal(int healAmount, bool isHealthChange, int initialAmount)
    {
        _mainEffectsAudioSource.PlayOneShot(_onHeal);
        
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

    // Setzt die PlayerStats (Health- und Ego-Anzeige sowie Slider)
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

    // Zögert kurz heraus ob eine Aktion erfolreich war oder nicht
    protected IEnumerator AnticipationTextCoroutine(bool isGoodOutcome)
    {
        float counter = 0f;
        string singleChar = ". ";
        string finalLine = ". . . ";

        while (counter < GameConfig.AnticipationLength)
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

        if (!isGoodOutcome)
        {
            _textBox.text = "!!!";

            yield return new WaitForSeconds(GameConfig.TimeAfterAnticipation);
        }

    }

    // Druckt die entsprechenden Zeilen beim Tag/Nacht-Wechsel, wenn der Player ein Vampir ist
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

    // Schaltet den Cursor sichtbar oder unsichtbar, je nach Szene
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

    // Holt sich das IUsable-Interface des entsprechenden Items und führt die UseItem-Methode aus, die ein String-Array zurückgibt
    public virtual IEnumerator UseSelectedItem()
    {
        if (_currentItem is not IUsable)
        {
            yield break;
        }

        var iUsable = _currentItem as IUsable;

        string[] currentItemLines = iUsable.UseItem().ToArray();

        // Erste Line immer zeigen, auswürfeln ob auch eine zweite angezeigt wird (eine Art Easter Egg)
        _currentLine = currentItemLines[0];
        yield return HandleTextOutput(_currentLine, false);

        if (currentItemLines.Length > 1)
        {
            if (DiceUtil.D10() > GameConfig.ChanceForSecondLine)
            {
                _currentLine = currentItemLines[UnityEngine.Random.Range(1, currentItemLines.Length)];
                yield return HandleTextOutput(_currentLine, false);
            }
        }
    }

    // Aktiviert die Continue/Skip-Prompts am Anfang einer Szene je nachdem ob der Auto-Modus aktiviert ist oder nicht
    protected void SetPrompts()
    {
        if (PlayerManager.Instance.IsAuto)
        {
            _autoArrows.enabled = true;
            _promptSkip.enabled = false;
        }
        else
        {
            _autoArrows.enabled = false;
            _promptSkip.enabled = true;
        }

        _promptContinue.enabled = false;
    }
}
