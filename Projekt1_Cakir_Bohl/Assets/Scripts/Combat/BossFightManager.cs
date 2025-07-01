using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossFightManager : CombatManager, ISelectable
{
    [SerializeField] private Canvas _dialogueCanvas;
    [SerializeField] private VoiceLines _attackLines;
    [SerializeField] private VoiceLines _potionLines;
    [SerializeField] private VoiceLines _retreatLines;
    [SerializeField] private InsultLines _selfInsultLines;

    private Dictionary<string, int> _currentPlayerInsultsAndValues = new Dictionary<string, int>();

    IEnumerator Start()
    {
        Instantiate(_endBoss);
        _enemy = _endBoss.GetComponent<Combatant>();

        SetCombatants();

        SetInitialStats();

        _hitParticlesInsult.SetActive(false);
        _hitParticlesStrike.SetActive(false);

        foreach (GameObject exclamation in _exclamations)
        {
            exclamation.SetActive(false);
        }

        for (int i = 0; i < _selfInsultLines.Insults.Length; i++)
        {
            _playerInsultsAndValues.Add(_selfInsultLines.Insults[i], PlayerManager.Instance.InsultLines.Values[i]);
        }

        ToggleCanvas(_initialSelectionMenuCanvas, false);
        ToggleCanvas(_dialogueCanvas, false);
        ToggleCanvas(_insultMenuCanvas, false);

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
        if (_initialSelectionMenuCanvas.isActiveAndEnabled)
        {
            _initialSelectionMenuCanvas.enabled = false;
            //ToggleCanvas(_initialSelectionMenuCanvas, false);
            // 0 = Insult, 1 = Fight, 2 = UsePotion, 3 = Retreat
            switch (index)
            {
                case 0:
                    DisplaySelfInsultOptions();

                    ToggleCanvas(_insultMenuCanvas, true);
                    
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

            return;
        }

        if (_insultMenuCanvas.isActiveAndEnabled)
        {
             // 0 = Option 1, 1 = Option 2, 2 = Return
            switch (index)
            {
                case 0:
                    _turnCoroutine = StartCoroutine(SelfInsultTurn(PlayerManager.Instance.EgoPoints, 0));

                    break;

                case 1:
                    _turnCoroutine = StartCoroutine(SelfInsultTurn(PlayerManager.Instance.EgoPoints, 1));

                    break;

                case 2:
                    for (int i = 0; i < _currentPlayerInsultsAndValues.Count; i++)
                    {
                        if (_playerInsultsAndValues.ContainsKey(_currentPlayerInsultsAndValues.ElementAt(i).Key))
                        {
                            continue;
                        }

                        _playerInsultsAndValues.Add(_currentPlayerInsultsAndValues.ElementAt(i).Key, _currentPlayerInsultsAndValues.ElementAt(i).Value);
                    }

                    _currentPlayerInsultsAndValues.Clear();

                    ToggleCanvas(_insultMenuCanvas, false);

                    ToggleCanvas(_initialSelectionMenuCanvas, true);

                    break;
            }
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

    // Eine Runde Insult-Kampf, variiert je nachdem ob Player oder Enemy Angreifer ist
    private IEnumerator SelfInsultTurn(int currentEgoPoints, int optionIndex = 0)
    {
        string line = "";
        int value = 0;

        _textBox.enabled = true;

        // Je nach ausgewählter Option Line und Value zuweisen sowie mögliche Antworten bereits laden
        // Nicht gewählte Option wieder dem Original-Dictionary hinzufügen
        switch (optionIndex)
        {
            case 0:
                line = $"{PlayerManager.Instance.Name}: '{_currentPlayerInsultsAndValues.ElementAt(0).Key}'";
                value = _currentPlayerInsultsAndValues.ElementAt(0).Value;

                _egoHitLine = _selfInsultLines.AnswersWhenHit[Array.IndexOf(_selfInsultLines.Insults, _currentPlayerInsultsAndValues.ElementAt(0).Key)];

                _playerInsultsAndValues.Add(_currentPlayerInsultsAndValues.ElementAt(1).Key, _currentPlayerInsultsAndValues.ElementAt(1).Value);

                break;

            case 1:
                line = $"{PlayerManager.Instance.Name}: '{_currentPlayerInsultsAndValues.ElementAt(1).Key}'";
                value = _currentPlayerInsultsAndValues.ElementAt(1).Value;

                _egoHitLine = _selfInsultLines.AnswersWhenHit[Array.IndexOf(_selfInsultLines.Insults, _currentPlayerInsultsAndValues.ElementAt(1).Key)];

                _playerInsultsAndValues.Add(_currentPlayerInsultsAndValues.ElementAt(0).Key, _currentPlayerInsultsAndValues.ElementAt(0).Value);

                break;

            default:
                Debug.LogError("Insult index not set correctly!");
                break;
        }

        ToggleCanvas(_initialSelectionMenuCanvas, false);

        _textBox.enabled = true;

        // Insult-Kampf nur ausführen, wenn eine Option ausgewählt wurde
        if (line.Length > 0)
        {
            ToggleCanvas(_insultMenuCanvas, false);

            //_currentLine = DialogueUtil.CreateCombatLog(PlayerManager.Instance, "attempts", $"insult yourself!");
            _currentLine = "You attempt to insult yourself!";
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));

            // hier ausgwählte Line
        }

        if (currentEgoPoints <= 0)
        {
            _currentLine = "You have lost all ego!";
            yield return StartCoroutine(HandleTextOutput(_currentLine, false));
        }

        //dann Voice-Line und dann noch mal random VoiceLine später

        //ResetInsultTurn();
    }

    private void DisplaySelfInsultOptions()
    {
        ToggleCanvas(_initialSelectionMenuCanvas, false);

        TextMeshProUGUI[] options = _insultMenuCanvas.GetComponentsInChildren<TextMeshProUGUI>();

        // Zwei zufällige Optionen und Werte zuweisen und temporärem Dicitionary hinzufügen (falls nicht schon vorhanden, wenn zuvor return ausgewählt wurde und man wieder zurückkehrt)
        // Option aus Original-Dictionary entfernen
        if (_currentPlayerInsultsAndValues.Count == 0)
        {
            for (int i = 0; i < options.Length - 1; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, _playerInsultsAndValues.Count - 1);

                options[i].text = _playerInsultsAndValues.ElementAt(randomIndex).Key;

                _currentPlayerInsultsAndValues.Add(options[i].text, _playerInsultsAndValues.ElementAt(randomIndex).Value);

                _playerInsultsAndValues.Remove(options[i].text);
            }
        }
    }
}
