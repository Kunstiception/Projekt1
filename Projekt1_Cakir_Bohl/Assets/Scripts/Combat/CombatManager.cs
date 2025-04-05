using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private Enemy _enemy;
    [SerializeField] private TextMeshProUGUI _uiElement;
    
    private int _playerRoll;
    private int _enemyRoll;
    private int _combatantHealth1;
    private int _combatantHealth2;
    private int _damage;
    private Combatant _combatant1;
    private Combatant _combatant2;

    IEnumerator Start()
    {
        yield return StartCoroutine(RollInitiative(false));
        yield return StartCoroutine(CombatCoroutine());
    }

    void Update()
    {
        
    }

    private IEnumerator RollInitiative(bool isAmbushed)
    {
        _playerRoll = PlayerManager.Instance.Initiative + DiceUtil.D10();
        _enemyRoll = _enemy.Initiative + DiceUtil.D10();

        _combatant1 = _playerRoll >= _enemyRoll || !isAmbushed ? PlayerManager.Instance : _enemy;
        _combatant2 = _playerRoll >= _enemyRoll || !isAmbushed ? _enemy : PlayerManager.Instance;

        _combatantHealth1 = _combatant1.HealthPoints;
        _combatantHealth2 = _combatant2.HealthPoints;

        DialogueUtil.ShowFullLine($"{_combatant1.Name} strikes first!", _uiElement);

        yield return new WaitForSeconds(2);
    }


    private IEnumerator CombatCoroutine()
    {
        while (_combatantHealth1 > 0 && _combatantHealth2 > 0)
        {
            _damage = _combatant1.DoDamge();
            if (_damage >= _combatantHealth2)
            {
                _combatantHealth2 = 0;
            }
            else
            {
                _combatantHealth2 -= _damage;
            }

            DialogueUtil.ShowFullLine($"{_combatant2.Name} was damaged and lost {_damage} health and now has {_combatantHealth2} health!", _uiElement);

            yield return new WaitForSeconds(2);

            if (_combatantHealth2 <= 0) continue;

            _damage = _combatant2.DoDamge();

            if (_damage >= _combatantHealth1)
            {
                _combatantHealth1 = 0;
            }
            else
            {
                _combatantHealth1 -= _damage;
            }

            DialogueUtil.ShowFullLine($"{_combatant1.Name} was damaged and lost {_damage} health and now has {_combatantHealth1} health!", _uiElement);

            yield return new WaitForSeconds(2);
        }
    }
}
