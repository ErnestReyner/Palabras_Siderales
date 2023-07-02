using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 
using UnityEngine.UI; 
using KKSpeech; 
using System;

public class GM_tutorial : MonoBehaviour
{
    public GameObject correctSign;
    public GameObject incorrectSign;
    public List<GameObject> images; 
    private List<string> hints = new List<string>();
    private List<string> instructions = new List<string>();
    public TextMeshProUGUI scoreText;
    int i = 0;
    int counter = 0;
    int lengthImages;
    public bool isGameActive;
    public AudioSource source;
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
    public TextMeshProUGUI hintText;
    [SerializeField] GameObject pauseMenu;
    public Button nextLevel;
    public TextMeshProUGUI successText;
    [SerializeField] GameObject successMenu;
    public GameObject Game_Manager_tutorial;
    private int score;
    [SerializeField] GameObject tutorialMenu;
    public TextMeshProUGUI instructionsText;
    public Button rulesButton;
    [SerializeField] GameObject rulesMenu;
    public TextMeshProUGUI rulesMenuText;
    [SerializeField] GameObject repositoryMenu;
    public TextMeshProUGUI repositoryText;
    int wrongTimes = 0;
    FirebaseManager firebaseManager;
    Dictionary<string, object> wrongResultsData = new Dictionary<string, object>();
    Dictionary<string, object> correctResultsData = new Dictionary<string, object>();
    public Username usernameSO;
    private string gameID = "Tutorial";
    private string endDate;
    [SerializeField] GameObject gameOverMenu;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;
    public Button menuButton;
    public Button quitButton;



    void Awake()
    {
        firebaseManager = Game_Manager_tutorial.GetComponent<FirebaseManager>();
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


        hints.Add("Conjunto enorme de estrellas dentro del universo"); //galaxia
        hints.Add("Estrella luminosa, centro del sistema planetario donde está la Tierra"); //Sol
        hints.Add("Satélite que da vueltas a la Tierra"); //Luna
        hints.Add("Planeta que habitamos"); //Tierra
        hints.Add("Aeronave que viaja por el espacio"); //cohete
        hints.Add("Número que sigue al dos"); //tres
        hints.Add("Número que sigue al uno"); //dos
        hints.Add("Número que representa la unidad"); //uno
        hints.Add("Imitación del sonido de una explosión"); //bum.



        instructions.Add("¡Bienvenido/a "+ usernameSO.username +" a mi odisea terrestre! \nMe llamo Sideral y soy un robot inteligente del espacio" +
            " exterior. ¿Estás listo para ayudarme en mi aventura en la Tierra?");
        instructions.Add("Para ayudarme tendrás que usar la voz y el micrófono. Cada vez que quieras hablar tienes que presionar (sin mantener) " +
            "el botón del micrófono. \nA continuación, di la palabra escrita en el centro para descubrir cómo empezó todo");
        instructions.Add("¡Genial! \nYa sabes cómo funciona el micrófono. Ahora sigue con las siguientes palabras." +
            " Si son correctas obtendrás 1 punto, pero ten en cuenta que el tiempo va disminuyendo.");
        instructions.Add("Botón pista (?): este botón te ayudará dándote alguna pista para resolver el nivel.");
        instructions.Add("Por último, el botón de volver (CASA) te permite volver a la pantalla principal. No lo presiones ahora si quieres terminar de escuchar mi historia.");
        instructions.Add("¡Enhorabuena " + usernameSO.username + "! Has completado con éxito mi caída a la Tierra. El botón ESTRELLA te " +
            "permite ver las palabras recuperadas en cada nivel, ¡ahora a por los siguientes!");

        Tutorial(0);
        scoreText.gameObject.SetActive(true);
        lengthImages = images.Count;
        isGameActive = true;
        score = 0;
        timerOn = true;
        currentWordText.text = images[0].tag;
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

    IEnumerator ShowAndWait(float delay)
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
        images[i].SetActive(false);
        startRecordingButton.gameObject.SetActive(false);
        timeLeft = 0;
        timerOn = false;
        source.Stop();
        source.PlayOneShot(gameOverSound);
        correctSign.SetActive(false);
        incorrectSign.SetActive(false);
        scoreText.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
        currentWordText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
        menuOnPlayButton.gameObject.SetActive(false);
        hintButton.gameObject.SetActive(false);
        isGameActive = false;
        firebaseManager.WrongAnswers(gameID, wrongResultsData);
        StartCoroutine(firebaseManager.UpdateUser(gameID, timeUsed, score, endDate));
    }

