using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecuteAnimation : MonoBehaviour
{
    public string animationName;
    public Animator m_animation;

    public void ExecuteAnim()
    {
        m_animation.enabled = true;
    }
}
