using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

public class FirebaseManager : MonoBehaviour
{
    private static string encryptionKey;

    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
    public DatabaseReference DBreference;

    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    //Register variables
    [Header("Register")]
    public TMP_InputField age;
    public TMP_Dropdown genre;
    public TMP_Dropdown patient;
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    //User Data variables
    [Header("UserData")]
    public TMP_Text usernameText;
    public TMP_Text currentLevelText;
    public TMP_Text totalScoreText;
    public GameObject scoreElement;
    public Transform scoreboardContent;
    public Username usernameSO;


    //Select level variables
    [Header("SelectLevel Menu")]
    public TMP_Dropdown selectLevel;

    static string userID;
    
    
    void Awake()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {

                //If they are avalible Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference; 
    }

    

    public void ClearLoginFields()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }
    public void ClearRegisterFields()
    {
        age.text = "";
        usernameRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }

    //Function for the login button
    public void LoginButton()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    public void ResetPasswordButton()
    {
        StartCoroutine(ResetPassword(emailLoginField.text));
    }

    public void DisclaimerButton ()
    {
        UIManager.instance.RegisterUserMenu();
    }

    //Function for the register button
    public void RegisterButton()
    {
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    public void SignOutButton()
    {
        auth.SignOut();
        UIManager.instance.LoadUserMenu();
        ClearRegisterFields();
        ClearLoginFields();
    }

    public void ScoreboardButton()
    {
        StartCoroutine(LoadScoreboardData());
    }

    public void BackButtonUserScreen ()
    {
        UIManager.instance.UserDataScreen();
        ClearRegisterFields();
        ClearLoginFields();
    }

    public void BackButtonLoginScreen()
    {
        UIManager.instance.LoadUserMenu();
        ClearRegisterFields();
        ClearLoginFields();
    }

    public void GoToSelectLevel()
    {
        UIManager.instance.SelectLevelMenu();
    }

    //Function for saving data
    public void SaveData()
    {
        StartCoroutine(UpdateUsernameAuth(usernameRegisterField.text));
        StartCoroutine(CreateNewUserDatabase(usernameRegisterField.text));
    }

    public void CorrectAnswers(Dictionary<string, object> correctResultsData)
    {
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Repositorio").UpdateChildrenAsync(correctResultsData);
    }

    public string RecoverRepository(string gameID)
    {
        //Get the currently logged in user data
        var DBTask = DBreference.Child("Usuarios").Child(userID).Child("Resultados").GetValueAsync();

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            return "hola1";
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet
            return "hola2";
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;
            string repositoryResults = snapshot.Child("Repositorio").Child(gameID).Value.ToString();
            return repositoryResults;


        }
    }


    public void WrongAnswers(string gameID, Dictionary<string,object> wrongResultsData)
    {
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child(gameID).Child("Respuestas").Child("Incorrectas").UpdateChildrenAsync(wrongResultsData);
    }

    private static byte[] EncryptString(string input, byte[] key, byte[] iv)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        using (SymmetricAlgorithm algorithm = Aes.Create())
        {
            algorithm.Key = key;
            algorithm.IV = iv;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, algorithm.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputBytes, 0, inputBytes.Length);
                    cs.FlushFinalBlock();
                }

                return ms.ToArray();
            }
        }
    }

    public static string DecryptString(byte[] inputBytes, byte[] key, byte[] iv)
    {
        using (SymmetricAlgorithm algorithm = Aes.Create())
        {
            algorithm.Key = key;
            algorithm.IV = iv;

            using (MemoryStream ms = new MemoryStream(inputBytes))
            using (CryptoStream cs = new CryptoStream(ms, algorithm.CreateDecryptor(), CryptoStreamMode.Read))
            using (StreamReader reader = new StreamReader(cs))
            {
                return reader.ReadToEnd();
            }
        }
    }

    private static void SaveEncryptedCredentials(string email, string password)
    {
        byte[] encryptionKey = GetEncryptionKey();

        using (SymmetricAlgorithm algorithm = Aes.Create())
        {
            algorithm.GenerateIV();
            algorithm.Key = encryptionKey;
            byte[] emailIV = algorithm.IV;
            byte[] encryptedEmailBytes = EncryptString(email, algorithm.Key, emailIV);
            string encryptedEmail = Convert.ToBase64String(encryptedEmailBytes);

            algorithm.GenerateIV();
            byte[] passwordIV = algorithm.IV;
            byte[] encryptedPasswordBytes = EncryptString(password, algorithm.Key, passwordIV);
            string encryptedPassword = Convert.ToBase64String(encryptedPasswordBytes);

            PlayerPrefs.SetString("EncryptedEmail", encryptedEmail);
            PlayerPrefs.SetString("EncryptedPassword", encryptedPassword);
            PlayerPrefs.SetString("EmailEncryptionIV", Convert.ToBase64String(emailIV));
            PlayerPrefs.SetString("PasswordEncryptionIV", Convert.ToBase64String(passwordIV));
            PlayerPrefs.Save();
        }
    }

    public static byte[] GetEncryptionKey()
    {
        string encryptionKey = PlayerPrefs.GetString("EncryptionKey");
        if (string.IsNullOrEmpty(encryptionKey))
        {
            byte[] generatedKey = GenerateEncryptionKey();
            encryptionKey = Convert.ToBase64String(generatedKey);
            PlayerPrefs.SetString("EncryptionKey", encryptionKey);
            PlayerPrefs.Save();
            return generatedKey;
        }
        return Convert.FromBase64String(encryptionKey);
    }

    private static byte[] GenerateEncryptionKey()
    {
        byte[] randomBytes = new byte[32];
        using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
        {
            rngCsp.GetBytes(randomBytes);
        }
        return randomBytes;
    }

    public IEnumerator UpdateUser(string gameID, float gameDuration, int score, string endDate)
    {
        var DBTask = DBreference.Child("Usuarios").Child(userID).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;
            string oldScore = snapshot.Child("Puntuación total").Value.ToString();
            Debug.Log("snapshot");
            int totalScore = int.Parse(oldScore) + score;
            Dictionary<string, object> newUserData = new Dictionary<string, object>();
            newUserData["Puntuación total"] = totalScore;
            if (gameID == "Nivel 15")
            {
                DBreference.Child("Usuarios").Child(userID).Child("Juego completado").SetValueAsync("si");
            }
            newUserData["Nivel actual de juego"] = gameID;
            Dictionary<string, object> newResultsData = new Dictionary<string, object>();
            newResultsData["Fecha fin"] = endDate;
            Dictionary<string, object> gameIDData = new Dictionary<string, object>();
            string oldRepetitionValue = snapshot.Child("Resultados").Child("Juegos").Child(gameID).Child("Repeticiones").Value.ToString();
            if (oldRepetitionValue == "no")
            {
                int newRepetitionValue = 0;
                gameIDData["Repeticiones"] = newRepetitionValue;
            }
            else
            {
                int newRepetitionValue = int.Parse(oldRepetitionValue) + 1;
                gameIDData["Repeticiones"] = newRepetitionValue;
            }
            gameIDData["Duración del juego"] = gameDuration;
            gameIDData["Nivel de dificultad"] = 0;
            gameIDData["Puntuación"] = score;
            

            DBreference.Child("Usuarios").Child(userID).UpdateChildrenAsync(newUserData);
            DBreference.Child("Usuarios").Child(userID).Child("Resultados").UpdateChildrenAsync(newResultsData);
            DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child(gameID).UpdateChildrenAsync(gameIDData);
        }
    }

    private IEnumerator UpdateUsernameAuth(string _username)
    {
        //Create a user profile and set the username
        UserProfile profile = new UserProfile { DisplayName = _username };
        userID = User.UserId;
        //Call the Firebase auth update user profile function passing the profile with the username
        var ProfileTask = User.UpdateUserProfileAsync(profile);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

        if (ProfileTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
        }
        else
        {
            //Auth username is now updated
        }
    }

    private IEnumerator CreateNewUserDatabase(string _username)
    {
        //Set the currently logged in user username in the database
        userID = User.UserId;
        var DBTask = DBreference.Child("Usuarios").Child(userID).Child("Nombre de usuario").SetValueAsync(_username);
        DBreference.Child("Usuarios").Child(userID).Child("Correo eléctronico").SetValueAsync(emailRegisterField.text);
        DBreference.Child("Usuarios").Child(userID).Child("Género").SetValueAsync(genre.value);
        DBreference.Child("Usuarios").Child(userID).Child("Edad").SetValueAsync(age.text);
        DBreference.Child("Usuarios").Child(userID).Child("Paciente").SetValueAsync(patient.value);
        DBreference.Child("Usuarios").Child(userID).Child("Nivel actual de juego").SetValueAsync(0);
        DBreference.Child("Usuarios").Child(userID).Child("Puntuación total").SetValueAsync(0);

        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Fecha inicio").SetValueAsync(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Fecha fin").SetValueAsync(0);
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child("Tutorial").Child("Repeticiones").SetValueAsync("no");
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child("Nivel 1").Child("Repeticiones").SetValueAsync("no");
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child("Nivel 2").Child("Repeticiones").SetValueAsync("no");
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child("Nivel 3").Child("Repeticiones").SetValueAsync("no");
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child("Nivel 4").Child("Repeticiones").SetValueAsync("no");
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child("Nivel 5").Child("Repeticiones").SetValueAsync("no");
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child("Nivel 6").Child("Repeticiones").SetValueAsync("no");
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child("Nivel 7").Child("Repeticiones").SetValueAsync("no");
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child("Nivel 8").Child("Repeticiones").SetValueAsync("no");
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child("Nivel 9").Child("Repeticiones").SetValueAsync("no");
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child("Nivel 10").Child("Repeticiones").SetValueAsync("no");
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child("Nivel 11").Child("Repeticiones").SetValueAsync("no");
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child("Nivel 12").Child("Repeticiones").SetValueAsync("no");
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child("Nivel 13").Child("Repeticiones").SetValueAsync("no");
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child("Nivel 14").Child("Repeticiones").SetValueAsync("no");
        DBreference.Child("Usuarios").Child(userID).Child("Resultados").Child("Juegos").Child("Nivel 15").Child("Repeticiones").SetValueAsync("no");

        Debug.Log("Nuevo usuario creado");

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }

    private IEnumerator LoadUserData()
    {
        //userID = auth.CurrentUser.UserId;
        userID = User.UserId;
        //Get the currently logged in user data
        var DBTask = DBreference.Child("Usuarios").Child(userID).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet
            
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;
            if (snapshot.Child("Juego completado").Exists && snapshot.Child("Juego completado").Value.ToString() == "si")
            {
                usernameText.text = snapshot.Child("Nombre de usuario").Value.ToString();
                usernameSO.username = usernameText.text;
                currentLevelText.text = snapshot.Child("Nivel actual de juego").Value.ToString();
                totalScoreText.text = snapshot.Child("Puntuación total").Value.ToString();
                List<string> selectLevelOptions15 = new List<string>();
                selectLevel.ClearOptions();
                selectLevelOptions15.Add("Tutorial");
                selectLevelOptions15.Add("Nivel 1");
                selectLevelOptions15.Add("Nivel 2");
                selectLevelOptions15.Add("Nivel 3");
                selectLevelOptions15.Add("Nivel 4");
                selectLevelOptions15.Add("Nivel 5");
                selectLevelOptions15.Add("Nivel 6");
                selectLevelOptions15.Add("Nivel 7");
                selectLevelOptions15.Add("Nivel 8");
                selectLevelOptions15.Add("Nivel 9");
                selectLevelOptions15.Add("Nivel 10");
                selectLevelOptions15.Add("Nivel 11");
                selectLevelOptions15.Add("Nivel 12");
                selectLevelOptions15.Add("Nivel 13");
                selectLevelOptions15.Add("Nivel 14");
                selectLevelOptions15.Add("Nivel 15");
                selectLevel.AddOptions(selectLevelOptions15);
            }
            else
            {
                usernameText.text = snapshot.Child("Nombre de usuario").Value.ToString();
                usernameSO.username = usernameText.text;
                currentLevelText.text = snapshot.Child("Nivel actual de juego").Value.ToString();
                totalScoreText.text = snapshot.Child("Puntuación total").Value.ToString();
                List<string> selectLevelOptions = new List<string>();
                selectLevel.ClearOptions();

                if (currentLevelText.text == "0")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevel.AddOptions(selectLevelOptions);
                }

                if (currentLevelText.text == "Tutorial")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevel.AddOptions(selectLevelOptions);
                }

                if (currentLevelText.text == "Nivel 1")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevelOptions.Add("Nivel 1");
                    selectLevel.AddOptions(selectLevelOptions);
                }
                if (currentLevelText.text == "Nivel 2")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevelOptions.Add("Nivel 1");
                    selectLevelOptions.Add("Nivel 2");
                    selectLevel.AddOptions(selectLevelOptions);
                }
                if (currentLevelText.text == "Nivel 3")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevelOptions.Add("Nivel 1");
                    selectLevelOptions.Add("Nivel 2");
                    selectLevelOptions.Add("Nivel 3");
                    selectLevel.AddOptions(selectLevelOptions);
                }
                if (currentLevelText.text == "Nivel 4")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevelOptions.Add("Nivel 1");
                    selectLevelOptions.Add("Nivel 2");
                    selectLevelOptions.Add("Nivel 3");
                    selectLevelOptions.Add("Nivel 4");
                    selectLevel.AddOptions(selectLevelOptions);
                }
                if (currentLevelText.text == "Nivel 5")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevelOptions.Add("Nivel 1");
                    selectLevelOptions.Add("Nivel 2");
                    selectLevelOptions.Add("Nivel 3");
                    selectLevelOptions.Add("Nivel 4");
                    selectLevelOptions.Add("Nivel 5");
                    selectLevel.AddOptions(selectLevelOptions);
                }
                if (currentLevelText.text == "Nivel 6")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevelOptions.Add("Nivel 1");
                    selectLevelOptions.Add("Nivel 2");
                    selectLevelOptions.Add("Nivel 3");
                    selectLevelOptions.Add("Nivel 4");
                    selectLevelOptions.Add("Nivel 5");
                    selectLevelOptions.Add("Nivel 6");
                    selectLevel.AddOptions(selectLevelOptions);
                }
                if (currentLevelText.text == "Nivel 7")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevelOptions.Add("Nivel 1");
                    selectLevelOptions.Add("Nivel 2");
                    selectLevelOptions.Add("Nivel 3");
                    selectLevelOptions.Add("Nivel 4");
                    selectLevelOptions.Add("Nivel 5");
                    selectLevelOptions.Add("Nivel 6");
                    selectLevelOptions.Add("Nivel 7");
                    selectLevel.AddOptions(selectLevelOptions);
                }
                if (currentLevelText.text == "Nivel 8")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevelOptions.Add("Nivel 1");
                    selectLevelOptions.Add("Nivel 2");
                    selectLevelOptions.Add("Nivel 3");
                    selectLevelOptions.Add("Nivel 4");
                    selectLevelOptions.Add("Nivel 5");
                    selectLevelOptions.Add("Nivel 6");
                    selectLevelOptions.Add("Nivel 7");
                    selectLevelOptions.Add("Nivel 8");
                    selectLevel.AddOptions(selectLevelOptions);
                }
                if (currentLevelText.text == "Nivel 9")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevelOptions.Add("Nivel 1");
                    selectLevelOptions.Add("Nivel 2");
                    selectLevelOptions.Add("Nivel 3");
                    selectLevelOptions.Add("Nivel 4");
                    selectLevelOptions.Add("Nivel 5");
                    selectLevelOptions.Add("Nivel 6");
                    selectLevelOptions.Add("Nivel 7");
                    selectLevelOptions.Add("Nivel 8");
                    selectLevelOptions.Add("Nivel 9");
                    selectLevel.AddOptions(selectLevelOptions);
                }
                if (currentLevelText.text == "Nivel 10")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevelOptions.Add("Nivel 1");
                    selectLevelOptions.Add("Nivel 2");
                    selectLevelOptions.Add("Nivel 3");
                    selectLevelOptions.Add("Nivel 4");
                    selectLevelOptions.Add("Nivel 5");
                    selectLevelOptions.Add("Nivel 6");
                    selectLevelOptions.Add("Nivel 7");
                    selectLevelOptions.Add("Nivel 8");
                    selectLevelOptions.Add("Nivel 9");
                    selectLevelOptions.Add("Nivel 10");
                    selectLevel.AddOptions(selectLevelOptions);
                }
                if (currentLevelText.text == "Nivel 11")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevelOptions.Add("Nivel 1");
                    selectLevelOptions.Add("Nivel 2");
                    selectLevelOptions.Add("Nivel 3");
                    selectLevelOptions.Add("Nivel 4");
                    selectLevelOptions.Add("Nivel 5");
                    selectLevelOptions.Add("Nivel 6");
                    selectLevelOptions.Add("Nivel 7");
                    selectLevelOptions.Add("Nivel 8");
                    selectLevelOptions.Add("Nivel 9");
                    selectLevelOptions.Add("Nivel 10");
                    selectLevelOptions.Add("Nivel 11");
                    selectLevel.AddOptions(selectLevelOptions);
                }
                if (currentLevelText.text == "Nivel 12")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevelOptions.Add("Nivel 1");
                    selectLevelOptions.Add("Nivel 2");
                    selectLevelOptions.Add("Nivel 3");
                    selectLevelOptions.Add("Nivel 4");
                    selectLevelOptions.Add("Nivel 5");
                    selectLevelOptions.Add("Nivel 6");
                    selectLevelOptions.Add("Nivel 7");
                    selectLevelOptions.Add("Nivel 8");
                    selectLevelOptions.Add("Nivel 9");
                    selectLevelOptions.Add("Nivel 10");
                    selectLevelOptions.Add("Nivel 11");
                    selectLevelOptions.Add("Nivel 12");
                    selectLevel.AddOptions(selectLevelOptions);
                }
                if (currentLevelText.text == "Nivel 13")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevelOptions.Add("Nivel 1");
                    selectLevelOptions.Add("Nivel 2");
                    selectLevelOptions.Add("Nivel 3");
                    selectLevelOptions.Add("Nivel 4");
                    selectLevelOptions.Add("Nivel 5");
                    selectLevelOptions.Add("Nivel 6");
                    selectLevelOptions.Add("Nivel 7");
                    selectLevelOptions.Add("Nivel 8");
                    selectLevelOptions.Add("Nivel 9");
                    selectLevelOptions.Add("Nivel 10");
                    selectLevelOptions.Add("Nivel 11");
                    selectLevelOptions.Add("Nivel 12");
                    selectLevelOptions.Add("Nivel 13");
                    selectLevel.AddOptions(selectLevelOptions);
                }
                if (currentLevelText.text == "Nivel 14")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevelOptions.Add("Nivel 1");
                    selectLevelOptions.Add("Nivel 2");
                    selectLevelOptions.Add("Nivel 3");
                    selectLevelOptions.Add("Nivel 4");
                    selectLevelOptions.Add("Nivel 5");
                    selectLevelOptions.Add("Nivel 6");
                    selectLevelOptions.Add("Nivel 7");
                    selectLevelOptions.Add("Nivel 8");
                    selectLevelOptions.Add("Nivel 9");
                    selectLevelOptions.Add("Nivel 10");
                    selectLevelOptions.Add("Nivel 11");
                    selectLevelOptions.Add("Nivel 12");
                    selectLevelOptions.Add("Nivel 13");
                    selectLevelOptions.Add("Nivel 14");
                    selectLevel.AddOptions(selectLevelOptions);
                }
                if (currentLevelText.text == "Nivel 15")
                {
                    selectLevelOptions.Add("Tutorial");
                    selectLevelOptions.Add("Nivel 1");
                    selectLevelOptions.Add("Nivel 2");
                    selectLevelOptions.Add("Nivel 3");
                    selectLevelOptions.Add("Nivel 4");
                    selectLevelOptions.Add("Nivel 5");
                    selectLevelOptions.Add("Nivel 6");
                    selectLevelOptions.Add("Nivel 7");
                    selectLevelOptions.Add("Nivel 8");
                    selectLevelOptions.Add("Nivel 9");
                    selectLevelOptions.Add("Nivel 10");
                    selectLevelOptions.Add("Nivel 11");
                    selectLevelOptions.Add("Nivel 12");
                    selectLevelOptions.Add("Nivel 13");
                    selectLevelOptions.Add("Nivel 14");
                    selectLevelOptions.Add("Nivel 15");
                    selectLevel.AddOptions(selectLevelOptions);
                }
            }
        }
    }

    private IEnumerator UpdateXp(int _xp)
    {
        //Set the currently logged in user xp
        var DBTask = DBreference.Child("users").Child(userID).Child("xp").SetValueAsync(_xp);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Xp is now updated
        }
    }

    private IEnumerator ResetPassword(string emailAddress)
    {
        emailAddress = emailLoginField.text;
        var ResetPasswordTask = auth.SendPasswordResetEmailAsync(emailAddress);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => ResetPasswordTask.IsCompleted);

        if (ResetPasswordTask.IsCanceled)
        {
            Debug.LogError("SendPasswordResetEmailAsync was canceled.");
        }

        if (ResetPasswordTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to reset password task with {ResetPasswordTask.Exception}");
            FirebaseException firebaseEx = ResetPasswordTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Reset password Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Falta el correo electrónico";
                    break;
                case AuthError.InvalidEmail:
                    message = "Correo electrónico incorrecto";
                    break;
                case AuthError.UserNotFound:
                    message = "No existe usuario";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            Debug.Log("Password reset email sent successfully.");
            warningLoginText.text = "";
            confirmLoginText.text = "Correo de recuperación de contraseña enviado correctamente";
        }
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Error de autenticación";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Falta el correo electrónico";
                    break;
                case AuthError.MissingPassword:
                    message = "Falta la contraseña";
                    break;
                case AuthError.WrongPassword:
                    message = "Contraseña incorrecta";
                    break;
                case AuthError.InvalidEmail:
                    message = "Correo electrónico incorrecto";
                    break;
                case AuthError.UserNotFound:
                    message = "No existe usuario";
                    break;
            }
            warningLoginText.text = message;

            if (message == "No existe usuario")
            {
                warningLoginText.text = "";
                confirmLoginText.text = "Creando usuario...";
                yield return new WaitForSeconds(2);
                UIManager.instance.DisclaimerMenu();
                emailRegisterField.text = emailLoginField.text;
                passwordRegisterField.text = passwordLoginField.text;
                confirmLoginText.text = "";
                
                ClearLoginFields();
                ClearRegisterFields();
            }
        }
        else
        {
            //User is now logged in
            //Now get the result
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            SaveEncryptedCredentials(_email, _password);
            warningLoginText.text = "";
            confirmLoginText.text = "Autenticación correcta";
            StartCoroutine(LoadUserData());

            yield return new WaitForSeconds(2);

            UIManager.instance.UserDataScreen(); // Change to user data UI
            
            confirmLoginText.text = "";
            ClearLoginFields();
            ClearRegisterFields();
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            warningRegisterText.text = "Falta el nombre de usuario";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            warningRegisterText.text = "Las contraseñas no coinciden";
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Falta el correo electrónico";
                        break;
                    case AuthError.MissingPassword:
                        message = "Falta la contraseña";
                        break;
                    case AuthError.WeakPassword:
                        message = "Contraseña débil";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Correo electrónico ya registrado";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result;

                if (User != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "Nombre de usuario no establecido";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        SaveData();
                        warningLoginText.text="";
                        confirmLoginText.text = "";
                        UIManager.instance.LoadUserMenu();
                        warningRegisterText.text = "";
                        ClearRegisterFields();
                        ClearLoginFields();
                    }
                }
            }
        }
    }

    private IEnumerator LoadScoreboardData()
    {
        //Get all the users data ordered by kills amount
        var DBTask = DBreference.Child("Usuarios").OrderByChild("Puntuación total").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            //Destroy any existing scoreboard elements
            foreach (Transform child in scoreboardContent.transform)
            {
                Destroy(child.gameObject);
            }

            //Loop through every users UID
            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                string username = childSnapshot.Child("Nombre de usuario").Value.ToString();
                int score = int.Parse(childSnapshot.Child("Puntuación total").Value.ToString());
                string currentLevel = childSnapshot.Child("Nivel actual de juego").Value.ToString();


                //Instantiate new scoreboard elements
                GameObject scoreboardElement = Instantiate(scoreElement, scoreboardContent);
                scoreboardElement.GetComponent<ScoreElement>().NewScoreElement(username, score, currentLevel);
            }

            //Go to scoareboard screen
            UIManager.instance.ScoreboardScreen();
        }
    }

    public void PlayGame()
    {

        UIManager.instance.ClearScreen();
        if (selectLevel == null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            Dictionary<int, string> levels = new Dictionary<int, string>();
            levels.Add(0,"0_tutorial");
            levels.Add(1,"denom_cocina");
            levels.Add(2,"relacionar_cocina");
            levels.Add(3, "relacionar_platos");
            levels.Add(4, "1_1_denominacion");
            levels.Add(5, "1_2_frases");
            levels.Add(6, "definiciones_humanos");
            levels.Add(7, "denom_cuerpo");
            levels.Add(8, "relacionar_profesiones");
            levels.Add(9, "contar_silabas");
            levels.Add(10, "colores");
            levels.Add(11, "denom_transporte");
            levels.Add(12, "categorias_transporte");
            levels.Add(13, "1_3_silabas");
            levels.Add(14, "silabas_lugares");
            levels.Add(15, "nivel_final");

            SceneManager.LoadScene(levels[selectLevel.value]);

        }  
    }
}
