using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



[RequireComponent(typeof(Altitude))]
[RequireComponent(typeof(Rigidbody2D))]
public class KeyMovement : MonoBehaviour
{
    public float speed = 5f;
    public bool onStairs = false;
    public GameObject tileMapParent;
    private Altitude altitude;
    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake() {
        altitude = GetComponent<Altitude>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            move(new Vector2(speed * Time.deltaTime, 0f));
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            move(new Vector2(-speed * Time.deltaTime, 0f));
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            move(new Vector2(0f, speed * Time.deltaTime));
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            move(new Vector2(0f, -speed * Time.deltaTime));
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
        if(!onStairs) {
            var highestTilemap = getAltitudeOfTile(rb.position + v);
            jumpToAltitude(highestTilemap);
        } else {
            // ignore altitude when calculating tile position on stairs
            var newAltitude = getAltitudeOfTileIgnoringAltitude(rb.position + v);
            altitude.changeAltitude(newAltitude);
        }
        rb.position += v;
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
            var newAltitude = getAltitudeOfTileIgnoringAltitude(transform.position);
            if(newAltitude == altitude.value) {
                renderInHigherLayer();
                onStairs = true;
            }
        }
    }

    public void exitStairs() {
        if(onStairs) {
            var newAltitude = getAltitudeOfTile(transform.position);
            altitude.changeAltitude(newAltitude);
            renderInDefaultLayer();
        }
        onStairs = false;
    }
}
