using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// basiert auf: https://www.youtube.com/watch?v=vR6H3mu_xD8&list=PLSR2vNOypvs4Pc72kaB_Y1v3AszNd-UuF
public class DialogueManager : Manager, ISelectable
{
    [SerializeField] private DialogueLines _initialOptions;
    [SerializeField] private DialogueTopic[] _topics;
    [SerializeField] private GameObject _menuOptions;

    private DialogueLines _currentTopic;

    void Start()
    {

    }

    void Update()
    {

    }

    private IEnumerator DialogueCoroutine()
    {
        //for (int i = 0; i < _topics.Inclu.Length; i++)
        {
            yield return PrintMultipleLines(_currentTopic.Lines);
            

        }
    }

    // if no next Lines given, return to Initial
    // if no options given, return to Initial

    public void HandleSelectedMenuPoint(int index)
    {

    }
}
