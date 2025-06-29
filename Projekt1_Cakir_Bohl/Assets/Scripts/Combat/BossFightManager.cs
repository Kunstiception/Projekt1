using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFightManager : Manager, ISelectable
{
    [SerializeField] Canvas _initialSelectionMenuCanvas;
    [SerializeField] Canvas _dialogueCanvas;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void HandleSelectedMenuPoint(int index)
    {
        ToggleCanvas(_initialSelectionMenuCanvas, false);

        // 0 = Insult, 1 = Fight, 2 = UsePotion, 3 = Retreat
        switch (index)
        {
            case 0:
                ToggleCanvas(_dialogueCanvas, true);

                break;

            case 1:
                _currentLine = "This enemy does not seem to take any damage from your attacks!";
                // yield return HandleTextOutput(_currentLine, false);

                ToggleCanvas(_initialSelectionMenuCanvas, true);

                break;

            case 2:
                _currentLine = "You don't need those!";
                // yield return HandleTextOutput(_currentLine, false);

                break;

            case 3:
                // Eine random Line

                break;
        }
    }
}
