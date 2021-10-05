using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    NavMeshAgent agent;
    List<Transform> waypoints;
    int currentIndex;

    public void SetEnemy(List<Transform> waypointsPositions, int index)
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = Random.Range(1.5f, 5);
        agent.angularSpeed = Random.Range(90f, 150f);
        waypoints = new List<Transform>();
        currentIndex = index;

        foreach (Transform waypoint in waypointsPositions)
        {
            waypoints.Add(waypoint);
        }

        agent.SetDestination(waypoints[currentIndex].position);
    }

    private void Update()
    {
        if (agent.remainingDistance < 2 && !agent.isStopped)
        {
            currentIndex = (currentIndex + 1) % waypoints.Count;
            agent.isStopped = true;
            StartCoroutine(Wait());
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 2));
        agent.SetDestination(waypoints[currentIndex].position);
        agent.isStopped = false;
    }
}