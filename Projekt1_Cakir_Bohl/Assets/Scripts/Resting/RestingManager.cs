using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class RestingManager : MonoBehaviour, ISelectable
{
    [SerializeField] private TextMeshProUGUI _promptSkip;
    [SerializeField] private TextMeshProUGUI _promptContinue;
    [SerializeField] private TextMeshProUGUI _textBox;
    [SerializeField] private Canvas _selectionMenuCanvas;
    private Coroutine _textCoroutine;
    private string _currentLine;
    private bool _isAmbush;

    void Start()
    {
        _textBox.enabled = false;
    }

    void OnDisable()
    {
        StopAllCoroutines();
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
    public void HandleSelectedItem(int index, bool isFirstLayer)
    {
        ToggleCanvas(_selectionMenuCanvas, false);
        
        // 0 = sleep, 1 = show items, 2 = continue quest
        switch(index)
        {
            case 0:
                _isAmbush = DecideIfAmbush();

                // Play corresponding animation

                StartCoroutine(SleepingCoroutine(_isAmbush));

                break;
        }


    // Play Anim and wait for return
    // If not Ambush: Heal Player fully
    // If is Ambush: LoadCombatScene and set PlayerManager.isDisadvantage = true

    // Sleeping
    //                    case "1":
    //                        Console.WriteLine("The hero is falling asleep.");
    //                        int encounter;
    //                        break;

    //                    // Show items
    //                    case "2":
    //                        string itemNumber;
    //                        do
    //                        {
    //                            bool hasCollectedItems = false;
    //                            int number = 1;
    //                            foreach (string item in currentItems)
    //                            {
    //                                if (item != null)
    //                                {
    //                                    Console.WriteLine($"{number}.\t{item}");
    //                                    number++;
    //                                    hasCollectedItems = true;
    //                                }

    //                            }
    //                            if (hasCollectedItems)
    //                            {
    //                                Console.WriteLine("Enter the number of the item you want to interact with (or press Esc if you do not wish to interact with any item).");
    //                            }
    //                            else
    //                            {
    //                                Console.WriteLine("The hero carries no items at the moment.");
    //                                break;
    //                            }

    //                            itemNumber = Console.ReadLine().ToLower();
    //                            var cancelKey = ConsoleKey.Escape;
    //                            if (Console.ReadKey().Key != cancelKey)
    //                            {
    //                                if (itemNumber != null && hasCollectedItems)
    //                                {
    //                                    itemNumber = itemNumber.Trim();
    //                                    Console.WriteLine($"You chose {currentItems[Convert.ToInt32(itemNumber) - 1]}.");
    //                                    // List of interactions with items
    //                                    switch (currentItems[Convert.ToInt32(itemNumber) - 1])
    //                                    {
    //                                        case "Health Potion":
    //                                            int healingValue = random.Next(3, 8);
    //                                            if (currentPlayerHealth != maxPlayerHealth)
    //                                            {
    //                                                if (healingValue > (maxPlayerHealth - currentPlayerHealth))
    //                                                {
    //                                                    healingValue = (maxPlayerHealth - currentPlayerHealth);
    //                                                }
    //                                                currentPlayerHealth += healingValue;
    //                                                Array.Clear(currentItems, Convert.ToInt32(itemNumber) - 1, 1);
    //                                                Console.WriteLine($"The hero drinks the potion and gains {healingValue} lifepoints.");

    //                                            }
    //                                            else
    //                                            {
    //                                                Console.WriteLine($"The hero is already at full health.");
    //                                            }

    //                                            break;
    //                                    }
    //                                }
    //                            }
    //                            else
    //                            {
    //                                isResting = true;
    //                                break;
    //                            }

    //                        } while (itemNumber == null);
    //                        break;

    //                    case "3":
    //                        Console.WriteLine("The quest continues!");
    //                        isResting = false;
    //                        isMoving = true;
    //                        isFighting = false;
    //                        break;
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
                Random.Range(1, GameConfig.PlayerStartingHealth - PlayerManager.Instance.HealthPoints) : 0;
            

            int egoHeal;
            egoHeal = PlayerManager.Instance.EgoPoints < GameConfig.PlayerStartingEgo ?
                Random.Range(1, GameConfig.PlayerStartingEgo - PlayerManager.Instance.EgoPoints) : 0;

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
        ToggleCanvas(_selectionMenuCanvas, true);
    }

     private IEnumerator HandleTextOutput(string line, bool isLastLine)
    {
        _textBox.enabled = true;
        _textCoroutine = StartCoroutine(DialogueUtil.DisplayTextOverTime(line, _textBox, _promptSkip, _promptContinue));

        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WaitUntil.html
        yield return new WaitUntil(() => _currentLine == _textBox.text);

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
