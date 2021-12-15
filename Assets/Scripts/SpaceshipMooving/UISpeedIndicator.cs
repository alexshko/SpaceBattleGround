using TMPro;
using UnityEngine;

public class UISpeedIndicator : MonoBehaviour
{
    public Rigidbody rbSpaceship;

    private void Update()
    {
        if (rbSpaceship)
        {
            GetComponentInChildren<TMP_Text>().text = rbSpaceship.velocity.magnitude.ToString("0.0");
        }
    }
}
