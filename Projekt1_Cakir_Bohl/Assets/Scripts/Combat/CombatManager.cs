using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour, ISelectable
{
    [SerializeField] private Combatant _enemy;
    [SerializeField] private TextMeshProUGUI _textBox;
    [SerializeField] private TextMeshProUGUI _promptSkip;
    [SerializeField] private TextMeshProUGUI _promptContinue;
    [SerializeField] private TextMeshProUGUI _playerHealth;
    [SerializeField] private TextMeshProUGUI _playerEgo;
    [SerializeField] private TextMeshProUGUI _enemyHealth;
    [SerializeField] private TextMeshProUGUI _enemyEgo;
    [SerializeField] private Slider _enemyHealthBarBelow;
    [SerializeField] private Slider _playerHealthBarBelow;
    [SerializeField] private Slider _enemyEgoBarBelow;
    [SerializeField] private Slider _playerEgohBarBelow;
    [SerializeField] private Canvas _selectionMenuCanvas;
    [SerializeField] private Canvas _persuasionMenuCanvas;

    private int _playerRoll;
    private int _enemyRoll;
    private int _combatantHealth1;
    private int _combatantHealth2;
    private int _rawDamage;
    private int _finalDamage;
    private int _accuracy;
    private int _evasion;
    private int _currentInsultIndex;
    private int _enemyEgoPoints;
    private bool _hasFightStarted;
    private string _currentLine;
    private string _egoHitLine;
    private string _egoResistLine;
    private Combatant _combatant1;
    private Combatant _combatant2;
    private Coroutine _combatCoroutine;
    private Coroutine _textCoroutine;
    private Coroutine _insultCoroutine;
    private Dictionary<string, int> _insultsAndValues = new Dictionary<string, int>();
    private Dictionary<string, int> _currentInsultsAndValues = new Dictionary<string, int>();

    public static event Action OnFightFinished;

    void Start()
    {
        _hasFightStarted = false;
        _textBox.enabled = false;
        _promptSkip.enabled = false;
        _promptContinue.enabled = false;
        _persuasionMenuCanvas.enabled = false;
        _persuasionMenuCanvas.GetComponent<SelectionMenu>().enabled = false;

        SetInitialStats();

        // Alle Insult Lines und Values des jeweiligen Gegners holen
        for (int i = 0; i < _enemy.InsultLines.Insults.Length; i++)
        {
            _insultsAndValues.Add(_enemy.InsultLines.Insults[i], _enemy.InsultLines.Values[i]);
        }
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void Update()
    {
        // Ermöglicht sofortiges Anzeigen der gesamten derzeitigen Line
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

    private void SetInitialStats()
    {
        _enemyEgoPoints = _enemy.EgoPoints;
        _playerHealth.text = $"{PlayerManager.Instance.HealthPoints}/{GameConfig.PlayerStartingHealth}";
        _playerEgo.text = $"{PlayerManager.Instance.EgoPoints}/{PlayerManager.Instance.EgoPoints}";
        _enemyHealth.text = $"{_enemy.HealthPoints}/{_enemy.HealthPoints}";
        _enemyEgo.text = $"{_enemy.EgoPoints}/{_enemy.EgoPoints}";
        _playerHealthBarBelow.value = (float)GameConfig.PlayerStartingHealth / (float)PlayerManager.Instance.HealthPoints;

        // Weiße Healthbar setzen
        var childSlider = UnityUtil.GetFirstComponentInChildren<Slider>(_playerHealthBarBelow.gameObject);
        childSlider.GetComponent<Slider>().value = (float)GameConfig.PlayerStartingHealth / (float)PlayerManager.Instance.HealthPoints;
    }

    private IEnumerator DisplayInsultOptions()
    {
        _selectionMenuCanvas.enabled = false;
        _selectionMenuCanvas.GetComponent<SelectionMenu>().enabled = false;
        
        // Wenn keine 2 Optionen mehr gegeben werden können: Ende
        if (_insultsAndValues.Count < 2)
        {
            _textBox.enabled = true;
            _currentLine = "You can't think of anything else to say!";
            yield return StartCoroutine(HandleTextOutput(_currentLine, true));

            _textBox.enabled = false;
            _persuasionMenuCanvas.enabled = false;
            _selectionMenuCanvas.enabled = true;
            _selectionMenuCanvas.GetComponent<SelectionMenu>().enabled = true;
            yield break;
        }

        _persuasionMenuCanvas.enabled = true;
        _persuasionMenuCanvas.GetComponent<SelectionMenu>().enabled = true;

        TextMeshProUGUI[] options = _persuasionMenuCanvas.GetComponentsInChildren<TextMeshProUGUI>();

        // Zwei zufällige Optionen und Werte zuweisen, danach jeweils aus dem Original-Dictionary entfernen, damit nicht zweimal dieselbe Option angezeigt wird
        for (int i = 0; i < options.Length - 1; i++)
        {
            int index = UnityEngine.Random.Range(0, _insultsAndValues.Count - 1);

            options[i].text = _insultsAndValues.ElementAt(index).Key;
            _currentInsultsAndValues.Add(options[i].text, _insultsAndValues.ElementAt(index).Value);
            _insultsAndValues.Remove(options[i].text);
        }
    }

    private IEnumerator InsultCoroutine(int optionIndex)
    {
        string line = "";
        int value = 0;

        _textBox.enabled = true;

        // Je nach ausgewählter Option Line und Value zuweisen sowie mögliche Antworten bereits laden, nicht gewählte Optionen wieder ins Original-Dictionary einfügen
        switch(optionIndex)
        {
            case 0:
                line = $"{PlayerManager.Instance.Name}: '{_currentInsultsAndValues.ElementAt(0).Key}'";
                value = _currentInsultsAndValues.ElementAt(0).Value;
                _egoHitLine = _enemy.InsultLines.AnswersWhenHit[Array.IndexOf(_enemy.InsultLines.Insults, _currentInsultsAndValues.ElementAt(0).Key)];
                _egoResistLine = _enemy.InsultLines.AnswersWhenResisted[Array.IndexOf(_enemy.InsultLines.Insults, _currentInsultsAndValues.ElementAt(0).Key)];
                _insultsAndValues.Add( _currentInsultsAndValues.ElementAt(1).Key, _currentInsultsAndValues.ElementAt(1).Value);
                break;

            case 1:
                line = $"{PlayerManager.Instance.Name}: '{_currentInsultsAndValues.ElementAt(1).Key}'";
                value = _currentInsultsAndValues.ElementAt(1).Value;
                _egoHitLine = _enemy.InsultLines.AnswersWhenHit[Array.IndexOf(_enemy.InsultLines.Insults, _currentInsultsAndValues.ElementAt(1).Key)];
                _egoResistLine = _enemy.InsultLines.AnswersWhenResisted[Array.IndexOf(_enemy.InsultLines.Insults, _currentInsultsAndValues.ElementAt(1).Key)];
                _insultsAndValues.Add(_currentInsultsAndValues.ElementAt(0).Key, _currentInsultsAndValues.ElementAt(0).Value);
                break;

            case 2:
                _textBox.enabled = false;
                _persuasionMenuCanvas.enabled = false;
                break;

            default:
                Debug.LogError("Insult index not set correctly!");
                break;
        }

        _textBox.enabled = true;

        // Insult-Kampf nur ausführen, wenn eine Option ausgewählt wurde
        if (line.Length > 0)
        {
            _persuasionMenuCanvas.enabled = false;
            _persuasionMenuCanvas.GetComponent<SelectionMenu>().enabled = false;

            _playerRoll = value + DiceUtil.D4();

            _currentLine = line;
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
                
            _finalDamage = _playerRoll - _enemy.InsultResistance;

            if (_finalDamage > 0)
            {
                _currentLine = $"{_enemy.Name}: '{_egoHitLine}'";
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));

                _enemyEgoPoints -= _finalDamage;

                StartCoroutine(UpdateUIStats(combatant: _enemy, damage: _finalDamage, currentHealth: _enemyEgoPoints, isHealthDamage: false, currentEgo: _enemyEgoPoints));

                _currentLine = $"{_enemy.Name} took {_finalDamage} ego damage!";
                yield return StartCoroutine(HandleTextOutput(_currentLine, true));

            }
            else
            {
                _currentLine = $"{_enemy.Name}: '{_egoResistLine}'";
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));

                _currentLine = $"{_enemy.Name} has resisted your insult!";
                yield return StartCoroutine(HandleTextOutput(_currentLine, true));
            }
        }

        if (_enemyEgoPoints <= 0)
        {
            _insultCoroutine = null;
            StartCoroutine(EndFight(PlayerManager.Instance));
            yield break;
        }

        //Insult-Kampf abwickeln
        _currentInsultsAndValues.Clear();

        _textBox.text = "";
        _textBox.enabled = false;
        _selectionMenuCanvas.enabled = true;
        _selectionMenuCanvas.GetComponent<SelectionMenu>().enabled = true;
        _insultCoroutine = null;
    }

    // Festlegen, wer zuerst angreift
    private IEnumerator RollInitiative(bool isDisadvantage)
    {
        if (isDisadvantage)
        {
            _combatant1 = _enemy;
            _combatant2 = PlayerManager.Instance;
        }
        else
        {
            _playerRoll = PlayerManager.Instance.RollInitiative();
            _enemyRoll = _enemy.RollInitiative();

            _combatant1 = _playerRoll >= _enemyRoll ? PlayerManager.Instance : _enemy;
            _combatant2 = _playerRoll >= _enemyRoll ? _enemy : PlayerManager.Instance;
            
            _currentLine = DialogueUtil.CreateCombatLog(_combatant1, "goes", "first!");
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }

        _combatantHealth1 = _combatant1.HealthPoints;
        _combatantHealth2 = _combatant2.HealthPoints;
    }

    private IEnumerator CombatCoroutine(bool isDisadvantage)
    {
        _textBox.enabled = true;

        if (!_hasFightStarted)
        {
            yield return StartCoroutine(RollInitiative(isDisadvantage));

            // Nach Disadvantage-Angriff neues Zuweisen der Reihenfolge möglich
            if(!isDisadvantage)
            {
                _hasFightStarted = true;
            }
        }
        
        if(isDisadvantage)
        {
            _currentLine = DialogueUtil.CreateCombatLog(_combatant1, "attacks", "you!");
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }
        else
        {
            _currentLine = DialogueUtil.CreateCombatLog(_combatant1, "prepares", "to strike!");
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }

        _rawDamage = PerformAttack(_combatant1, _combatant2);
        print($"{_combatant1.Name} Damage: {_rawDamage}");

        if (_rawDamage <= 0)
        {
            _currentLine = DialogueUtil.CreateCombatLog(_combatant2, "has", "evaded the attack!");

            if (isDisadvantage)
            {
                yield return StartCoroutine(HandleTextOutput(_currentLine, true));
            }
            else
            {
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));
            }
        }

        CalculateDamage(_rawDamage, _combatant2, _combatantHealth2);

        if (_finalDamage >= _combatantHealth2)
        {
            _combatantHealth2 = 0;
        }
        else
        {
            _combatantHealth2 -= _finalDamage;
        }

        if (_finalDamage > 0)
        {
            _currentLine = DialogueUtil.CreateCombatLog(_combatant2, "takes", $"{_finalDamage} damage!");
            StartCoroutine(UpdateUIStats(combatant: _combatant2, damage: _finalDamage, currentHealth: _combatantHealth2, isHealthDamage: true));

            if (isDisadvantage)
            {
                yield return StartCoroutine(HandleTextOutput(_currentLine, true));
            }
            else
            {
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));
            }
        }

        if (_combatantHealth2 <= 0)
        {
            _combatCoroutine = null;
            StartCoroutine(EndFight(_combatant1, _combatantHealth1));
            yield break;
        }

        if (isDisadvantage)
        {
            isDisadvantage = false;

            _combatCoroutine = null;
            StartCoroutine(EndFight(null));
            yield break;
        }

        _currentLine = DialogueUtil.CreateCombatLog(_combatant2, "prepares", "to strike!");
        yield return StartCoroutine(HandleTextOutput(_currentLine, false));

        _rawDamage = PerformAttack(_combatant2, _combatant1);
        print($"{_combatant1.Name} Damage: {_rawDamage}");

        if (_rawDamage <= 0)
        {
            _currentLine = DialogueUtil.CreateCombatLog(_combatant1, "has", "evaded the attack!");
            yield return StartCoroutine(HandleTextOutput(_currentLine, true));
        }

        CalculateDamage(_rawDamage, _combatant1, _combatantHealth1);

        if (_finalDamage >= _combatantHealth1)
        {
            _combatantHealth1 = 0;
        }
        else
        {
            _combatantHealth1 -= _finalDamage;
        }

        if (_finalDamage > 0)
        {
            _currentLine = DialogueUtil.CreateCombatLog(_combatant1, "takes", $"{_finalDamage} damage!");
            StartCoroutine(UpdateUIStats(combatant: _combatant1, damage: _finalDamage, currentHealth: _combatantHealth1, isHealthDamage: true));

            yield return StartCoroutine(HandleTextOutput(_currentLine, true));
        }

        if (_combatantHealth1 <= 0)
        {
            _combatCoroutine = null;
            StartCoroutine(EndFight(_combatant2, _combatantHealth2));
            yield break;
        }

        StartCoroutine(EndFight(null));
    }

    // Berechnen, ob Angriff trifft und mit welcher Stärke
    private int PerformAttack(Combatant attacker, Combatant defender)
    {
        _accuracy = attacker.RollAccuracy();
        print($"{attacker.Name} Accuracy: {_accuracy}");

        _evasion = defender.RollEvasion();
        print($"{defender.Name} Evasion: {_evasion}");

        if (_evasion >= _accuracy)
        {
            return 0;
        }

        return attacker.RollDamge();
    }

    private void CalculateDamage(int damage, Combatant defender, int defenderHealth)
    {
        if (defender.ArmorStrength <= 0)
        {
            _finalDamage = _rawDamage;
            return;
        }

        int damageReduction = (damage / defender.ArmorStrength);

        _finalDamage = _rawDamage - damageReduction;
    }

    // Je nach Ausgang Kampf abwickeln
    private IEnumerator EndFight(Combatant winner, int healthPoints = 0)
    {
        if(_combatCoroutine != null)
        {
            StopCoroutine(_combatCoroutine);
            _combatCoroutine = null;
        }

        if(_insultCoroutine != null)
        {
            StopCoroutine(_insultCoroutine);
            _insultCoroutine = null;
        }

        if (winner)
        {
            _currentLine = DialogueUtil.CreateCombatLog(winner, "has", "won the fight!");
            yield return StartCoroutine(HandleTextOutput(_currentLine, true));

            if(winner == PlayerManager.Instance)
            {
                PlayerManager.Instance.HealthPoints = healthPoints;
                
                SceneManager.LoadScene("MapTest");
                yield break;
            }
        }

        if (PlayerManager.Instance.HealthPoints <= 0)
        {
            _currentLine = "You have died. Your quest has ended.";
            yield return StartCoroutine(HandleTextOutput(_currentLine, true));

            SceneManager.LoadScene("StartMenu");
            yield break;
        }
        else
        {
            _textBox.enabled = false;
            _selectionMenuCanvas.enabled = true;
            OnFightFinished?.Invoke();
        }

    }

    private IEnumerator TryRetreat()
    {
        _textBox.enabled = true;
        _playerRoll = PlayerManager.Instance.Initiative - DiceUtil.D6();
        print($"Player Initiative: {_playerRoll}");
        print($"Enemy Initiative: {_enemy.Initiative}");

        if (_playerRoll >= _enemy.Initiative)
        {
            _currentLine = "You manage to escape!";

            yield return StartCoroutine(HandleTextOutput(_currentLine, true));

            StopAllCoroutines();
            SceneManager.LoadScene("MapTest");
            yield break;
        }
        else
        {
            _currentLine = $"The {_enemy.Name} is too fast for you to escape!";
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            if (_combatCoroutine == null)
            {
                _combatCoroutine = StartCoroutine(CombatCoroutine(isDisadvantage: true));
            }
        }
    }

    public void HandleSelectedItem(int index, bool isFirstLayer)
    {
        if(isFirstLayer)
        {
            _selectionMenuCanvas.enabled = false; 
            // 0 = Insult, 1 = Fight, 2 = Retreat
            switch (index)
            {
                case 0:
                    StartCoroutine(DisplayInsultOptions());
                    break;

                case 1:
                    if (_combatCoroutine == null)
                    {
                        _combatCoroutine = StartCoroutine(CombatCoroutine(isDisadvantage: false));
                    }
                    break;

                case 2:
                    StartCoroutine(TryRetreat());
                    break;
            }
        }
        else
        {
            // 0 = Option 1, 1 = Option 2, 2 = Return
            switch (index)
            {
                case 0:
                    _insultCoroutine = StartCoroutine(InsultCoroutine(0));
                    break;

                case 1:
                    _insultCoroutine = StartCoroutine(InsultCoroutine(1));
                    break;

                case 2:
                    _insultCoroutine = StartCoroutine(InsultCoroutine(2));
                    break;
            }
        }
    }         

    // Umfasst mehrere Methoden der Dalogue.Util-Klasse, händelt z.B. auch das Beenden eines Abschnitts wenn isLastLine == true
    private IEnumerator HandleTextOutput(string line, bool isLastLine)
    {
        _textCoroutine = StartCoroutine(DialogueUtil.DisplayTextOverTime(line, _textBox, _promptSkip, _promptContinue));

        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitUntil.html
        yield return new WaitUntil(() => _currentLine == _textBox.text);

        if (isLastLine)
        {
            yield return new WaitForSeconds(GameConfig.TimeBeforeLevelLoad);
            yield break;
        }

        // Einen Frame warten, damit Input nicht beide GetKeyDown-Events triggert
        yield return null;

        yield return StartCoroutine(DialogueUtil.WaitForContinue(_promptContinue));
    }

    // Zeigt visuelles Feedback bei den Health- und Ego-Balken an, zuerst wird ein Balken auf den Zielwert gesetzt und der andere gelerpt
    private IEnumerator UpdateUIStats(Combatant combatant, int damage, int currentHealth, bool isHealthDamage, int currentEgo = 0)
    {
        float hitValue = 0;
        Slider slider = null;
        TextMeshProUGUI text = null;
        
        if (isHealthDamage)
        {
            slider = combatant.Name == PlayerManager.Instance.Name ? _playerHealthBarBelow : _enemyHealthBarBelow;
            hitValue = (float)damage / (float)combatant.HealthPoints;
            text = combatant.Name == PlayerManager.Instance.Name ? _playerHealth : _enemyHealth;

            if(currentHealth <= 0)
            {
                currentHealth = 0;
            }

            text.text = $"{currentHealth}/{combatant.HealthPoints}";
        }
        else
        {
            slider = _enemyEgoBarBelow;
            hitValue = (float)damage / (float)combatant.EgoPoints;
            text = combatant.Name == PlayerManager.Instance.Name ? _playerEgo : _enemyEgo;

            if (currentEgo <= 0)
            {
                currentEgo = 0;
            }

            text.text = $"{currentEgo}/{combatant.EgoPoints}";
        }

        float currentValue = slider.value;       
        float nextValue = currentValue - hitValue;
        float lerpValue = 0;


        // Weiße Healthbar setzen
        var childSlider = UnityUtil.GetFirstComponentInChildren<Slider>(slider.gameObject);
        childSlider.GetComponent<Slider>().value = nextValue;

        yield return new WaitForSeconds(GameConfig.TimeBeforeHealthbarUpdate);

        while (lerpValue <= 1 && lerpValue >= 0)
        {
            lerpValue += GameConfig.BarsLerpSpeed * Time.deltaTime;
            slider.value = Mathf.Lerp(currentValue, nextValue, lerpValue / hitValue);
            yield return null;
        }

        slider.value = nextValue;
    }
}
