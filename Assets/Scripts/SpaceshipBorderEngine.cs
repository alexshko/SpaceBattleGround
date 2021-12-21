using UnityEngine;

namespace SpaceBattle.Spaceship
{
    public class SpaceshipBorderEngine : MonoBehaviour
    {
        private Transform borderRef;
        private int shaderPlayerPosProperty;
        private void Start()
        {
            borderRef = GameObject.Find("SphereBorder").transform;
            shaderPlayerPosProperty = Shader.PropertyToID("_playerpos");
        }

        private void Update()
        {
            borderRef.GetComponent<MeshRenderer>().material.SetVector(shaderPlayerPosProperty, transform.position);
        }
    }
}
