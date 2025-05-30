using System;
using UnityEngine;

/// <summary>
/// Callback Personal Information
/// </summary>

public class UserService
{
    private readonly GameManager gameManager = GameManager.Instance;
    private PersonalInfo cachedInfo;

    public void FetchPersonalInfo(Action<PersonalInfo> callback)
    {
        if (cachedInfo != null)
        {
            callback?.Invoke(cachedInfo);
            return;
        }

        GameManager.Instance.StartCoroutine(
            gameManager.GetRequest($"{gameManager.baseUrl}/user/me", (json) =>
            {
                PersonalInfoWrapper wrapper = JsonUtility.FromJson<PersonalInfoWrapper>(json);
                cachedInfo = wrapper.data;
                callback?.Invoke(wrapper.data);
            }));
    }
}