using UnityEngine;
using Mirror;

public class PickUpObject : MonoBehaviour
{
    [Tooltip("number of extra shots the player will get")]
    public int numOfExtraShots = 100;
    [Tooltip("amount of health restored")]
    public int RestoreHealth = 200;
    [Tooltip("Prefab to play when a player picked an item")]
    public Transform PickUpSpark;

    private AudioSource audioSource;
    public AudioClip pickUpSound;

    [Server]
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            audioSource = GameObject.Find("SoundManager").GetComponent<AudioSource>();
            audioSource.PlayOneShot(pickUpSound);
            Transform expl = Instantiate(PickUpSpark, transform.position, PickUpSpark.rotation);
            NetworkServer.Spawn(expl.gameObject);
            Destroy(gameObject);
        }
    }
}

