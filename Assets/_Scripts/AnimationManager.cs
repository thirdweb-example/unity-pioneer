using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    public void SetTrigger(string triggerName)
    {
        _animator.SetTrigger(triggerName);
    }
}
