using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



[RequireComponent(typeof(Altitude))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class KeyMovement : MonoBehaviour
{
    public float speed = 5f;
    public bool onStairs = false;
    public GameObject tileMapParent;
    private Altitude altitude;
    private Rigidbody2D rb;
    private BoxCollider2D box;

    void Awake() {
        altitude = GetComponent<Altitude>();
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate() {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            move(new Vector2(speed * Time.fixedDeltaTime, 0f));
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            move(new Vector2(-speed * Time.fixedDeltaTime, 0f));
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            move(new Vector2(0f, speed * Time.fixedDeltaTime));
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            move(new Vector2(0f, -speed * Time.fixedDeltaTime));
        }
    }

    int _getAltitudeOfTileWithAdjustment(Vector2 position, Func<int,int> adjustment) {
        int highestTilemap = 0;
        foreach (var tilemap in tileMapParent.GetComponentsInChildren<Tilemap>()){
            var tilemapAltitude= tilemap.gameObject.GetComponent<Altitude>().value;
            var adjustedPosition = new Vector2(position.x, position.y + adjustment(tilemapAltitude));
            var cellPosition = tilemap.WorldToCell(adjustedPosition);
            var tile = tilemap.GetTile(cellPosition);
            if(tile) {
                if(tilemapAltitude > highestTilemap) {
                    highestTilemap = tilemapAltitude;
                }
            }
        }
        return (int) highestTilemap;
    }

    int getAltitudeOfTileIgnoringAltitude(Vector2 position) {
        return _getAltitudeOfTileWithAdjustment(position, (int x) => 0);
    }

    int getAltitudeOfTile(Vector2 position) {
        return _getAltitudeOfTileWithAdjustment(position, (int tilemapAltitude) => tilemapAltitude - altitude.value);
    }

    void jumpToAltitude(int newAltitude) {
        rb.position +=  new Vector2(0, newAltitude - altitude.value);
        altitude.changeAltitude(newAltitude);
    }

    void move(Vector2 v) {
        var renderer = GetComponent<Renderer>();

        // need to check all four points of the box actually not just the leading and trailing edges
        var topLeft = box.bounds.min;
        var bottomRight = box.bounds.max;
        var topRight = box.bounds.min + new Vector3(box.bounds.size.x, 0, 0);
        var bottomLeft = box.bounds.min + new Vector3(0, box.bounds.size.y, 0);
        Vector3[] points = {topLeft, bottomRight, topRight, bottomLeft};

        if(!onStairs) {
            var highestAltitude = -1;
            foreach(var point in points) {
                var newAltitude = getAltitudeOfTile(point + (Vector3) v);
                if(newAltitude > altitude.value) {
                    // Debug.Log("Refusing to jump");
                    return;
                } else {
                    highestAltitude = Math.Max(newAltitude, highestAltitude);
                }
            }
            if(highestAltitude < altitude.value) {
                jumpToAltitude(highestAltitude);
            }
        } else {
            // ignore altitude when calculating tile position on stairs
            var newAltitude = getAltitudeOfTileIgnoringAltitude(bottomLeft);
            altitude.changeAltitude(newAltitude);
        }
        if(!checkCollision(v)) {
            rb.position += v;
        }
    }

    bool checkCollision(Vector2 v) {
        var sprite = GetComponent<Renderer>();
        var ray = (Vector3) (v.normalized);
        // var ray = v.normalized;
        List<RaycastHit2D> results = new List<RaycastHit2D>();
        var filter = new ContactFilter2D();
        var hit = Physics2D.Raycast(rb.position, ray);
        var hasCollision = false;
        rb.position += v;
        if(hit.collider != null && hit.rigidbody != null) {
            if(rb.IsTouching(hit.collider)) {
                Debug.Log("has collision");
                hasCollision = true;
            }
        }
        rb.position -= v;
        return hasCollision;
    }

    void renderInHigherLayer() {
        var renderer = GetComponent<Renderer>();
        renderer.sortingOrder++;
    }

    void renderInDefaultLayer() {
        var renderer = GetComponent<Renderer>();
        renderer.sortingOrder--;
    }

    public void enterStairs() {
        if(!onStairs) {
            // only get on the stairs if they're on the same altitude
            var newAltitude = getAltitudeOfTileIgnoringAltitude(rb.position);
            if(newAltitude == altitude.value) {
                renderInHigherLayer();
                onStairs = true;
            }
        }
    }

    public void exitStairs() {
        if(onStairs) {
            var newAltitude = getAltitudeOfTile(rb.position);
            // jumpToAltitude(newAltitude);
            altitude.changeAltitude(newAltitude);
            renderInDefaultLayer();
        }
        onStairs = false;
    }
}
