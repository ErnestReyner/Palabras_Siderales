using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 
using UnityEngine.UI; 
using KKSpeech; 
using System;


public class GM_categ_transporte : MonoBehaviour
{
    public GameObject correctSign;
    public GameObject incorrectSign;
    public List<GameObject> images; 
    public TextMeshProUGUI scoreText;
    int i = 0;
    int lengthImages;
    public bool isGameActive;
    public AudioSource source;
    public AudioSource backgroundMusic;
    public AudioClip correctSound;
    public AudioClip incorrectSound;
    public AudioClip gameOverSound;
    public AudioClip victorySound;
    public float timeLeft;
    private float timeUsed;
    public bool timerOn = false;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalTimeText;
    public Button startRecordingButton;
    public TextMeshProUGUI resultText; 
    public TextMeshProUGUI aire;
    public TextMeshProUGUI tierra;
    public TextMeshProUGUI agua;
    public Button menuOnPlayButton;
    public Button hintButton;
    public Button resumeButton;
    public Button musicOffButton;
    public TextMeshProUGUI hintText;
    [SerializeField] GameObject pauseMenu;
    public Button nextLevel;
    public TextMeshProUGUI successText;
    [SerializeField] GameObject successMenu;
    public GameObject gameManager_category;
    [SerializeField] GameObject rulesMenu;
    private int score;
    [SerializeField] GameObject tutorialMenu;
    public TextMeshProUGUI instructionsText;
    private List<string> instructions = new List<string>();
    public Username usernameSO;
    private string gameID = "Nivel 12";
    private string endDate;
    FirebaseManager firebaseManager;
    int wrongTimes = 0;
    Dictionary<string, object> wrongResultsData = new Dictionary<string, object>();
    private bool isMusicOff = false;
    [SerializeField] GameObject gameOverMenu;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;
    public Button menuButton;
    public Button quitButton;


    void Awake()
    {
        firebaseManager = gameManager_category.GetComponent<FirebaseManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        startRecordingButton.gameObject.SetActive(true);

        if (SpeechRecognizer.ExistsOnDevice())
        {
            SpeechRecognizerListener listener = GameObject.FindObjectOfType<SpeechRecognizerListener>();
            listener.onAuthorizationStatusFetched.AddListener(OnAuthorizationStatusFetched);
            listener.onAvailabilityChanged.AddListener(OnAvailabilityChange);
            listener.onErrorDuringRecording.AddListener(OnError);
            listener.onErrorOnStartRecording.AddListener(OnError);
            listener.onFinalResults.AddListener(OnFinalResult);
            listener.onPartialResults.AddListener(OnPartialResult);
            listener.onEndOfSpeech.AddListener(OnEndOfSpeech);
            SpeechRecognizer.RequestAccess();
            SpeechRecognizer.SetDetectionLanguage("es-ES");
        }
        else
        {
            resultText.text = "Sorry, but this device doesn't support speech recognition";
            startRecordingButton.enabled = false;
        }

        instructions.Add("Como aún no tengo mi cohete puedo usar algunos de estos transportes, ¿pero en qué situaciones es más adecuado uno u otro?");
        Tutorial(0);

        scoreText.gameObject.SetActive(true);
        lengthImages = images.Count;
        isGameActive = true;
        score = 0;
        timerOn = true;
        images[0].SetActive(true);
        UpdateScore(0);
        wrongResultsData.Clear();



    }

