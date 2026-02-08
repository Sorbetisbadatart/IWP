using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTab : MonoBehaviour
{
    [SerializeField] private Unit playerUnit;

    [SerializeField] private TMP_Text CharacterName;
    [SerializeField] private TMP_Text ClassName;
    [SerializeField] private TMP_Text CharacterLevel;
    [SerializeField] private TMP_Text Exp_text;   

    [SerializeField] private Image CharacterPortrait;
    [SerializeField] private BarTracker HealthBar;
    [SerializeField] private BarTracker ManaBar;
    [SerializeField] private BarTracker EXPBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        CharacterName.text = playerUnit.name;
        //ClassName.text = playerUnit.;

        CharacterLevel.text = "lvl " + playerUnit.Level;
        Exp_text.text = $"EXP {playerUnit.CurrentExp}/{playerUnit.ExpToNextLevel}";

        CharacterPortrait.sprite = playerUnit.unitSprite;

        HealthBar.SetMax(playerUnit.MaxHealthPoints);
        HealthBar.SetHealthToValue(playerUnit.HealthPoints);

        ManaBar.SetMax(playerUnit.MaxManaPoints);
        ManaBar.SetHealthToValue(playerUnit.ManaPoints);

        EXPBar.SetMax(playerUnit.ExpToNextLevel);
        EXPBar.SetHealthToValue(playerUnit.CurrentExp);
    }
}
