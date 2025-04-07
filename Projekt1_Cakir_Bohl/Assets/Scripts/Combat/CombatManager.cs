using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour, ISelectable
{
    [SerializeField] private Combatant _enemy;
    [SerializeField] private TextMeshProUGUI _uiElement;
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
    private Combatant _combatant1;
    private Combatant _combatant2;
    private Coroutine _combatCoroutine;

    public static event Action OnFightFinished;

    void Start()
    {
        _hasFightStarted = false;
        _uiElement.enabled = false;
    }

    private IEnumerator RollInitiative(bool isDisadvantage)
    {
        if(isDisadvantage)
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

        DialogueUtil.ShowFullLine($"{_combatant1.Name} strikes first!", _uiElement);

        yield return new WaitForSeconds(GameConfig.TimeBetweenCombatLogs);
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

        if(_rawDamage <= 0)
        {
            yield return new WaitForSeconds(GameConfig.TimeBetweenCombatLogs);
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
            DialogueUtil.ShowFullLine($"{_combatant2.Name} was attacked and lost {_finalDamage} health and now has {_combatantHealth2} health!", _uiElement);
            yield return new WaitForSeconds(GameConfig.TimeBetweenCombatLogs);
        }

        if(_combatantHealth2 <= 0)
        {
            StartCoroutine(EndFight(_combatant1));
            yield break;
        }

        if(isDisadvantage)
        {
            isDisadvantage = false;
            
            StartCoroutine(EndFight(null));
            yield break;
        }

        _rawDamage = PerformAttack(_combatant2, _combatant1);
        print($"{_combatant1.Name} Damage: {_rawDamage}");

        if (_rawDamage <= 0)
        {
            yield return new WaitForSeconds(GameConfig.TimeBetweenCombatLogs);
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
            DialogueUtil.ShowFullLine($"{_combatant1.Name} was attacked and lost {_finalDamage} health and now has {_combatantHealth1} health!", _uiElement);
            yield return new WaitForSeconds(GameConfig.TimeBetweenCombatLogs);
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

        if(_evasion >= _accuracy)
        {
            DialogueUtil.ShowFullLine($"{defender.Name} has evaded the attack!", _uiElement);
            
            return 0;
        }

        return attacker.RollDamge();
    }

    private void CalculateDamage(int damage, Combatant defender, int defenderHealth)
    {
        if(defender.ArmorStrength <= 0)
        {
            _finalDamage = _rawDamage;
            return;
        }

        int damageReduction = (damage/defender.ArmorStrength);
        print($"{defender.Name} Damage Reduction: {damageReduction}");

        _finalDamage = _rawDamage - damageReduction;
    }

    private IEnumerator EndFight(Combatant winner)
    {
        StopCoroutine(_combatCoroutine);
        _combatCoroutine = null;

        if (winner)
        {
            DialogueUtil.ShowFullLine($"{winner.Name} has won the fight!", _uiElement);
            yield return new WaitForSeconds(GameConfig.TimeBetweenCombatLogs);

            StopAllCoroutines();
            SceneManager.LoadScene("MapTest");
            yield break;
        }
        
        if(PlayerManager.Instance.HealthPoints <= 0)
        {
            DialogueUtil.ShowFullLine($"The hero has died.", _uiElement);
            yield return new WaitForSeconds(GameConfig.TimeBetweenCombatLogs);

            DialogueUtil.ShowFullLine($"His quest has ended.", _uiElement);
            yield return new WaitForSeconds(GameConfig.TimeBetweenCombatLogs);

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
            DialogueUtil.ShowFullLine($"You manage to escape!", _uiElement);
            yield return new WaitForSeconds(GameConfig.TimeBetweenCombatLogs);

            StopAllCoroutines();
            SceneManager.LoadScene("MapTest");
            yield break;
        }
        else
        {
            DialogueUtil.ShowFullLine($"The {_enemy.Name} is too fast for you to escape!", _uiElement);
            yield return new WaitForSeconds(GameConfig.TimeBetweenCombatLogs);

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
                if(_combatCoroutine == null)
                {
                    _combatCoroutine = StartCoroutine(CombatCoroutine(isDisadvantage: false));
                }
                break;

            case 2:
                StartCoroutine(TryRetreat());
                break;
        }
    }
}
