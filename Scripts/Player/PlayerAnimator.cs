using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Invoked method: SwipeListener
    ///<summary>
    ///Changes the value of boolean variables.
    ///</summary>
    public void ChangeBool(string boolName)
    {
        bool boolValue = !animator.GetBool(boolName);
        animator.SetBool(boolName, boolValue);
    }
}
