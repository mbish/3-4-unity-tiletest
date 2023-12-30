using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class KeyMovement : MonoBehaviour
{
    public float speed = 5f;
    public float altitude = 0;
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

    void move(Vector3 v) {
        Vector2 currentPosition = (Vector2) gameObject.transform.position;
        float newY = currentPosition.y;
        Tilemap highestTilemap = null;
        foreach (var tilemap in tileMapParent.GetComponentsInChildren<Tilemap>()){
            var tilemapZ = tilemap.transform.position.z;
            var adjustedPosition = new Vector3(currentPosition.x, currentPosition.y - tilemapZ + altitude);
            var cellPosition = tilemap.WorldToCell(adjustedPosition);
            var tile = tilemap.GetTile(cellPosition);
            if(tile) {
                if(highestTilemap == null || tilemapZ < highestTilemap.transform.position.z) {
                    highestTilemap = tilemap;
                }
            }
        }
        transform.position += v;
        if (highestTilemap) {
            transform.position +=  new Vector3(0, altitude - highestTilemap.transform.position.z, -(altitude - highestTilemap.transform.position.z));
            altitude = highestTilemap.transform.position.z;
        }
    }
}
