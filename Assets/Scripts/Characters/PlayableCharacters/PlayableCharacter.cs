using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayableCharacter : ScriptableObject
{
    public string CharacterName;
    public string Title;
    public Sprite CharacterSprite;
    public int currentLevel;
    private readonly BattleStats stats;
}


