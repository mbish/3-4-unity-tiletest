using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Stairs : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        other.SendMessage("enterStairs");
    }
    private void OnTriggerExit2D(Collider2D other) {
        other.SendMessage("exitStairs");
    }
}
