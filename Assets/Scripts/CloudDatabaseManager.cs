using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Collections;
using Unity.VisualScripting;


public class CloudDatabaseManager : MonoBehaviour
{
    [Header("Endpoints")]
    private const string BaseURL =
        "https://accounts.google.com/o/oauth2/auth/";
    const string CLIENT_ID = "52091827088-pbgejjotri0blnaopafj867oasops64k.apps.googleusercontent.com";
    const int PORT = 5000;

    HttpListener listener;

    string pendingAuthCode;

    private const string WEB_CLIENT_ID =
        "115960215654-2mp1q281f1rmj07ovqo724k23269jk66.apps.googleusercontent.com";

    public string sub;

    #region AUTH

    void Update()
    {
        if (!string.IsNullOrEmpty(pendingAuthCode))
        {
            string code = pendingAuthCode;
            pendingAuthCode = null;

            UnityEngine.Debug.Log("Starting token exchange on main thread");
            StartCoroutine(ExchangeCodeForToken(code));
        }
    }

    void ParseToken(string json)
    {
        var token = JsonUtility.FromJson<GoogleTokenResponse>(json);

        if (string.IsNullOrEmpty(token.id_token))
        {
            UnityEngine.Debug.LogError("ID token missing");
            return;
        }

        UnityEngine.Debug.Log("ID TOKEN:");
        UnityEngine.Debug.Log(token.id_token);

        UnityEngine.Debug.Log(GetSubFromIdToken(token.id_token));
        sub = GetSubFromIdToken(token.id_token);
    }

    string GetSubFromIdToken(string idToken)
    {
        string payload = idToken.Split('.')[1];

        // Fix Base64 padding
        payload = payload.Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        string json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        var data = JsonUtility.FromJson<GoogleJwtPayload>(json);

        return data.sub;
    }

    [Serializable]
    class GoogleJwtPayload
    {
        public string sub;
        public string email;
        public string name;
    }




    public void HandleAuthCode(string query)
    {
        UnityEngine.Debug.Log(query);
        var parameters = query.TrimStart('?').Split('&');
        string code = null;

        foreach (var p in parameters)
        {
            if (p.StartsWith("code="))
            {
                code = Uri.UnescapeDataString(p.Substring(5));
                break;
            }
        }

        if (string.IsNullOrEmpty(code))
        {
            UnityEngine.Debug.LogError("Authorization code missing");
            return;
        }

        UnityEngine.Debug.Log("Authorization code: " + code);
        pendingAuthCode = code;
    }


    IEnumerator ExchangeCodeForToken(string code)
    {
        string body =
            "client_id=" + Uri.EscapeDataString(CLIENT_ID) +
            "&client_secret=" + Uri.EscapeDataString("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") +
            "&code=" + Uri.EscapeDataString(code) +
            "&code_verifier=" + Uri.EscapeDataString(codeVerifier) +
            "&redirect_uri=" + Uri.EscapeDataString("http://localhost/") +
            "&grant_type=authorization_code";

        byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

        UnityWebRequest www = new UnityWebRequest(
            "https://oauth2.googleapis.com/token",
            "POST"
        );

        www.uploadHandler = new UploadHandlerRaw(bodyBytes);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            UnityEngine.Debug.LogError("Token request failed");
            UnityEngine.Debug.LogError(www.error);
            UnityEngine.Debug.LogError(www.downloadHandler.text);
            yield break;
        }

        string json = www.downloadHandler.text;
        UnityEngine.Debug.Log("TOKEN JSON:");
        UnityEngine.Debug.Log(json);

        ParseToken(json);
    }





    public void StartListener()
    {
        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost/");
        listener.Start();

        UnityEngine.Debug.Log("Listening on http://localhost/");
        listener.BeginGetContext(OnRequest, null);
    }

    void OnRequest(IAsyncResult result)
    {
        var context = listener.EndGetContext(result);

        string query = context.Request.Url.Query;
        UnityEngine.Debug.Log("Incoming request: " + context.Request.RawUrl);

        // Always respond to browser
        string html = "<html><body>You can close this window.</body></html>";
        byte[] buffer = Encoding.UTF8.GetBytes(html);
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.Close();

        // ❌ Ignore favicon and empty requests
        if (string.IsNullOrEmpty(query) || !query.Contains("code="))
        {
            // 🔁 KEEP LISTENING
            listener.BeginGetContext(OnRequest, null);
            return;
        }

        // ✅ NOW we have the OAuth redirect
        listener.Stop();

        UnityEngine.Debug.Log("OAuth redirect received: " + query);
        HandleAuthCode(query);
    }



    string GenerateCodeVerifier()
    {
        byte[] bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Base64Url(bytes);
    }

    string GenerateCodeChallenge(string verifier)
    {
        using var sha = SHA256.Create();
        return Base64Url(sha.ComputeHash(Encoding.ASCII.GetBytes(verifier)));
    }

    string Base64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    string codeVerifier;
    string codeChallenge;

    public void OpenGoogleLogin()
    {
        codeVerifier = GenerateCodeVerifier();
        codeChallenge = GenerateCodeChallenge(codeVerifier);

        string url =
            "https://accounts.google.com/o/oauth2/v2/auth" +
            "?client_id=" + CLIENT_ID +
            "&response_type=code" +
            "&scope=openid email profile" +
            "&redirect_uri=http://localhost/" +
            "&code_challenge=" + codeChallenge +
            "&code_challenge_method=S256";

        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    public async Task<bool> Register(string username, string email, string password, string userId)
    {
        UserData user = new UserData
        {
            user_id = userId,
            username = username,
            email = email,
            password = password,
            History = "New account"
        };

        string json = JsonUtility.ToJson(user);

        UnityWebRequest req = new UnityWebRequest(BaseURL + "createUser", "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        await req.SendWebRequest();

        return req.result == UnityWebRequest.Result.Success;
    }

    #endregion

    #region INVENTORY

    public async Task<int> CreateInventory(string userId)
    {
        InventoryCreateBody body = new InventoryCreateBody { user_id = userId };

        UnityWebRequest req = new UnityWebRequest(BaseURL + "createInventory", "POST");
        req.uploadHandler = new UploadHandlerRaw(
            Encoding.UTF8.GetBytes(JsonUtility.ToJson(body)));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        await req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            UnityEngine.Debug.LogError(req.downloadHandler.text);
            return -1;
        }

        return JsonUtility.FromJson<InventoryResponse>(
            req.downloadHandler.text).storage_id;
    }

    public async Task<InventoryData> GetInventory(string userId)
    {
        UnityWebRequest req = UnityWebRequest.Get(
            BaseURL + "getInventory?user_id=" + UnityWebRequest.EscapeURL(userId));

        await req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            UnityEngine.Debug.LogError(req.downloadHandler.text);
            return null;
        }

        return JsonUtility.FromJson<InventoryData>(req.downloadHandler.text);
    }

    #endregion
}

[Serializable]
class GoogleTokenResponse
{
    public string id_token;
    public string access_token;
}

[System.Serializable]
public class UserData
{
    public string user_id;
    public string username;
    public string email;
    public string password;
    public string bday;
    public string createwhen;
    public string History;
    public string profilePicBase64;
}

[Serializable]
public class GoogleUser
{
    public string user_id;
    public string email;
    public string username;
}

[Serializable]
public class InventoryResponse
{
    public int storage_id;
    public string message;
}

[Serializable]
public class InventoryData
{
    public int storage_id;
    public string user_id;
}

[Serializable]
public class InventoryCreateBody
{
    public string user_id;
}
