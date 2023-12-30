using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class KeyMovement : MonoBehaviour
{
    public float speed = 5f;
    public float altitude = 0;
    public bool onStairs = false;
    public GameObject tileMapParent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            move(new Vector3(speed * Time.deltaTime, 0f, 0f));
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            move(new Vector3(-speed * Time.deltaTime, 0f, 0f));
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            move(new Vector3(0f, speed * Time.deltaTime, 0f));
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            move(new Vector3(0f, -speed * Time.deltaTime, 0f));
        }
    }

    float getAltitudeOfCurrentTile() {
        Vector2 currentPosition = (Vector2) gameObject.transform.position;
        float newY = currentPosition.y;
        float highestTilemap = 0;
        foreach (var tilemap in tileMapParent.GetComponentsInChildren<Tilemap>()){
            var tilemapZ = tilemap.transform.position.z;
            var adjustedPosition = new Vector3(currentPosition.x, currentPosition.y - tilemapZ + altitude);
            var cellPosition = tilemap.WorldToCell(adjustedPosition);
            var tile = tilemap.GetTile(cellPosition);
            if(tile) {
                if(tilemapZ < highestTilemap) {
                    highestTilemap = tilemapZ;
                }
            }
        }
        return highestTilemap;
    }

    void jumpToAltitude(float newAltitude) {
        transform.position +=  new Vector3(0, altitude - newAltitude, -(altitude - newAltitude));
        altitude = newAltitude;
    }

    void move(Vector3 v) {
        var highestTilemap = getAltitudeOfCurrentTile();
        transform.position += v;

        if(!onStairs) {
            jumpToAltitude(highestTilemap);
        }
    }

    void renderInHigherLayer() {
        var renderer = gameObject.GetComponent<Renderer>();
        renderer.sortingOrder = 1;
    }

    void renderInDefaultLayer() {
        var renderer = gameObject.GetComponent<Renderer>();
        renderer.sortingOrder = 0;
    }

    public void enterStairs() {
        if(!onStairs) {
            renderInHigherLayer();
        }
        onStairs = true;
    }

    public void exitStairs() {
        if(onStairs) {
            // transform.position +=  new Vector3(0, 0, 1);
            var newAltitude = getAltitudeOfCurrentTile();
            transform.position +=  new Vector3(0, 0, -(altitude - newAltitude));
            altitude = newAltitude;
            renderInDefaultLayer();
        }
        onStairs = false;
    }
}
