using UnityEngine;

public class MovingStarProp : MonoBehaviour
{
    public float speedOfMove = 5000;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(speedOfMove * Vector3.forward * Time.deltaTime);
    }
}
