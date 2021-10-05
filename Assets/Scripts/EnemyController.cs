using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private const float DistanceThreshold = 2f;

    NavMeshAgent agent;
    List<Transform> waypoints;
    int currentIndex;

    public void SetEnemy(List<Transform> waypointsPositions, int index)
    {
        SetNavAgent();
        SetWaypoints(waypointsPositions, index);
        agent.SetDestination(waypoints[currentIndex].position);
    }

    private void SetNavAgent()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = Random.Range(1.5f, 5);
        agent.angularSpeed = Random.Range(90f, 150f);
    }

    private void SetWaypoints(List<Transform> waypointsPositions, int index)
    {
        waypoints = new List<Transform>();
        currentIndex = index;

        foreach (Transform waypoint in waypointsPositions)
            waypoints.Add(waypoint);
    }

    private void Update()
    {
        if (agent.remainingDistance < DistanceThreshold)
        {
            currentIndex = (currentIndex + 1) % waypoints.Count;
            agent.SetDestination(waypoints[currentIndex].position);
        }
    }
}