using JetBrains.Annotations;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LogInGoogle : MonoBehaviour
{
    private string clientId = "533627938955-l5b18rcahau188jbu4ta1r6ik2m47o6l.apps.googleusercontent.com";
    private string redirectUri = "http://localhost:5000/";

    //외부 브라우저를 열고 redirect uri로 code를 전달 -> uri가 유니티 로컬 서버인 경우 유니티로 전달됨
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