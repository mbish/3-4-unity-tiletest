using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Altitude : MonoBehaviour
{
    public int value = 0;
    public bool dynamic = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake() {
        if(dynamic) {
            adjustRenderingOrder(value);
        }
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