    // Update is called once per frame
    void Update()
    {
        if (timerOn)
        {
            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                UpdateTimer(timeLeft);
            }
            else
            {
                GameOver();
                Debug.Log("¡El tiempo se ha acabado!");
                timeLeft = 0;
                timerOn = false;
            }
        }

    }


    void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "Puntuación: " + score;

    }

    IEnumerator ShowAndHide(GameObject go, float delay)
    {
        go.SetActive(true);
        yield return new WaitForSeconds(delay);
        go.SetActive(false);
    }

    IEnumerator NextGO(GameObject go, float delay)
    {
        go.SetActive(false);
        yield return new WaitForSeconds(delay);
        go.SetActive(true);
    }

    public void Success()
    {
        endDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        timeUsed = 180 - timeLeft;
        UpdateTimer(timeUsed);
        successMenu.SetActive(true);
        startRecordingButton.gameObject.SetActive(false);
        isGameActive = false;
        timeLeft = 0;
        timerOn = false;
        backgroundMusic.Stop();
        source.Stop();
        source.PlayOneShot(victorySound);
        scoreText.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
        successText.gameObject.SetActive(true);
        finalScoreText.gameObject.SetActive(true);
        finalTimeText.gameObject.SetActive(true);
        finalScoreText.text = " * Puntuación final: " + score + "/10";
        finalTimeText.text = " * Tiempo utilizado: " + timerText.text;
        firebaseManager.WrongAnswers(gameID, wrongResultsData);
        StartCoroutine(firebaseManager.UpdateUser(gameID, timeUsed, score, endDate));
    }

    public void GameOver()
    {
        endDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        timeUsed = 180 - timeLeft;
        UpdateTimer(timeUsed);
        gameOverMenu.SetActive(true);
        images[i].SetActive(false);
        startRecordingButton.gameObject.SetActive(false);
        timeLeft = 0;
        timerOn = false;
        backgroundMusic.Stop();
        source.Stop();
        source.PlayOneShot(gameOverSound);
        scoreText.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
        aire.gameObject.SetActive(false);
        tierra.gameObject.SetActive(false);
        agua.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
        menuOnPlayButton.gameObject.SetActive(false);
        hintButton.gameObject.SetActive(false);
        musicOffButton.gameObject.SetActive(false);
        isGameActive = false;
        firebaseManager.WrongAnswers(gameID, wrongResultsData);
        StartCoroutine(firebaseManager.UpdateUser(gameID, timeUsed, score, endDate));
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void UpdateTimer(float currentTime)
    {
        currentTime += 1;
        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
    }

    public void ReturnLoadUserMenu()
    {
        SceneManager.LoadScene("RegisterScene");
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Debug.Log("Salir del juego");
        Application.Quit();
    }

    public void MusicOff()
    {
        if (!isMusicOff)
        {
            backgroundMusic.Pause();
            isMusicOff = true;
        }
        else
        {
            backgroundMusic.UnPause();
            isMusicOff = false;
        }
    }

    public void OnFinalResult(string result)
    {
        resultText.gameObject.SetActive(true);
        resultText.text = result;
        startRecordingButton.enabled = true;
        startRecordingButton.GetComponent<Image>().color = Color.white;

        if (i == (lengthImages - 1) & isGameActive & timerOn)
        {
            if (images[i].CompareTag(result))
            {
                source.PlayOneShot(correctSound);
                StartCoroutine(ShowAndHide(correctSign, 2.0f));
                StartCoroutine(ShowAndHide(resultText.gameObject, 2.0f));
                UpdateTimer(timeLeft);
                images[i].SetActive(false);
                UpdateScore(1);
                wrongTimes = 0;
                Success();

            }
            else
            {
                wrongTimes += 1;
                source.PlayOneShot(incorrectSound);
                StartCoroutine(ShowAndHide(incorrectSign, 2.0f));
                StartCoroutine(ShowAndHide(resultText.gameObject, 2.0f));
                UpdateTimer(timeLeft);
                if (wrongTimes == 1)
                {
                    string result1 = result + " ";
                    wrongResultsData[images[i].tag] = result1;
                }

                if (wrongTimes == 2)
                {
                    string result1_2 = (string)wrongResultsData[images[i].tag];
                    string result2 = string.Concat(result1_2, (result + " "));
                    wrongResultsData[images[i].tag] = result2;
                }
                if (wrongTimes == 3)
                {
                    string result2_2 = (string)wrongResultsData[images[i].tag];
                    string result3 = string.Concat(result2_2, (result + " "));
                    wrongResultsData[images[i].tag] = result3;
                    wrongTimes = 0;
                    images[i].SetActive(false);
                    Success();
                }
            }
        }

        if (i < (lengthImages - 1) & isGameActive & timerOn)
        {
            if (images[i].CompareTag(result))
            {
                source.PlayOneShot(correctSound);
                StartCoroutine(ShowAndHide(images[i], 2.0f));
                StartCoroutine(ShowAndHide(correctSign, 2.0f));
                StartCoroutine(ShowAndHide(resultText.gameObject, 2.0f));
                StartCoroutine(NextGO(images[i + 1], 2.0f));
                UpdateTimer(timeLeft);
                UpdateScore(1);
                wrongTimes = 0;
                i++;

            }
            else
            {
                wrongTimes += 1;
                source.PlayOneShot(incorrectSound);
                StartCoroutine(ShowAndHide(incorrectSign, 2.0f));
                StartCoroutine(ShowAndHide(resultText.gameObject, 2.0f));
                UpdateTimer(timeLeft);
                if (wrongTimes == 1)
                {
                    string result1 = result + " ";
                    wrongResultsData[images[i].tag] = result1;
                }

                if (wrongTimes == 2)
                {
                    string result1_2 = (string)wrongResultsData[images[i].tag];
                    string result2 = string.Concat(result1_2, (result + " "));
                    wrongResultsData[images[i].tag] = result2;
                }
                if (wrongTimes == 3)
                {
                    string result2_2 = (string)wrongResultsData[images[i].tag];
                    string result3 = string.Concat(result2_2, (result + " "));
                    wrongResultsData[images[i].tag] = result3;
                    wrongTimes = 0;
                    images[i].SetActive(false);
                    images[i + 1].SetActive(true);
                    i++;
                }
            }
        }
    }

    public void OnPartialResult(string result)
    {
        resultText.text = result;
        startRecordingButton.GetComponent<Image>().color = Color.white;
    }

    public void OnAvailabilityChange(bool available)
    {
        startRecordingButton.enabled = available;
        if (!available)
        {
            resultText.text = "Speech Recognition not available";
        }
        else
        {
            resultText.text = "Diga algo";
        }
    }

    public void OnAuthorizationStatusFetched(AuthorizationStatus status)
    {
        switch (status)
        {
            case AuthorizationStatus.Authorized:
                startRecordingButton.enabled = true;
                break;
            default:
                startRecordingButton.enabled = false;
                resultText.text = "Cannot use Speech Recognition, authorization status is " + status;
                break;
        }
    }

    public void OnEndOfSpeech()
    {
        startRecordingButton.enabled = true;
        startRecordingButton.GetComponent<Image>().color = Color.white;
    }

    public void OnError(string error)
    {
        Debug.LogError(error);
        startRecordingButton.enabled = true;
        startRecordingButton.GetComponent<Image>().color = Color.white;
    }

    public void OnStartRecordingPressed()
    {
        startRecordingButton.GetComponent<Image>().color = Color.green;
        resultText.gameObject.SetActive(false);
        if (SpeechRecognizer.IsRecording())
        {
#if UNITY_IOS && !UNITY_EDITOR
			SpeechRecognizer.StopIfRecording();
			//startRecordingButton.GetComponentInChildren<Text>().text = "Stopping";
			startRecordingButton.enabled = false;
#elif UNITY_ANDROID && !UNITY_EDITOR
			SpeechRecognizer.StopIfRecording();
			//startRecordingButton.GetComponentInChildren<Text>().text = "Iniciar grabación";
#endif
        }
        else
        {
            SpeechRecognizer.StartRecording(true);
            resultText.text = "Diga la categoría";
        }
    }

    public void Pause()
    {
        source.Pause();
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;

        if (images[i].CompareTag("aire"))
        {
            hintText.text = "Capaz de elevarse y volar";
        }

        if (images[i].CompareTag("agua"))
        {
            hintText.text = "Capaz de flotar y navegar";
        }

        if (images[i].CompareTag("tierra"))
        {
            hintText.text = "Capaz de desplazarse por el suelo";
        }
    }

    public void Resume()
    {
        source.UnPause();
        tutorialMenu.SetActive(false);
        pauseMenu.SetActive(false);
        rulesMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Home()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void Rules()
    {
        Time.timeScale = 0f;
        tutorialMenu.SetActive(false);
        rulesMenu.SetActive(true);
    }

    public void Tutorial(int j)
    {
        tutorialMenu.SetActive(true);
        Time.timeScale = 0f;
        instructionsText.text = instructions[j];
    }
}
