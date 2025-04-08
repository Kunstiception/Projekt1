using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour, ISelectable
{
    [SerializeField] private Combatant _enemy;
    [SerializeField] private TextMeshProUGUI _uiElement;
    [SerializeField] private TextMeshProUGUI _promptSkip;
    [SerializeField] private TextMeshProUGUI _promptContinue;
    [SerializeField] private Canvas _selectionMenuCanvas;

    private int _playerRoll;
    private int _enemyRoll;
    private int _combatantHealth1;
    private int _combatantHealth2;
    private int _rawDamage;
    private int _finalDamage;
    private int _accuracy;
    private int _evasion;
    private bool _hasFightStarted;
    private string _currentLine;
    private Combatant _combatant1;
    private Combatant _combatant2;
    private Coroutine _combatCoroutine;
    private Coroutine _textCoroutine;
    private Coroutine _waitForContinueCoroutine;

    public static event Action OnFightFinished;

    void Start()
    {
        _hasFightStarted = false;
        _uiElement.enabled = false;
        _promptSkip.enabled = false;
        _promptContinue.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_textCoroutine != null)
            {
                _promptSkip.enabled = false;
                StopCoroutine(_textCoroutine);
                _textCoroutine = null;
                DialogueUtil.ShowFullLine(_currentLine, _uiElement, _promptSkip);
            }
        }
    }

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
            print($"{PlayerManager.Instance.Name} Initiative: {_playerRoll}");
            _enemyRoll = _enemy.RollInitiative();
            print($"{_enemy.Name} Initiative: {_enemyRoll}");

            _combatant1 = _playerRoll >= _enemyRoll ? PlayerManager.Instance : _enemy;
            _combatant2 = _playerRoll >= _enemyRoll ? _enemy : PlayerManager.Instance;
        }

        _combatantHealth1 = _combatant1.HealthPoints;
        _combatantHealth2 = _combatant2.HealthPoints;

        _currentLine = $"{_combatant1.Name} strikes first!";

        yield return StartCoroutine(HandleTextOutput(_currentLine, false));
    }

    private IEnumerator CombatCoroutine(bool isDisadvantage)
    {
        _uiElement.enabled = true;

        if (!_hasFightStarted || !isDisadvantage)
        {
            yield return StartCoroutine(RollInitiative(isDisadvantage));
            _hasFightStarted = true;
        }

        _rawDamage = PerformAttack(_combatant1, _combatant2);
        print($"{_combatant1.Name} Damage: {_rawDamage}");

        if (_rawDamage <= 0)
        {
            _currentLine = $"{_combatant2.Name} has evaded the attack!";

            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
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
            _currentLine = $"{_combatant2.Name} was attacked and lost {_finalDamage} health and now has {_combatantHealth2} health!";

            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }

        if (_combatantHealth2 <= 0)
        {
            StartCoroutine(EndFight(_combatant1));
            yield break;
        }

        if (isDisadvantage)
        {
            isDisadvantage = false;

            StartCoroutine(EndFight(null));
            yield break;
        }

        _rawDamage = PerformAttack(_combatant2, _combatant1);
        print($"{_combatant1.Name} Damage: {_rawDamage}");

        if (_rawDamage <= 0)
        {
            _currentLine = $"{_combatant1.Name} has evaded the attack!";

            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
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
            _currentLine = $"{_combatant1.Name} was attacked and lost {_finalDamage} health and now has {_combatantHealth1} health!";

            yield return StartCoroutine(HandleTextOutput(_currentLine, true));
        }

        if (_combatantHealth1 <= 0)
        {
            StartCoroutine(EndFight(_combatant2));
            yield break;
        }

        StartCoroutine(EndFight(null));
    }

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
        print($"{defender.Name} Damage Reduction: {damageReduction}");

        _finalDamage = _rawDamage - damageReduction;
    }

    private IEnumerator EndFight(Combatant winner)
    {
        StopCoroutine(_combatCoroutine);
        _combatCoroutine = null;

        if (winner)
        {
            _currentLine = $"{winner.Name} has won the fight!";

            yield return StartCoroutine(HandleTextOutput(_currentLine, true));

            StopAllCoroutines();
            SceneManager.LoadScene("MapTest");
            yield break;
        }

        if (PlayerManager.Instance.HealthPoints <= 0)
        {
            _currentLine = "You have died. Your quest has ended.";

            yield return StartCoroutine(HandleTextOutput(_currentLine, true));

            StopAllCoroutines();
            SceneManager.LoadScene("StartMenu");
            yield break;
        }

        _uiElement.enabled = false;
        _selectionMenuCanvas.enabled = true;
        OnFightFinished?.Invoke();
    }

    private IEnumerator TryRetreat()
    {
        _uiElement.enabled = true;
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

            yield return StartCoroutine(HandleTextOutput(_currentLine, true));

            if (_combatCoroutine == null)
            {
                _combatCoroutine = StartCoroutine(CombatCoroutine(isDisadvantage: true));
            }
        }
    }

    public void HandleSelectedItem(int index)
    {
        _selectionMenuCanvas.enabled = false;

        // 0 = Talk, 1 = Fight, 2 = Retreat
        switch (index)
        {
            case 0:
                Debug.LogError("Feature awaiting implementation");
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

    private IEnumerator HandleTextOutput(string line, bool isLastLine)
    {
        _textCoroutine = StartCoroutine(DialogueUtil.DisplayTextOverTime(line, _uiElement, _promptSkip, _promptContinue));

        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitUntil.html
        yield return new WaitUntil(() => _currentLine == _uiElement.text);

        if (isLastLine)
        {
            yield break;
        }

        // Einen Frame warten, damit Input nicht beide GetKeyDown-Events triggert
        yield return null;

        yield return StartCoroutine(DialogueUtil.WaitForContinue(_promptContinue));
    }
}
