using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;


namespace Ap.CodeChallenge
{
    public class Player : MonoBehaviour
    {
        public enum PlayerStates
        {
            Idle,
            Moving
        }

        public bool isMousePressed = false;

        private NavMeshAgent navMeshAgentComp;
        private PlayerStates currentPlayerState = PlayerStates.Idle;

        private void Awake()
        {
            navMeshAgentComp = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (isMousePressed)
            {
                MovingTowardsMouse();
            }

            CheckMovement();
        }

        // Event on Player Input
        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                isMousePressed = true;
            }

            if (context.canceled)
            {
                isMousePressed = false;
            }
        }

        private void MovingTowardsMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 40f))
            {
                // Check where in the mesh we are hitting
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navMeshHit, 0.5f, 1 << 0))
                {
                    navMeshAgentComp.SetDestination(navMeshHit.position);
                    currentPlayerState = PlayerStates.Moving;
                }
            }
        }

        private void CheckMovement()
        {
            if (!navMeshAgentComp.pathPending)
            {
                if (navMeshAgentComp.remainingDistance <= navMeshAgentComp.stoppingDistance)
                {
                    if (!navMeshAgentComp.hasPath || navMeshAgentComp.velocity.sqrMagnitude == 0f)
                    {
                        currentPlayerState = PlayerStates.Idle;
                    }
                }
            }
        }
    }
}
