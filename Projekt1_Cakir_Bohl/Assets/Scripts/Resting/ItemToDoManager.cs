public class ItemToDoManager : SelectionMenu
{
    void Update()
    {
        ListenForInputs();
    }

    public override void HandleSelection(int menuPoint)
    {
        _iSelectable.HandleSelectedMenuPoint(menuPoint);
    }
}
