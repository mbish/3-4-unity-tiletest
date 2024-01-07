using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
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
    int _getAltitudeOfTileWithAdjustment(Vector2 position, Func<int,int> adjustment, Tilemap[] tilemaps) {
        int highestTilemap = 0;
        foreach (var tilemap in tilemaps){
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
        return _getAltitudeOfTileWithAdjustment(position, (int x) => 0, tileMapParent.GetComponentsInChildren<Tilemap>());

    }

    int getAltitudeOfTile(Vector2 position) {
        return _getAltitudeOfTileWithAdjustment(position, (int tilemapAltitude) => tilemapAltitude - altitude.value, tileMapParent.GetComponentsInChildren<Tilemap>());
    }

    Vector3 jumpToAltitude(int newAltitude) {
        var oldAltitude = altitude.value;
        altitude.changeAltitude(newAltitude);
        return new Vector2(0, newAltitude - oldAltitude);
    }

    List<RaycastHit2D> CastAtAltitude<T>(int castAltitude, Vector3 direction, ContactFilter2D filter, float distance) where T : Component {
        List<RaycastHit2D> results = new List<RaycastHit2D>();
        List<RaycastHit2D> filtered = new List<RaycastHit2D>();
        var hits = objectBase.Cast(direction, filter, results, distance);
        foreach(var hit in results) {
            var hitAltitude = hit.transform.gameObject.GetComponent<Altitude>();
            if(hitAltitude && hitAltitude.value == castAltitude) {
                if(hit.transform.gameObject.GetComponent<T>() != null) {
                    filtered.Add(hit);
                }
            }
        }
        return filtered;
    }

    List<int> GetAltitudes() {
        HashSet<int> altitudes = new HashSet<int>();
        foreach(var tilemap in tileMapParent.GetComponentsInChildren<Tilemap>()) {
            if (tilemap.TryGetComponent<Altitude>(out Altitude tilemapAltitude)) {
                altitudes.Add(tilemapAltitude.value);
            }
        }
        return altitudes.ToList();
    }

    public Vector3 move2(Vector2 v) {
        var filter = new ContactFilter2D();
        var startingPosition = new Vector2(objectBase.offset.x, objectBase.offset.y);
        foreach(var worldAltitude in GetAltitudes()) {
            objectBase.offset = startingPosition + new Vector2(0, worldAltitude);
            var hits = CastAtAltitude<Tilemap>(worldAltitude, v, filter.NoFilter(), v.magnitude);
            foreach(var hit in hits) {
                objectBase.offset = startingPosition;
                return Vector3.zero;
            }
        }
        objectBase.offset = startingPosition;
        return v;

        // var filter = new ContactFilter2D();

        // // this can't work because the colliders are in the wrong position.
        // // maybe we dynamically offset the tilemap colliders on Start if
        // // they have an altitude?
        // // alternatively we could re-do the cast at each altitude by adjusting the origin's Y coordinates
        // // at that point I might just want to do a boxcast?
        // var hits = objectBase.Cast(v, filter.NoFilter(), v.magnitude);
        // Debug.Log(hits);
        // if(hits == 0) {
        //     return v;
        // }
        // var minimumHitAtElevation = v;
        // foreach(var hit in results)  {
        //     var tilemap = hit.transform.gameObject.GetComponent<Tilemap>();
        //     if(tilemap) {
        //         Tilemap[] tilemaps = {tilemap};
        //         Debug.Log("checking with out point");
        //         Debug.Log(hit.point);
        //         var newAltitude = _getAltitudeOfTileWithAdjustment(hit.point, (int tilemapAltitude) => tilemapAltitude - altitude.value, tilemaps);
        //         Debug.Log("new altitude");
        //         Debug.Log(newAltitude);
        //         if(newAltitude != altitude.value) {
        //             if(hit.distance < minimumHitAtElevation.magnitude) {
        //                 minimumHitAtElevation = v.normalized * hit.distance;
        //             }
        //         }
        //     }
        // }
        // return minimumHitAtElevation;

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
                // try to move a smaller distance
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
