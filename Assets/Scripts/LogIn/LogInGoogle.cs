using JetBrains.Annotations;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

//njh
public class LogInGoogle : MonoBehaviour
{
    private string clientId = "545083093858-ihct97o56rq64mpuv9j3fns0tdlcdan7.apps.googleusercontent.com";
    private string redirectUri = "http://localhost:5000/";

    public void signInWithGoogle()
    {
        string scope = Uri.EscapeDataString("openid email profile");
        string encodedRedirectUri = Uri.EscapeDataString(redirectUri);

        string oauthUrl = "https://accounts.google.com/o/oauth2/v2/auth" +
            "?client_id=" + clientId +
            "&response_type=code" +
            "&scope=" + scope +
            "&redirect_uri=" + encodedRedirectUri +
            "&access_type=offline" +
            "&prompt=consent";

        Application.OpenURL(oauthUrl);
    }
}