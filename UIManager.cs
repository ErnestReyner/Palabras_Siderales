using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    //Screen object variables
    public GameObject canvas;
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject userDataUI;
    public GameObject scoreboardUI;
    public GameObject disclaimerMenu;
    public GameObject selectLevelMenu;
    public GameObject AuthGO;

    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    //Functions to change the login screen UI
    public void ClearScreen() //Turn off all screens
    {
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        userDataUI.SetActive(false);
        scoreboardUI.SetActive(false);
        disclaimerMenu.SetActive(false);
        selectLevelMenu.SetActive(false);
    }

    public void LoadUserMenu() //Back button
    {
        ClearScreen();
        loginUI.SetActive(true);
        registerUI.SetActive(false);
    }
    public void RegisterUserMenu() // Regester button
    {
        ClearScreen();
        loginUI.SetActive(false);
        registerUI.SetActive(true);
    }

    public void UserDataScreen() //Logged in
    {
        ClearScreen();
        userDataUI.SetActive(true);
    }

    public void ScoreboardScreen() //Scoreboard button
    {
        ClearScreen();
        scoreboardUI.SetActive(true);
    }

    public void DisclaimerMenu ()
    {
        ClearScreen();
        disclaimerMenu.SetActive(true);
        
    }

    public void SelectLevelMenu ()
    {
        ClearScreen();
        selectLevelMenu.SetActive(true);
    }

}
