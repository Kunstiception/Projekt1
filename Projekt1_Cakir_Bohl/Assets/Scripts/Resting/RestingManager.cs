using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class RestingManager : MonoBehaviour, ISelectable
{
    [SerializeField] public Canvas SelectionMenuCanvas;
    [SerializeField] public Canvas InventoryCanvas;
    [SerializeField] public Canvas ItemToDoCanvas;
    [SerializeField] private TextMeshProUGUI _promptSkip;
    [SerializeField] private TextMeshProUGUI _promptContinue;
    [SerializeField] private TextMeshProUGUI _textBox;
    private Coroutine _textCoroutine;
    private string _currentLine;
    private string[] _currentItemLines;
    private bool _isAmbush;
    private Item _currentItem;

    void Start()
    {
        _textBox.enabled = false;
        _promptContinue.enabled = false;
        _promptSkip.enabled = false;
        ToggleCanvas(InventoryCanvas, false);
        ToggleCanvas(ItemToDoCanvas, false);
    }

    private void OnEnable()
    {      
        InventoryDisplayer.itemSelection += SetCurrentItem;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        InventoryDisplayer.itemSelection -= SetCurrentItem;
    }

    void Update()
    {
        // Ermöglicht sofortiges Anzeigen der gesamten derzeitigen Line
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

    // Bestimmt, was die Auswahl im Menü auslöst, zwei Menü-Ebenen möglich
    public void HandleSelectedMenuPoint(int index, bool isFirstLayer)
    {
        ToggleCanvas(SelectionMenuCanvas, false);
        
        // 0 = sleep, 1 = show items, 2 = continue quest
        switch(index)
        {
            case 0:
                _isAmbush = DecideIfAmbush();

                // Play corresponding animation

                StartCoroutine(SleepingCoroutine(_isAmbush));

                break;

            case 1:
                ToggleCanvas(InventoryCanvas, true);

                _textBox.enabled = true;

                SelectionMenuCanvas.GetComponent<SelectionMenu>().enabled = false;

                break;
        }
    }

    public void HandleSelectedItemOrEquipment(int index)
    {
        switch (index)
        {
            case 0:  
                StartCoroutine(UseOrDiscardItem(isUse: true));

                break;
                
            case 1:
                StartCoroutine(UseOrDiscardItem(isUse: false));

                break;

            case 2:
                ToggleCanvas(ItemToDoCanvas, false);

                InventoryCanvas.GetComponent<InventoryDisplayer>().IsActive = true;

                break;
        }
    }

    private IEnumerator UseOrDiscardItem(bool isUse)
    {
        if(!isUse)
        {
            _currentLine = $"You discard {_currentItem}.";
            yield return HandleTextOutput(_currentLine, false);

            InventoryManager.Instance.ManageInventory(_currentItem, 1, false);

            _textBox.text = "";

            yield break;
        }

        InventoryManager.Instance.ManageInventory(_currentItem, 1, false);

        InventoryCanvas.GetComponent<InventoryDisplayer>().UpdateDisplayedInventory();

        for(int i = 0; i < 2; i++)
        {
            if(i > 0)
            {
                if(DiceUtil.D10() > 7)
                {

                    yield return HandleTextOutput(_currentItemLines[i], false);
                }

                yield break;
            }

            yield return HandleTextOutput(_currentItemLines[i], false);
        }
    }

    private bool DecideIfAmbush()
    {
        int random = DiceUtil.D10();

        if (random >= 6)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private IEnumerator SleepingCoroutine(bool isAmbush)
    {
        _textBox.enabled = true;
        
        _currentLine = "You are falling asleep.";
        yield return HandleTextOutput(_currentLine, false);
        
        if(isAmbush)
        {
            int healthHeal;
            healthHeal = PlayerManager.Instance.HealthPoints < GameConfig.PlayerStartingHealth ?
                UnityEngine.Random.Range(1, GameConfig.PlayerStartingHealth - PlayerManager.Instance.HealthPoints) : 0;
            

            int egoHeal;
            egoHeal = PlayerManager.Instance.EgoPoints < GameConfig.PlayerStartingEgo ?
                UnityEngine.Random.Range(1, GameConfig.PlayerStartingEgo - PlayerManager.Instance.EgoPoints) : 0;

            PlayerManager.Instance.HealthPoints += healthHeal;
            PlayerManager.Instance.EgoPoints += egoHeal;

            //Wait for anim
            yield return new WaitForSeconds(5);

            if(healthHeal == 0 && egoHeal == 0)
            {
                _currentLine = $"You sleep well...";
                yield return HandleTextOutput(_currentLine, false);
            }
            else
            {
                _currentLine = $"You have recovered {healthHeal} health and {egoHeal} ego...";
                yield return HandleTextOutput(_currentLine, false);
            }

            _currentLine = "... before being ambushed!";
            yield return HandleTextOutput(_currentLine, true);
            

            PlayerManager.Instance.HasDisadvantage = true;

            SceneManager.LoadScene("CombatTest");
            yield break;
        }
        else
        {
            //Wait for anim
            yield return new WaitForSeconds(5);
            
            PlayerManager.Instance.HealthPoints = GameConfig.PlayerStartingHealth;
            PlayerManager.Instance.EgoPoints = GameConfig.PlayerStartingEgo;
            
            _currentLine = "You have slept through the night and are now fully recovered!";
            yield return HandleTextOutput(_currentLine, true);
        }

        _textBox.enabled = false;
        ToggleCanvas(SelectionMenuCanvas, true);
    }

    private void SetCurrentItem(Item item)
    {
        _currentItem = item;
        _currentItemLines = _currentItem.GetComponent<IConsumable>().UseItem();
    }

     private IEnumerator HandleTextOutput(string line, bool isLastLine)
    {
        _textBox.enabled = true;
        _textCoroutine = StartCoroutine(DialogueUtil.DisplayTextOverTime(line, _textBox, _promptSkip, _promptContinue));

        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitUntil.html
        yield return new WaitUntil(() => line == _textBox.text);

        if (isLastLine)
        {          
            yield return new WaitForSeconds(GameConfig.TimeBeforeLevelLoad);
            _textBox.enabled = false;
            yield break;
        }

        // Einen Frame warten, damit Input nicht beide GetKeyDown-Events triggert
        yield return null;

        yield return StartCoroutine(DialogueUtil.WaitForContinue(_promptContinue));

        _textBox.enabled = false;
    }

    // Übergebenen Canvas und Skript an- oder ausschalten
    public void ToggleCanvas(Canvas canvas, bool isActive)
    {
        canvas.enabled = isActive;
        canvas.GetComponent<SelectionMenu>().enabled = isActive;
    }
}
