using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BattleSystem battleSystem;
    [SerializeField] private int BuildID;
  
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(BuildID);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            FreezeTime();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            ResetTime();
        }

    }
    public void TriggerBattle(Unit player, Unit enemy)
    {
        battleSystem.StartBattle(player,enemy);
    }

    public void FreezeTime()
    {
        Time.timeScale = 0;
    }

    public void ResetTime()
    {
        Time.timeScale = 1;
    }
}
