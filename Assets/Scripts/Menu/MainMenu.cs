using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public void StartGame()
    {
        //Debug.Log("You pressed the play button");
        SceneManager.LoadScene("GameScene");
    }

    public void Quit()
    {
        // Quits the application, doesn't work in the unity environment
        // Prints the text for indication
        //Debug.Log("You pressed the quit button");
        Application.Quit();
    }
}
