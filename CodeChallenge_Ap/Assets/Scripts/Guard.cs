using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Ap.CodeChallenge
{
    /// <summary>
    /// This basic enemy is going to follow a patrol path, reach point A, wait X amount of time, move to point B, and so on.
    /// </summary>
    public class Guard : Enemy
    {
        [SerializeField] private List<Transform> patrollingPath;
        [SerializeField] private float wonderingTime;
        [SerializeField] private LayerMask ignoredMaskForLoS;

        private float wonderingTimeElapsed = 2f;
        private float maxLineOfSightRayDistance = 20f;

        private int currentWaypointIndex = -1;

        private void Start()
        {
            currentState = EnemyState.Patrolling;
        }

        private void Update()
        {
            CheckLineOfSight();
            CheckState();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                possibleTarget = other.GetComponent<NavMeshAgent>();
                if (IsInLineOfSight(possibleTarget.transform))
                {
                    currentTarget = possibleTarget;
                    possibleTarget = null;
                    currentState = EnemyState.Chasing;
                    ChangeMaterial();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                ChangeToWonder();
            }
        }

        private void CheckState()
        {
            switch (currentState)
            {
                case EnemyState.Idle:
                    if (previousState != EnemyState.Idle)
                    {
                        // Just started being idle
                    }
                    break;

                case EnemyState.Patrolling:
                    Patrol();
                    break;

                case EnemyState.Wondering:
                    Wonder();
                    break;

                case EnemyState.Chasing:
                    Chase();
                    break;
            }
        }

        private void Patrol()
        {
            if (patrollingPath.Count <= 0)
            {
                // We have no path to follow
                Debug.LogWarning("No path to follow!");

                currentState = EnemyState.Idle;
                return;
            }

            if (IsStillMovingTowardsDestination())
            {
                // If we are still moving, let him continue
                return;
            }

            currentWaypointIndex++;

            if (currentWaypointIndex < 0 || currentWaypointIndex >= patrollingPath.Count)
            {
                currentWaypointIndex = 0;
            }
            Seek(patrollingPath[currentWaypointIndex].position);
        }

        private void Wonder()
        {
            wonderingTimeElapsed += Time.deltaTime;
            if (wonderingTimeElapsed >= wonderingTime)
            {
                // Stop having an existential crisis and continue patrolling
                currentState = EnemyState.Patrolling;
                ChangeMaterial();
                wonderingTimeElapsed = 0f;
            }
        }

        private void Chase()
        {
            if (currentTarget == null)
            {
                ChangeToWonder();
                return;
            }

            Transform targetTransform = currentTarget.transform;

            // Check Line of Sight
            if (!IsInLineOfSight(targetTransform))
            {
                ChangeToWonder();
                return;
            }

            Vector3 targetDir = currentTarget.transform.position - transform.position;

            // Check if target is moving.
            if (currentTarget.velocity.sqrMagnitude <= 0.01f)
            {
                Seek(targetTransform.position);
                return;
            }

            // Move towards the position where the target is heading.
            float lookAhead = targetDir.magnitude / (navMeshAgentComp.speed + currentTarget.speed);
            Seek(targetTransform.position + targetTransform.forward * lookAhead);
        }

        private void ChangeToWonder()
        {
            possibleTarget = null;
            currentTarget = null;
            wonderingTimeElapsed = 0f;
            currentState = EnemyState.Wondering;
            ChangeMaterial();

            // Since the enemy goes into wonder state before patrol, patrol will move towards the next
            // waypoint if not moving, so, we just substract one from the current index
            currentWaypointIndex--;
        }

        // Check if we have a current target and if we do, check line of sight
        private void CheckLineOfSight()
        {
            if (possibleTarget == null)
            {
                return;
            }

            if (IsInLineOfSight(possibleTarget.transform))
            {
                currentState = EnemyState.Chasing;
                ChangeMaterial();
                currentTarget = possibleTarget;
                possibleTarget = null;
            }
        }

        private bool IsInLineOfSight(Transform target)
        {
            Vector3 rayToTarget = target.transform.position - transform.position;

            if (Physics.Raycast(transform.position, rayToTarget, out RaycastHit hitInfo, maxLineOfSightRayDistance, ~ignoredMaskForLoS))
            {
                if (hitInfo.collider.gameObject == target.gameObject)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsStillMovingTowardsDestination()
        {
            if (!navMeshAgentComp.pathPending)
            {
                if (navMeshAgentComp.remainingDistance <= navMeshAgentComp.stoppingDistance)
                {
                    if (!navMeshAgentComp.hasPath || navMeshAgentComp.velocity.sqrMagnitude == 0f)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
