using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    Scene currentScene;
    float proceed;
    SerialReceiver receiver;

    void Start()
    {
        currentScene = SceneManager.GetActiveScene();
        receiver = GameObject.FindGameObjectWithTag("Communication").GetComponent<SerialReceiver>();
    }

    void Update()
    {
        string name = currentScene.name;
        proceed = CrossPlatformInputManager.GetAxis("Jump"); //in the text display scenes, press SPACE to go to the next scene
        if (name.Contains("Text"))
        {
            if(proceed > 0)
            {
                switch (name)
                {
                    case "WelcomeText":
                        SceneManager.LoadScene("Training");
                        break;
                    case "PredrivingText":
                        receiver.sceneChanged = true; //flag to mark that the training is over; new GSR file is created
                        SceneManager.LoadScene("Driving", LoadSceneMode.Single);
                        break;
                    case "EndText":
                        Application.Quit();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
