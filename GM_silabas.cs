using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 
using UnityEngine.UI; 
using KKSpeech; 
using System;

public class GM_silabas : MonoBehaviour
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
    public GameObject Game_Manager_silabas;
    [SerializeField] GameObject rulesMenu;
    [SerializeField] GameObject repositoryMenu;
    public TextMeshProUGUI repositoryText;
    private int score;
    [SerializeField] GameObject tutorialMenu;
    public TextMeshProUGUI instructionsText;
    private List<string> instructions = new List<string>();
    public Username usernameSO;
    private string gameID = "Nivel 13";
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
    public TextMeshProUGUI ciudadText1;
    public TextMeshProUGUI tiendaText1;
    public TextMeshProUGUI calleText1;
    public TextMeshProUGUI parqueText1;
    public TextMeshProUGUI puebloText1;
    public TextMeshProUGUI parqueText2;
    public TextMeshProUGUI calleText2;
    public TextMeshProUGUI tiendaText2;
    public TextMeshProUGUI puebloText2;
    public TextMeshProUGUI ciudadText2;

    FirebaseManager firebaseManager;

    void Awake()
    {
        firebaseManager = Game_Manager_silabas.GetComponent<FirebaseManager>();
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
        //hints.Add("Pista de la 2a parte"); //actualizar

        instructions.Add("Ahora que ya sé cómo moverme por el planeta, me pregunto qué lugares hay para visitar en la Tierra, ¿me acompañas?");
        Tutorial(0);

        scoreText.gameObject.SetActive(true);

        syllables.SetActive(true);

        wordToNumber.Add("calle", 0);
        wordToNumber.Add("ciudad", 1);
        wordToNumber.Add("parque", 2);
        wordToNumber.Add("pueblo", 3);
        wordToNumber.Add("tienda", 4);

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
        //rulesText.gameObject.SetActive(false);
        successText.gameObject.SetActive(true);
        finalScoreText.gameObject.SetActive(true);
        finalTimeText.gameObject.SetActive(true);
        finalScoreText.text = " * Puntuación final: " + score + "/5";
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
            if (ciudadText1.gameObject.CompareTag(result))
            {
                string initialResult = (string)correctResultsData[gameID];
                string finalResult = string.Concat((initialResult + " "), ciudadText1.gameObject.tag);
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
                ciudadText1.gameObject.SetActive(false);
                ciudadText2.gameObject.SetActive(false);
                StartCoroutine(ShowAndWait(1.0f));
            }
            if (tiendaText1.gameObject.CompareTag(result))
            {
                string initialResult = (string)correctResultsData[gameID];
                string finalResult = string.Concat((initialResult + " "), tiendaText1.gameObject.tag);
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
                tiendaText1.gameObject.SetActive(false);
                tiendaText2.gameObject.SetActive(false);
                StartCoroutine(ShowAndWait(1.0f));
            }
            if (calleText1.gameObject.CompareTag(result))
            {
                string initialResult = (string)correctResultsData[gameID];
                string finalResult = string.Concat((initialResult + " "), calleText1.gameObject.tag);
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
                calleText1.gameObject.SetActive(false);
                calleText2.gameObject.SetActive(false);
                StartCoroutine(ShowAndWait(1.0f));
            }
            if (parqueText1.gameObject.CompareTag(result))
            {
                string initialResult = (string)correctResultsData[gameID];
                string finalResult = string.Concat((initialResult + " "), parqueText1.gameObject.tag);
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
                parqueText1.gameObject.SetActive(false);
                parqueText2.gameObject.SetActive(false);
                StartCoroutine(ShowAndWait(1.0f));
            }
            if (puebloText1.gameObject.CompareTag(result))
            {
                string initialResult = (string)correctResultsData[gameID];
                string finalResult = string.Concat((initialResult + " "), puebloText1.gameObject.tag);
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
                puebloText1.gameObject.SetActive(false);
                puebloText2.gameObject.SetActive(false);
                StartCoroutine(ShowAndWait(1.0f));
            }
            
            if (ciudadText1.gameObject.CompareTag(result) == false && tiendaText1.gameObject.CompareTag(result) == false &&
                calleText1.gameObject.CompareTag(result) == false && parqueText1.gameObject.CompareTag(result) == false &&
                puebloText1.gameObject.CompareTag(result) == false)
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
                    ciudadText1.gameObject.SetActive(false);
                    ciudadText2.gameObject.SetActive(false);
                }
                if (wrongTimes == 5)
                {
                    string result3_2 = (string)wrongResultsData[gameID];
                    string result4 = string.Concat(result3_2, (result + " "));
                    wrongResultsData[gameID] = result4;
                    tiendaText1.gameObject.SetActive(false);
                    tiendaText2.gameObject.SetActive(false);
                }
                if (wrongTimes == 7)
                {
                    string result4_2 = (string)wrongResultsData[gameID];
                    string result5 = string.Concat(result4_2, (result + " "));
                    wrongResultsData[gameID] = result5;
                    calleText1.gameObject.SetActive(false);
                    calleText2.gameObject.SetActive(false);
                }
                if (wrongTimes == 9)
                {
                    string result5_2 = (string)wrongResultsData[gameID];
                    string result6 = string.Concat(result5_2, (result + " "));
                    wrongResultsData[gameID] = result6;
                    parqueText1.gameObject.SetActive(false);
                    parqueText2.gameObject.SetActive(false);
                }
                if (wrongTimes == 11)
                {
                    string result6_2 = (string)wrongResultsData[gameID];
                    string result7 = string.Concat(result6_2, (result + " "));
                    wrongResultsData[gameID] = result7;
                    puebloText1.gameObject.SetActive(false);
                    puebloText2.gameObject.SetActive(false);
                    wrongTimes = 0;
                    Success();
                }
            }
            if (ciudadText1.IsActive() == false && tiendaText1.IsActive() == false && calleText1.IsActive() == false
                && parqueText1.IsActive() == false && puebloText1.IsActive() == false)
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
			//startRecordingButton.GetComponentInChildren<Text>().text = "Iniciar grabación";
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
