using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Altitude : MonoBehaviour
{
    public int value = 0;
    public Collider2D objectBase;
    private Vector2 startingPosition;

    void Awake() {
        startingPosition = new Vector2(objectBase.offset.x, objectBase.offset.y);
        adjustCollider();
        adjustRenderingOrder(value);
    }


    void adjustCollider() {
        var localAltitudeOffset = (Vector2) objectBase.gameObject.transform.InverseTransformVector(new Vector2(0, value));
        objectBase.offset = startingPosition - localAltitudeOffset;
    }

    private void adjustRenderingOrder(int delta) {
        var renderer = gameObject.transform.parent.GetComponent<Renderer>();
        if(renderer) {
            renderer.sortingOrder += delta;
        }
    }

    public void changeAltitude(int newValue) {
        Debug.Log("new value: " + newValue, gameObject);
        var difference = newValue - value;
        adjustRenderingOrder(difference);
        value = newValue;
        adjustCollider();
    }
}
