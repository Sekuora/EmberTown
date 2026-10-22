using UnityEngine;

public class AbilityController : MonoBehaviour
{

    public Ability ActiveAbility { get; private set; }


    public bool HasEffect(EffectsDB inId)
    {
        if (!ActiveAbility) { return false; }

        return ActiveAbility.HasEffect(inId);
    }

    public T GetEffectValue<T>(EffectsDB inId)
    {
        if (!ActiveAbility) { return default; }

        return ActiveAbility.GetEffectValue<T>(inId);
    }


    public bool TryUseAbility(Ability ability)
    {
        if (!ability || !ability.CheckCanUse()) { return false; }

        if (ActiveAbility == ability && ability.AbilityState == Ability.State.Active) { return false; }

        if (ActiveAbility)
        {
            if (!ActiveAbility.CanBeInterruptedBy(ability)) { return false; }

            ActiveAbility.Interrupt(ability);
        }

        ActiveAbility = ability;

        ActiveAbility.Activate();

        return true;
    }

    public void StopAbility(Ability ability)
    {
        if (ActiveAbility != ability) { return; }

        ActiveAbility.Stop();

        ActiveAbility = null;
    }

}
