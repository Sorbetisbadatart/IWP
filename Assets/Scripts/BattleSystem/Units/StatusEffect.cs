using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Status Effect", menuName = "Battle/Status Effect")]
public class StatusEffect : ScriptableObject
{
    public StatusEffectType effectType;
    public string displayName;
    public Sprite icon;
    public Color color = Color.white;

    [Header("Effect Properties")]
    public int power = 10; // Damage/heal amount or percentage
    public int duration = 3;
    public bool isBuff = false;
    public bool isDebuff = false;
    public bool canStack = false;
    public int maxStacks = 1;

    [Header("Timing")]
    public bool applyOnTurnStart = true;
    public bool applyOnTurnEnd = false;
    public bool applyOnAttack = false;
    public bool applyOnDefend = false;

    [Header("Visual Effects")]
    public GameObject visualEffect;
    public ParticleSystem particles;
    public AudioClip applySound;
    public AudioClip tickSound;

    [TextArea(3, 5)]
    public string description;
}

public enum StatusEffectType
{
    POISON,      // Damage over time
    BURN,        // Damage over time + attack reduction
    FREEZE,      // Skip turns
    PARALYZE,    // Chance to skip turn
    SLEEP,       // Skip turns until hit
    CONFUSE,     // Chance to hit self
    BUFF_ATTACK,
    BUFF_DEFENSE,
    BUFF_SPEED,
    DEBUFF_ATTACK,
    DEBUFF_DEFENSE,
    DEBUFF_SPEED,
    REGENERATE,  // Heal over time
    SHIELD,      // Damage absorption
    STUN,
    SILENCE      // Cannot use skills
}

// Class to track active status effects on a unit
[System.Serializable]
public class ActiveStatus
{
    public StatusEffect effect;
    public int remainingTurns;
    public int stacks;

    public ActiveStatus(StatusEffect effect, int duration)
    {
        this.effect = effect;
        this.remainingTurns = duration;
        this.stacks = 1;
    }
}