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
    private void OnTriggerStay2D(Collider2D other) {
        other.SendMessage("enterStairs");
    }
    private void OnTriggerExit2D(Collider2D other) {
        other.SendMessage("exitStairs");
    }
}
