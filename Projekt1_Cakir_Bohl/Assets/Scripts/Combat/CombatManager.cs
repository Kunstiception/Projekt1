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
    [SerializeField] private GameObject[] _enemies;
    [SerializeField] private TextMeshProUGUI _textBox;
    [SerializeField] private TextMeshProUGUI _promptSkip;
    [SerializeField] private TextMeshProUGUI _promptContinue;
    [SerializeField] private TextMeshProUGUI _playerUIHealth;
    [SerializeField] private TextMeshProUGUI _playerUIEgo;
    [SerializeField] private TextMeshProUGUI _enemyUIHealth;
    [SerializeField] private TextMeshProUGUI _enemyUIEgo;
    [SerializeField] private Slider _enemyHealthBarBelow;
    [SerializeField] private Slider _playerHealthBarBelow;
    [SerializeField] private Slider _enemyEgoBarBelow;
    [SerializeField] private Slider _playerEgoBarBelow;
    [SerializeField] private Canvas _selectionMenuCanvas;
    [SerializeField] private Canvas _persuasionMenuCanvas;

    private int _playerRoll;
    private int _enemyRoll;
    private int _combatantHealth1;
    private int _combatantHealth2;
    private int _playerHealth;
    private int _enemyHealth;
    private int _attackerHealth;
    private int _defenderHealth;
    private int _defenderEgoPoints;
    private int _rawDamage;
    private int _finalDamage;
    private int _accuracy;
    private int _evasion;
    private int _currentInsultIndex;
    private int _combatant1EgoPoints;
    private int _combatant2EgoPoints;
    private bool _hasFightStarted;
    private bool _isFighting;
    private bool _hasDisadvantage;
    private bool _isFirstCombatant;
    private string _currentLine;
    private string _egoHitLine;
    private string _egoResistLine;
    private Combatant _enemy;
    private Combatant _attackingCombatant;
    private Combatant _defendingCombatant;
    private Combatant _combatant1;
    private Combatant _combatant2;
    private Coroutine _turnCoroutine;
    private Coroutine _textCoroutine;
    private Coroutine _insultTurn;
    private Dictionary<string, int> _enemyInsultsAndValues = new Dictionary<string, int>();
    private Dictionary<string, int> _enemyCurrentInsultsAndValues = new Dictionary<string, int>();
    private Dictionary<string, int> _playerInsultsAndValues = new Dictionary<string, int>();

    public static event Action OnFightFinished;

    IEnumerator Start()
    {
        _hasFightStarted = false;
        _isFighting = true;
        _textBox.enabled = true;
        _promptSkip.enabled = true;
        _promptContinue.enabled = false;
        ToggleCanvas(_selectionMenuCanvas, false);
        ToggleCanvas(_persuasionMenuCanvas, false);
        _hasDisadvantage = PlayerManager.Instance.HasDisadvantage;
        PlayerManager.Instance.HasDisadvantage = false;

        var randomIndex = UnityEngine.Random.Range(0, _enemies.Length);
        _enemy = _enemies[randomIndex].GetComponent<Combatant>();
        Instantiate(_enemies[randomIndex]);

        // Alle Insult Lines und Values des jeweiligen Gegners holen
        for (int i = 0; i < _enemy.InsultLines.Insults.Length; i++)
        {
            _enemyInsultsAndValues.Add(_enemy.InsultLines.Insults[i], _enemy.InsultLines.Values[i]);
        }

        for(int i = 0; i < PlayerManager.Instance.InsultLines.Insults.Length; i++)
        {
            _playerInsultsAndValues.Add(PlayerManager.Instance.InsultLines.Insults[i], PlayerManager.Instance.InsultLines.Values[i]);
        }

        yield return StartCoroutine(RollInitiative(_hasDisadvantage));
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

    // Initiative würfeln, Reihenfolge festlegen
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
            
        }

        SetInitialStats();

        _currentLine = $"You encounter a {_enemy.Name}!";
        yield return StartCoroutine(HandleTextOutput(_currentLine, false));

        _currentLine = DialogueUtil.CreateCombatLog(_combatant1, "goes", "first!");
        if(_combatant1 == PlayerManager.Instance)
        {
            yield return StartCoroutine(HandleTextOutput(_currentLine, true));
        }
        else
        {
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }

        _isFirstCombatant = true;

        StartCoroutine(CombatCoroutine());
    }

    // Anfangswerte zuweisen
    private void SetInitialStats()
    {
        _combatantHealth1 = _combatant1.HealthPoints;
        _combatantHealth2 = _combatant2.HealthPoints;
        _combatant1EgoPoints = _combatant1.EgoPoints;
        _combatant2EgoPoints = _combatant2.EgoPoints;
        _playerUIHealth.text = $"{PlayerManager.Instance.HealthPoints}/{GameConfig.PlayerStartingHealth}";
        _playerUIEgo.text = $"{PlayerManager.Instance.EgoPoints}/{GameConfig.PlayerStartingEgo}";
        _enemyUIHealth.text = $"{_enemy.HealthPoints}/{_enemy.HealthPoints}";
        _enemyUIEgo.text = $"{_enemy.EgoPoints}/{_enemy.EgoPoints}";

        // Weiße Healthbar setzen
        _playerHealthBarBelow.value = (float)PlayerManager.Instance.HealthPoints / (float)GameConfig.PlayerStartingHealth;
        var childSlider = UnityUtil.GetFirstComponentInChildren<Slider>(_playerHealthBarBelow.gameObject);
        childSlider.GetComponent<Slider>().value = (float)PlayerManager.Instance.HealthPoints / (float)GameConfig.PlayerStartingHealth;

        _playerEgoBarBelow.value = (float)PlayerManager.Instance.EgoPoints / (float)GameConfig.PlayerStartingEgo;
        childSlider = UnityUtil.GetFirstComponentInChildren<Slider>(_playerEgoBarBelow.gameObject);
        childSlider.GetComponent<Slider>().value = (float)PlayerManager.Instance.EgoPoints / (float)GameConfig.PlayerStartingEgo;
    }

    private IEnumerator CombatCoroutine()
    {
        while (true)
        {
            yield return StartCoroutine(CreateTurn());

            // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitUntil.html
            yield return new WaitUntil(() => _isFighting == false);
        }
    }

    // Legt Angreifer und Vertreidiger fest
    // Gibt falls Player an der Reihe die Auswahlmöglichkeit, wenn Enemy wird Aktion zufälig ausgewürfelt
    private IEnumerator CreateTurn()
    {
        ToggleCanvas(_selectionMenuCanvas, false);

        if (_isFirstCombatant)
        {
            _attackingCombatant = _combatant1;
            _defendingCombatant = _combatant2;
            _attackerHealth = _combatantHealth1;
            _defenderHealth = _combatantHealth2;
            _defenderEgoPoints = _combatant2EgoPoints;
            _isFirstCombatant = false;
        }
        else
        {
            _attackingCombatant = _combatant2;
            _defendingCombatant = _combatant1;
            _attackerHealth = _combatantHealth2;
            _defenderHealth = _combatantHealth1;
            _defenderEgoPoints = _combatant1EgoPoints;
            _isFirstCombatant = true;
        }

        if (_attackingCombatant == PlayerManager.Instance)
        {
            if (_hasFightStarted)
            {               
                _currentLine = "Your turn!";
                yield return StartCoroutine(HandleTextOutput(_currentLine, true));
            }

            _textBox.enabled = false;
            ToggleCanvas(_selectionMenuCanvas, true);       
            _isFighting = true;
            _hasFightStarted = true;

            yield break;
        }
        else
        {
            if (_hasFightStarted)
            {
                _currentLine = $"{_enemy.Name}'s turn!";
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));
            }
        }

        var randomIndex = 0;

        // wenn keine Insults mehr übrig, bleibt nur Angriff
        if (_playerInsultsAndValues.Count < 1)
        {
            randomIndex = UnityEngine.Random.Range(1, 2);
        }
        else
        {
            randomIndex = UnityEngine.Random.Range(0, 2);
        }

        switch (randomIndex)
        {
            case 0:
                _turnCoroutine = StartCoroutine(InsultTurn(attacker: _enemy, defender: PlayerManager.Instance, _defenderEgoPoints));
                break;

            case 1:
                 _turnCoroutine = StartCoroutine(CombatTurn(attacker: _enemy, defender: PlayerManager.Instance, defenderHealth: _defenderHealth, isDisadvantage: false));
                break;
        }

        _hasFightStarted = true;
        _isFighting = true;
    }

    private IEnumerator DisplayInsultOptions()
    {
        ToggleCanvas(_selectionMenuCanvas, false);
        
        // Wenn keine 2 Optionen mehr gegeben werden können: Ende
        if (_enemyInsultsAndValues.Count < 2)
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

        ToggleCanvas(_persuasionMenuCanvas, true);

        TextMeshProUGUI[] options = _persuasionMenuCanvas.GetComponentsInChildren<TextMeshProUGUI>();

        // Zwei zufällige Optionen und Werte zuweisen, danach jeweils aus dem Original-Dictionary entfernen, damit nicht zweimal dieselbe Option angezeigt wird
        for (int i = 0; i < options.Length - 1; i++)
        {
            int index = UnityEngine.Random.Range(0, _enemyInsultsAndValues.Count - 1);

            options[i].text = _enemyInsultsAndValues.ElementAt(index).Key;
            _enemyCurrentInsultsAndValues.Add(options[i].text, _enemyInsultsAndValues.ElementAt(index).Value);
            _enemyInsultsAndValues.Remove(options[i].text);
        }
    }

    // Eine Runde Insult-Kampf, variiert je nachdem ob Player oder Enemy Angreifer ist
    private IEnumerator InsultTurn(Combatant attacker, Combatant defender, int defenderEgoPoints, int optionIndex = 0)
    {
        string line = "";
        int value = 0;

        _textBox.enabled = true;

        if(attacker == PlayerManager.Instance)
        {        
            // Je nach ausgewählter Option Line und Value zuweisen sowie mögliche Antworten bereits laden, nicht gewählte Optionen wieder ins Original-Dictionary einfügen
            switch(optionIndex)
            {
                case 0:
                    line = $"{PlayerManager.Instance.Name}: '{_enemyCurrentInsultsAndValues.ElementAt(0).Key}'";
                    value = _enemyCurrentInsultsAndValues.ElementAt(0).Value;
                    _egoHitLine = _enemy.InsultLines.AnswersWhenHit[Array.IndexOf(_enemy.InsultLines.Insults, _enemyCurrentInsultsAndValues.ElementAt(0).Key)];
                    _egoResistLine = _enemy.InsultLines.AnswersWhenResisted[Array.IndexOf(_enemy.InsultLines.Insults, _enemyCurrentInsultsAndValues.ElementAt(0).Key)];
                    _enemyInsultsAndValues.Add( _enemyCurrentInsultsAndValues.ElementAt(1).Key, _enemyCurrentInsultsAndValues.ElementAt(1).Value);
                    break;

                case 1:
                    line = $"{PlayerManager.Instance.Name}: '{_enemyCurrentInsultsAndValues.ElementAt(1).Key}'";
                    value = _enemyCurrentInsultsAndValues.ElementAt(1).Value;
                    _egoHitLine = _enemy.InsultLines.AnswersWhenHit[Array.IndexOf(_enemy.InsultLines.Insults, _enemyCurrentInsultsAndValues.ElementAt(1).Key)];
                    _egoResistLine = _enemy.InsultLines.AnswersWhenResisted[Array.IndexOf(_enemy.InsultLines.Insults, _enemyCurrentInsultsAndValues.ElementAt(1).Key)];
                    _enemyInsultsAndValues.Add(_enemyCurrentInsultsAndValues.ElementAt(0).Key, _enemyCurrentInsultsAndValues.ElementAt(0).Value);
                    break;

                case 2:
                    _textBox.enabled = false;
                    _persuasionMenuCanvas.enabled = false;
                    break;

                default:
                    Debug.LogError("Insult index not set correctly!");
                    break;
            }
        }
        else
        {
            ToggleCanvas(_selectionMenuCanvas, false);

            var random = UnityEngine.Random.Range(0, _playerInsultsAndValues.Count - 1);         
            line = $"{_enemy.Name}: '{_playerInsultsAndValues.ElementAt(random).Key}'";
            value = _playerInsultsAndValues.ElementAt(random).Value;
            _egoHitLine = PlayerManager.Instance.InsultLines.AnswersWhenHit[Array.IndexOf(PlayerManager.Instance.InsultLines.Insults, _playerInsultsAndValues.ElementAt(random).Key)];
            _egoResistLine = PlayerManager.Instance.InsultLines.AnswersWhenResisted[Array.IndexOf(PlayerManager.Instance.InsultLines.Insults, _playerInsultsAndValues.ElementAt(random).Key)];
            _playerInsultsAndValues.Remove(_playerInsultsAndValues.ElementAt(random).Key);
        }

        _textBox.enabled = true;

        // Insult-Kampf nur ausführen, wenn eine Option ausgewählt wurde
        if (line.Length > 0)
        {
            ToggleCanvas(_persuasionMenuCanvas, false);

            _currentLine = DialogueUtil.CreateCombatLog(attacker, "attempts", $"to insult {defender.Name}!");
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            var attackRoll = value + DiceUtil.D6();

            _currentLine = line;
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
                
            _finalDamage = attackRoll - defender.InsultResistance;

            if (_finalDamage > 0)
            {
                _currentLine = $"{defender.Name}: '{_egoHitLine}'";
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));

                defenderEgoPoints -= _finalDamage;

                StartCoroutine(UpdateUI(combatant: defender, damage: _finalDamage, isHealthDamage: false, currentEgo: defenderEgoPoints));

                UpdateStats(defender: defender, newValue: defenderEgoPoints, false);

                _currentLine = DialogueUtil.CreateCombatLog(defender, "takes", $"{_finalDamage} ego damage!");
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));
            }
            else
            {
                _currentLine = $"{defender.Name}: '{_egoResistLine}'";
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));

                _currentLine = DialogueUtil.CreateCombatLog(defender, "has", "resisted the insult!");
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));
            }
        }

        if (defenderEgoPoints <= 0)
        {
            _insultTurn = null;
            StartCoroutine(EndFight(attacker));
            yield break;
        }

        //Insult-Kampf abwickeln
        _enemyCurrentInsultsAndValues.Clear();
        _textBox.text = "";
        _textBox.enabled = false;
        _insultTurn = null;

        ToggleCanvas(_selectionMenuCanvas, false);

        StartCoroutine(EndFight(null));
    }

    // Eine Runde Kampf
    private IEnumerator CombatTurn(Combatant attacker, Combatant defender, int defenderHealth, bool isDisadvantage)
    {
        ToggleCanvas(_selectionMenuCanvas, false);

        _textBox.enabled = true;

        if (isDisadvantage)
        {
            _currentLine = DialogueUtil.CreateCombatLog(attacker, "attacks", "you!");
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }
        else
        {
            _currentLine = DialogueUtil.CreateCombatLog(attacker, "prepares", "to strike!");
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }

        _rawDamage = PerformAttack(attacker, defender);
        print($"{attacker.Name} Damage: {_rawDamage}");

        if (_rawDamage <= 0)
        {
            _currentLine = DialogueUtil.CreateCombatLog(defender, "has", "evaded the attack!");
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }

        CalculateDamage(_rawDamage, defender, defenderHealth);

        if (_finalDamage >= defenderHealth)
        {
            defenderHealth = 0;
        }
        else
        {
            defenderHealth -= _finalDamage;
        }

        if (_finalDamage > 0)
        {
            StartCoroutine(UpdateUI(combatant: defender, damage: _finalDamage, currentHealth: defenderHealth, isHealthDamage: true));

            UpdateStats(defender: defender, newValue: defenderHealth, isHealthDamage: true);

            _currentLine = DialogueUtil.CreateCombatLog(defender, "takes", $"{_finalDamage} damage!");
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }

        if (defenderHealth <= 0)
        {
            _turnCoroutine = null;
            StartCoroutine(EndFight(attacker));
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

    // Schaden berechnen, falls vorhanden Rüstung beachten
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
    private IEnumerator EndFight(Combatant winner, bool isRetreat = false)
    {
        if(_turnCoroutine != null)
        {
            StopCoroutine(_turnCoroutine);
            _turnCoroutine = null;
        }

        if (winner)
        {
            if (!isRetreat)
            {
                _currentLine = DialogueUtil.CreateCombatLog(winner, "has", "won the fight!");
                yield return StartCoroutine(HandleTextOutput(_currentLine, true));
            }

            if(winner == PlayerManager.Instance)
            {
                if(_combatant1 == PlayerManager.Instance)
                {
                    PlayerManager.Instance.HealthPoints = _combatantHealth1;
                    PlayerManager.Instance.EgoPoints = _combatant1EgoPoints;
                }
                else
                {
                    PlayerManager.Instance.HealthPoints = _combatantHealth2;
                    PlayerManager.Instance.EgoPoints = _combatant2EgoPoints;
                }

                SceneManager.LoadScene("MapTest");
                yield break;
            }
            else
            {
                _currentLine = "You have died. Your quest has ended.";
                yield return StartCoroutine(HandleTextOutput(_currentLine, true));

                SceneManager.LoadScene("StartMenu");
                yield break;
            }
        }
        else
        {
            _textBox.enabled = false;
            _isFighting = false;
            OnFightFinished?.Invoke();
        }
    }

    // Wenn erfolgreich, dann aus Kampf geflohen, wenn nicht, Gegner greift einmal an
    private IEnumerator TryRetreat()
    {
        _textBox.enabled = true;

        ToggleCanvas(_selectionMenuCanvas, false);

        _playerRoll = PlayerManager.Instance.Initiative - DiceUtil.D6();
        print($"Player Initiative: {_playerRoll}");
        print($"Enemy Initiative: {_enemy.Initiative}");

        if (_playerRoll >= _enemy.Initiative)
        {        
            _currentLine = "You manage to escape!";
            yield return StartCoroutine(HandleTextOutput(_currentLine, true));

            yield return StartCoroutine(EndFight(PlayerManager.Instance, isRetreat: true));

            StopAllCoroutines();
            SceneManager.LoadScene("MapTest");

            yield break;
        }
        else
        {
            _currentLine = $"The {_enemy.Name} is too fast for you to escape!";
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            if (_turnCoroutine == null)
            {
                if(_combatant1 == PlayerManager.Instance)
                {
                    _turnCoroutine = StartCoroutine(CombatTurn(attacker: _enemy, defender: PlayerManager.Instance, 
                        defenderHealth: _combatantHealth2, isDisadvantage: true));

                    yield break;
                }

                _turnCoroutine = StartCoroutine(CombatTurn(attacker: _enemy, defender: PlayerManager.Instance,
                        defenderHealth: _combatantHealth1, isDisadvantage: true));
            }
        }
    }

    // Bestimmt, was die Auswahl im Menü auslöst, zwei Menü-Ebenen möglich
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
                    if (_turnCoroutine == null)
                    {
                        var enemyHealth = _combatant2 == _enemy ? _combatantHealth2 : _combatantHealth1;
                        
                        _turnCoroutine = StartCoroutine(CombatTurn(attacker: PlayerManager.Instance, defender: _enemy, defenderHealth: enemyHealth, isDisadvantage: false));
                    }
                    break;

                case 2:
                    StartCoroutine(TryRetreat());
                    break;
            }
        }
        else
        {      
            var enemyEgo = _combatant2 == _enemy ? _combatant2EgoPoints : _combatant1EgoPoints;

            // 0 = Option 1, 1 = Option 2, 2 = Return
            switch (index)
            {
                case 0:

                    _turnCoroutine = StartCoroutine(InsultTurn(PlayerManager.Instance, _enemy, enemyEgo, 0));
                    break;

                case 1:
                    _turnCoroutine = StartCoroutine(InsultTurn(PlayerManager.Instance, _enemy, enemyEgo, 1));
                    break;

                case 2:
                    ToggleCanvas(_persuasionMenuCanvas, false);

                    ToggleCanvas(_selectionMenuCanvas, true);
                    break;
            }
        }
    }         

    // Umfasst mehrere Methoden der Dalogue.Util-Klasse, händelt z.B. auch das Beenden eines Abschnitts wenn isLastLine == true
    private IEnumerator HandleTextOutput(string line, bool isLastLine)
    {
        _textBox.enabled = true;
        _textCoroutine = StartCoroutine(DialogueUtil.DisplayTextOverTime(line, _textBox, _promptSkip, _promptContinue));

        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitUntil.html
        yield return new WaitUntil(() => _currentLine == _textBox.text);

        if (isLastLine)
        {          
            yield return new WaitForSeconds(GameConfig.TimeBeforeLevelLoad);
            _textBox.enabled = false;
            yield break;
        }

        // Einen Frame warten, damit Input nicht beide GetKeyDown-Events triggert
        yield return null;

        yield return StartCoroutine(DialogueUtil.WaitForContinue(_promptContinue));

        _textBox.enabled = false;
    }

    // Zeigt visuelles Feedback bei den Health- und Ego-Balken an, zuerst wird ein Balken auf den Zielwert gesetzt und der andere gelerpt
    private IEnumerator UpdateUI(Combatant combatant, int damage, bool isHealthDamage, int currentHealth = 0, int currentEgo = 0)
    {
        float hitValue = 0;
        Slider slider = null;
        TextMeshProUGUI text = null;
        
        if (isHealthDamage)
        {
            slider = combatant.Name == PlayerManager.Instance.Name ? _playerHealthBarBelow : _enemyHealthBarBelow;
            hitValue = combatant.Name == PlayerManager.Instance.Name ? (float)damage / (float)GameConfig.PlayerStartingHealth : (float)damage / (float)_enemy.HealthPoints;
            text = combatant.Name == PlayerManager.Instance.Name ? _playerUIHealth : _enemyUIHealth;

            if(currentHealth <= 0)
            {
                currentHealth = 0;
            }

            text.text = combatant.Name == PlayerManager.Instance.Name ? $"{currentHealth}/{GameConfig.PlayerStartingHealth}" : $"{currentHealth}/{combatant.HealthPoints}";
        }
        else
        {
            slider = combatant.Name == PlayerManager.Instance.Name ? _playerEgoBarBelow : _enemyEgoBarBelow;
            hitValue = combatant.Name == PlayerManager.Instance.Name ? (float)damage / (float)GameConfig.PlayerStartingEgo : (float)damage / (float)_enemy.EgoPoints;
            text = combatant.Name == PlayerManager.Instance.Name ? _playerUIEgo : _enemyUIEgo;

            if (currentEgo <= 0)
            {
                currentEgo = 0;
            }

            text.text = combatant.Name == PlayerManager.Instance.Name ? $"{currentEgo}/{GameConfig.PlayerStartingEgo}" : $"{currentEgo}/{combatant.EgoPoints}";
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

    // Stats im Hintergrund wieder korrekt setzen
    private void UpdateStats(Combatant defender, int newValue, bool isHealthDamage)
    {      
        if(defender == _combatant1)
        {
            if (isHealthDamage)
            {
                _combatantHealth1 = newValue;
            }
            else
            {
                _combatant1EgoPoints = newValue;
            }

            return;
        }

        if(isHealthDamage)
        {
            _combatantHealth2 = newValue;
        }
        else
        {
            _combatant2EgoPoints = newValue;
        }       
    }

    // Übergebenen Canvas und Skript an- oder ausschalten
    public void ToggleCanvas(Canvas canvas, bool isActive)
    {
        canvas.enabled = isActive;
        canvas.GetComponent<SelectionMenu>().enabled = isActive;
    }
}
