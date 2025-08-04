using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossFightManager : CombatManager, ISelectable
{
    [SerializeField] private Canvas _dialogueCanvas;
    [SerializeField] private VoiceLines _attackLines;
    [SerializeField] private VoiceLines _potionLines;
    [SerializeField] private VoiceLines _retreatLines;
    [SerializeField] private VoiceLines _noMoreEgoLines;
    [SerializeField] private InsultLines _selfInsultLines;
    [SerializeField] private GameObject _tree;
    private bool _isAbleToAttack = false;

    private Dictionary<string, int> _currentPlayerInsultsAndValues = new Dictionary<string, int>();

    // Zu Anfang werden praktisch alle UI_Elemente ausgeblendet und nur der Text erscheint unten
    IEnumerator Start()
    {
        ToggleCanvas(_statsCanvas, false);
        ToggleCanvas(_initialSelectionMenuCanvas, false);
        ToggleCanvas(_insultMenuCanvas, false);

        _tree.SetActive(false);
        _hitParticlesInsult.SetActive(false);
        _hitParticlesStrike.SetActive(false);

        foreach (GameObject exclamation in _exclamations)
        {
            exclamation.SetActive(false);
        }

        SetPrompts();

        _mainEffectsAudioSource.PlayOneShot(_playerInsultHit);

        yield return PrintMultipleLines(UIDialogueStorage.SecondPhaseEntryLines);

        GameObject enemyObject = Instantiate(_endBoss);
        _enemy = enemyObject.GetComponent<Combatant>();
        _enemyAnimator = enemyObject.GetComponent<Animator>();

        _tree.SetActive(true);

        ToggleCanvas(_statsCanvas, true);

        SetPrompts();

        SetCombatants();

        SetInitialStats();

        for (int i = 0; i < _selfInsultLines.Insults.Length; i++)
        {
            _playerInsultsAndValues.Add(_selfInsultLines.Insults[i], PlayerManager.Instance.InsultLines.Values[i]);
        }

        _musicSource.Play();

        _currentLine = $"{_enemy.Name}: Stop this! Give yourself to me!";
        yield return HandleTextOutput(_currentLine, false, true);

        _textBox.text = "";

        ToggleCanvas(_initialSelectionMenuCanvas, true);
    }

    void Update()
    {
        ListenForSkipOrAuto();
    }

    new public void HandleSelectedMenuPoint(int index)
    {
        if (_initialSelectionMenuCanvas.isActiveAndEnabled)
        {
            _initialSelectionMenuCanvas.enabled = false;

            // 0 = Insult, 1 = Fight, 2 = UsePotion, 3 = Retreat
            switch (index)
            {
                case 0:
                    if (PlayerManager.Instance.EgoPoints > 0)
                    {
                        DisplaySelfInsultOptions();
                    }
                    else
                    {
                        StartCoroutine(PrintRandomLine(_noMoreEgoLines));
                    }

                    break;

                case 1:
                    if (!_isAbleToAttack)
                    {
                        StartCoroutine(PrintRandomLine(_attackLines));

                        break;
                    }

                    StartCoroutine(PerformFinishingBlow());

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
                    _turnCoroutine = StartCoroutine(SelfInsultTurn(0));

                    break;

                case 1:
                    _turnCoroutine = StartCoroutine(SelfInsultTurn(1));

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

    // Zufällige Line ausgeben, um zu erklären wieso eine Aktion gerade nicht ausgeführt werden kann
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

    // Führt Selbstbeleidigung aus und zeigt danach sogleich die Reaktion des Gegners und führt eine Gegenheilung aus (50% des Schadens)
    private IEnumerator SelfInsultTurn(int optionIndex = 0)
    {
        string line = "";
        int damageValue = 0;

        _textBox.enabled = true;

        switch (optionIndex)
        {
            case 0:
                line = $"{PlayerManager.Instance.Name}: '{_currentPlayerInsultsAndValues.ElementAt(0).Key}'";
                damageValue = _currentPlayerInsultsAndValues.ElementAt(0).Value;

                _egoHitLine = $"{_enemy.Name}: '{_selfInsultLines.AnswersWhenHit[Array.IndexOf(_selfInsultLines.Insults, _currentPlayerInsultsAndValues.ElementAt(0).Key)]}'";

                _playerInsultsAndValues.Add(_currentPlayerInsultsAndValues.ElementAt(1).Key, _currentPlayerInsultsAndValues.ElementAt(1).Value);

                break;

            case 1:
                line = $"{PlayerManager.Instance.Name}: '{_currentPlayerInsultsAndValues.ElementAt(1).Key}'";
                damageValue = _currentPlayerInsultsAndValues.ElementAt(1).Value;

                _egoHitLine = $"{_enemy.Name}: '{_selfInsultLines.AnswersWhenHit[Array.IndexOf(_selfInsultLines.Insults, _currentPlayerInsultsAndValues.ElementAt(1).Key)]}'";

                _playerInsultsAndValues.Add(_currentPlayerInsultsAndValues.ElementAt(0).Key, _currentPlayerInsultsAndValues.ElementAt(0).Value);

                break;

            default:
                Debug.LogError("Insult index not set correctly!");
                break;
        }

        _currentPlayerInsultsAndValues.Clear();

        ToggleCanvas(_initialSelectionMenuCanvas, false);

        _textBox.enabled = true;

        // Insult-Kampf nur ausführen, wenn eine Option ausgewählt wurde
        if (line.Length > 0)
        {
            ToggleCanvas(_insultMenuCanvas, false);

            _currentLine = "You attempt to insult yourself!";
            yield return HandleTextOutput(_currentLine, false);

            _currentLine = line;
            yield return HandleTextOutput(_currentLine, false, true);

            PlayerManager.Instance.EgoPoints -= damageValue;

            StartCoroutine(UpdateUI(PlayerManager.Instance, damageValue, false, currentEgo: PlayerManager.Instance.EgoPoints));

            _currentLine = $"{PlayerManager.Instance.Name} take {damageValue} ego damage!";
            yield return HandleTextOutput(_currentLine, false);


            if (PlayerManager.Instance.EgoPoints <= 0)
            {
                PlayerManager.Instance.EgoPoints = 0;
            }
            else
            {
                _currentLine = _egoHitLine;
                yield return HandleTextOutput(_currentLine, false, true);

                int healValue = damageValue / 2;

                StartCoroutine(UpdateUIHeal(healValue, false, PlayerManager.Instance.EgoPoints));

                PlayerManager.Instance.EgoPoints += healValue;

                _currentLine = $"You have recovered {healValue} ego.";
                yield return HandleTextOutput(_currentLine, false);
            }
        }

        if (PlayerManager.Instance.EgoPoints <= 0)
        {
            yield return PrintMultipleLines(UIDialogueStorage.ReadyToAttackBossLines);

            _isAbleToAttack = true;
        }

        _textBox.text = "";

        ToggleCanvas(_initialSelectionMenuCanvas, true);
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

        ToggleCanvas(_insultMenuCanvas, true);
    }

    // Nachdem die Kampf-Option freigeschalten wurde, kann dieser geskriptete Schlag ausgeführt werden
    private IEnumerator PerformFinishingBlow()
    {
        _currentLine = "You prepare to strike!";
        yield return HandleTextOutput(_currentLine, false);

        StartCoroutine(UpdateUI(_enemy, _enemy.HealthPoints, true, currentHealth: 0));

        _currentLine = $"{_enemy.Name} takes {_enemy.HealthPoints} health damage!";
        yield return HandleTextOutput(_currentLine, false);

        _enemyAnimator.SetTrigger("OnDefeat");

        yield return PrintMultipleLines(UIDialogueStorage.BossDefeatedLines);

        SceneManager.LoadScene(15);
    }
}
