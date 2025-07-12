using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CombatManager : Manager, ISelectable
{
    [SerializeField] protected AudioSource _audioSource;
    [SerializeField] protected GameObject _hitParticlesInsult;
    [SerializeField] protected GameObject _hitParticlesStrike;
    [SerializeField] protected GameObject[] _exclamations;
    [SerializeField] protected AudioClip _insultHit;
    [SerializeField] protected AudioClip _strikeHit;
    [SerializeField] protected TextMeshProUGUI _enemyUIHealth;
    [SerializeField] protected TextMeshProUGUI _enemyUIEgo;
    [SerializeField] protected Slider _enemyHealthBarBelow;
    [SerializeField] protected Slider _enemyEgoBarBelow;
    [SerializeField] protected Canvas _initialSelectionMenuCanvas;
    [SerializeField] protected Canvas _insultMenuCanvas;
    [SerializeField] protected GameObject _endBoss;
    [SerializeField] private GameObject[] _enemiesDay;
    [SerializeField] private GameObject[] _enemiesNight;
    [SerializeField] private GameObject _guard;
    [SerializeField] private GameObject[] _itemOptions;
    [SerializeField] private Item _coin;
    [SerializeField] private Canvas _itemUseCanvas;

    protected int _playerRoll;
    protected int _enemyRoll;
    protected int _combatant1Health;
    protected int _combatant2Health;
    protected int _attackerHealth;
    protected int _defenderHealth;
    protected int _defenderEgoPoints;
    protected int _rawDamage;
    protected int _finalDamage;
    protected int _accuracy;
    protected int _evasion;
    protected int _combatant1EgoPoints;
    protected int _combatant2EgoPoints;
    protected bool _hasFightStarted;
    protected bool _isFighting;
    protected bool _isFirstCombatant;
    protected string _egoHitLine;
    protected string _egoResistLine;
    protected Combatant _enemy;
    protected Combatant _attackingCombatant;
    protected Combatant _defendingCombatant;
    protected Combatant _combatant1;
    protected Combatant _combatant2;
    protected Coroutine _turnCoroutine;
    protected Coroutine _insultTurn;
    protected Dictionary<string, int> _playerInsultsAndValues = new Dictionary<string, int>();
    private Dictionary<string, int> _enemyInsultsAndValues = new Dictionary<string, int>();
    private Dictionary<string, int> _enemyCurrentInsultsAndValues = new Dictionary<string, int>();
    private bool _hasEnemyFled;
    private int _intitialPlayerHealth;
    private int _bossCounter;
    private bool _hasDisadvantage;
    private List<Item> _healingItems = new List<Item>();
    public static event Action OnFightFinished;

    IEnumerator Start()
    {
        ToggleCursorState(true);

        _hasFightStarted = false;
        _hasEnemyFled = false;
        _isFighting = true;
        _textBox.enabled = true;
        _promptSkip.enabled = true;
        _promptContinue.enabled = false;
        _hitParticlesInsult.SetActive(false);
        _hitParticlesStrike.SetActive(false);

        foreach (GameObject exclamation in _exclamations)
        {
            exclamation.SetActive(false);
        }

        ToggleCanvas(_initialSelectionMenuCanvas, false);
        ToggleCanvas(_insultMenuCanvas, false);
        ToggleCanvas(_itemUseCanvas, false);

        foreach (GameObject option in _itemOptions)
        {
            option.SetActive(false);
        }

        _hasDisadvantage = PlayerManager.Instance.HasDisadvantage;

        if (!PlayerManager.Instance.GotCaught)
        {
            if (!PlayerManager.Instance.HasReachedBoss)
            {
                if (MainManager.Instance.IsDay)
                {
                    var randomIndex = UnityEngine.Random.Range(0, _enemiesDay.Length);
                    Instantiate(_enemiesDay[randomIndex]);
                    _enemy = _enemiesDay[randomIndex].GetComponent<Combatant>();
                }
                else
                {
                    var randomIndex = UnityEngine.Random.Range(0, _enemiesNight.Length);
                    Instantiate(_enemiesNight[randomIndex]);
                    _enemy = _enemiesNight[randomIndex].GetComponent<Combatant>();
                }
            }
            else
            {
                Instantiate(_endBoss);
                _enemy = _endBoss.GetComponent<Combatant>();
            }
        }
        else
        {
            Instantiate(_guard);
            _enemy = _guard.GetComponent<Combatant>();
        }

        // Alle Insult Lines und Values des jeweiligen Gegners holen
        for (int i = 0; i < _enemy.InsultLines.Insults.Length; i++)
        {
            _enemyInsultsAndValues.Add(_enemy.InsultLines.Insults[i], _enemy.InsultLines.Values[i]);
        }

        for (int i = 0; i < PlayerManager.Instance.InsultLines.Insults.Length; i++)
        {
            _playerInsultsAndValues.Add(PlayerManager.Instance.InsultLines.Insults[i], PlayerManager.Instance.InsultLines.Values[i]);
        }

        _intitialPlayerHealth = PlayerManager.Instance.HealthPoints;

        yield return StartCoroutine(RollInitiative(_hasDisadvantage));
    }

    void OnEnable()
    {
        HealingItem.onHeal += SetUIUpdate;
    }

    void OnDisable()
    {
        StopAllCoroutines();

        HealingItem.onHeal -= SetUIUpdate;
    }

    void Update()
    {
        ListenForSkip();
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

        if (_enemy.EntrySound != null)
        {
            _audioSource.PlayOneShot(_enemy.EntrySound, 1.5f);           
        }

        _currentLine = $"You encounter a {_enemy.Name}!";
        yield return StartCoroutine(HandleTextOutput(_currentLine, false));

        if (PlayerManager.Instance.GotCaught)
        {
            _currentLine = UIDialogueStorage.GuardAppearsLines[UnityEngine.Random.Range(0, UIDialogueStorage.GuardAppearsLines.Length)];
            yield return HandleTextOutput(_currentLine, false);
        }

        yield return StartCoroutine(EvaluateVampire());

        _currentLine = DialogueUtil.CreateCombatLog(_combatant1, "goes", "first!");
        if (_combatant1 == PlayerManager.Instance)
        {
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            if (MainManager.Instance.IsDay && ConditionManager.Instance.IsVampire)
            {
                _combatant1Health -= GameConfig.VampireSunDamage;          
            }
        }
        else
        {
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            if (MainManager.Instance.IsDay && ConditionManager.Instance.IsVampire)
            {
                _combatant2Health -= GameConfig.VampireSunDamage;          
            }
        }

        _isFirstCombatant = true;

        StartCoroutine(CombatCoroutine());
    }

    // Anfangswerte zuweisen
    protected void SetInitialStats()
    {
        _combatant1Health = _combatant1.HealthPoints;
        _combatant2Health = _combatant2.HealthPoints;

        _combatant1EgoPoints = _combatant1.EgoPoints;
        _combatant2EgoPoints = _combatant2.EgoPoints;

        _enemyUIHealth.text = $"{_enemy.HealthPoints}/{_enemy.HealthPoints}";
        _enemyUIEgo.text = $"{_enemy.EgoPoints}/{_enemy.EgoPoints}";

        InitializePlayerStats();
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
        ToggleCanvas(_initialSelectionMenuCanvas, false);

        if (_isFirstCombatant)
        {
            _attackingCombatant = _combatant1;
            _defendingCombatant = _combatant2;

            _attackerHealth = _combatant1Health;
            _defenderHealth = _combatant2Health;

            _defenderEgoPoints = _combatant2EgoPoints;

            _isFirstCombatant = false;
        }
        else
        {
            _attackingCombatant = _combatant2;
            _defendingCombatant = _combatant1;

            _attackerHealth = _combatant2Health;
            _defenderHealth = _combatant1Health;

            _defenderEgoPoints = _combatant1EgoPoints;

            _isFirstCombatant = true;
        }

        _textBox.enabled = true;

        if (_attackingCombatant == PlayerManager.Instance)
        {
            if (_hasFightStarted)
            {
                _currentLine = "Your turn!";
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));
            }

            _textBox.enabled = false;

            ToggleCanvas(_initialSelectionMenuCanvas, true);

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

        if (ConditionManager.Instance.IsZombie)
        {
            if (DiceUtil.D10() < GameConfig.EnemyFleeChance)
            {
                _currentLine = $"{_enemy.Name} seems scared.";
                yield return HandleTextOutput(_currentLine, false);

                foreach (string line in UIDialogueStorage.EnemyFleeLines)
                {
                    _currentLine = $"{_enemy.Name}: " + "'" + line + "'";
                    yield return HandleTextOutput(_currentLine, false);
                }

                _hasEnemyFled = true;

                StopAllCoroutines();

                StartCoroutine(EndFight(PlayerManager.Instance));

                ToggleCanvas(_initialSelectionMenuCanvas, false);

                yield break;
            }
        }

        var randomIndex = 0;

        // wenn keine Insults mehr übrig, bleibt nur Angriff
        // der Endboss greift nur die Lebenspunkte an
        if (_playerInsultsAndValues.Count < 1 || _enemy.Name == "Voice")
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
                _turnCoroutine = StartCoroutine(CombatTurn(attacker: _enemy, defender: PlayerManager.Instance,
                       defenderHealth: _defenderHealth, defenderEgoPoints: _defenderEgoPoints, isDisadvantage: false));
                break;
        }

        _hasFightStarted = true;
        _isFighting = true;
    }

    private IEnumerator DisplayInsultOptions()
    {
        ToggleCanvas(_initialSelectionMenuCanvas, false);

        // Wenn keine 2 Optionen mehr gegeben werden können: Ende
        if (_enemyInsultsAndValues.Count < 2)
        {
            _textBox.enabled = true;

            _currentLine = "You can't think of anything else to say!";
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            _textBox.enabled = false;

            ToggleCanvas(_insultMenuCanvas, false);
            ToggleCanvas(_initialSelectionMenuCanvas, true);
            yield break;
        }

        ToggleCanvas(_insultMenuCanvas, true);

        TextMeshProUGUI[] options = _insultMenuCanvas.GetComponentsInChildren<TextMeshProUGUI>();

        // Zwei zufällige Optionen und Werte zuweisen und temporärem Dicitionary hinzufügen (falls nicht schon vorhanden, wenn zuvor return ausgewählt wurde und man wieder zurückkehrt)
        // Option aus Original-Dictionary entfernen
        if (_enemyCurrentInsultsAndValues.Count == 0)
        {
            for (int i = 0; i < options.Length - 1; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, _enemyInsultsAndValues.Count - 1);

                options[i].text = _enemyInsultsAndValues.ElementAt(randomIndex).Key;

                _enemyCurrentInsultsAndValues.Add(options[i].text, _enemyInsultsAndValues.ElementAt(randomIndex).Value);

                _enemyInsultsAndValues.Remove(options[i].text);
            }
        }
    }

    // Eine Runde Insult-Kampf, variiert je nachdem ob Player oder Enemy Angreifer ist
    private IEnumerator InsultTurn(Combatant attacker, Combatant defender, int defenderEgoPoints, int optionIndex = 0)
    {
        string line = "";
        int value = 0;

        _textBox.enabled = true;

        if (defenderEgoPoints <= 0)
        {
            ToggleCanvas(_insultMenuCanvas, false);

            _currentLine = DialogueUtil.CreateCombatLog(defender, "has", $"no ego left to damage!");
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            if (attacker != _enemy)
            {
                _currentLine = "There is no need for cruelty.";
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));
            }

            ResetInsultTurn();

            yield break;
        }

        if (attacker == PlayerManager.Instance)
        {
            if (ConditionManager.Instance.IsZombie)
            {
                ToggleCanvas(_insultMenuCanvas, false);

                yield return StartCoroutine(PrintMultipleLines(UIDialogueStorage.ZombieInsultAttemptLines));

                StartCoroutine(EndFight(null));

                yield break;
            }

            // Je nach ausgewählter Option Line und Value zuweisen sowie mögliche Antworten bereits laden
            // Nicht gewählte Option wieder dem Original-Dictionary hinzufügen
            switch (optionIndex)
            {
                case 0:
                    line = $"{PlayerManager.Instance.Name}: '{_enemyCurrentInsultsAndValues.ElementAt(0).Key}'";
                    value = _enemyCurrentInsultsAndValues.ElementAt(0).Value;

                    _egoHitLine = _enemy.InsultLines.AnswersWhenHit[Array.IndexOf(_enemy.InsultLines.Insults, _enemyCurrentInsultsAndValues.ElementAt(0).Key)];
                    _egoResistLine = _enemy.InsultLines.AnswersWhenResisted[Array.IndexOf(_enemy.InsultLines.Insults, _enemyCurrentInsultsAndValues.ElementAt(0).Key)];

                    _enemyInsultsAndValues.Add(_enemyCurrentInsultsAndValues.ElementAt(1).Key, _enemyCurrentInsultsAndValues.ElementAt(1).Value);

                    break;

                case 1:
                    line = $"{PlayerManager.Instance.Name}: '{_enemyCurrentInsultsAndValues.ElementAt(1).Key}'";
                    value = _enemyCurrentInsultsAndValues.ElementAt(1).Value;

                    _egoHitLine = _enemy.InsultLines.AnswersWhenHit[Array.IndexOf(_enemy.InsultLines.Insults, _enemyCurrentInsultsAndValues.ElementAt(1).Key)];
                    _egoResistLine = _enemy.InsultLines.AnswersWhenResisted[Array.IndexOf(_enemy.InsultLines.Insults, _enemyCurrentInsultsAndValues.ElementAt(1).Key)];

                    _enemyInsultsAndValues.Add(_enemyCurrentInsultsAndValues.ElementAt(0).Key, _enemyCurrentInsultsAndValues.ElementAt(0).Value);

                    break;

                default:
                    Debug.LogError("Insult index not set correctly!");
                    break;
            }
        }
        else
        {
            ToggleCanvas(_initialSelectionMenuCanvas, false);

            if (_enemy.Name != ConditionManager.Instance.ZombieTerm)
            {
                var random = UnityEngine.Random.Range(0, _playerInsultsAndValues.Count - 1);

                line = $"{_enemy.Name}: '{_playerInsultsAndValues.ElementAt(random).Key}'";
                value = _playerInsultsAndValues.ElementAt(random).Value;

                _egoHitLine = PlayerManager.Instance.InsultLines.AnswersWhenHit[Array.IndexOf(PlayerManager.Instance.InsultLines.Insults, _playerInsultsAndValues.ElementAt(random).Key)];
                _egoResistLine = PlayerManager.Instance.InsultLines.AnswersWhenResisted[Array.IndexOf(PlayerManager.Instance.InsultLines.Insults, _playerInsultsAndValues.ElementAt(random).Key)];

                _playerInsultsAndValues.Remove(_playerInsultsAndValues.ElementAt(random).Key);
            }
            else
            {
                _currentLine = $"The zombie can't think of anything to insult you with.";
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));

                _insultTurn = null;
                StartCoroutine(EndFight(null));
                yield break;
            }

        }

        _textBox.enabled = true;

        // Insult-Kampf nur ausführen, wenn eine Option ausgewählt wurde
        if (line.Length > 0)
        {
            ToggleCanvas(_insultMenuCanvas, false);

            _currentLine = DialogueUtil.CreateCombatLog(attacker, "attempts", $"to insult {defender.Name}!");
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            int attackRoll = 0;
            if (attacker is PlayerManager)
            {
                attackRoll = value + DiceUtil.D6() + PlayerManager.Instance.InsultDamageModifier;
            }
            else
            {
                attackRoll = value + DiceUtil.D6();
            }

            _currentLine = line;
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            if (defender is PlayerManager)
            {
                if (ConditionManager.Instance.IsZombie)
                {
                    _finalDamage = 0;
                }
                else
                {
                    _finalDamage = attackRoll - PlayerManager.Instance.GetEgoResistence();
                }
            }
            else
            {
                if (defender.Name != ConditionManager.Instance.ZombieTerm)
                {
                    _finalDamage = attackRoll - defender.InsultResistance;
                }
                else
                {
                    _finalDamage = 0;
                }
            }

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
                if (defender == PlayerManager.Instance && ConditionManager.Instance.IsZombie)
                {
                    yield return PrintMultipleLines(UIDialogueStorage.ZombieInsultResistLines);
                }
                else
                {
                    _currentLine = $"{defender.Name}: '{_egoResistLine}'";
                    yield return StartCoroutine(HandleTextOutput(_currentLine, false));
                }

                _currentLine = DialogueUtil.CreateCombatLog(defender, "has", "resisted the insult!");
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));
            }
        }

        if (defenderEgoPoints <= 0)
        {
            _currentLine = DialogueUtil.CreateCombatLog(defender, "has", "lost all ego!");
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }

        ResetInsultTurn();
    }

    //Insult-Kampf abwickeln
    private void ResetInsultTurn()
    {
        _enemyCurrentInsultsAndValues.Clear();
        _textBox.text = "";
        _textBox.enabled = false;
        _insultTurn = null;

        ToggleCanvas(_initialSelectionMenuCanvas, false);

        StartCoroutine(EndFight(null));
    }

    // Eine Runde Kampf
    private IEnumerator CombatTurn(Combatant attacker, Combatant defender, int defenderHealth, int defenderEgoPoints, bool isDisadvantage)
    {
        ToggleCanvas(_initialSelectionMenuCanvas, false);

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

            StartCoroutine(EndFight(null));

            yield break;
        }

        CalculateDamage(_rawDamage, defenderEgoPoints);

        if (_finalDamage <= 0)
        {
            _currentLine = $"The attack does not pierce throught the ego.";
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }

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

    // Schaden berechnen, falls noch Ego vorhanden
    // Wenn viel Ego vorhanden wird Schaden gemindert, wenn wenig Ego vorhanden wird der Schaden größer
    private void CalculateDamage(int damage, int defenderEgoPoints)
    {
        if (defenderEgoPoints <= 0)
        {
            // https://learn.microsoft.com/en-us/dotnet/api/system.mathf.round?view=net-9.0
            _finalDamage = (int)MathF.Round(damage * GameConfig.MaximumDamageModifier);
            return;
        }

        float floatDamage = (float)damage / ((float)defenderEgoPoints / 3);

        if (floatDamage > (int)MathF.Round(damage * GameConfig.MaximumDamageModifier))
        {
            floatDamage = damage * GameConfig.MaximumDamageModifier;
        }

        _finalDamage = (int)MathF.Round(floatDamage);
    }

    // Je nach Ausgang Kampf abwickeln
    private IEnumerator EndFight(Combatant winner, bool isRetreat = false)
    {
        if (_turnCoroutine != null)
        {
            StopCoroutine(_turnCoroutine);
            _turnCoroutine = null;
        }

        if (_enemy.Name == "Voice" && _attackingCombatant == PlayerManager.Instance)
        {
            if (_bossCounter >= GameConfig.TurnsBeforeSecondStage)
            {
                _textBox.enabled = true;

                _currentLine = $"You realize you cannot win this way.";
                yield return HandleTextOutput(_currentLine, false);

                SceneManager.LoadScene(13);
            }

            _bossCounter++;
        }

        if (winner)
            {
                if (isRetreat)
                {
                    if (PlayerManager.Instance.GotCaught)
                    {
                        yield return PrintMultipleLines(UIDialogueStorage.EscapedGuardLines);

                        _textBox.text = "";

                        PlayerManager.Instance.GotCaught = false;

                        SceneManager.LoadScene(7);

                        yield break;
                    }
                    _currentLine = "You manage to escape!";
                    yield return StartCoroutine(HandleTextOutput(_currentLine, false));
                }
                else
                {
                    if (!_hasEnemyFled)
                    {
                        _currentLine = DialogueUtil.CreateCombatLog(winner, "has", "won the fight!");
                        yield return StartCoroutine(HandleTextOutput(_currentLine, false));
                    }
                    else
                    {
                        _currentLine = DialogueUtil.CreateCombatLog(winner, "has", $"won by scaring off {_enemy.Name} with your despicable existence!");
                        yield return StartCoroutine(HandleTextOutput(_currentLine, false));
                    }
                }

                if (winner == PlayerManager.Instance)
                {
                    if (!isRetreat)
                    {
                        MainManager.Instance.NumberOfDefeatedEnemies++;

                        yield return StartCoroutine(ManageLoot());
                    }

                    if (PlayerManager.Instance.GotCaught)
                    {
                        yield return PrintMultipleLines(UIDialogueStorage.SlayedGuardLines);

                        _textBox.text = "";

                        PlayerManager.Instance.GotCaught = false;

                        SceneManager.LoadScene(7);

                        yield break;
                    }

                    StartCoroutine(CheckForSleepDeprived());

                    yield break;
                }
                else
                {
                    _currentLine = "You have died. Your quest has ended.";
                    yield return StartCoroutine(HandleTextOutput(_currentLine, false));

                    SceneManager.LoadScene("StartMenu");
                    yield break;
                }
            }
            else
            {
                if (_combatant1 == PlayerManager.Instance)
                {
                    PlayerManager.Instance.HealthPoints = _combatant1Health;
                    PlayerManager.Instance.EgoPoints = _combatant1EgoPoints;
                }
                else
                {
                    PlayerManager.Instance.HealthPoints = _combatant2Health;
                    PlayerManager.Instance.EgoPoints = _combatant2EgoPoints;
                }

                if (PlayerManager.Instance.EgoPoints < 0)
                {
                    PlayerManager.Instance.EgoPoints = 0;
                }

                _textBox.enabled = false;
                _isFighting = false;
                _promptSkip.enabled = false;
                _promptContinue.enabled = false;
                OnFightFinished?.Invoke();
            }
    }

    // Wenn erfolgreich, dann aus Kampf geflohen, wenn nicht, dann ist der Gegner dran
    private IEnumerator TryRetreat()
    {
        _textBox.enabled = true;

        ToggleCanvas(_initialSelectionMenuCanvas, false);

        _playerRoll = PlayerManager.Instance.GetInitiative() - DiceUtil.D6();
        print($"Player Initiative: {_playerRoll}");
        print($"Enemy Initiative: {_enemy.Initiative}");

        if (_playerRoll >= _enemy.Initiative)
        {
            StartCoroutine(EndFight(PlayerManager.Instance, isRetreat: true));

            yield break;
        }
        else
        {
            _currentLine = $"The {_enemy.Name} is too fast for you to escape!";
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            StartCoroutine(EndFight(null));
        }
    }

    // Bestimmt, was die Auswahl im Menü auslöst
    public void HandleSelectedMenuPoint(int index)
    {
        if (_initialSelectionMenuCanvas.isActiveAndEnabled)
        {
            _initialSelectionMenuCanvas.enabled = false;

            // 0 = Insult, 1 = Fight, 2 = UsePotion, 3 = Retreat
            switch (index)
            {
                case 0:
                    StartCoroutine(DisplayInsultOptions());

                    break;

                case 1:
                    if (_turnCoroutine == null)
                    {
                        var enemyHealth = _combatant2 == _enemy ? _combatant2Health : _combatant1Health;

                        var enemyEgoPoints = _combatant2 == _enemy ? _combatant2EgoPoints : _combatant1EgoPoints;

                        _turnCoroutine = StartCoroutine(CombatTurn(attacker: PlayerManager.Instance, defender: _enemy,
                            defenderHealth: enemyHealth, defenderEgoPoints: enemyEgoPoints, isDisadvantage: false));
                    }

                    break;

                case 2:
                    ToggleCanvas(_initialSelectionMenuCanvas, false);
                    ToggleCanvas(_itemUseCanvas, true);

                    StartCoroutine(SetItemDisplay());

                    break;

                case 3:
                    StartCoroutine(TryRetreat());

                    break;
            }

            return;
        }

        if (_insultMenuCanvas.isActiveAndEnabled)
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
                    for (int i = 0; i < _enemyCurrentInsultsAndValues.Count; i++)
                    {
                        if (_enemyInsultsAndValues.ContainsKey(_enemyCurrentInsultsAndValues.ElementAt(i).Key))
                        {
                            continue;
                        }

                        _enemyInsultsAndValues.Add(_enemyCurrentInsultsAndValues.ElementAt(i).Key, _enemyCurrentInsultsAndValues.ElementAt(i).Value);
                    }

                    _enemyCurrentInsultsAndValues.Clear();

                    ToggleCanvas(_insultMenuCanvas, false);

                    ToggleCanvas(_initialSelectionMenuCanvas, true);

                    break;
            }

            return;
        }

        if (_itemUseCanvas.isActiveAndEnabled)
        {
            switch (index)
            {
                case 0:
                    _currentItem = _healingItems[index];

                    StartCoroutine(UseSelectedItem());

                    ToggleCanvas(_itemUseCanvas, false);

                    break;

                case 1:
                    if (_healingItems.Count <= index)
                    {
                        ToggleCanvas(_itemUseCanvas, false);
                        ToggleCanvas(_initialSelectionMenuCanvas, true);

                        _itemOptions[0].GetComponent<SelectionMenu>().SetInitialPointer();
                    }
                    else
                    {
                        _currentItem = _healingItems[index];

                        StartCoroutine(UseSelectedItem());

                        ToggleCanvas(_itemUseCanvas, false);
                    }

                    break;

                case 2:
                    ToggleCanvas(_itemUseCanvas, false);
                    ToggleCanvas(_initialSelectionMenuCanvas, true);

                    _itemOptions[1].GetComponent<SelectionMenu>().SetInitialPointer();

                    break;
            }
        }
    }

        // Nimmt die nötigen Infos für das UI-Update auf und startet Coroutine
    private void SetUIUpdate(bool isHealthHeal, int initialAmount, int healingAmount)
    {
        StartCoroutine(UpdateUIHeal(healingAmount, isHealthHeal, initialAmount));
    }

    // Zeigt visuelles Feedback bei den Health- und Ego-Balken an, zuerst wird ein Balken auf den Zielwert gesetzt und der andere gelerpt
    protected IEnumerator UpdateUI(Combatant combatant, int damage, bool isHealthDamage, int currentHealth = 0, int currentEgo = 0)
    {
        float hitValue = 0;
        Slider slider = null;
        TextMeshProUGUI text = null;

        if (isHealthDamage)
        {
            slider = combatant.Name == PlayerManager.Instance.Name ? _playerHealthBarBelow : _enemyHealthBarBelow;
            hitValue = combatant.Name == PlayerManager.Instance.Name ? (float)damage / (float)PlayerManager.Instance.GetStartingHealth() : (float)damage / (float)_enemy.HealthPoints;
            text = combatant.Name == PlayerManager.Instance.Name ? _playerUIHealth : _enemyUIHealth;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
            }

            text.text = combatant.Name == PlayerManager.Instance.Name ? $"{currentHealth}/{PlayerManager.Instance.GetStartingHealth()}" : $"{currentHealth}/{combatant.HealthPoints}";
        }
        else
        {
            slider = combatant.Name == PlayerManager.Instance.Name ? _playerEgoBarBelow : _enemyEgoBarBelow;
            hitValue = combatant.Name == PlayerManager.Instance.Name ? (float)damage / (float)PlayerManager.Instance.GetStartingEgo() : (float)damage / (float)_enemy.EgoPoints;
            text = combatant.Name == PlayerManager.Instance.Name ? _playerUIEgo : _enemyUIEgo;

            if (currentEgo <= 0)
            {
                currentEgo = 0;
            }

            text.text = combatant.Name == PlayerManager.Instance.Name ? $"{currentEgo}/{PlayerManager.Instance.GetStartingEgo()}" : $"{currentEgo}/{combatant.EgoPoints}";
        }

        float currentValue = slider.value;
        float nextValue = currentValue - hitValue;
        float lerpValue = 0;

        // Weiße Healthbar setzen
        var childSlider = UnityUtil.GetFirstComponentInChildren<Slider>(slider.gameObject);
        childSlider.GetComponent<Slider>().value = nextValue;

        if (combatant == _enemy)
        {
            StartCoroutine(PlayHitParticlesAndAudio(isHealthDamage));
            StartCoroutine(PlayExclamation());
        }

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
        if (defender == _combatant1)
        {
            if (isHealthDamage)
            {
                _combatant1Health = newValue;
            }
            else
            {
                _combatant1EgoPoints = newValue;
            }

            return;
        }

        if (isHealthDamage)
        {
            _combatant2Health = newValue;
        }
        else
        {
            _combatant2EgoPoints = newValue;
        }
    }

    // Checken, welcher Gegner die Condition zugefügt hat
    private IEnumerator EndSceneWithCondition()
    {
        switch (_enemy.Name)
        {
            case "Vampire":
                if (!ConditionManager.Instance.IsVampire)
                {
                    PlayerManager.Instance.LatestCondition = ConditionManager.Conditions.Vampire;

                    yield return StartCoroutine(PrintMultipleLines(ConditionManager.Instance.ApplyCondition(ConditionManager.Conditions.Vampire, true)));
                }

                break;

            case "Werewolf":
                if (!ConditionManager.Instance.IsWerewolf)
                {
                    PlayerManager.Instance.LatestCondition = ConditionManager.Conditions.Werewolf;

                    yield return StartCoroutine(PrintMultipleLines(ConditionManager.Instance.ApplyCondition(ConditionManager.Conditions.Werewolf, true)));
                }

                break;

            case "Zombie":
                if (!ConditionManager.Instance.IsZombie)
                {
                    PlayerManager.Instance.LatestCondition = ConditionManager.Conditions.Zombie;

                    yield return StartCoroutine(PrintMultipleLines(ConditionManager.Instance.ApplyCondition(ConditionManager.Conditions.Zombie, true)));
                }

                break;
        }

        SceneManager.LoadScene(8);
    }

    // Überpürfen, ob SleepDeprived-Condition angewendet werden soll
    IEnumerator CheckForSleepDeprived()
    {
        if (_hasDisadvantage)
        {
            PlayerManager.Instance.HasFinishedDay = true;
        }

        if (_intitialPlayerHealth > PlayerManager.Instance.HealthPoints && !MainManager.Instance.IsDay)
        {
            if (_hasDisadvantage)
            {
                ConditionManager.Instance.ApplyCondition(ConditionManager.Conditions.SleepDeprived, true);
                PlayerManager.Instance.HasDisadvantage = false;
            }

            StartCoroutine(EndSceneWithCondition());

            yield break;
        }

        if (_hasDisadvantage && !ConditionManager.Instance.IsSleepDeprived)
        {
            yield return StartCoroutine(PrintMultipleLines(ConditionManager.Instance.ApplyCondition(ConditionManager.Conditions.SleepDeprived, true)));

            PlayerManager.Instance.HasDisadvantage = false;

            SceneManager.LoadScene(8);

            yield break;
        }

        if (!PlayerManager.Instance.HasFinishedDay)
        {
            SceneManager.LoadScene(2);

            yield break;
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    protected IEnumerator PlayHitParticlesAndAudio(bool isHealthDamage)
    {
        GameObject hitParticles;

        if (isHealthDamage)
        {
            _audioSource.PlayOneShot(_strikeHit);
            
            hitParticles = _hitParticlesStrike;
        }
        else
        {
            _audioSource.PlayOneShot(_insultHit);

            hitParticles = _hitParticlesInsult;
        }

        hitParticles.SetActive(true);

        yield return new WaitForSeconds(GameConfig.HitParticlesLength);

        hitParticles.SetActive(false);
    }

    protected IEnumerator PlayExclamation()
    {
        var randomIndex = UnityEngine.Random.Range(0, _exclamations.Length);

        _exclamations[randomIndex].SetActive(true);

        yield return new WaitForSeconds(GameConfig.ExclamationLength);

        _exclamations[randomIndex].SetActive(false);
    }

    private IEnumerator ManageLoot()
    {
        int randomAmount = 0;

        if (MainManager.Instance.IsDay)
        {
            randomAmount = UnityEngine.Random.Range(GameConfig.MinimumCoinAmountDay, GameConfig.MaximumCoinAmountDay + 1);
        }
        else
        {
            randomAmount = UnityEngine.Random.Range(GameConfig.MinimumCoinAmountNight, GameConfig.MaximumCoinAmountNight + 1);
        }

        InventoryManager.Instance.ManageInventory(_coin, randomAmount, true);

        _currentLine = $"You have received {randomAmount} coins.";
        yield return HandleTextOutput(_currentLine, false);
    }

    private IEnumerator SetItemDisplay()
    {
        _healingItems.Clear();

        foreach (GameObject option in _itemOptions)
        {
            option.SetActive(false);
        }

        List<int> amounts = new List<int>();

        foreach (Item item in InventoryManager.Instance.InventoryItems)
        {
            if (item is IUsable && item is HealingItem)
            {
                int itemAmount = InventoryManager.Instance.InventoryAmounts[InventoryManager.Instance.InventoryItems.IndexOf(item)];

                if (itemAmount == 0)
                {
                    continue;
                }

                amounts.Add(InventoryManager.Instance.InventoryAmounts[InventoryManager.Instance.InventoryItems.IndexOf(item)]);

                _healingItems.Add(item);
            }
        }

        if (_healingItems.Count == 0)
        {
            _textBox.enabled = true;

            ToggleCanvas(_itemUseCanvas, false);

            _currentLine = "You don't carry any potions with you.";
            yield return HandleTextOutput(_currentLine, false);

            _textBox.text = "";

            ToggleCanvas(_itemUseCanvas, false);
            ToggleCanvas(_initialSelectionMenuCanvas, true);

            yield break;
        }

        TextMeshProUGUI[] texts;

        if (_healingItems.Count == 1)
        {
            _itemOptions[0].SetActive(true);
            _itemOptions[0].GetComponent<SelectionMenu>().IsActive = true;

            texts = _itemOptions[0].GetComponentsInChildren<TextMeshProUGUI>();
        }
        else
        {
            _itemOptions[1].SetActive(true);
            _itemOptions[1].GetComponent<SelectionMenu>().IsActive = true;

            texts = _itemOptions[1].GetComponentsInChildren<TextMeshProUGUI>();
        }

        for (int i = 0; i < texts.Length; i++)
        {
            if (i > _healingItems.Count)
            {
                texts[i].enabled = false;

                continue;
            }

            if (i == _healingItems.Count)
            {
                texts[i].text = "Return";

                continue;
            }

            texts[i].text = $"{_healingItems[i].Name} x {amounts[i]}";
        }
    }

    public override IEnumerator UseSelectedItem()
    {
        _textBox.enabled = true;

        bool isHealthHeal = false;
        bool isEgoHeal = false;

        int egoBefore = PlayerManager.Instance.EgoPoints;
        int healthBefore = PlayerManager.Instance.HealthPoints;

        yield return base.UseSelectedItem();

        int egoAfter = PlayerManager.Instance.EgoPoints;
        int healthAfter = PlayerManager.Instance.HealthPoints;

        if (egoAfter > egoBefore)
        {
            isEgoHeal = true;
        }

        if (healthAfter > healthBefore)
        {
            isHealthHeal = true;
        }     

        if (_combatant1 == PlayerManager.Instance)
            {
                if (isHealthHeal)
                {
                    _combatant1Health = PlayerManager.Instance.HealthPoints;               
                }

                if (isEgoHeal)
                {
                    _combatant1EgoPoints = PlayerManager.Instance.EgoPoints;           
                }
            }
            else
            {
                if (isHealthHeal)
                {
                    _combatant2Health = PlayerManager.Instance.HealthPoints;               
                }

                if (isEgoHeal)
                {
                    _combatant2EgoPoints = PlayerManager.Instance.EgoPoints;           
                }
            }

        StartCoroutine(EndFight(null));
    }
}
