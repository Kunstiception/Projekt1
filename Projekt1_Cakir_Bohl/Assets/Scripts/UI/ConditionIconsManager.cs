using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConditionIconsManager : MonoBehaviour
{
    [SerializeField] private Sprite _sleepDeprivedIcon;
    [SerializeField] private Sprite _vampireIcon;
    [SerializeField] private Sprite _zombieIcon;
    [SerializeField] private Sprite _werewolfIcon;
    [SerializeField] private Transform[] _iconAnchors;
    private List<Transform> _tempIconAnchors = new List<Transform>();

    void Start()
    {
        foreach (Transform transform in _iconAnchors)
        {
            _tempIconAnchors.Add(transform);
        }

        SetIcons();
    }

    private void SetIcons()
    {
        var _conditions = ConditionManager.Instance.GetCurrentConditions();

        for (int i = 0; i < _conditions.Count; i++)
        {
            Sprite iconSprite = null;

            switch (_conditions[i])
            {
                case ConditionManager.Conditions.SleepDeprived:
                    iconSprite = _sleepDeprivedIcon;

                    break;

                case ConditionManager.Conditions.Vampire:
                    iconSprite = _vampireIcon;

                    break;

                case ConditionManager.Conditions.Zombie:
                    iconSprite = _zombieIcon;

                    break;

                case ConditionManager.Conditions.Werewolf:
                    iconSprite = _werewolfIcon;

                    break;

                default:
                    Debug.LogError("No icon selected. Check assigned item icons");

                    break;
            }

            Instantiate(iconSprite, _tempIconAnchors[0]);

            _tempIconAnchors.RemoveAt(0);
        }

        if (_tempIconAnchors.Count > 0)
        {
            foreach (Transform transform in _tempIconAnchors)
            {
                transform.GetComponent<Image>().enabled = false;
            }
        }
    }
}
