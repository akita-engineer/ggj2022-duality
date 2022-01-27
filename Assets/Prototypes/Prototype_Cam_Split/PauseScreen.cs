using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject pauseScreen = default;

    private void Awake()
    {
        pauseScreen.SetActive(false);
        Time.timeScale = 1.0f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseScreen.activeSelf)
            {
                pauseScreen.SetActive(false);
                Time.timeScale = 1.0f;

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                pauseScreen.SetActive(true);
                Time.timeScale = 0.0f;

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        } 
    }
}
