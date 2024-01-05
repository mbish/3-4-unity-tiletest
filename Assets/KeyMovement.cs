using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(RespectTileAltitude))]
public class KeyMovement : MonoBehaviour
{
    public float speed = 5f;
    public bool onStairs = false;
    private Rigidbody2D rb;
    private BoxCollider2D box;
    private RespectTileAltitude tileAltitudeMover;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        tileAltitudeMover = GetComponent<RespectTileAltitude>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate() {
        var movement = new Vector3(0,0,0);
        if (Input.GetKey(KeyCode.RightArrow))
        {
            movement += tileAltitudeMover.move2(new Vector2(speed * Time.fixedDeltaTime, 0f));
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            movement += tileAltitudeMover.move2(new Vector2(-speed * Time.fixedDeltaTime, 0f));
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            movement += tileAltitudeMover.move2(new Vector2(0f, speed * Time.fixedDeltaTime));
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            movement += tileAltitudeMover.move2(new Vector2(0f, -speed * Time.fixedDeltaTime));
        }
        rb.position += (Vector2) movement;
    }


    // bool checkCollision(Vector2 v) {
    //     var sprite = GetComponent<Renderer>();
    //     var ray = (Vector3) (v.normalized);
    //     // var ray = v.normalized;
    //     List<RaycastHit2D> results = new List<RaycastHit2D>();
    //     var filter = new ContactFilter2D();
    //     var hit = Physics2D.Raycast(rb.position, ray);
    //     var hasCollision = false;
    //     rb.position += v;
    //     if(hit.collider != null && hit.rigidbody != null) {
    //         if(rb.IsTouching(hit.collider)) {
    //             Debug.Log("has collision");
    //             hasCollision = true;
    //         }
    //     }
    //     rb.position -= v;
    //     return hasCollision;
    // }

    void renderInHigherLayer() {
        var renderer = GetComponent<Renderer>();
        renderer.sortingOrder += 5;

    }void renderInLowerLayer() {
        var renderer = GetComponent<Renderer>();
        renderer.sortingOrder -= 5;
    }

    public void enterStairs() {
        if(!onStairs && !tileAltitudeMover.willDisablingChangeAltitude()) {
            renderInHigherLayer();
            tileAltitudeMover.disable();
            onStairs = true;
        }
    }

    public void exitStairs() {
        if(onStairs) {
            renderInLowerLayer();
            tileAltitudeMover.enable();
            onStairs = false;
        }
    }
}
