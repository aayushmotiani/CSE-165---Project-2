using UnityEngine;

public class Colluisiontest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision start!");
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Collision with Ground detected!");
        }
    }
}
