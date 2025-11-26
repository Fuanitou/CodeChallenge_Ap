using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Ap.CodeChallenge
{
    public class Enemy : MonoBehaviour
    {
        public enum EnemyState
        {
            Idle,
            Patrolling,
            Wondering, // Not wander, little guy is just having a crisis
            Chasing
        }

        protected EnemyState previousState;
        protected EnemyState currentState;

        protected NavMeshAgent navMeshAgentComp;
        protected NavMeshAgent currentTarget;
        protected NavMeshAgent possibleTarget;

        [SerializeField] protected float attackRange = 1.5f;

        // In case we want to do or run something in particular when the state of the enemy has changed (UI effects, environment effects)
        public UnityEvent<Enemy> OnIdleStarted;
        public UnityEvent<Enemy> OnPatrollingResumed;
        public UnityEvent<Enemy, NavMeshAgent> OnPursuingStarted;

        virtual protected void Awake()
        {
            navMeshAgentComp = GetComponent<NavMeshAgent>();
        }

        virtual protected void Seek(Vector3 dest)
        {
            navMeshAgentComp.SetDestination(dest);
        }
    }
}
