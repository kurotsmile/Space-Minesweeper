using UnityEngine;

public class SkyboxScript : MonoBehaviour
{
    public Vector3 rotation;

	void Update () 
    {
        transform.Rotate(rotation);
	}
}
