using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LoginHandler : MonoBehaviour
{
    public static LoginHandler Instance { get; private set; }
    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }




    [SerializeField] private GameObject blackScreenCover;
    [SerializeField] private GameObject invisibleScreenCover;

    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;

    [SerializeField] private TextMeshProUGUI errorTextField;


#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [Header(">>DEBUG<<")]

    [Header("Make a temp auto account while testing stuff, login automatically")]
    [SerializeField] private bool devAutoLogin;

    [Header("Auto log out before starting the game and after ending it,\noverriden by devAutoLogin")]
    [SerializeField] private bool autoLogOut;

    [SerializeField] private bool printLoginErrors;
#endif


    private const string _invalidUsernameError = "Signing Up failed:\nUsername does not match requirements. Insert only letters, digits and symbols among {., -, _, @}. With a minimum of 3 characters and a maximum of 20";
    private const string _invalidPasswordError = "Signing Up failed:\nPassword does not match requirements. Insert at least 1 uppercase, 1 lowercase, 1 digit and 1 symbol. With minimum 8 characters and a maximum of 30";
    private const string _usernameTakenError = "Signing Up failed:\nUsername is already taken.";

    private const string _emptyFieldError = "Signing In/Up failed:\nPlease fill in all the fields.";
    private const string _wrongLoginInfoError = "Signing In failed:\nAccount doesnt exist or password is wrong.";


    private AsyncOperation mainSceneLoadOperation;


    private async void Start()
    {
        mainSceneLoadOperation = SceneManager.LoadSceneAsync("Setup Network", LoadSceneMode.Additive, false);

        mainSceneLoadOperation.completed += (AsyncOperation sceneLoadOperation) =>
        {
            SceneManager.UnLoadSceneAsync("Login Screen");
        };

        await UnityServices.InitializeAsync();

        TryAutoLoginWithSessionToken();
    }


    private async void TryAutoLoginWithSessionToken()
    {
        blackScreenCover.SetActive(true);

#if UNITY_EDITOR || DEVELOPMENT_BUILD

        //while in dev mode, create a temp account and auto login everytime.
        if (devAutoLogin)
        {
            AuthenticationService.Instance.SignOut();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            mainSceneLoadOperation.allowSceneActivation = true;

            print("Logged in with auto dev account automatically");

            return;
        }

        //logout of logged in account before any code runs.
        if (autoLogOut && AuthenticationService.Instance.SessionTokenExists)
        {
            AuthenticationService.Instance.SignOut();
            AuthenticationService.Instance.ClearSessionToken();

            print("Session Token cleared, logged out");

            blackScreenCover.SetActive(false);

            return;
        }
#endif

        //login with previously logged in account if that SessionToken is still valid
        if (AuthenticationService.Instance.SessionTokenExists)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            mainSceneLoadOperation.allowSceneActivation = true;

            print("Logged in with cached account automatically, name: " + AuthenticationService.Instance.PlayerInfo.Username);

            return;
        }

        blackScreenCover.SetActive(false);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            InputFieldSelectionStarted();

            if (usernameField.isFocused)
            {
                usernameField.DeactivateInputField();
                passwordField.ActivateInputField();
            }
            else
            {
                passwordField.DeactivateInputField();
                usernameField.ActivateInputField();
            }
        }
    }


    //reset errorField when username or password is being edited
    public void InputFieldSelectionStarted()
    {
        errorTextField.text = "";
    }


    public void UpdateUsername(string newUsername)
    {
        if (string.IsNullOrEmpty(newUsername))
        {
            return;
        }

        bool valid = IsCharacterValid(newUsername[^1]);

        if (valid == false)
        {
            usernameField.text = newUsername.Substring(0, newUsername.Length - 1);
        }
    }



    /// <summary>
    /// Return if char is an unvalid character
    /// </summary>
    /// <param name="addedChar"></param>
    /// <returns></returns>
    private bool IsCharacterValid(char addedChar)
    {
        if (char.IsLetterOrDigit(addedChar) || addedChar == '_' || addedChar == '-' || addedChar == '@' || addedChar == '.')
        {
            return true; // new typed character is valid
        }
        else
        {
            return false;
        }
    }



    public async void TrySignIn() => await TrySignInAsync(usernameField.text, passwordField.text);

    /// <summary>
    /// Try to sign in with username and password
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    private async Task TrySignInAsync(string username, string password)
    {
        invisibleScreenCover.SetActive(true);

        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);

            mainSceneLoadOperation.allowSceneActivation = true;

            print("Player is signed in with name: " + AuthenticationService.Instance.PlayerInfo.Username);
        }
        catch (Exception ex)
        {
            string exString = ex.ToString();

            //Username doesnt exist or password is wrong
            if (exString.StartsWith("Unity.Services.Core.RequestFailedException: Invalid username or password"))
            {
                errorTextField.text = _wrongLoginInfoError;
            }
            //Username and/or Password are not in the correct format (one of the fields are empty)
            else if (exString.StartsWith("Unity.Services.Authentication.AuthenticationException: Username and/or Password are not in the correct format"))
            {
                errorTextField.text = _emptyFieldError;
            }

            invisibleScreenCover.SetActive(false);


#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (printLoginErrors)
            {
                print(ex);
            }
#endif
        }
    }




    public async void TrySignUp() => await TrySignUpAsync(usernameField.text, passwordField.text);

    /// <summary>
    /// Try to sign up with username and password
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    private async Task TrySignUpAsync(string username, string password)
    {
        invisibleScreenCover.SetActive(true);

        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);

            mainSceneLoadOperation.allowSceneActivation = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            PlayerInfo playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();

            print("Player is signed up with name: " + AuthenticationService.Instance.PlayerInfo.Username);
#endif
        }
        catch (AuthenticationException ex)
        {
            string exString = ex.ToString();

            //Username is already taken
            if (exString.StartsWith("Unity.Services.Authentication.AuthenticationException: username already exists"))
            {
                errorTextField.text = _usernameTakenError;
            }
            //Username and/or Password are not in the correct format (one of the fields are empty
            else if (exString.StartsWith("Unity.Services.Authentication.AuthenticationException: Username and/or Password are not in the correct format"))
            {
                errorTextField.text = _emptyFieldError;
            }

            invisibleScreenCover.SetActive(false);


#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (printLoginErrors)
            {
                print(ex);
            }
#endif
        }
        catch (RequestFailedException ex)
        {
            string exString = ex.ToString();

            //Username does not match requirements. Insert only letters, digits and symbols among {., -, _, @}. With a minimum of 3 characters and a maximum of 20
            if (exString.StartsWith("Unity.Services.Core.RequestFailedException: Username does not match requirements"))
            {
                errorTextField.text = _invalidUsernameError;

                print("printed");
            }
            //Password does not match requirements. Insert at least 1 uppercase, 1 lowercase, 1 digit and 1 symbol. With minimum 8 characters and a maximum of 30
            else if (exString.StartsWith("Unity.Services.Core.RequestFailedException: Password does not match requirements"))
            {
                errorTextField.text = _invalidPasswordError;
            }


#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (printLoginErrors)
            {
                print(ex);
            }
#endif
        }
    }



#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void OnApplicationQuit()
    {
        if ((devAutoLogin || autoLogOut) && AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut();
            AuthenticationService.Instance.ClearSessionToken();

            Debug.Log("Session token cleared on application quit.");
        }
    }
#endif
}
