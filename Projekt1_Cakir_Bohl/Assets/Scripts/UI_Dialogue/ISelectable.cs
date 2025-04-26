using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface ISelectable
{
    void HandleSelectedMenuPoint(int index, bool isFirstLayer);

    void ToggleCanvas(Canvas canvas, bool isActive);
}
