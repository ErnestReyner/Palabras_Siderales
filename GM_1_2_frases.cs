using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; 
using KKSpeech; 
using System;

public class GM_1_2_frases : MonoBehaviour
{
    public GameObject correctSign;
    public GameObject incorrectSign;
    public List<GameObject> images; 
    private List<string> hints = new List<string>();
    private List<string> sentences = new List<string>();
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
    public TextMeshProUGUI currentWordText; 
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalTimeText;
    public Button startRecordingButton;
    public TextMeshProUGUI resultText; 
    public TextMeshProUGUI sentencesText;
    public Button menuOnPlayButton;
    public Button hintButton;
    public Button resumeButton;
    public Button musicOffButton;
    public TextMeshProUGUI hintText;
    [SerializeField] GameObject pauseMenu;
    public Button nextLevel;
    public TextMeshProUGUI successText;
    [SerializeField] GameObject successMenu;
    public GameObject gameManager_1_2_frases;
    [SerializeField] GameObject rulesMenu;
    private int score;
    [SerializeField] GameObject tutorialMenu;
    public TextMeshProUGUI instructionsText;
    private List<string> instructions = new List<string>();
    public Username usernameSO;
    private string gameID = "Nivel 5";
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


    void Awake ()
    {
        firebaseManager = gameManager_1_2_frases.GetComponent<FirebaseManager>();
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

        hints.Add("Permite mirar a través, da luz y ventilación"); //ventana
        hints.Add("Dormimos y descansamos en ella"); //cama
        hints.Add("Sirve para sentarnos"); //silla
        hints.Add("Abren cerraduras"); //llaves
        hints.Add("Podemos vernos reflejados en él"); //espejo
        hints.Add("Permite ver diferentes programas para entretenernos"); //televisor
        hints.Add("Abertura que se usa para entrar o salir de lugares"); //puerta
        hints.Add("Lo utilizamos para lavarnos"); //jabón
        hints.Add("Las usamos para estar por casa"); //zapatillas
        hints.Add("Mueble que sirve de soporte para comer, escribir, etc"); //mesa
        hints.Add("¿Cómo puedes salir de la casa? Di las dos palabras correspondientes con 'Y' entre ellas"); //llaves y puerta
        
        sentences.Add("Abre la _ _ _ _ _ _ _ para que entre el aire");
        sentences.Add("Me voy a la _ _ _ _ que tengo mucho sueño");
        sentences.Add("Trae otra _ _ _ _ _ para que nos podamos sentar todos");
        sentences.Add("He perdido las _ _ _ _ _ _ de casa y no sé cómo entrar");
        sentences.Add("Me peino mirándome en el _ _ _ _ _ _");
        sentences.Add("Me he comprado un nuevo _ _ _ _ _ _ _ _ _ de alta definición");
        sentences.Add("Si abro la _ _ _ _ _ _ podré salir de la casa");
        sentences.Add("Este _ _ _ _ _ de manos huele muy bien");
        sentences.Add("Al llegar a casa siempre me pongo las _ _ _ _ _ _ _ _ _ _");
        sentences.Add("Me he comprado una nueva _ _ _ _ de escritorio");
        sentences.Add("Necesito las _ _ _ _ _ _ para poder abrir la _ _ _ _ _ _ de la casa");

        instructions.Add("Perfecto, ahora que ya conozco algunos de los objetos de una casa humana tengo que salir de ella y buscar mi cohete");
        Tutorial(0);

        scoreText.gameObject.SetActive(true);
        timerText.gameObject.SetActive(true);
        lengthImages = images.Count;
        isGameActive = true;
        score = 0;
        timerOn = true;
        sentencesText.text = sentences[0];
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

    IEnumerator ShowAndHide (GameObject go, float delay)
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

    IEnumerator WaitAndSuccess(float delay)
    {
        yield return new WaitForSeconds(delay);
        Success();
    }


    void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "Puntuación: " + score;

    }

    public void GameOver()
    {
        endDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        timeUsed = 180 - timeLeft;
        UpdateTimer(timeUsed);
        images[i].SetActive(false);
        startRecordingButton.gameObject.SetActive(false);
        gameOverMenu.SetActive(true);
        timeLeft = 0;
        timerOn = false;
        backgroundMusic.Stop();
        source.Stop();
        source.PlayOneShot(gameOverSound);
        scoreText.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
        currentWordText.gameObject.SetActive(false);
        sentencesText.gameObject.SetActive(false);
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
        images[i].SetActive(false);
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
        sentencesText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
        successText.gameObject.SetActive(true);
        finalScoreText.gameObject.SetActive(true);
        finalTimeText.gameObject.SetActive(true);
        finalScoreText.text = " * Puntuación final: " + score + "/11";
        finalTimeText.text = " * Tiempo utilizado: " + timerText.text;
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
                currentWordText.text = result;
                currentWordText.color = new Color32(20, 255, 0, 255);
                StartCoroutine(ShowAndHide(images[i], 2.0f));
                StartCoroutine(ShowAndHide(currentWordText.gameObject, 2.0f));
                source.PlayOneShot(correctSound);
                StartCoroutine(ShowAndHide(correctSign, 2.0f));
                StartCoroutine(ShowAndHide(resultText.gameObject, 2.0f));
                UpdateTimer(timeLeft);
                UpdateScore(1);
                wrongTimes = 0;
                StartCoroutine(WaitAndSuccess(1.5f));

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
                    Success();
                }
            }
        }

        if (i < (lengthImages - 1) & isGameActive & timerOn)
        {
            if (images[i].CompareTag(result))
            {
                currentWordText.text = result;
                currentWordText.color = new Color32(20, 255, 0, 255);
                StartCoroutine(ShowAndHide(images[i], 2.0f));
                StartCoroutine(ShowAndHide(currentWordText.gameObject, 2.0f));
                source.PlayOneShot(correctSound);
                StartCoroutine(ShowAndHide(correctSign, 2.0f));
                StartCoroutine(ShowAndHide(resultText.gameObject, 2.0f));
                sentencesText.text = sentences[i + 1];
                StartCoroutine(NextGO(sentencesText.gameObject, 2.0f));
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
                    sentencesText.text = sentences[i + 1];
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
        currentWordText.gameObject.SetActive(false);
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
        }
    }

    public void Pause()
    {
        source.Pause();
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        hintText.text = hints[i];
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
