using UnityEngine;
using UnityEngine.UI;

public class UIAccelerationThrottle : MonoBehaviour
{
    [Range(0,1)]
    [SerializeField] private float threshold = 0.15f;
    private Slider throttleRef;

    private void Start()
    {
        throttleRef = GetComponent<Slider>();
        if (!throttleRef)
        {
            Debug.LogError("Missing throttle ref");
        }
    }

    //the function is activated from the Slider ui's handle object, onPointerUP:
    public void AdjustThrottleUIToThreshold()
    {
        if (throttleRef.value <= threshold)
        {
            throttleRef.value = 0;
        }
    }
}
