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
    [SerializeField] private GameObject[] _enemiesDay;
    [SerializeField] private GameObject[] _enemiesNight;
    [SerializeField] private GameObject _guard;
    [SerializeField] private TextMeshProUGUI _enemyUIHealth;
    [SerializeField] private TextMeshProUGUI _enemyUIEgo;
    [SerializeField] private Slider _enemyHealthBarBelow;
    [SerializeField] private Slider _enemyEgoBarBelow;
    [SerializeField] private Canvas _selectionMenuCanvas;
    [SerializeField] private Canvas _persuasionMenuCanvas;
    [SerializeField] private GameObject _hitParticles;
    [SerializeField] private GameObject[] _exclamations;

    private int _intitialPlayerHealth;
    private int _playerRoll;
    private int _enemyRoll;
    private int _combatant1Health;
    private int _combatant2Health;
    private int _attackerHealth;
    private int _defenderHealth;
    private int _defenderEgoPoints;
    private int _rawDamage;
    private int _finalDamage;
    private int _accuracy;
    private int _evasion;
    private int _combatant1EgoPoints;
    private int _combatant2EgoPoints;
    private bool _hasFightStarted;
    private bool _isFighting;
    private bool _hasDisadvantage;
    private bool _isFirstCombatant;
    private string _egoHitLine;
    private string _egoResistLine;
    private Combatant _enemy;
    private Combatant _attackingCombatant;
    private Combatant _defendingCombatant;
    private Combatant _combatant1;
    private Combatant _combatant2;
    private Coroutine _turnCoroutine;
    private Coroutine _insultTurn;
    private Dictionary<string, int> _enemyInsultsAndValues = new Dictionary<string, int>();
    private Dictionary<string, int> _enemyCurrentInsultsAndValues = new Dictionary<string, int>();
    private Dictionary<string, int> _playerInsultsAndValues = new Dictionary<string, int>();

    public static event Action OnFightFinished;

    IEnumerator Start()
    {
        ToggleCursorState(true);

        _hasFightStarted = false;
        _isFighting = true;
        _textBox.enabled = true;
        _promptSkip.enabled = true;
        _promptContinue.enabled = false;
        _hitParticles.SetActive(false);

        foreach (GameObject exclamation in _exclamations)
        {
            exclamation.SetActive(false);
        }

        ToggleCanvas(_selectionMenuCanvas, false);
        ToggleCanvas(_persuasionMenuCanvas, false);

        _hasDisadvantage = PlayerManager.Instance.HasDisadvantage;

        if (!PlayerManager.Instance.GotCaught)
        {
            if (MainManager.Instance.IsDay)
            {
                var randomIndex = UnityEngine.Random.Range(0, _enemiesDay.Length);
                _enemy = _enemiesDay[randomIndex].GetComponent<Combatant>();
            }
            else
            {
                var randomIndex = UnityEngine.Random.Range(0, _enemiesNight.Length);
                _enemy = _enemiesNight[randomIndex].GetComponent<Combatant>();
            }
        }
        else
        {
            _enemy = _guard.GetComponent<Combatant>();
        }

        Instantiate(_enemy);

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

    void OnDisable()
    {
        StopAllCoroutines();
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

        _currentLine = $"You encounter a {_enemy.Name}!";
        yield return StartCoroutine(HandleTextOutput(_currentLine, false));

        if (PlayerManager.Instance.GotCaught)
        {
            _currentLine = UIDialogueStorage.GuardAppearsLines[UnityEngine.Random.Range(0, UIDialogueStorage.GuardAppearsLines.Length)];
            yield return HandleTextOutput(_currentLine, false);
        }

        if (EvaluateVampire())
        {
            _currentLine = UIDialogueStorage.VampireSunDamageLines[0];
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            if (PlayerManager.Instance == _combatant1)
            {
                _combatant1Health -= GameConfig.VampireSunDamage;
            }
            else
            {
                _combatant2Health -= GameConfig.VampireSunDamage;
            }

            PlayerManager.Instance.HealthPoints -= GameConfig.VampireSunDamage;

            StartCoroutine(UpdateUI(PlayerManager.Instance, GameConfig.VampireSunDamage, true, PlayerManager.Instance.HealthPoints));

            _currentLine = UIDialogueStorage.VampireSunDamageLines[1];
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }

        _currentLine = DialogueUtil.CreateCombatLog(_combatant1, "goes", "first!");
        if (_combatant1 == PlayerManager.Instance)
        {
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
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
        ToggleCanvas(_selectionMenuCanvas, false);

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

        if (ConditionManager.Instance.IsZombie)
        {
            if (DiceUtil.D10() < GameConfig.EnemyFleeChance)
            {
                _currentLine = $"{_enemy.Name} seems scared.";
                yield return HandleTextOutput(_currentLine, false);

                yield return PrintMultipleLines(UIDialogueStorage.EnemyFleeLines);

                StopAllCoroutines();

                StartCoroutine(EndFight(PlayerManager.Instance));

                ToggleCanvas(_selectionMenuCanvas, false);

                yield break;
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
                _turnCoroutine = StartCoroutine(CombatTurn(attacker: _enemy, defender: PlayerManager.Instance,
                       defenderHealth: _defenderHealth, defenderEgoPoints: _defenderEgoPoints, isDisadvantage: false));
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
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            _textBox.enabled = false;

            ToggleCanvas(_persuasionMenuCanvas, false);
            ToggleCanvas(_selectionMenuCanvas, true);
            yield break;
        }

        ToggleCanvas(_persuasionMenuCanvas, true);

        TextMeshProUGUI[] options = _persuasionMenuCanvas.GetComponentsInChildren<TextMeshProUGUI>();

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
            ToggleCanvas(_persuasionMenuCanvas, false);

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
                ToggleCanvas(_persuasionMenuCanvas, false);

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
            ToggleCanvas(_selectionMenuCanvas, false);

            if (_enemy.Name != "Zombie")
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
            ToggleCanvas(_persuasionMenuCanvas, false);

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
                if (defender.Name != "Zombie")
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

        ToggleCanvas(_selectionMenuCanvas, false);

        StartCoroutine(EndFight(null));
    }

    // Eine Runde Kampf
    private IEnumerator CombatTurn(Combatant attacker, Combatant defender, int defenderHealth, int defenderEgoPoints, bool isDisadvantage)
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
                _currentLine = DialogueUtil.CreateCombatLog(winner, "has", "won the fight!");
                yield return StartCoroutine(HandleTextOutput(_currentLine, false));
            }

            if (winner == PlayerManager.Instance)
            {
                if (PlayerManager.Instance.GotCaught)
                {
                    yield return PrintMultipleLines(UIDialogueStorage.SlayedGuardLines);

                    _textBox.text = "";

                    PlayerManager.Instance.GotCaught = false;

                    SceneManager.LoadScene(7);

                    yield break;
                }

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

        ToggleCanvas(_selectionMenuCanvas, false);

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

    // Bestimmt, was die Auswahl im Menü auslöst, zwei Menü-Ebenen möglich
    public void HandleSelectedMenuPoint(int index)
    {
        if (_selectionMenuCanvas.isActiveAndEnabled)
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
                        var enemyHealth = _combatant2 == _enemy ? _combatant2Health : _combatant1Health;

                        var enemyEgoPoints = _combatant2 == _enemy ? _combatant2EgoPoints : _combatant1EgoPoints;

                        _turnCoroutine = StartCoroutine(CombatTurn(attacker: PlayerManager.Instance, defender: _enemy,
                            defenderHealth: enemyHealth, defenderEgoPoints: enemyEgoPoints, isDisadvantage: false));
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
                    for (int i = 0; i < _enemyCurrentInsultsAndValues.Count; i++)
                    {
                        if (_enemyInsultsAndValues.ContainsKey(_enemyCurrentInsultsAndValues.ElementAt(i).Key))
                        {
                            continue;
                        }

                        _enemyInsultsAndValues.Add(_enemyCurrentInsultsAndValues.ElementAt(i).Key, _enemyCurrentInsultsAndValues.ElementAt(i).Value);
                    }

                    _enemyCurrentInsultsAndValues.Clear();

                    ToggleCanvas(_persuasionMenuCanvas, false);

                    ToggleCanvas(_selectionMenuCanvas, true);
                    break;
            }
        }
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
            StartCoroutine(PlayHitParticles());
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

        SceneManager.LoadScene(2);

        yield break;
    }

    private IEnumerator PlayHitParticles()
    {
        _hitParticles.SetActive(true);

        yield return new WaitForSeconds(GameConfig.HitParticlesLength);

        _hitParticles.SetActive(false);
    }

    private IEnumerator PlayExclamation()
    {
        var randomIndex = UnityEngine.Random.Range(0, _exclamations.Length);

        _exclamations[randomIndex].SetActive(true);

        yield return new WaitForSeconds(GameConfig.ExclamationLength);

        _exclamations[randomIndex].SetActive(false);
    }
}
