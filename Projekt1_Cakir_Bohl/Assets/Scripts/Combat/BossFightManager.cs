using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossFightManager : CombatManager, ISelectable
{
    [SerializeField] private Canvas _dialogueCanvas;
    [SerializeField] private VoiceLines _attackLines;
    [SerializeField] private VoiceLines _potionLines;
    [SerializeField] private VoiceLines _retreatLines;

    IEnumerator Start()
    {
        Instantiate(_endBoss);
        _enemy = _endBoss.GetComponent<Combatant>();

        SetCombatants();

        SetInitialStats();

        _hitParticles.SetActive(false);

        foreach (GameObject exclamation in _exclamations)
        {
            exclamation.SetActive(false);
        }

        ToggleCanvas(_initialSelectionMenuCanvas, false);
        ToggleCanvas(_dialogueCanvas, false);

        _currentLine = $"{_enemy.Name}: Give me all your ego!";
        yield return HandleTextOutput(_currentLine, false);

        StartCoroutine(CombatCoroutine());
    }

    void Update()
    {
        ListenForSkip();
    }

    new public void HandleSelectedMenuPoint(int index)
    {
        ToggleCanvas(_initialSelectionMenuCanvas, false);

        // 0 = Insult, 1 = Fight, 2 = UsePotion, 3 = Retreat
        switch (index)
        {
            case 0:
                ToggleCanvas(_dialogueCanvas, true);

                break;

            case 1:
                StartCoroutine(PrintRandomLine(_attackLines));

                break;

            case 2:
                StartCoroutine(PrintRandomLine(_potionLines));

                break;

            case 3:
                StartCoroutine(PrintRandomLine(_retreatLines));

                break;
        }
    }

    private IEnumerator PrintRandomLine(VoiceLines possibleLines)
    {
        _textBox.enabled = true;

        ToggleCanvas(_initialSelectionMenuCanvas, false);

        _currentLine = possibleLines.Lines[UnityEngine.Random.Range(0, possibleLines.Lines.Length)];
        yield return HandleTextOutput(_currentLine, false);

        ToggleCanvas(_initialSelectionMenuCanvas, true);

        _textBox.text = "";
    }

    private void SetCombatants()
    {
        _combatant1 = _enemy;
        _combatant2 = PlayerManager.Instance;
    }
}
