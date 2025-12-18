using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusController : MonoBehaviour
{
    public bool isTagger;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    { 
        if (collision.gameObject.tag == "Enemy")
        {
            Debug.Log("Touch");
            PlayerStatusController collisionStatusController = collision.gameObject.GetComponent<PlayerStatusController>();
            if (isTagger)
            {
                Debug.Log("Tag");
                collisionStatusController.isTagger = true;
                isTagger = false;
            }
            else
            {
                if (collisionStatusController.isTagger)
                {
                    Debug.Log("I'm Tagger");
                    isTagger = true;
                    collisionStatusController.isTagger = false;
                }
            }
        }
    }
}
