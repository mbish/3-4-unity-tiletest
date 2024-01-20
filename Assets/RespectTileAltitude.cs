using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Tilemaps;

public class AltitudeMoveResult {
    public AltitudeMoveResult(bool isFalling, bool hitWall, Vector3 resultingMove, int altitude) => (IsFalling, HitWall, ResultingMove, Altitude) = (isFalling, hitWall, resultingMove, altitude);
    public bool IsFalling;
    public bool HitWall;
    public Vector3 ResultingMove;
    public int Altitude;
}

[RequireComponent(typeof(Altitude))]
public class RespectTileAltitude : MonoBehaviour
{
    private Altitude altitude;
    private bool isEnabled = true;

    void Awake() {
        altitude = GetComponent<Altitude>();
    }
    public Vector3 jumpToAltitude(int newAltitude) {
        var oldAltitude = altitude.value;
        return new Vector2(0, newAltitude - oldAltitude);
    }

    int _getAltitudeOfPositionWithAdjustment(Func<int, int> adjustment) {
        var maxAltitude = 0;
        var startingPosition = new Vector2(altitude.objectBase.offset.x, altitude.objectBase.offset.y);
        foreach(var checkAltitude in altitudeObjects()) {
            var localAltitudeOffset = (Vector2) altitude.objectBase.gameObject.transform.InverseTransformVector(new Vector2(0, adjustment(checkAltitude.value)));
            altitude.objectBase.offset = startingPosition + localAltitudeOffset;

            if(isInside(checkAltitude.objectBase)) {
                if(checkAltitude.value > maxAltitude) {
                    maxAltitude = checkAltitude.value;
                }
            }
            altitude.objectBase.offset = startingPosition;
        }
        return maxAltitude;
    }

     int getAltitudeOfCurrentPositionIgnoringAltitude() {
        return _getAltitudeOfPositionWithAdjustment((int checkAltitude) => altitude.value - checkAltitude);
    }

     int getAltitudeOfCurrentPosition() {
        return _getAltitudeOfPositionWithAdjustment((int checkAltitude) => 0);
     }


    List<RaycastHit2D> CastAtAltitude(int castAltitude, Vector3 direction, ContactFilter2D filter, float distance) {
        List<RaycastHit2D> results = new List<RaycastHit2D>();
        List<RaycastHit2D> filtered = new List<RaycastHit2D>();

        var hits = altitude.objectBase.Cast(direction, filter, results, distance);

        foreach(var hit in results) {
            var hitAltitude = hit.transform.gameObject.GetComponent<Altitude>();
            if(hitAltitude && hitAltitude.value == castAltitude) {
                filtered.Add(hit);
            }
        }
        return filtered;
    }

    Altitude[] altitudeObjects() {
        return FindObjectsOfType<Altitude>();
    }

    List<int> getAltitudes() {
        HashSet<int> altitudes = new HashSet<int>();
        foreach(var checkAlttiude in altitudeObjects()) {
            altitudes.Add(checkAlttiude.value);
        }
        return altitudes.ToList();
    }

    List<Altitude> GetObjectsAtAltitude(int atAltitude) {
        var altitudes = new List<Altitude>();
        foreach(var checkAltitude in altitudeObjects()) {
            if(checkAltitude.value == atAltitude) {
                altitudes.Add(checkAltitude);
            }
        }
        return altitudes;
    }

    bool colliderInDirection(Vector2 direction, Collider2D collider) {
        var filter = new ContactFilter2D();
        List<RaycastHit2D> results = new List<RaycastHit2D>();
        var hits = altitude.objectBase.Cast(direction, filter.NoFilter(), results);

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

    bool isGrounded() {
        // Check for falls
        var grounded = false;
        var objectsOnAltitude = GetObjectsAtAltitude(altitude.value);
        foreach(var obj in objectsOnAltitude) {
            if(isInside(obj.objectBase)) {
                grounded = true;
            }
        }
        return grounded;
    }

    public AltitudeMoveResult _move(Vector2 v) {
        // Check for falls
        if(!isGrounded()) {
            // we need to fall
            var newAltitude = getAltitudeOfCurrentPosition();
            // still add the original vector since movement isn't blocked
            var moveVector = v + (Vector2) jumpToAltitude(newAltitude);
            return new AltitudeMoveResult(true, false, moveVector, newAltitude);
        }

        // Check for walls
        var filter = new ContactFilter2D();
        foreach(var worldAltitude in getAltitudes()) {
            if(worldAltitude == altitude.value)
                continue;

            // Offset our collider to compensate for altitude differences
            var hits = CastAtAltitude(worldAltitude, v, filter.NoFilter(), v.magnitude);

            foreach(var hit in hits) {
                // Hit some tile that's above us, stop moving
                if(worldAltitude > altitude.value) {
                   if(hit.distance > 2*Mathf.Epsilon) {
                       return new AltitudeMoveResult(false, true, v.normalized * Mathf.Epsilon, altitude.value);
                   } else {
                       return new AltitudeMoveResult(false, true, Vector3.zero, altitude.value);
                   }
                }
            }
        }
        return new AltitudeMoveResult(false, false, v, altitude.value);
    }

    public AltitudeMoveResult move(Vector2 v) {
        if(!isEnabled) {
            return new AltitudeMoveResult(false, false, v, altitude.value);
        }
        return _move(v);
    }

    public bool willDisablingChangeAltitude() {
        if(!isEnabled) {
            return false;
        }
        var newAltitude = getAltitudeOfCurrentPositionIgnoringAltitude();
        return newAltitude != altitude.value;
    }

    public void enable() {
        if(!isEnabled) {
            isEnabled = true;
            var newAltitude = getAltitudeOfCurrentPositionIgnoringAltitude();
            altitude.changeAltitude(newAltitude);
        }
    }
    public void disable() {
        if(isEnabled) {
            isEnabled = false;
        }
    }
}