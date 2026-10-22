using System.Collections.Generic;
using UnityEngine;



public class Dash : Ability
{

    [Space(10)]
    [Header("Dash Settings")]

    [SerializeField]
    private float dashDistance;

    [SerializeField]
    private float dashTime;

    [SerializeField]
    private float jumpInterruptWindow = 0.15f;


    private float dashFriction;

    private float dashImpulse;

    private Vector3 dashDirection;

    private float currentDashVelocity;

    private float currentDashTime;

    protected override void InitializeConditions()
    {
        if (Conditions == null)
        {
            Conditions = new List<AbilityCondition>();
        }

        // Conditions.Add(new AirborneCondition());

        if (!InterruptorAbilities.Contains(typeof(Jump).Name))
        {
            InterruptorAbilities.Add(typeof(Jump).Name);
        }

        if (!InterruptorAbilities.Contains(typeof(WallSlide).Name))
        {
            InterruptorAbilities.Add(typeof(WallSlide).Name);
        }
    }


    private void Start()
    {


        dashFriction = 2 * dashDistance / Mathf.Pow(dashTime, 2);

        dashImpulse = dashFriction * dashTime;


    }

    private void Update()
    {
        if (input.DashPressed)
        {
            abilityController.TryUseAbility(this);
        }

        if (AbilityState != State.Active)
        {
            animator.SetBool("Dash", false);
            return;
        }

        currentDashTime += Time.deltaTime;

        if (currentDashVelocity > 0)
        {
            animator.SetBool("Dash", true);
            currentDashVelocity -= dashFriction * Time.deltaTime;
            rData.HorizontalDirection = dashDirection * currentDashVelocity;
        }

        // Return defaults when dash end
        if (currentDashVelocity <= 0)
        {
            currentDashVelocity = 0;



            rData.VerticalDirection = data.Gravity;

            StopAbility();
        }
    }

    protected override void UseAbility()
    {
        Effects.Add(new Effect(EffectsDB.BlockMovement, true));

        rData.VerticalDirection = 0f;

        currentDashVelocity = dashImpulse;

        currentDashTime = 0f;

        dashDirection = transform.forward;


    }

    public override bool CanBeInterruptedBy(Ability inAbility)
    {
        if (!base.CanBeInterruptedBy(inAbility)) { return false; }

        if (inAbility is Jump)
        {
            return currentDashTime >= Mathf.Max(0f, dashTime - jumpInterruptWindow);
        }

        return true;
    }

    protected override void WhenInterrupted()
    {
        currentDashVelocity = 0f;


    }


}
