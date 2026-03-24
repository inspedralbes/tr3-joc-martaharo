using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    private const string BASE_URL = "http://localhost:3000/api";
    private string _authToken;
    public string AuthToken => _authToken;
    public string CurrentUsername { get; private set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(_authToken);

    public event Action<bool, string> OnLoginResponse;
    public event Action<bool, string> OnRegisterResponse;
    public event Action OnLogout;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register(string username, string password)
    {
        StartCoroutine(RegisterCoroutine(username, password));
    }

    public void Login(string username, string password)
    {
        StartCoroutine(LoginCoroutine(username, password));
    }

    public void Logout()
    {
        if (IsLoggedIn)
        {
            StartCoroutine(LogoutCoroutine());
        }
        _authToken = null;
        CurrentUsername = null;
        OnLogout?.Invoke();
    }

    private IEnumerator RegisterCoroutine(string username, string password)
    {
        string json = JsonUtility.ToJson(new LoginRequest(username, password));
        using (UnityWebRequest request = new UnityWebRequest(BASE_URL + "/register", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                _authToken = response.token;
                CurrentUsername = response.username;
                OnRegisterResponse?.Invoke(true, response.username);
            }
            else
            {
                ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text);
                OnRegisterResponse?.Invoke(false, error.error);
            }
        }
    }

    private IEnumerator LoginCoroutine(string username, string password)
    {
        string json = JsonUtility.ToJson(new LoginRequest(username, password));
        using (UnityWebRequest request = new UnityWebRequest(BASE_URL + "/login", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                _authToken = response.token;
                CurrentUsername = response.username;
                OnLoginResponse?.Invoke(true, response.username);
            }
            else
            {
                ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text);
                OnLoginResponse?.Invoke(false, error.error);
            }
        }
    }

    private IEnumerator LogoutCoroutine()
    {
        using (UnityWebRequest request = new UnityWebRequest(BASE_URL + "/logout", "POST"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + _authToken);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();
        }
    }

    public IEnumerator VerifyToken(string token, Action<bool, string> callback)
    {
        using (UnityWebRequest request = new UnityWebRequest(BASE_URL + "/verify", "GET"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                VerifyResponse response = JsonUtility.FromJson<VerifyResponse>(request.downloadHandler.text);
                callback(response.valid, response.username);
            }
            else
            {
                callback(false, null);
            }
        }
    }

    [Serializable]
    private class LoginRequest
    {
        public string username;
        public string password;

        public LoginRequest(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }

    [Serializable]
    private class LoginResponse
    {
        public string token;
        public string username;
    }

    [Serializable]
    private class ErrorResponse
    {
        public string error;
    }

    [Serializable]
    private class VerifyResponse
    {
        public string username;
        public bool valid;
    }
}
