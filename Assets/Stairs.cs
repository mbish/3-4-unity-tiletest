using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Stairs : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void attemptWithComponent<T>(GameObject o, Action<T> callback) {
        var component = o.GetComponent<T>();
        if(component != null) {
            callback(component);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        attemptWithComponent<KeyMovement>(other.gameObject, player => {
            player.enterStairs();
        });
    }
    private void OnTriggerExit2D(Collider2D other) {
        attemptWithComponent<KeyMovement>(other.gameObject, player => {
            player.exitStairs();
        });
    }
}
