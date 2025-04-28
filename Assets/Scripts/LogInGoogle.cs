using System;
using UnityEngine;

public class LogInGoogle : MonoBehaviour
{
    private string clientId = "533627938955-l5b18rcahau188jbu4ta1r6ik2m47o6l.apps.googleusercontent.com";
    private string redirectUri = "http://localhost:8080/user/login/google";

    public void signInWithGoogle()
    {
        string scope = Uri.EscapeDataString("openid email profile");
        string encodedRedirectUri = Uri.EscapeDataString(redirectUri);

        string oauthUrl = "https://accounts.google.com/o/oauth2/v2/auth" +
            "?client_id=" + clientId +
            "&response_type=code" +
            "&scope=" + scope +
            "&redirect_uri=" + encodedRedirectUri +
            "&access_type=offline";

        Debug.Log("Opening OAuth URL: " + oauthUrl);
        Application.OpenURL(oauthUrl);
    }
}
    