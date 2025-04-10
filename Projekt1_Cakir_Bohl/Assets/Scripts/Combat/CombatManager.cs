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
    [SerializeField] private Slider _enemyHealthBarBelow;
    [SerializeField] private Slider _playerHealthBarBelow;
    [SerializeField] private Slider _enemyEgoBarTop;
    [SerializeField] private Slider _playerEgohBarTop;
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
    private bool _hasFightStarted;
    private string _currentLine;
    private Combatant _combatant1;
    private Combatant _combatant2;
    private Coroutine _combatCoroutine;
    private Coroutine _textCoroutine;
    private List<string> _wrongInsults = new List<string>();
    private List<string> _correctInsults = new List<string>();

    public static event Action OnFightFinished;

    void Start()
    {
        _hasFightStarted = false;
        _textBox.enabled = false;
        _promptSkip.enabled = false;
        _promptContinue.enabled = false;
        _persuasionMenuCanvas.enabled = false;
        _wrongInsults = _enemy.PersuasionLines.WrongInsults;
        _correctInsults = _enemy.PersuasionLines.CorrectInsults;
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
                DialogueUtil.ShowFullLine(_currentLine, _textBox, _promptSkip);
            }
        }
    }

    private void DisplayPersuasionOptions()
    {
        _textBox.enabled = true;
        _persuasionMenuCanvas.enabled = true;

        TextMeshProUGUI[] options = _persuasionMenuCanvas.GetComponentsInChildren<TextMeshProUGUI>();

        int orderSeed = UnityEngine.Random.Range(0, 2);
        int insultSeed = UnityEngine.Random.Range(0, _correctInsults.Count - 1);

        for (int i = 0; i < options.Length; i++)
        {
            options[i].text = orderSeed == 0 ? _correctInsults[insultSeed] :
                _wrongInsults[insultSeed];
        }

        _correctInsults.RemoveAt(insultSeed);
        _wrongInsults.RemoveAt(insultSeed);
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

            StartCoroutine(UpdateHealthbar(combatant: _combatant2, damage: _finalDamage, currentHealth: _combatantHealth2, isHealthDamage: true));

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
            StartCoroutine(EndFight(_combatant1));
            yield break;
        }

        if (isDisadvantage)
        {
            isDisadvantage = false;

            StartCoroutine(EndFight(null));
            yield break;
        }

        _currentLine = DialogueUtil.CreateCombatLog(_combatant2, "prepares", "to strike!");

        yield return StartCoroutine(HandleTextOutput(_currentLine, false));

        _rawDamage = PerformAttack(_combatant2, _combatant1);
        print($"{_combatant1.Name} Damage: {_rawDamage}");

        if (_rawDamage <= 0)
        {
            _currentLine = $"{_combatant1.Name} has evaded the attack!";

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

            StartCoroutine(UpdateHealthbar(combatant: _combatant1, damage: _finalDamage, currentHealth: _combatantHealth1, isHealthDamage: true));

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
            _currentLine = DialogueUtil.CreateCombatLog(winner, "has", "won the fight!");

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

        _textBox.enabled = false;
        _selectionMenuCanvas.enabled = true;
        OnFightFinished?.Invoke();
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
        if(!isFirstLayer)
        {
            _selectionMenuCanvas.enabled = false; 
            // 0 = Talk, 1 = Fight, 2 = Retreat
            switch (index)
            {
                case 0:
                    _textBox.enabled = true;
                    _persuasionMenuCanvas.enabled = true;
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
            // 0 = , 1 = Flirt, 2 = Return
            switch (index)
            {
                case 0:
                    //TryPersuasion(0);
                    break;

                case 1:
                    //TryPersuasion(1);
                    break;

                case 2:
                    //TryPersuasion(2);
                    break;
            }
        }
    }         


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

    private IEnumerator UpdateHealthbar(Combatant combatant, int damage, int currentHealth, bool isHealthDamage)
    {
        Slider slider = combatant.Name == PlayerManager.Instance.Name ? _playerHealthBarBelow : _enemyHealthBarBelow;
        float currentValue = slider.value;
        float hitValue = 0;

        if (isHealthDamage)
        {
            hitValue = (float)damage / (float)combatant.HealthPoints;
        }
        else
        {
            hitValue = (float)damage / (float)combatant.EgoPoints;
        }
        
        float nextValue = currentValue - hitValue;
        float lerpValue = 0;

        // Weiﬂe Healthbar setzen
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
