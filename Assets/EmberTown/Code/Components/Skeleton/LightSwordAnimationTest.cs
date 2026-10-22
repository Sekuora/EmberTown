using UnityEngine;

/// <summary>
/// Plays light sword animations with the weapon correction on the same timeline.
/// </summary>
[RequireComponent(typeof(Animator), typeof(MeshTargetSocket))]
public class LightSwordAnimationTest : MonoBehaviour
{
    // Properties
    [SerializeField]
    private AnimationClip[] animations;

    [SerializeField]
    private RuntimeAnimatorController animationController;

    [SerializeField]
    private RuntimeAnimatorController swordAnimationController;

    [SerializeField]
    private bool playAnimations;

    [SerializeField]
    private bool cycleAnimations = true;

    [SerializeField]
    private int selectedAnimationIndex;

    // References
    private Animator animator;
    private Animator swordAnimator;
    private MeshTargetSocket meshTargetSocket;

    private RuntimeAnimatorController originalController;
    private RuntimeAnimatorController originalSwordController;
    private AnimatorCullingMode originalCullingMode;
    private int currentAnimationIndex = -1;
    private bool applyRootMotion;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        meshTargetSocket = GetComponent<MeshTargetSocket>();
    }

    private void Update()
    {
        if (!playAnimations)
        {
            if (currentAnimationIndex >= 0) StopAnimations();
            return;
        }

        if (animations == null || animations.Length == 0) return;

        selectedAnimationIndex = Mathf.Clamp(selectedAnimationIndex, 0, animations.Length - 1);

        if (currentAnimationIndex != selectedAnimationIndex)
        {
            PlayAnimation(selectedAnimationIndex);
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            int nextIndex = cycleAnimations ? (currentAnimationIndex + 1) % animations.Length : currentAnimationIndex;
            PlayAnimation(nextIndex);
        }
    }

    public void PlayAnimation(int inIndex)
    {
        if (!Application.isPlaying || animations == null || inIndex < 0 || inIndex >= animations.Length) return;
        if (animations[inIndex] == null || animationController == null || swordAnimationController == null) return;

        if (swordAnimator == null)
        {
            GameObject sword = meshTargetSocket.EquippedRightHandMesh;
            if (sword == null) return;

            swordAnimator = sword.GetComponent<Animator>();
            if (swordAnimator == null) return;
        }

        if (currentAnimationIndex < 0)
        {
            originalController = animator.runtimeAnimatorController;
            originalSwordController = swordAnimator.runtimeAnimatorController;
            originalCullingMode = animator.cullingMode;
            applyRootMotion = animator.applyRootMotion;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.runtimeAnimatorController = animationController;
            swordAnimator.runtimeAnimatorController = swordAnimationController;
        }

        selectedAnimationIndex = inIndex;
        currentAnimationIndex = inIndex;
        playAnimations = true;

        animator.Play(animations[inIndex].name, 0, 0f);
        string swordState = animations[inIndex].name == "Anim_Light_Sword_Attack_02" ? "Anim_Light_Sword_Attack_Rotation" : "Rest";
        swordAnimator.Play(swordState, 0, 0f);
        animator.Update(0f);
        swordAnimator.Update(0f);
    }

    [ContextMenu("Play Light Sword Animations")]
    public void PlayAnimations()
    {
        playAnimations = true;
    }

    [ContextMenu("Stop Light Sword Animations")]
    public void StopAnimations()
    {
        playAnimations = false;
        if (currentAnimationIndex < 0) return;

        swordAnimator.Play("Rest", 0, 0f);
        swordAnimator.Update(0f);
        swordAnimator.runtimeAnimatorController = originalSwordController;
        animator.runtimeAnimatorController = originalController;
        animator.applyRootMotion = applyRootMotion;
        animator.cullingMode = originalCullingMode;
        currentAnimationIndex = -1;
    }

    private void OnDisable()
    {
        StopAnimations();
    }
}
