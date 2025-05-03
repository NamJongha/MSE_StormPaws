using JetBrains.Annotations;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LogInGoogle : MonoBehaviour
{
    private string clientId = "533627938955-l5b18rcahau188jbu4ta1r6ik2m47o6l.apps.googleusercontent.com";
    private string redirectUri = "http://localhost:5000/";

    //�ܺ� �������� ���� redirect uri�� code�� ���� -> uri�� ����Ƽ ���� ������ ��� ����Ƽ�� ���޵�
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