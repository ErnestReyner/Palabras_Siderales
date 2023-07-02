using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreElement : MonoBehaviour
{

    public TMP_Text usernameText;
    public TMP_Text scoreText;
    public TMP_Text currentLevelText;

    public void NewScoreElement (string _username, int _score, string _currentLevel)
    {
        usernameText.text = _username;
        scoreText.text = _score.ToString();
        currentLevelText.text = _currentLevel.ToString();
    }

}
