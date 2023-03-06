using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using UnityEngine;
using RPG.Attributes;
using RPG.Utils;
using System;
using System.Collections; 

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] float chaseDistance = 5f; 
        [SerializeField] float suspicionTime = 3f; 
        [SerializeField] float aggroCooldownTime = 5f; 
        [SerializeField] PatrolPath patrolPath;
        [SerializeField] float waypointTollerance = 1f;
        [SerializeField] float waypointDwellTime = 3f; 
        [Range(0,1)]
        [SerializeField] float patrolSpeedFraction = 0.2f; 
        [SerializeField] float shoutDistance = 5f; 
        bool canBeAggrivated = true; 

        Fighter fighter; 
        Health health; 
        Mover mover; 
        GameObject player;

        EarlyInit<Vector3> guardPosition;
        float timeSinceLastSawPlayer = Mathf.Infinity;
        float timeSinceArrivedWaypoint = Mathf.Infinity;
        float timeSinceAggrivated = Mathf.Infinity; 
        int currentWaypointIndex = 0;

        private void Awake()
        {
            fighter = GetComponent<Fighter>();
            health = GetComponent<Health>();
            mover = GetComponent<Mover>();
            player = GameObject.FindWithTag("Player");

            guardPosition = new EarlyInit<Vector3>(GetGuardPosition);
        }

        private Vector3 GetGuardPosition()
        {
            return transform.position;
        }

        private void Start()
        {
            guardPosition.ForceInit();
        }

        private void Update()
        {
            if (health.IsDead()) return;

            GameObject player = GameObject.FindWithTag("Player");
            if (IsAggrivated() && fighter.CanAttack(player))
            {
                AttackBehaviour(player);
                canBeAggrivated = false; 
                // if (gameObject.name == "Dark Elf" || gameObject.name == "Soldier") print(gameObject.name + " in attack behaviour");
            }
            else if (timeSinceLastSawPlayer < suspicionTime)
            {
                SuspicionBehaviour();
                // if (gameObject.name == "Dark Elf" || gameObject.name == "Soldier") print(gameObject.name + " in suspicion behaviour");
                canBeAggrivated = true; 
                // canBeAggrivated = false;  
            }
            else
            {
                PatrolBehaviour();
                // if (gameObject.name == "Dark Elf" || gameObject.name == "Soldier") print(gameObject.name + " in patrol behaviour");
                canBeAggrivated = true; 
            }

            UpdateTimers();

            // if (gameObject.name == "Dark Elf" || gameObject.name == "Soldier") print(gameObject.name + " " + timeSinceAggrivated + " time since aggrivated");
        }

        public void Aggrivate()
        {
            if (canBeAggrivated)
            {
                timeSinceAggrivated = 0;
                // if (gameObject.name == "Dark Elf" || gameObject.name == "Soldier") print (gameObject.name + " has been aggrivated"); 
            }
            // else
            // {
                // if (gameObject.name == "Dark Elf" || gameObject.name == "Soldier")  print (gameObject.name + " can't be aggrivated"); 
            // }
        }

        private void UpdateTimers()
        {
            timeSinceLastSawPlayer += Time.deltaTime;
            timeSinceArrivedWaypoint += Time.deltaTime;
            timeSinceAggrivated += Time.deltaTime; 
        }

        private void PatrolBehaviour()
        {
            Vector3 nextPosition = guardPosition.value;

            if (patrolPath != null)
            {
                if (AtWaypoint())
                {
                    timeSinceArrivedWaypoint = 0; 
                    CycleWaypoint(); 
                }
                nextPosition = GetCurrentWaypoint(); 
            }

            if (timeSinceArrivedWaypoint > waypointDwellTime)
            {
                mover.StartMoveAction(nextPosition, patrolSpeedFraction); 
            }
        }

        private bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint()); 
            return distanceToWaypoint < waypointTollerance;
        }

        private void CycleWaypoint()
        {
            currentWaypointIndex = patrolPath.GetNextIndex(currentWaypointIndex); 
        }

        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypoint(currentWaypointIndex);
        }

        private void SuspicionBehaviour()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
            // canBeAggrivated = true; 
        }

        private void AttackBehaviour(GameObject player)
        {
            timeSinceLastSawPlayer = 0; 
            fighter.Attack(player);

            // canBeAggrivated = false; 
            if (canBeAggrivated) // only aggrivates enemies in range if bool is set to true
            {
                AggravateNearbyEnemies(); 
            }
            // canBeAggrivated = false; 
        }

        private void AggravateNearbyEnemies()
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, shoutDistance, Vector3.up, 0); 
            foreach (RaycastHit hit in hits)
            {
                AIController ai = hit.collider.GetComponent<AIController>(); 
                if (ai == null) continue;
                // canBeAggrivated = false; 
                // ai.canBeAggrivated = false;
                ai.Aggrivate();
                // ai.canBeAggrivated = false; 
                canBeAggrivated = false;
            }
        }

        private bool IsAggrivated()
        {
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
            return distanceToPlayer < chaseDistance || timeSinceAggrivated < aggroCooldownTime;
        }

        // Called by Unity
        private void OnDrawGizmosSelected() 
        {
            Gizmos.color = Color.blue; 
            Gizmos.DrawWireSphere(transform.position, chaseDistance); 
        }
    }
}
