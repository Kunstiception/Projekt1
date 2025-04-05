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
    private string _combatant1;
    private string _combatant2;

    void Start()
    {
        RollInitiative();
    }

    void Update()
    {
        
    }

    private void RollInitiative()
    {
        _playerRoll = PlayerManager.Instance.Initiative + Random.Range(1, 11);
        print(_playerRoll);
        _enemyRoll = _enemy.Initiative + Random.Range(1, 11);
        print(_enemyRoll);

        if (_playerRoll >= _enemyRoll)
        {
            _combatant1 = "Hero";
            _combatant2 = "Monster";
            _combatantHealth1 = PlayerManager.Instance.HealthPoints;
            _combatantHealth2 = _enemy.HealthPoints;
            DialogueUtil.ShowFullLine("Hero strikes first!", _uiElement);
        }
        else
        {
            _combatant1 = "Monster";
            _combatant2 = "Hero";
            _combatantHealth1 = _enemy.HealthPoints;
            _combatantHealth2 = PlayerManager.Instance.HealthPoints;
            DialogueUtil.ShowFullLine($"{_enemy.Name} strikes first!", _uiElement);
        }
    }

}
