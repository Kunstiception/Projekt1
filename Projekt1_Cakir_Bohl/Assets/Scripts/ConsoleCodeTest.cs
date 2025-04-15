using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class ConsoleCodeTest : MonoBehaviour
{
    //int combatantHealth1 = 0;
    //string combatant1 = "";

    //int combatantHealth2 = 0;
    //string combatant2 = "";


    //int maxPlayerHealth = 10;
    //int currentPlayerHealth = 10;

    //int playerInitiative = 5;

    //int maxEnemyHealth = 6;
    //int enemyHealth = 6;

    //int enemyInitiative = 2;

    //System.Random random = new System.Random();

    //int damage = 0;

    //bool isMoving = true;

    //bool isFighting = false;

    //bool isRunning = true;

    //bool isResting = false;

    //bool isAmbushed = false;

    //int totalMovement = 0;

    //string? readResult;

    //string[] possibleItems = ["Health Potion", "Ring of Life", "Ring of Protection"];

    //string[] currentItems = new string[10];

    //// Only run through loops while the game is running
    //while(isRunning)
    //{
    //    // Movement loop
    //    while (isMoving && totalMovement< 10)
    //    {
    //        int movement;
    //        isMoving = true;
    //        movement = random.Next(1, 4);
        
    //        int encounter;
    //        if (!isAmbushed)
    //        {
    //            encounter = random.Next(1, 6);
    //        }
    //        else
    //        {
    //            encounter = 4;
    //        }

    //    // Possible things to encounter on the quest
    //    switch (encounter)
    //    {
    //        // Just movement
    //        case 1:
    //        case 2:
    //        case 3:
    //            totalMovement = totalMovement + movement;
    //            isMoving = true;
    //            break;
    //        // Triggers a fight
    //        case 4:
    //            totalMovement += movement;
    //            Console.WriteLine($"The hero has travelled for {totalMovement} hours before encountering an enemy!");
    //            totalMovement = 0;
    //            isMoving = false;
    //            isResting = false;
    //            isFighting = true;

    //            int playerThrow = playerInitiative + new Random().Next(1, 11);
    //            int enemyThrow = enemyInitiative + new Random().Next(1, 11);
    //            if (playerThrow >= enemyThrow)
    //            {
    //                combatantHealth1 = currentPlayerHealth;
    //                combatantHealth2 = enemyHealth;
    //                combatant1 = "Hero";
    //                combatant2 = "Monster";
    //                Console.WriteLine("The hero strikes first!");
    //            }
    //            else
    //            {
    //                combatantHealth1 = enemyHealth;
    //                combatantHealth2 = currentPlayerHealth;
    //                combatant1 = "Monster";
    //                combatant2 = "Hero";
    //                Console.WriteLine("The monster strikes first!");
    //                isAmbushed = false;
    //            }

    //            break;
    //        // Loot was found
    //        case 5:
    //            totalMovement += movement;
    //            int randomItem = random.Next(0, 3);
    //            Console.WriteLine($"The hero has found {possibleItems[randomItem]} after travelling for {totalMovement} hours!");

    //            for (int i = 0; i <= currentItems.Length; i++)
    //            {
    //                if (currentItems[i] == null)
    //                {
    //                    currentItems[i] = possibleItems[randomItem];
    //                    break;
    //                }
    //                if (i >= currentItems.Length)
    //                {
    //                    Console.WriteLine($"Unfortunately the hero already carries as much weight as he can carry.");
    //                }
    //            }


    //            totalMovement = 0;
    //            isMoving = true;
    //            totalMovement = 0;
    //            break;
    //    }

        
    // }

    //        if (totalMovement >= 10)
    //    {
    //        isMoving = false;
    //        Console.WriteLine("The hero is exhausted from his travels. He needs to rest.");
    //        totalMovement = 0;
    //        isResting = true;
    //    }

    //    // Resting loop
    //    while (isResting)
    //    {
    //        Console.WriteLine("The hero is now resting.");
    //        Console.WriteLine("What should he do?");
    //        Console.WriteLine("\t1. Sleep until he is fully recovered (the hero could be ambushed during his sleep).");
    //        Console.WriteLine("\t2. Look at items.");
    //        Console.WriteLine("\t3. Continue his quest.");

    //        // Give player the decision on how to proceed
    //        do
    //        {
    //            readResult = Console.ReadLine().ToLower();
    //            if (readResult != null)
    //            {
    //                if (readResult != "1" && readResult != "2" && readResult != "3")
    //                {
    //                    Console.WriteLine("Invalid input. Use (1/2).");
    //                    isMoving = false;
    //                }
    //                switch (readResult)
    //                {
    //                    // Sleeping
    //                    case "1":
    //                        Console.WriteLine("The hero is falling asleep.");
    //                        int encounter;
    //                        encounter = random.Next(1, 11);
    //                        if (encounter >= 6)
    //                        {
    //                            if (currentPlayerHealth < maxPlayerHealth)
    //                            {
    //                                int heal;
    //                                heal = random.Next(1, maxPlayerHealth - currentPlayerHealth);
    //                                Console.WriteLine($"The hero has recovered {heal} health.");
    //                                currentPlayerHealth += heal;
    //                                Console.WriteLine("The hero is being ambushed!");
    //                                Console.WriteLine($"The enemy strikes first!");
    //                                isResting = false;
    //                                isAmbushed = true;
    //                                isFighting = true;
    //                            }
    //                            else
    //                            {
    //                                Console.WriteLine($"The hero is already at full health.");
    //                            }
    //                        }
    //                        else
    //                        {
    //                            currentPlayerHealth = maxPlayerHealth;
    //                            Console.WriteLine("The hero has slept through the night and is now fully recovered!");
    //                            isResting = true;
    //                        }
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
    //                }
    //            }
    //        } while (readResult != "1" && readResult != "2" && readResult != "3");
    //    }

    //    if (isFighting)
    //    {
    //        // Fighting loop
    //        while (isFighting && combatantHealth1 > 0 && combatantHealth2 > 0)
    //        {
    //            if (isAmbushed)
    //            {
    //                combatantHealth1 = enemyHealth;
    //                combatantHealth2 = currentPlayerHealth;
    //                combatant1 = "Monster";
    //                combatant2 = "Hero";
    //                Console.WriteLine("The monster strikes first!");
    //                isAmbushed = false;
    //            }
    //            damage = random.Next(1, 11);
    //            if (damage >= combatantHealth2)
    //            {
    //                combatantHealth2 = 0;

    //            }
    //            else
    //            {
    //                combatantHealth2 -= damage;
    //            }
    //            Console.WriteLine($"{combatant2} was damaged and lost {damage} health and now has {combatantHealth2} health!");
    //            if (combatantHealth2 <= 0) continue;

    //            damage = random.Next(1, 11);
    //            if (damage >= combatantHealth1)
    //            {
    //                combatantHealth1 = 0;

    //            }
    //            else
    //            {
    //                combatantHealth1 -= damage;
    //            }
    //            Console.WriteLine($"{combatant1} was damaged and lost {damage} health and now has {combatantHealth1} health!");
    //        }

    //        // What to do when the fight has ended?
    //        // Reassign health
    //        if (combatant1 == "Hero")
    //        {
    //            currentPlayerHealth = combatantHealth1;
    //            enemyHealth = combatantHealth2;
    //        }
    //        else
    //        {
    //            enemyHealth = combatantHealth1;
    //            currentPlayerHealth = combatantHealth2;
    //        }
    //        Console.WriteLine(currentPlayerHealth > enemyHealth ? "Hero wins!" : "Monster wins!");

    //        if (currentPlayerHealth > enemyHealth)
    //        {
    //            // Give player the decision on how to proceed
    //            do
    //            {
    //                enemyHealth = 10;
    //                Console.WriteLine($"The hero has won the battle with {currentPlayerHealth} health remaining. Continue the quest? (1) or rest up? (2)");
    //                readResult = Console.ReadLine().ToLower();
    //                if (readResult != null)
    //                {
    //                    if (readResult != "1" && readResult != "2")
    //                    {
    //                        Console.WriteLine("Invalid input. Use (1/2).");
    //                        isMoving = false;
    //                    }
    //                    else
    //                    {
    //                        // Continue quest
    //                        if (readResult == "1")
    //                        {
    //                            isMoving = true;
    //                        }
    //                        else
    //                        {
    //                            isMoving = false;
    //                            isResting = true;
    //                        }
    //                    }
    //                }
    //                enemyHealth = maxEnemyHealth;
    //            } while (readResult != "1" && readResult != "2");
    //        }
    //        // End the game
    //        else
    //        {
    //            Console.WriteLine("The hero has died. His quest has ended.");
    //            isRunning = false;
    //        }
    //    }

    //    if (currentPlayerHealth <= 0)
    //    {
    //        isRunning = false;
    //    }
    
    //}

}
