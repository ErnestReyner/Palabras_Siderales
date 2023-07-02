using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using KKSpeech; 
using System;

public class GM_silabas_lugares : MonoBehaviour
{
    [Header("Game Tools")]
    public GameObject correctSign;
    public GameObject incorrectSign;
    public List<GameObject> images;
    private List<string> hints = new List<string>();
    private Dictionary<string, int> wordToNumber = new Dictionary<string, int>();
    public TextMeshProUGUI scoreText;
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
    public TextMeshProUGUI currentWordText; 
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalTimeText;
    public Button startRecordingButton;
    public TextMeshProUGUI resultText; 
    public Button menuOnPlayButton;
    public Button hintButton;
    public Button resumeButton;
    public Button musicOffButton;
    public TextMeshProUGUI hintText;
    [SerializeField] GameObject pauseMenu;
    public Button nextLevel;
    public TextMeshProUGUI successText;
    [SerializeField] GameObject successMenu;
    public GameObject Game_Manager_silabas_lugares;
    [SerializeField] GameObject rulesMenu;
    [SerializeField] GameObject repositoryMenu;
    public TextMeshProUGUI repositoryText;
    private int score;
    [SerializeField] GameObject tutorialMenu;
    public TextMeshProUGUI instructionsText;
    private List<string> instructions = new List<string>();
    public Username usernameSO;
    private string gameID = "Nivel 14";
    private string endDate;
    int wrongTimes = 0;
    Dictionary<string, object> wrongResultsData = new Dictionary<string, object>();
    Dictionary<string, object> correctResultsData = new Dictionary<string, object>();
    private bool isMusicOff = false;
    [SerializeField] GameObject gameOverMenu;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;
    public Button menuButton;
    public Button quitButton;

    [Header("Game Objects")]
    public GameObject syllables;
    public TextMeshProUGUI teatroText1;
    public TextMeshProUGUI islaText1;
    public TextMeshProUGUI casaText1;
    public TextMeshProUGUI jardinText1;
    public TextMeshProUGUI mundoText1;
    public TextMeshProUGUI jardinText2;
    public TextMeshProUGUI casaText2;
    public TextMeshProUGUI islaText2;
    public TextMeshProUGUI mundoText2;
    public TextMeshProUGUI teatroText2;

    FirebaseManager firebaseManager;

