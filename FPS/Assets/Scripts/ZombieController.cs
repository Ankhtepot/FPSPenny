using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private static readonly int Walk = Animator.StringToHash("Walk");
    private static readonly int Death = Animator.StringToHash("Death");
    private static readonly int Run = Animator.StringToHash("Run");
    private static readonly int Attack = Animator.StringToHash("Attack");

    void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            animator.SetBool(Walk, true);
        }
        else
        {
            animator.SetBool(Walk, false);
        }
        
        if (Input.GetKey(KeyCode.Alpha2))
        {
            animator.SetBool(Attack, true);
        }
        else
        {
            animator.SetBool(Attack, false);
        }
        
        if (Input.GetKey(KeyCode.Alpha3))
        {
            animator.SetBool(Run, true);
        }
        else
        {
            animator.SetBool(Run, false);
        }
        
        if (Input.GetKey(KeyCode.Alpha4))
        {
            animator.SetBool(Death, true);
        }
        else
        {
            animator.SetBool(Death, false);
        }
    }
}
