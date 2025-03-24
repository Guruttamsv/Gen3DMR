using UnityEngine;

public class RotateAroundPoint : MonoBehaviour
{
    public float rotationSpeed;
    public GameObject pivotObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(pivotObject.transform.position, new Vector3(0, 1, 0), rotationSpeed * Time.deltaTime);
    }
}
