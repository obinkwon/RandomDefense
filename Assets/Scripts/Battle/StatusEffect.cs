public enum StatusEffectType
{
    None,
    Slow,
    Poison,
    Stun
}

[System.Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public float duration;
    public float value;

    public StatusEffect(
        StatusEffectType type,
        float duration,
        float value
    )
    {
        this.type = type;
        this.duration = duration;
        this.value = value;
    }
}