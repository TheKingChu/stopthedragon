using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public Animator animator;

    public void TriggerShake()
    {
        animator.SetTrigger("Shake");
    }

    public void TriggerEventShake()
    {
        animator.SetTrigger("EventShake");
    }
}
