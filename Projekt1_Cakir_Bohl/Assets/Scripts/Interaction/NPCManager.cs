using TMPro;
using UnityEngine;

public class NPCManager : Manager, ISelectable
{
    [SerializeField] private GameObject[] _npcs;
    [SerializeField] private Canvas _dialogueCanvas;
    private string[] _playerQuestions;
    private string[] _npcAnswers;
    private string[] _currentPlayerQuestions;

    void Start()
    {
        TextMeshProUGUI[] options = _dialogueCanvas.GetComponentsInChildren<TextMeshProUGUI>();       
    }

    void Update()
    {
        
    }

    private void SetQuestionsAndAnswers()
    {

    }

    public void HandleSelectedMenuPoint(int index)
    {
        throw new System.NotImplementedException();
    }
}
