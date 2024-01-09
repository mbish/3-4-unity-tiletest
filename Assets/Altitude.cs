using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Altitude : MonoBehaviour
{
    public int value = 0;
    public Collider2D objectBase;

    void Awake() {
        //objectBase = GetComponent<Collider2D>();
    }

    private void adjustRenderingOrder(int delta) {
        var renderer = gameObject.GetComponent<Renderer>();
        if(renderer) {
            renderer.sortingOrder += delta;
        }
    }

    public void changeAltitude(int newValue) {
        var difference = newValue - value;
        adjustRenderingOrder(difference);
        value = newValue;
    }
}
