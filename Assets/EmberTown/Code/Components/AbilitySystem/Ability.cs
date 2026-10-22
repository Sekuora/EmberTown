using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EntityDataBridge), typeof(InputDataBridge), typeof(RuntimeDataBridge))]
[RequireComponent(typeof(AbilityController))]

public abstract class Ability : MonoBehaviour
{


    public enum State
    {
        None,
        Active,
        Stopped
    }

    [SerializeReference]
    public List<AbilityCondition> Conditions = new List<AbilityCondition>();

    [SerializeField]
    public List<string> InterruptorAbilities = new List<string>();

    public List<Effect> Effects { get; private set; } = new List<Effect>();

    // Required Components
    protected EntityDataBridge data;

    protected RuntimeDataBridge rData;

    protected InputDataBridge input;

    protected CharacterController controller;

    protected Animator animator;

    protected AbilityController abilityController;

    protected Ability interrupter;

    // Ability
    protected int Level;

    [SerializeField]
    private string id;

    protected Timer cooldownTimer;

    protected Timer durationTimer;

    public Faction Faction { get; private set; }

    public State AbilityState { get; private set; } = State.None;

    public string Id { get => string.IsNullOrEmpty(id) ? GetType().Name : id; }

    private void Awake()
    {
        data = GetComponent<EntityDataBridge>();

        input = GetComponent<InputDataBridge>();

        controller = GetComponent<CharacterController>();

        animator = GetComponent<Animator>();

        rData = GetComponent<RuntimeDataBridge>();

        abilityController = GetComponent<AbilityController>();

        if (!abilityController)
        {
            abilityController = gameObject.AddComponent<AbilityController>();
        }

        if (InterruptorAbilities == null)
        {
            InterruptorAbilities = new List<string>();
        }

        if (!data || !input)
        {
            Debug.Log("Components Missing in Ability: " + name);
        }

        InitializeConditions();
    }

    // Final Check for each condition usability.
    public virtual bool CheckCanUse()
    {
        if (Conditions == null || Conditions.Count == 0) return true;
        if (!rData) rData = GetComponent<RuntimeDataBridge>();

        foreach (AbilityCondition condition in Conditions)
        {
            if (!condition.CanUse(rData))
            {
                return false;
            }
        }

        return true;
    }

    public virtual bool CanBeInterruptedBy(Ability inAbility)
    {
        if (!inAbility) { return false; }

        return InterruptorAbilities.Contains(inAbility.Id);
    }

    public bool HasEffect(EffectsDB inId)
    {
        return GetEffect(inId) != null;
    }

    public Effect GetEffect(EffectsDB inId)
    {
        foreach (Effect effect in Effects)
        {
            if (effect.Id == inId)
            {
                return effect;
            }
        }

        return default;
    }

    public T GetEffectValue<T>(EffectsDB inId)
    {
        Effect effect = GetEffect(inId);

        if (effect == null) { return default; }

        return effect.GetValue<T>();
    }

    internal void Interrupt(Ability inAbility = null)
    {
        interrupter = inAbility;

        AbilityState = State.Stopped;

        WhenInterrupted();

        Effects.Clear();
    }

    internal void Activate()
    {
        Effects.Clear();

        AbilityState = State.Active;

        UseAbility();
    }

    internal void Stop()
    {
        AbilityState = State.None;

        Effects.Clear();
    }

    protected void StopAbility()
    {
        abilityController.StopAbility(this);
    }

    protected abstract void InitializeConditions();

    protected abstract void UseAbility();

    protected abstract void WhenInterrupted();

}
