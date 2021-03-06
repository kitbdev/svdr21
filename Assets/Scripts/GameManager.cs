using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

public class GameManager : Singleton<GameManager>
{


    public void StartNewLevel()
    {
        // LevelGen.Instance
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
