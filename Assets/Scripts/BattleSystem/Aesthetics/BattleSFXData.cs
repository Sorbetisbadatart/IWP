using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BattleSFXData", menuName = "Sound/Battle SFX Data")]
public class BattleSFXData : ScriptableObject
{
    [Header("Attack Sounds")]
    public AudioClip[] normalAttackSounds;
    public AudioClip[] strongAttackSounds;
    public AudioClip[] criticalHitSounds;
    public AudioClip[] missSounds;

    [Header("Damage Sounds")]
    public AudioClip[] lightDamageSounds;
    public AudioClip[] heavyDamageSounds;
    public AudioClip[] defenseDamageSounds;

    [Header("Defense Sounds")]
    public AudioClip defenseStartSound;
    public AudioClip defenseBlockSound;
    public AudioClip defenseBreakSound;

    [Header("Healing Sounds")]
    public AudioClip[] healSounds;
    public AudioClip[] potionSounds;

    [Header("Status Sounds")]
    public AudioClip[] victorySounds;
    public AudioClip[] defeatSounds;
    public AudioClip[] levelUpSounds;

    [Header("UI Sounds")]
    public AudioClip buttonClickSound;
    public AudioClip buttonHoverSound;
    public AudioClip selectionSound;
    public AudioClip errorSound;

    [Header("Background Music")]
    public AudioClip battleMusic;
    public AudioClip victoryMusic;
    public AudioClip defeatMusic;

    // Helper methods to get random sounds
    public AudioClip GetRandomNormalAttack() => GetRandomClip(normalAttackSounds);
    public AudioClip GetRandomHeal() => GetRandomClip(healSounds);
    public AudioClip GetRandomDamage(bool heavy = false) =>
        GetRandomClip(heavy ? heavyDamageSounds : lightDamageSounds);

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}