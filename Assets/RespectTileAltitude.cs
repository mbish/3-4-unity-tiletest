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

    List<Tilemap> GetTilemapAtAltitude(int atAltitude) {
        List<Tilemap> tilemaps = new List<Tilemap>();
        foreach(var tilemap in tileMapParent.GetComponentsInChildren<Tilemap>()) {
            if (tilemap.TryGetComponent<Altitude>(out Altitude tilemapAltitude)) {
                if(tilemapAltitude.value == atAltitude) {
                    tilemaps.Add(tilemap);
                }
            }
        }
        return tilemaps;
    }

    bool colliderInDirection(Vector2 direction, Collider2D collider) {
        var filter = new ContactFilter2D();
        List<RaycastHit2D> results = new List<RaycastHit2D>();
        var hits = objectBase.Cast(direction, filter.NoFilter(), results);

        foreach(var result in results) {
            if(result.collider == collider) {
                return true;
            }
        }
        return false;
    }

    bool isInside(Collider2D collider) {
        return colliderInDirection(Vector2.up, collider) && colliderInDirection(Vector2.down, collider);
    }

    public Vector3 move2(Vector2 v) {
        var filter = new ContactFilter2D();
        var startingPosition = new Vector2(objectBase.offset.x, objectBase.offset.y);

        // if we don't overlap the current altitude we have to fall
        var grounded = false;
        var tilemapsOnAltitude = GetTilemapAtAltitude(altitude.value);
        foreach(var tilemap in tilemapsOnAltitude) {
            if (tilemap.TryGetComponent<CompositeCollider2D>(out CompositeCollider2D tilemapCollider)) {
                if(isInside(tilemapCollider)) {
                    grounded = true;
                }
            }
        }
        if(!grounded) {
            // we need to fall
            var newAltitude = getAltitudeOfTile(objectBase.transform.position);
            // still add the original vector since movement isn't blocked
            return v + (Vector2) jumpToAltitude(newAltitude);
        }

        foreach(var worldAltitude in GetAltitudes()) {
            if(worldAltitude == altitude.value)
                continue;

            var localAltitudeOffset = (Vector2) objectBase.gameObject.transform.InverseTransformVector(new Vector2(0, worldAltitude - 1));
            objectBase.offset = startingPosition + localAltitudeOffset;
            var hits = CastAtAltitude<Tilemap>(worldAltitude, v, filter.NoFilter(), v.magnitude);
            foreach(var hit in hits) {
                // Hit some tile that's above us, stop moving
                if(worldAltitude > altitude.value) {
                    objectBase.offset = startingPosition;
                    // TODO snap to tile
                    return Vector3.zero;
                }
            }
        }
        objectBase.offset = startingPosition;
        return v;
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