    public void Success()
    {
        Tutorial(5);
        endDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        timeUsed = 180 - timeLeft;
        UpdateTimer(timeUsed);
        successMenu.SetActive(true);
        startRecordingButton.gameObject.SetActive(false);
        isGameActive = false;
        timeLeft = 0;
        timerOn = false;
        source.Stop();
        source.PlayOneShot(victorySound);
        correctSign.SetActive(false);
        incorrectSign.SetActive(false);
        scoreText.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
        currentWordText.gameObject.SetActive(false);
        successText.gameObject.SetActive(true);
        finalScoreText.gameObject.SetActive(true);
        finalTimeText.gameObject.SetActive(true);
        finalScoreText.text = " * Puntuación final: " + score + "/9";
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

    public void OnFinalResult(string result)
    {
        resultText.gameObject.SetActive(true);
        resultText.text = result;
        startRecordingButton.enabled = true;
        startRecordingButton.GetComponent<Image>().color = Color.white;


        if (i == (lengthImages - 1) & isGameActive & timerOn)
        {
            if (currentWordText.text == result)
            {
                string initialResult = (string)correctResultsData[gameID];
                string finalResult = string.Concat((initialResult + " "), images[i].tag);
                correctResultsData[gameID] = finalResult;
                source.PlayOneShot(correctSound);
                StartCoroutine(ShowAndHide(correctSign, 2.0f));
                StartCoroutine(ShowAndHide(resultText.gameObject, 2.0f));
                UpdateScore(1);
                wrongTimes = 0;
                UpdateTimer(timeLeft);
                resultText.gameObject.SetActive(false);
                currentWordText.gameObject.SetActive(false);
                images[i].SetActive(true);
                StartCoroutine(ShowAndWait(2.0f));
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
                    resultText.gameObject.SetActive(false);
                    currentWordText.gameObject.SetActive(false);
                    images[i].SetActive(true);
                    StartCoroutine(ShowAndWait(2.0f));
                }
            }
        }

        if (i < (lengthImages - 1) & isGameActive & timerOn)
        {
            if (currentWordText.text == result)
            {
                if (i == 0)
                {
                    correctResultsData[gameID] = "";
                }

                string initialResult = (string)correctResultsData[gameID];
                string finalResult = string.Concat((initialResult + " "), images[i].tag);
                correctResultsData[gameID] = finalResult;
                source.PlayOneShot(correctSound);
                StartCoroutine(ShowAndHide(correctSign, 2.0f));
                StartCoroutine(ShowAndHide(resultText.gameObject, 2.0f));
                currentWordText.text = images[i + 1].tag;
                StartCoroutine(NextGO(currentWordText.gameObject, 2.0f));
                UpdateScore(1);
                UpdateTimer(timeLeft);
                wrongTimes = 0;
                images[i].SetActive(true);
                i++;
                if (i==1)
                {
                    Tutorial(2);
                }
                if (i==2)
                {
                    Tutorial(3);
                }
                if (i==3)
                {
                    Tutorial(4);
                }
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
                    images[i].SetActive(true);
                    currentWordText.text = images[i + 1].tag;
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
        rulesMenu.SetActive(false);
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        hintText.text = hints[i];
    }

    public void Resume()
    {
        
        if (counter == 0)
        {
            Tutorial(1);
            counter++;
        }
        else
        {
            if (repositoryMenu.activeInHierarchy == true)
            {
                repositoryMenu.gameObject.SetActive(false);
                successMenu.gameObject.SetActive(true);
            }
            source.UnPause();
            pauseMenu.SetActive(false);
            tutorialMenu.SetActive(false);
            rulesMenu.SetActive(false);
            Time.timeScale = 1f;
        }
        

    }

    public void Rules ()
    {
        Time.timeScale = 0f;
        rulesMenu.SetActive(true);
        rulesMenuText.text = "1. Lee y di el nombre de la palabra escrito en pantalla \n 2. Cada acierto es 1 punto. Si fallas 3 veces " +
            "se cambia a la siguiente palabra. \n 3. Resuelve el tutorial antes de que se acabe el tiempo.";
    }

    public void Home()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void Tutorial (int j)
    {
        tutorialMenu.SetActive(true);
        Time.timeScale = 0f;
        instructionsText.text = instructions[j];
    }

    public void StarButton()
    {
        repositoryMenu.gameObject.SetActive(true);
        repositoryText.text = (string)correctResultsData[gameID];
    }

    public void ReturnSuccessMenu ()
    {
        repositoryMenu.gameObject.SetActive(false);
        successMenu.gameObject.SetActive(true);

    }
}