    void Awake()
    {
        firebaseManager = Game_Manager_silabas_lugares.GetComponent<FirebaseManager>();
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


        hints.Add("Son lugares a los que podemos ir");

        instructions.Add("Parece que voy entendiendo la vida en la Tierra, �qu� otros lugares existen donde encontrar mi cohete?");
        Tutorial(0);

        scoreText.gameObject.SetActive(true);

        syllables.SetActive(true);

        wordToNumber.Add("casa", 0);
        wordToNumber.Add("teatro", 1);
        wordToNumber.Add("jard�n", 2);
        wordToNumber.Add("mundo", 3);
        wordToNumber.Add("isla", 4);

        isGameActive = true;
        score = 0;
        timerOn = true;
        UpdateScore(0);
        wrongResultsData.Clear();
        correctResultsData[gameID] = "";


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
                Debug.Log("�El tiempo se ha acabado!");
                timeLeft = 0;
                timerOn = false;
            }
        }

    }


    void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "Puntuaci�n: " + score;

    }

    IEnumerator ShowAndHide(GameObject go, float delay)
    {
        go.SetActive(true);
        yield return new WaitForSeconds(delay);
        go.SetActive(false);
    }

    IEnumerator ShowAndWait(float delay)
    {

        yield return new WaitForSeconds(delay);
    }
    IEnumerator ShowAndSuccess(float delay)
    {

        yield return new WaitForSeconds(delay);
        Success();
    }

    public void GameOver()
    {
        endDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        timeUsed = 180 - timeLeft;
        UpdateTimer(timeUsed);
        gameOverMenu.SetActive(true);
        startRecordingButton.gameObject.SetActive(false);
        timeLeft = 0;
        timerOn = false;
        backgroundMusic.Stop();
        source.Stop();
        source.PlayOneShot(gameOverSound);
        scoreText.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
        currentWordText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);
        menuOnPlayButton.gameObject.SetActive(false);
        hintButton.gameObject.SetActive(false);
        musicOffButton.gameObject.SetActive(false);
        isGameActive = false;
        firebaseManager.WrongAnswers(gameID, wrongResultsData);
        StartCoroutine(firebaseManager.UpdateUser(gameID, timeUsed, score, endDate));
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
        currentWordText.gameObject.SetActive(false);
        successText.gameObject.SetActive(true);
        finalScoreText.gameObject.SetActive(true);
        finalTimeText.gameObject.SetActive(true);
        finalScoreText.text = " * Puntuaci�n final: " + score + "/5";
        finalTimeText.text = " * Tiempo utilizado: " + timerText.text;
        firebaseManager.CorrectAnswers(correctResultsData);
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


        if (isGameActive & timerOn)
        {
            if (teatroText1.gameObject.CompareTag(result))
            {
                string initialResult = (string)correctResultsData[gameID];
                string finalResult = string.Concat((initialResult + " "), teatroText1.gameObject.tag);
                correctResultsData[gameID] = finalResult;
                currentWordText.gameObject.SetActive(true);
                currentWordText.text = result;
                currentWordText.color = new Color32(20, 255, 0, 255);
                source.PlayOneShot(correctSound);
                StartCoroutine(ShowAndHide(correctSign, 2.0f));
                StartCoroutine(ShowAndHide(resultText.gameObject, 2.0f));
                StartCoroutine(ShowAndHide(currentWordText.gameObject, 2.0f));
                UpdateScore(1);
                StartCoroutine(ShowAndHide(images[wordToNumber[result]], 2.0f));
                wrongTimes = 0;
                UpdateTimer(timeLeft);
                teatroText1.gameObject.SetActive(false);
                teatroText2.gameObject.SetActive(false);
                StartCoroutine(ShowAndWait(1.0f));
            }
            if (islaText1.gameObject.CompareTag(result))
            {
                string initialResult = (string)correctResultsData[gameID];
                string finalResult = string.Concat((initialResult + " "), islaText1.gameObject.tag);
                correctResultsData[gameID] = finalResult;
                currentWordText.gameObject.SetActive(true);
                currentWordText.text = result;
                currentWordText.color = new Color32(20, 255, 0, 255);
                source.PlayOneShot(correctSound);
                StartCoroutine(ShowAndHide(correctSign, 2.0f));
                StartCoroutine(ShowAndHide(resultText.gameObject, 2.0f));
                StartCoroutine(ShowAndHide(currentWordText.gameObject, 2.0f));
                UpdateScore(1);
                StartCoroutine(ShowAndHide(images[wordToNumber[result]], 2.0f));
                wrongTimes = 0;
                UpdateTimer(timeLeft);
                islaText1.gameObject.SetActive(false);
                islaText2.gameObject.SetActive(false);
                StartCoroutine(ShowAndWait(1.0f));
            }
            if (casaText1.gameObject.CompareTag(result))
            {
                string initialResult = (string)correctResultsData[gameID];
                string finalResult = string.Concat((initialResult + " "), casaText1.gameObject.tag);
                correctResultsData[gameID] = finalResult;
                currentWordText.gameObject.SetActive(true);
                currentWordText.text = result;
                currentWordText.color = new Color32(20, 255, 0, 255);
                source.PlayOneShot(correctSound);
                StartCoroutine(ShowAndHide(correctSign, 2.0f));
                StartCoroutine(ShowAndHide(resultText.gameObject, 2.0f));
                UpdateScore(1);
                StartCoroutine(ShowAndHide(images[wordToNumber[result]], 1.5f));
                wrongTimes = 0;
                UpdateTimer(timeLeft);
                casaText1.gameObject.SetActive(false);
                casaText2.gameObject.SetActive(false);
                StartCoroutine(ShowAndWait(1.0f));
            }
            if (jardinText1.gameObject.CompareTag(result))
            {
                string initialResult = (string)correctResultsData[gameID];
                string finalResult = string.Concat((initialResult + " "), jardinText1.gameObject.tag);
                correctResultsData[gameID] = finalResult;
                currentWordText.gameObject.SetActive(true);
                currentWordText.text = result;
                currentWordText.color = new Color32(20, 255, 0, 255);
                source.PlayOneShot(correctSound);
                StartCoroutine(ShowAndHide(correctSign, 2.0f));
                StartCoroutine(ShowAndHide(resultText.gameObject, 2.0f));
                StartCoroutine(ShowAndHide(currentWordText.gameObject, 2.0f));
                UpdateScore(1);
                StartCoroutine(ShowAndHide(images[wordToNumber[result]], 2.0f));
                wrongTimes = 0;
                UpdateTimer(timeLeft);
                jardinText1.gameObject.SetActive(false);
                jardinText2.gameObject.SetActive(false);
                StartCoroutine(ShowAndWait(1.0f));
            }
            if (mundoText1.gameObject.CompareTag(result))
            {
                string initialResult = (string)correctResultsData[gameID];
                string finalResult = string.Concat((initialResult + " "), mundoText1.gameObject.tag);
                correctResultsData[gameID] = finalResult;
                currentWordText.gameObject.SetActive(true);
                currentWordText.text = result;
                currentWordText.color = new Color32(20, 255, 0, 255);
                source.PlayOneShot(correctSound);
                StartCoroutine(ShowAndHide(correctSign, 2.0f));
                StartCoroutine(ShowAndHide(resultText.gameObject, 2.0f));
                StartCoroutine(ShowAndHide(currentWordText.gameObject, 2.0f));
                UpdateScore(1);
                StartCoroutine(ShowAndHide(images[wordToNumber[result]], 2.0f));
                wrongTimes = 0;
                UpdateTimer(timeLeft);
                mundoText1.gameObject.SetActive(false);
                mundoText2.gameObject.SetActive(false);
                StartCoroutine(ShowAndWait(1.0f));
            }

            if (teatroText1.gameObject.CompareTag(result) == false && islaText1.gameObject.CompareTag(result) == false &&
                casaText1.gameObject.CompareTag(result) == false && jardinText1.gameObject.CompareTag(result) == false &&
                mundoText1.gameObject.CompareTag(result) == false)
            {
                wrongTimes += 1;
                source.PlayOneShot(incorrectSound);
                StartCoroutine(ShowAndHide(incorrectSign, 2.0f));
                StartCoroutine(ShowAndHide(resultText.gameObject, 2.0f));
                UpdateTimer(timeLeft);
                if (wrongTimes == 1)
                {
                    string result1 = result + " ";
                    wrongResultsData[gameID] = result1;
                }

                if (wrongTimes == 2)
                {
                    string result1_2 = (string)wrongResultsData[gameID];
                    string result2 = string.Concat(result1_2, (result + " "));
                    wrongResultsData[gameID] = result2;
                }
                if (wrongTimes == 3)
                {
                    string result2_2 = (string)wrongResultsData[gameID];
                    string result3 = string.Concat(result2_2, (result + " "));
                    wrongResultsData[gameID] = result3;
                    teatroText1.gameObject.SetActive(false);
                    teatroText2.gameObject.SetActive(false);
                }
                if (wrongTimes == 5)
                {
                    string result3_2 = (string)wrongResultsData[gameID];
                    string result4 = string.Concat(result3_2, (result + " "));
                    wrongResultsData[gameID] = result4;
                    islaText1.gameObject.SetActive(false);
                    islaText2.gameObject.SetActive(false);
                }
                if (wrongTimes == 7)
                {
                    string result4_2 = (string)wrongResultsData[gameID];
                    string result5 = string.Concat(result4_2, (result + " "));
                    wrongResultsData[gameID] = result5;
                    casaText1.gameObject.SetActive(false);
                    casaText2.gameObject.SetActive(false);
                }
                if (wrongTimes == 9)
                {
                    string result5_2 = (string)wrongResultsData[gameID];
                    string result6 = string.Concat(result5_2, (result + " "));
                    wrongResultsData[gameID] = result6;
                    jardinText1.gameObject.SetActive(false);
                    jardinText2.gameObject.SetActive(false);
                }
                if (wrongTimes == 11)
                {
                    string result6_2 = (string)wrongResultsData[gameID];
                    string result7 = string.Concat(result6_2, (result + " "));
                    wrongResultsData[gameID] = result7;
                    mundoText1.gameObject.SetActive(false);
                    mundoText2.gameObject.SetActive(false);
                    wrongTimes = 0;
                    Success();
                }
            }
            if (teatroText1.IsActive() == false && islaText1.IsActive() == false && casaText1.IsActive() == false
                && jardinText1.IsActive() == false && mundoText1.IsActive() == false)
            {
                StartCoroutine(ShowAndSuccess(1.0f));
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
        currentWordText.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
        if (SpeechRecognizer.IsRecording())
        {

#if UNITY_IOS && !UNITY_EDITOR
			SpeechRecognizer.StopIfRecording();
            //startRecordingButton.GetComponent<Image>().color = Color.white;
			//startRecordingButton.GetComponentInChildren<Text>().text = "Stopping";
			startRecordingButton.enabled = false;
#elif UNITY_ANDROID && !UNITY_EDITOR
			SpeechRecognizer.StopIfRecording();
            //startRecordingButton.GetComponent<Image>().color = Color.white;
			//startRecordingButton.GetComponentInChildren<Text>().text = "Iniciar grabaci�n";
#endif
        }
        else
        {
            SpeechRecognizer.StartRecording(true);
        }


    }

    public void Pause()
    {
        source.Pause();
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        hintText.text = hints[0];
    }

    public void Resume()
    {
        if (repositoryMenu.activeInHierarchy == true)
        {
            repositoryMenu.gameObject.SetActive(false);
            successMenu.gameObject.SetActive(true);
        }
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

    public void StarButton()
    {
        repositoryMenu.gameObject.SetActive(true);
        repositoryText.text = (string)correctResultsData[gameID];
    }

    public void ReturnSuccessMenu()
    {
        repositoryMenu.gameObject.SetActive(false);
        successMenu.gameObject.SetActive(true);
    }

    public void Tutorial(int j)
    {
        tutorialMenu.SetActive(true);
        Time.timeScale = 0f;
        instructionsText.text = instructions[j];
    }
}
