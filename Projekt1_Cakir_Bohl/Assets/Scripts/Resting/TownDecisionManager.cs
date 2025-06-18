using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TownDecisionManager : Manager, ISelectable
{
    [SerializeField] public Canvas SelectionMenuCanvas;

    IEnumerator Start()
    {
        ToggleCursorState(true);

        ToggleCanvas(SelectionMenuCanvas, false);

        _promptContinue.enabled = false;

        _currentLine = "You have reached a town!";
        yield return HandleTextOutput(_currentLine, false);

        if (ConditionManager.Instance.IsBoostedVampire)
        {
            yield return PrintMultipleLines(UIDialogueStorage.VampireBoostLossLines);

            ConditionManager.Instance.ApplyVampireBiteBoost(false);
        }

        _textBox.text = "";

        ToggleCanvas(SelectionMenuCanvas, true);
    }

    void Update()
    {
        ListenForSkip();
    }

    public void HandleSelectedMenuPoint(int index)
    {
        // 7 = Resting, 10 = Tavern
        switch (index)
        {
            case 0:
                SceneManager.LoadScene(10);

                break;

            case 1:
                ToggleCanvas(SelectionMenuCanvas, false);
                
                StartCoroutine(LoadOutside());

                break;
        }
    }

    // Lädt die Resting-Szene mit dem entsprechenden Hintergrund
    private IEnumerator LoadOutside()
    {
        string[] stayingOutsideLines = UIDialogueStorage.StayingOutsideOfTownLines;

        // Erste Line immer zeigen, auswürfeln ob auch eine zweite angezeigt wird (eine Art Easter Egg)
        _currentLine = stayingOutsideLines[0];
        yield return HandleTextOutput(_currentLine, false);

        if (stayingOutsideLines.Length > 1)
        {
            if (DiceUtil.D10() > GameConfig.ChanceForSecondLine)
            {
                _currentLine = stayingOutsideLines[UnityEngine.Random.Range(1, stayingOutsideLines.Length)];
                yield return HandleTextOutput(_currentLine, false);
            }
        }

        SceneManager.LoadScene(7);
    }
}
