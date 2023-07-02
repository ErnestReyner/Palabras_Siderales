using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 
using UnityEngine.UI; 
using KKSpeech; 
using System;

public class GM_cocina : MonoBehaviour
{
    public GameObject correctSign;
    public GameObject incorrectSign;
    public List<GameObject> images; 
    private List<string> hints = new List<string>();
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
    public TextMeshProUGUI rulesText;
    public Button menuOnPlayButton;
    public Button hintButton;
    public Button resumeButton;
    public Button musicOffButton;
    public TextMeshProUGUI hintText;
    [SerializeField] GameObject pauseMenu;
    public Button nextLevel;
    public TextMeshProUGUI successText;
    [SerializeField] GameObject successMenu;
    public GameObject Game_Manager_cocina;
    [SerializeField] GameObject rulesMenu;
    [SerializeField] GameObject repositoryMenu;
    public TextMeshProUGUI repositoryText;
    private int score;
    [SerializeField] GameObject tutorialMenu;
    public TextMeshProUGUI instructionsText;
    private List<string> instructions = new List<string>();
    private string gameID = "Nivel 1";
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

    FirebaseManager firebaseManager;


    void Awake()
    {
        firebaseManager = Game_Manager_cocina.GetComponent<FirebaseManager>();
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


        hints.Add("Líquido transparente que nos quita la sed"); //agua
        hints.Add("Alimento hecho a base de harina de trigo, levadura y agua cocida en un horno"); //pan
        hints.Add("Alimento procedente de algún animal"); //carne
        hints.Add("Bebida alcohólica procedente de las uvas"); //vino
        hints.Add("Fruta redonda y dulce, puede ser roja, verde o amarilla"); //manzana
        hints.Add("Alimento procedente de algún pez"); //pescado
        hints.Add("Fruto redondo y rojo de sabor ácido que se utiliza mucho para cocinar"); //tomate
        hints.Add("Postre frío y cremoso, se sirve en forma de bola y puede tener diferentes sabores"); //helado
        hints.Add("Alimento sólido y salado procedente de la leche"); //queso
        hints.Add("Infusión amarga con cafeína muy popular"); //café

        instructions.Add("Después del accidente de mi cohete, caí en un extraño lugar... \n¿Podrías ayudarme a identificar los" +
            " elementos de este entorno?");
        Tutorial(0);

        scoreText.gameObject.SetActive(true);
        lengthImages = images.Count;
        isGameActive = true;
        score = 0;
        timerOn = true;
        images[0].SetActive(true);
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
        backgroundMusic.Stop();
        source.Stop();
        source.PlayOneShot(gameOverSound);
        scoreText.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
        currentWordText.gameObject.SetActive(false);
        rulesText.gameObject.SetActive(false);
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
        rulesText.gameObject.SetActive(false);
        successText.gameObject.SetActive(true);
        finalScoreText.gameObject.SetActive(true);
        finalTimeText.gameObject.SetActive(true);
        finalScoreText.text = " * Puntuación final: " + score + "/10";
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


        if (i == (lengthImages - 1) & isGameActive & timerOn)
        {
            if (images[i].CompareTag(result))
            {
                string initialResult = (string)correctResultsData[gameID];
                string finalResult = string.Concat((initialResult + " "), images[i].tag);
                correctResultsData[gameID] = finalResult;
                currentWordText.gameObject.SetActive(true);
                currentWordText.text = result;
                currentWordText.color = new Color32(20, 255, 0, 255);
                source.PlayOneShot(correctSound);
                StartCoroutine(ShowAndHide(correctSign, 2.0f));
                StartCoroutine(ShowAndHide(resultText.gameObject, 2.0f));
                StartCoroutine(ShowAndHide(currentWordText.gameObject, 2.0f));
                UpdateScore(1);
                wrongTimes = 0;
                UpdateTimer(timeLeft);
                images[i].SetActive(false);
                StartCoroutine(ShowAndWait(1.0f));
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
                string initialResult = (string)correctResultsData[gameID];
                string finalResult = string.Concat((initialResult + " "), images[i].tag);
                correctResultsData[gameID] = finalResult;
                currentWordText.gameObject.SetActive(true);
                currentWordText.text = result;
                currentWordText.color = new Color32(20, 255, 0, 255);
                source.PlayOneShot(correctSound);
                StartCoroutine(ShowAndHide(images[i], 2.0f));
                StartCoroutine(ShowAndHide(correctSign, 2.0f));
                StartCoroutine(ShowAndHide(resultText.gameObject, 2.0f));
                StartCoroutine(ShowAndHide(currentWordText.gameObject, 2.0f));
                StartCoroutine(NextGO(images[i + 1], 2.0f));
                UpdateScore(1);
                UpdateTimer(timeLeft);
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
        hintText.text = hints[i];
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
        tutorialMenu.SetActive(false);
        Time.timeScale = 0f;
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
