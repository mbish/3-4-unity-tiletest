using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(Altitude))]
public class RespectTileAltitude : MonoBehaviour
{
    public GameObject tileMapParent;
    public BoxCollider2D objectBase;
    private Altitude altitude;
    private bool isEnabled = true;

    void Awake() {
        altitude = GetComponent<Altitude>();
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

    Vector3 jumpToAltitude(int newAltitude) {
        var oldAltitude = altitude.value;
        altitude.changeAltitude(newAltitude);
        return new Vector2(0, newAltitude - oldAltitude);
    }

    public Vector3 move(Vector2 v) {
        const float Epsilon = 0.00005f;
        // if we're not enabled we don't make any adjustments to the passed in vector
        if(!isEnabled) {
            return v;
        }
        if(v.sqrMagnitude < Epsilon) {
            return Vector3.zero;
        }

        var topLeft = objectBase.bounds.min;
        var bottomRight = objectBase.bounds.max;
        var topRight = objectBase.bounds.min + new Vector3(objectBase.bounds.size.x, 0, 0);
        var bottomLeft = objectBase.bounds.min + new Vector3(0, objectBase.bounds.size.y, 0);
        Vector3[] points = {topLeft, bottomRight, topRight, bottomLeft};

        var highestAltitude = -1;
        foreach(var point in points) {
            var newAltitude = getAltitudeOfTile(point + (Vector3) v);
            if(newAltitude > altitude.value) {
                return move(v/2);
            } else {
                highestAltitude = Math.Max(newAltitude, highestAltitude);
            }
        }
        if(highestAltitude < altitude.value) {
            return jumpToAltitude(highestAltitude);
        }
        return v;
    }

    public bool willDisablingChangeAltitude() {
        if(!isEnabled) {
            return false;
        }

        var newAltitude = getAltitudeOfTileIgnoringAltitude(objectBase.transform.position);
        return newAltitude != altitude.value;
    }

    public void enable() {
        if(!isEnabled) {
            isEnabled = true;
            altitude.changeAltitude(getAltitudeOfTileIgnoringAltitude(objectBase.transform.position));
        }
    }
    public void disable() {
        if(isEnabled) {
            isEnabled = false;
        }
    }
}
