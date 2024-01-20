using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Stairs : MonoBehaviour
{
    private BoxCollider2D box;
    void Awake() {
        box = GetComponent<BoxCollider2D>();
    }

    void FixedUpdate() {
    }
    private void OnTriggerStay2D(Collider2D other) {
        if(!other.GetComponent<Altitude>()) {
            Debug.Log(other);
            other.SendMessage("enterStairs");
        }
    }
    private void OnTriggerExit2D(Collider2D other) {
        if(!other.GetComponent<Altitude>()) {
            other.SendMessage("exitStairs");
        }
    }
}
