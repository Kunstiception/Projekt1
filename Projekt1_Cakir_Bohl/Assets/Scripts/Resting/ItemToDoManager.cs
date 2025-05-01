using UnityEngine;

public class ItemToDoManager : SelectionMenu
{
    [SerializeField] private RestingManager _restingManager;

    void Start()
    {
        SetInitialPointer();
    }

    void Update()
    {
        ListenForInputs();
    }

    public override void HandleSelection(int menuPoint)
    {
        _restingManager.HandleSelectedItemOrEquipment(menuPoint);
    }
}
