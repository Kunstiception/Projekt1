using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private Combatant _enemy;
    [SerializeField] private TextMeshProUGUI _uiElement;
    
    private int _playerRoll;
    private int _enemyRoll;
    private int _combatantHealth1;
    private int _combatantHealth2;
    private int _rawDamage;
    private int _finalDamage;
    private int _accuracy;
    private int _evasion;
    private Combatant _combatant1;
    private Combatant _combatant2;

    IEnumerator Start()
    {
        yield return StartCoroutine(RollInitiative());
        yield return StartCoroutine(CombatCoroutine());
    }

    void Update()
    {
        
    }

    private IEnumerator RollInitiative()
    {
        _playerRoll = PlayerManager.Instance.RollInitiative();
        print($"{PlayerManager.Instance.Name} Initiative: {_playerRoll}");
        _enemyRoll = _enemy.RollInitiative();
        print($"{_enemy.Name} Initiative: {_enemyRoll}");

        _combatant1 = _playerRoll >= _enemyRoll ? PlayerManager.Instance : _enemy;
        _combatant2 = _playerRoll >= _enemyRoll ? _enemy : PlayerManager.Instance;

        _combatantHealth1 = _combatant1.HealthPoints;
        _combatantHealth2 = _combatant2.HealthPoints;

        DialogueUtil.ShowFullLine($"{_combatant1.Name} strikes first!", _uiElement);

        yield return new WaitForSeconds(GameConfig.TimeBetweenCombatLogs);
    }


    private IEnumerator CombatCoroutine()
    {
        while (_combatantHealth1 > 0 && _combatantHealth2 > 0)
        {
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
        }
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
}
