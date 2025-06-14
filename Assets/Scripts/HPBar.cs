using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays and updates an HP bar above a target.
/// </summary>

public class HPBar : MonoBehaviour
{
    public Image fillImage;

    private Transform target;

    public Color healthyColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Updates the HP bar fill amount and color based on current HP.
    public void SetHP(int currentHp, int maxHp)
    {
        float ratio = Mathf.Clamp01((float)currentHp / maxHp);
        fillImage.fillAmount = ratio;

        if (ratio >= 0.6f)
        {
            fillImage.color = healthyColor;
        }
        else if (ratio >= 0.3f)
        {
            fillImage.color = warningColor;
        }
        else
        {
            fillImage.color = dangerColor;
        }
    }

    // Makes the HP bar follow the target's screen position.
    void Update()
    {
        if (target != null)
        {
            Vector3 offset = new Vector3(0, 5f, 0);
            transform.position = Camera.main.WorldToScreenPoint(target.position + offset);
        }
    }
}
