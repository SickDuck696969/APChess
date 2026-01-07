using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public class UnityStartup : MonoBehaviour
{
    public static bool Ready = false;

    private async void Start()
    {
        Debug.Log("[UnityStartup] Start() Running...");

        var env = "production";
        var options = new InitializationOptions().SetEnvironmentName(env);

        await UnityServices.InitializeAsync(options);
        Debug.Log("[UnityStartup] UnityServices initialized.");
        Debug.Log("[Services] Environment = " + env);

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("[UnityStartup] Signed in. PlayerID=" + AuthenticationService.Instance.PlayerId);

        Ready = true;
    }
}
