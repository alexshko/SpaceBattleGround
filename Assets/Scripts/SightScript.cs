using UnityEngine;
using UnityEngine.UI;

public class SightScript : MonoBehaviour
{
    [SerializeField] private RawImage sightImage;

    private void Start()
    {
        sightImage.color = new Color(1, 1, 1, 0.75f);
    }

    private void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 50f))
        {
            if (hit.transform.gameObject.CompareTag("Player"))
            {
                sightImage.color = new Color(1, 0, 0, 0.75f);
            }
        }
        else
        {
            sightImage.color = new Color(1, 1, 1, 0.75f);
        }
    }
}