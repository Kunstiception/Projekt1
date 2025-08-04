// Because GetComponentInChildren is a lie
using System.Linq;
using UnityEngine;

public static class UnityUtil
{
    // Wird beim Setzen der Health- und Egobars benötigt
    // Basiert auf:
    //https://discussions.unity.com/t/why-is-getcomponentsinchildren-returning-parents-component-aswell/637987/12
    public static T GetFirstComponentInChildren<T>(this GameObject parentObject) where T : Component
    {
        var components = parentObject.GetComponentsInChildren<T>(includeInactive: true);

        return components.FirstOrDefault(childComponent =>
            childComponent.transform != parentObject.transform);
    }
}
