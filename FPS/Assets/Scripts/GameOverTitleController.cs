using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverTitleController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI TMPText;
    private static readonly int Win = Animator.StringToHash("win");

    public void ShowGameOverTitle(string text = null)
    {
        if (!string.IsNullOrEmpty(text))
        {
            TMPText.text = text;
        }
        
        animator.SetTrigger(Win);
    }
}
