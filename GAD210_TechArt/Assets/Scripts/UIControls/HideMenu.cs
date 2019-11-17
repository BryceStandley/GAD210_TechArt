using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class HideMenu : MonoBehaviour
{
    public GameObject menuUI;

    private PlayerSettingsUI _playerSettings;

    private void Awake() 
    {
        _playerSettings = FindObjectOfType<PlayerSettingsUI>();
    }

    public void ToggleMenu()
    {
        if(menuUI.activeSelf)
        {
            menuUI.SetActive(false);
            if(_playerSettings.fpMode)
            {
                _playerSettings.player.GetComponent<RigidbodyFirstPersonController>().enabled = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        else if(!menuUI.activeSelf)
        {
            menuUI.SetActive(true);
            if(_playerSettings.fpMode)
            {
                _playerSettings.player.GetComponent<RigidbodyFirstPersonController>().enabled = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    private void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void QuitApp()
    {
        Application.Quit();
    }
}
