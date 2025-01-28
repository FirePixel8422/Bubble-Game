using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;



public static class SceneManager
{
    public static void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, mode);
    }

    public static AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, bool autoLoadSceneWhenFinished = true)
    {
        AsyncOperation loadSceneOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);

        loadSceneOperation.allowSceneActivation = autoLoadSceneWhenFinished;

        return loadSceneOperation;
    }

    public static AsyncOperation UnLoadSceneAsync(string sceneName)
    {
        return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
    }

    public static void SetActiveScene(string sceneName)
    {
        Scene sceneToSetActive = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);

        UnityEngine.SceneManagement.SceneManager.SetActiveScene(sceneToSetActive);
    }

    [ServerRpc(RequireOwnership = false)]
    public static SceneEventProgressStatus LoadSceneOnNetwork_ServerRPC(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        return NetworkManager.Singleton.SceneManager.LoadScene(sceneName, mode);
    }

    public static SceneEventProgressStatus LoadSceneOnNetwork(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        return NetworkManager.Singleton.SceneManager.LoadScene(sceneName, mode);
    }
}
