using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingManager : Manager
{
    [SerializeField] private GameObject[] _animations;

    void Start()
    {
        _textBox.enabled = false;
        _promptSkip.enabled = false;
        _promptContinue.enabled = false;

        foreach (GameObject animation in _animations)
        {
            animation.SetActive(false);
        }

        StartCoroutine(PlayEnding());
    }

    void Update()
    {

    }

    private IEnumerator PlayEnding()
    {
        for (int i = 0; i < _animations.Length; i++)
        {
            if (i > 0)
            {
                _animations[i - 1].SetActive(false);
            }

            _animations[i].SetActive(true);

            yield return new WaitForSeconds(GameConfig.AnimationTime);
        }
    }
}
