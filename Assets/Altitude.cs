using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Altitude : MonoBehaviour
{
    public int value = 0;
    public Collider2D objectBase;
    private Vector2 startingPosition;

    T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy as T;
    }

    void Awake() {
        startingPosition = new Vector2(objectBase.offset.x, objectBase.offset.y);
        adjustCollider();
    }


    void adjustCollider() {
        var localAltitudeOffset = (Vector2) objectBase.gameObject.transform.InverseTransformVector(new Vector2(0, value));
        objectBase.offset = startingPosition - localAltitudeOffset;
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
        adjustCollider();
        value = newValue;
    }
}
