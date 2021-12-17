using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyHealth : MonoBehaviour
{
    public float health;
    // For boss this is determined by the dominant attack 
    public string enemyType;
    public string attackType;
    private Seeker seeker;
    private Rigidbody2D rb;
    public PlayerController player;
    // 0: right, 1: up, 2: left, 3: down
    public int face;
    // public bool facing_right;
    public float safeDistance;
    public bool reachedEndOfPath;
    private int currentWaypoint = 0;
    public float nextWaypointDistance = 1f;
    public float speed;
    public Path path;
    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        // rb = GetComponent<Rigidbody2D>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        switch (gameObject.transform.GetChild(0).tag) {
            case "Melee":
            enemyType = "Melee";
            safeDistance = 0.5f;
            health = 50.0f;
            speed = 7.5f;
            break;
            case "Ranged":
            enemyType = "Ranged";
            safeDistance = 5.0f;
            health = 75.0f;
            speed = 5f;
            break;
            case "AOE":
            enemyType = "AOE";
            safeDistance = 2.0f;
            health = 100.0f;
            speed = 3.0f;
            break; 
        }
    }

     void Update () {
        float distToPlayer = Vector2.Distance(player.transform.position, transform.position);
        // close enough do not need to move
        if (Mathf.Abs(distToPlayer - safeDistance) <= 0.2) {
            // rb.velocity = Vector2.zero;
            return;
        }
         // for AOE and ranged they try to move away from player if too close
        if (distToPlayer < safeDistance) {
            if (enemyType == "Melee") {
                return;
            } else {
                if (seeker.IsDone())
                    seeker.StartPath(transform.position, transform.position - (player.transform.position - transform.position), OnPathComplete);
            }
        } else {
            if (seeker.IsDone()) 
                seeker.StartPath(transform.position, player.transform.position, OnPathComplete);
        }

        if (path == null) {
            // We have no path to follow yet, so don't do anything
            return;
        }

        // Check in a loop if we are close enough to the current waypoint to switch to the next one.
        // We do this in a loop because many waypoints might be close to each other and we may reach
        // several of them in the same frame.
        reachedEndOfPath = false;
        // The distance to the next waypoint in the path
        float distanceToWaypoint;
        while (true) {
            // If you want maximum performance you can check the squared distance instead to get rid of a
            // square root calculation. But that is outside the scope of this tutorial.
            distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
            if (distanceToWaypoint < nextWaypointDistance) {
                // Check if there is another waypoint or if we have reached the end of the path
                if (currentWaypoint + 1 < path.vectorPath.Count) {
                    currentWaypoint++;
                } else {
                    // Set a status variable to indicate that the agent has reached the end of the path.
                    // You can use this to trigger some special code if your game requires that.
                    reachedEndOfPath = true;
                    break;
                }
            } else {
                break;
            }
        }

        var speedFactor = reachedEndOfPath ? 0 : 1f;

        Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        Vector3 velocity = dir * speed * speedFactor;
        // rb.velocity = velocity;
        transform.position += velocity * Time.deltaTime;
    }

    public void OnPathComplete (Path p) {
        if (!p.error) {
            path = p;
            // Reset the waypoint counter so that we start to move towards the first point in the path
            currentWaypoint = 0;
        }
    }

    public void deductHealth(float amount) {
        health -= amount;
        if (health <= 0) 
            Destroy(gameObject);
    }

    // public void Flip() {
    //     facing_right = !facing_right;
    //     transform.Rotate(0f, 180f, 0f);
    // }

        // up down left right where to face for the boss
    public void determineFace(Vector2 dir) {
        float angle = Vector2.SignedAngle(Vector2.right, dir);
        if ((angle >= 0 && angle < 45) || (angle < 0 && angle > -45)) {
            face = 0;
        } else if (angle > 45 && angle <= 135) {
            face = 1;
        } else if ((angle > 135 && angle <= 180) || (angle >= -180 && angle <= -135)) {
            face = 2;
        } else {
            face = 3;
        }
    }
}
