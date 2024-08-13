using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FighterMoveset
{
    public enum FaceDirection
    {
        RIGHT,
        LEFT
    }

    public FighterController fighterController;
    private Animator animator;
    public FighterStats fighterStats;
    private string currentAnimationName;
    private bool loopAnimation;

    public HashSet<string> states { get; private set; }
    public Dictionary<string, bool> flags { get; private set; }
    public Dictionary<string, InputActionWrapper> inputActions { get; private set; }
    public List<GeneralAnimationCommandWrapper> generalAcmds { get; private set; }
    public Dictionary<string, ACMD> acmds { get; private set; }

    public FaceDirection faceDirection { get; private set; }

    private ACMD currentAcmd;
    public string currentAcmdName { get; private set; }
    public string currentState { get; private set; }

    [SerializeField] private List<AttackHitbox> activeAttackHitboxes;
    private List<AttackHitbox> expiredHitboxes;

    public int frameCounter { get; private set; } = 0;
    public int targetFrame { get; private set; } = 0;

    [SerializeField] public int airJumpsCount = 0;

    public FighterMoveset(FighterController fighterController, Animator animator, FighterStats fighterStats, Dictionary<string, ACMD> acmds,
        List<GeneralAnimationCommandWrapper> generalAcmds, HashSet<string> states,
        Dictionary<string, bool> flags, Dictionary<string, InputActionWrapper> inputActions, FaceDirection lookDirection)
    {
        this.fighterController = fighterController;
        this.animator = animator;
        this.fighterStats = fighterStats;

        this.acmds = acmds;
        this.states = states;
        this.flags = flags;
        this.inputActions = inputActions;
        this.generalAcmds = generalAcmds;

        this.faceDirection = lookDirection;

        this.currentAcmdName = "STANDING_ACMD";
        this.currentAcmd = acmds["STANDING_ACMD"];
        this.currentState = "STANDING_STATE";

        activeAttackHitboxes = new List<AttackHitbox>();
        PlayAnimation("Idle");
    }

    //private void AddDebugAcmd()
    //{
    //    if (!acmds.ContainsKey("print_error_move"))
    //
    //        if (!acmds.ContainsKey("error_acmd"))
    //            acmds.Add("error_acmd", fm => { });
    //
    //    if (!states.Contains("error_state"))
    //        states.Add("error_state");
    //}

    public void UpdateTick()
    {
        targetFrame = 0;
        fighterController.UpdateTick();

        activeAttackHitboxes.RemoveAll(hitbox =>
        {
            if (hitbox.endFrame <= frameCounter || hitbox.summoningState != currentState)
            {
                fighterController.DestroyHitbox(hitbox);
                return true;
            }
            return false;
        });

        foreach (var gacmd in generalAcmds)
        {
            gacmd.acmd(this);
        }

        if (frameCounter == 0)
            Debug.Log("(Re-)Starting ACMD: \"" + currentAcmdName + "\", Current State: " + currentState);

        currentAcmd(this);


        foreach (var pair in inputActions)
        {
            pair.Value.ResetInputStates();
        }
        frameCounter++;
    }

    public void setStickHoldingDown(bool isStickHoldingDown)
    {
        fighterController.isStickHoldingDown = isStickHoldingDown;
    }


    public void OnHit()
    {
        TransitionToState("FALLING_STATE");
        ForceTransitionToAcmd("FALLING_ACMD");
        PlayAnimation("AirIdle");
    }

    public void SetFaceDirection(FaceDirection fd)
    {
        faceDirection = fd;
        fighterController.UpdateLookDirection(faceDirection);
    }

    public void ForceTransitionToAcmd(string acmdName, bool resetFrameCounter = true)
    {
        if (acmds.ContainsKey(acmdName))
        {
            frameCounter = resetFrameCounter ? 0 : frameCounter;
            currentAcmdName = acmdName;
            currentAcmd = acmds.GetValueOrDefault(acmdName);
        }
        else
        {
            Debug.LogWarning("TransitionToMove(): ACMD with name \"" + acmdName + "\" not found. \n");
            currentAcmd = acmds.GetValueOrDefault("error_acmd");
        }
        Debug.Log("Force-transitioned to ACMD: " + currentAcmdName);
    }

    public void TransitionToAcmd(string acmdName, bool resetFrameCounter = true)
    {
        if (currentAcmdName == acmdName)
        {
            return;
        }
        if (acmds.ContainsKey(acmdName))
        {
            frameCounter = resetFrameCounter ? 0 : frameCounter;
            currentAcmdName = acmdName;
            currentAcmd = acmds.GetValueOrDefault(acmdName);
        }
        else
        {
            Debug.LogWarning("TransitionToMove(): ACMD with name \"" + acmdName + "\" not found. \n");
            currentAcmd = acmds.GetValueOrDefault("error_acmd");
        }
        //Debug.Log("Transitioned to ACMD: " + currentAcmdName);
    }

    public bool IsCurrentState(string stateName)
    {
        if (!states.Contains(stateName))
        {
            Debug.LogWarning("IsCurrentState(): State with name \"" + stateName + "\" not found. \n");
            return false;
        }
        return currentState == stateName;
    }

    public void TransitionToState(string stateName)
    {
        if (!states.Contains(stateName))
        {
            Debug.LogWarning("TransitionToState(): State with name \"" + stateName + "\" not found. \n");
            return;
        }
        if (currentState != stateName)
        {
            currentState = stateName;
        }
        //Debug.Log("Transitioned to State: " + currentState);
    }

    public void SetFlag(string flagName, bool value)
    {
        if (!flags.ContainsKey(flagName))
        {
            Debug.LogWarning("SetFlagValue(): Flag with name \"" + flagName + "\" not found. \n");
            return;
        }
        if (flags[flagName] != value)
        {
            flags[flagName] = value;
            Debug.Log("Set Flag " + flagName + " to value " + value);
        }
    }

    public bool IsFlagTrue(string flagName)
    {
        if (!flags.ContainsKey(flagName))
        {
            Debug.LogWarning("IsFlagSet(): Flag with name \"" + flagName + "\" not found. \n");
            return false;
        }
        return flags[flagName];
    }

    public bool IsFlagFalse(string flagName) => !IsFlagTrue(flagName);

    public bool OnFrame(int frame) => frame == frameCounter;

    public bool OnFrames(int firstFrame, int lastIncludedFrame) => frameCounter >= firstFrame && frameCounter <= lastIncludedFrame;

    public void PlayAnimation(string animationName, float speed = 1, bool loop = true)
    {
        Debug.Log("Playing Animation: \"" + animationName + "\", Speed: " + speed + ", Loop: " + (loop ? "True" : "False"));
        if (animator != null)
        {
            loopAnimation = true;
            animator.speed = speed;
            animator.SetBool("Loop", loop);
            animator.Play(animationName, 0, 0);
            currentAnimationName = animationName;
        }
        else
        {
            Debug.LogError("Animator ist nicht zugewiesen.");
        }
    }

    public float GetAnimationProgress()
    {
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // Layer 0

            float progress = stateInfo.normalizedTime >= 1 ? 1 : stateInfo.normalizedTime; // Verwenden Sie den Modulo-Operator, um Werte über 1 zu berücksichtigen
            return progress;
        }
        else
        {
            Debug.LogError("Animator ist nicht zugewiesen.");
            return 0; // Geben Sie zurück, dass die Animation nicht spielt und keinen Fortschritt hat
        }
    }

    public bool IsAnimationPlaying()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1; // Wenn normalizedTime kleiner als 1 ist, spielt die Animation noch
    }

    public void SetAnimationLoop(bool value) => loopAnimation = value;

    public void SetAnimationSpeed(float speed) => animator.speed = speed;

    public bool IsAnimationLooping() => loopAnimation;

    public string GetCurrentAnimationName() => currentAnimationName;

    public void CreateHitbox(int duration, float damage, float knockback, float baseKnockback, Vector2 direction, bool considerLookDirection, string boneName, float radius, Vector3 offset)
    {
        Transform bone = FindDeepChild(fighterController.gameObject.transform, boneName);
        AttackHitbox hitbox = fighterController.gameObject.AddComponent<AttackHitbox>();

        Vector3 direction3d = new Vector3(direction.x, direction.y, 0.0f).normalized;
        direction3d.x *= considerLookDirection && faceDirection == FaceDirection.RIGHT ? 1 : -1;

        hitbox.Initialize(fighterController.gameObject.GetComponent<Fighter>(), frameCounter, frameCounter + duration, damage, baseKnockback, knockback, direction3d, bone, radius, offset);

        activeAttackHitboxes.Add(hitbox);
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name)
        {
            return parent;
        }
        foreach (Transform child in parent)
        {
            Transform result = FindDeepChild(child, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

}