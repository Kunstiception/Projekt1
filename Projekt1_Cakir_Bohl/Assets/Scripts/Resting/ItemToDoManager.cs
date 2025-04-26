using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
