using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base abstract class for 2D sprite-based enemy AI
/// Uses 2D physics and sprite flipping for visuals
/// </summary>
public abstract class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] protected float chaseRange = 10f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float movementSpeed = 3f;
    [SerializeField] protected float rotationSpeed = 360f;
    [SerializeField] protected LayerMask detectionLayers = ~0;
    [SerializeField] protected LayerMask obstacleLayers = ~0;

    [Header("Roaming Settings")]
    [SerializeField] protected float roamRadius = 15f;
    [SerializeField] protected float minRoamWaitTime = 1f;
    [SerializeField] protected float maxRoamWaitTime = 3f;
    [SerializeField] protected float waypointDistanceThreshold = 0.5f;

    [Header("2D References")]
    [SerializeField] protected Transform playerTarget;
    [SerializeField] protected Rigidbody2D rb2D;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Collider2D enemyCollider;

    [Header("2D Pathfinding")]
    [SerializeField] protected bool usePathfinding = true;
    [SerializeField] protected float pathfindingUpdateRate = 0.5f;
    [SerializeField] protected float obstacleAvoidanceRadius = 1f;

    // State management
    public enum AIState { Idle, Roaming, Chasing, Attacking, Returning }
    protected AIState currentState = AIState.Idle;
    protected AIState previousState = AIState.Idle;

    // Protected fields for derived classes
    protected Vector2 startingPosition;
    protected Vector2 roamDestination;
    protected Vector2 currentVelocity;
    protected float stateTimer = 0f;
    protected bool isPlayerInRange = false;
    protected bool hasLineOfSight = false;
    protected bool isFacingRight = true;

    // Pathfinding
    protected Vector2[] currentPath;
    protected int currentPathIndex = 0;
    protected float pathUpdateTimer = 0f;

    // Property for external access
    public string CurrentStateName => currentState.ToString();
    public bool IsChasing => currentState == AIState.Chasing;
    public bool IsAlerted => currentState == AIState.Chasing || currentState == AIState.Attacking;
    public Vector2 Velocity => currentVelocity;

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        // Cache 2D references
        if (rb2D == null)
            rb2D = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (enemyCollider == null)
            enemyCollider = GetComponent<Collider2D>();

        // Find player if not assigned
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.transform;
        }

        startingPosition = transform.position;
        isFacingRight = transform.localScale.x > 0;
    }

    protected virtual void Start()
    {
        InitializeAI();
    }

    protected virtual void Update()
    {
        UpdateDetection();
        UpdateStateMachine();
        UpdateSpriteDirection();
        UpdateAnimations();
    }

    protected virtual void FixedUpdate()
    {
        UpdateMovement();
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // Draw detection ranges
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw roam radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startingPosition, roamRadius);

        // Draw current path if pathfinding
        if (currentPath != null && currentPath.Length > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < currentPath.Length - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }
            Gizmos.DrawSphere(currentPath[currentPathIndex], 0.3f);
        }

        // Draw current destination
        if (currentState == AIState.Roaming || currentState == AIState.Returning)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, roamDestination);
            Gizmos.DrawSphere(roamDestination, 0.5f);
        }
    }

    #endregion

    #region Core State Machine

    protected virtual void UpdateStateMachine()
    {
        switch (currentState)
        {
            case AIState.Idle:
                Idle();
                break;
            case AIState.Roaming:
                Roam();
                break;
            case AIState.Chasing:
                Chase();
                break;
            case AIState.Attacking:
                Attack();
                break;
            case AIState.Returning:
                ReturnToStart();
                break;
        }

        stateTimer -= Time.deltaTime;
        pathUpdateTimer -= Time.deltaTime;
    }

    protected virtual void TransitionToState(AIState newState, object transitionData = null)
    {
        OnStateExit(currentState, newState);
        previousState = currentState;
        currentState = newState;
        OnStateEnter(newState, previousState, transitionData);
        stateTimer = 0f;
    }

    protected virtual void OnStateEnter(AIState newState, AIState previousState, object transitionData) { }
    protected virtual void OnStateExit(AIState oldState, AIState newState) { }

    #endregion

    #region Core AI Behaviors (Virtual - Can be overridden)

    /// <summary>
    /// Idle behavior - enemy waits in place
    /// </summary>
    protected virtual void Idle()
    {
        // Stop movement
        currentVelocity = Vector2.zero;

        // Check for player detection
        if (isPlayerInRange && hasLineOfSight)
        {
            TransitionToState(AIState.Chasing);
            return;
        }

        // Wait for timer, then roam
        if (stateTimer <= 0)
        {
            TransitionToState(AIState.Roaming);
        }
    }

    /// <summary>
    /// Roaming behavior - enemy moves to random points
    /// </summary>
    protected virtual void Roam()
    {
        // Check for player detection
        if (isPlayerInRange && hasLineOfSight)
        {
            TransitionToState(AIState.Chasing);
            return;
        }

        // If we've reached destination or timer expired, pick new destination
        if (HasReachedDestination() || stateTimer <= 0)
        {
            SetRandomRoamingDestination();
            stateTimer = Random.Range(minRoamWaitTime, maxRoamWaitTime);
        }
    }

    /// <summary>
    /// Chase behavior - enemy pursues the player
    /// </summary>
    protected virtual void Chase()
    {
        if (playerTarget == null)
        {
            TransitionToState(AIState.Returning);
            return;
        }

        // Update destination to player position
        roamDestination = playerTarget.position;

        // Check if player is in attack range
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= attackRange && hasLineOfSight)
        {
            TransitionToState(AIState.Attacking);
        }
        // Check if player is out of chase range
        else if (distanceToPlayer > chaseRange || !hasLineOfSight)
        {
            TransitionToState(AIState.Returning);
        }
    }

    /// <summary>
    /// Attack behavior (to be implemented in derived classes)
    /// </summary>
    protected virtual void Attack()
    {
        // Stop movement during attack
        currentVelocity = Vector2.zero;

        // Check if player moved out of attack range
        if (playerTarget != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            if (distanceToPlayer > attackRange)
            {
                TransitionToState(AIState.Chasing);
            }
            else if (distanceToPlayer > chaseRange || !hasLineOfSight)
            {
                TransitionToState(AIState.Returning);
            }
        }
    }

    /// <summary>
    /// Return to starting position
    /// </summary>
    protected virtual void ReturnToStart()
    {
        roamDestination = startingPosition;

        // Check if reached starting position
        if (HasReachedDestination())
        {
            TransitionToState(AIState.Idle);
            stateTimer = Random.Range(minRoamWaitTime, maxRoamWaitTime);
        }

        // Check if player re-entered range while returning
        if (isPlayerInRange && hasLineOfSight)
        {
            TransitionToState(AIState.Chasing);
        }
    }

    #endregion

    #region 2D Movement & Pathfinding

    /// <summary>
    /// Update movement based on current state
    /// </summary>
    protected virtual void UpdateMovement()
    {
        Vector2 movementDirection = Vector2.zero;

        switch (currentState)
        {
            case AIState.Idle:
                // No movement
                break;

            case AIState.Roaming:
            case AIState.Returning:
            case AIState.Chasing:
                // Calculate movement direction
                movementDirection = CalculateMovementDirection();

                // Apply obstacle avoidance
                if (usePathfinding)
                {
                    movementDirection = ApplyObstacleAvoidance(movementDirection);
                }
                break;
        }

        // Apply movement
        currentVelocity = movementDirection * movementSpeed;

        // Use Rigidbody2D for movement
        if (rb2D != null)
        {
            rb2D.linearVelocity = currentVelocity;
        }
        else
        {
            // Fallback to transform movement
            transform.position += (Vector3)currentVelocity * Time.fixedDeltaTime;
        }
    }

    /// <summary>
    /// Calculate movement direction based on destination
    /// </summary>
    protected virtual Vector2 CalculateMovementDirection()
    {
        Vector2 targetPosition = roamDestination;

        // Use pathfinding if enabled
        if (usePathfinding && pathUpdateTimer <= 0)
        {
            UpdatePath(targetPosition);
            pathUpdateTimer = pathfindingUpdateRate;
        }

        // If we have a path, follow it
        if (currentPath != null && currentPath.Length > 0)
        {
            targetPosition = currentPath[currentPathIndex];

            // Move to next waypoint if reached current one
            if (Vector2.Distance(transform.position, targetPosition) < waypointDistanceThreshold)
            {
                currentPathIndex++;
                if (currentPathIndex >= currentPath.Length)
                {
                    currentPath = null;
                    currentPathIndex = 0;
                }
            }
        }

        // Calculate direction
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        // If we're very close to destination, slow down
        float distance = Vector2.Distance(transform.position, targetPosition);
        if (distance < 1f)
        {
            direction *= Mathf.Clamp01(distance);
        }

        return direction;
    }

    /// <summary>
    /// Simple pathfinding using raycasts
    /// </summary>
    protected virtual void UpdatePath(Vector2 target)
    {
        // Simple A* implementation for 2D
        List<Vector2> path = new List<Vector2>();

        // Direct path check
        if (!Physics2D.Linecast(transform.position, target, obstacleLayers))
        {
            // No obstacles, direct path
            path.Add(target);
        }
        else
        {
            // Find alternative path
            path = FindAlternativePath(target);
        }

        currentPath = path.ToArray();
        currentPathIndex = 0;
    }

    /// <summary>
    /// Find alternative path when direct path is blocked
    /// </summary>
    protected virtual List<Vector2> FindAlternativePath(Vector2 target)
    {
        List<Vector2> path = new List<Vector2>();

        // Get direction to target
        Vector2 direction = (target - (Vector2)transform.position).normalized;

        // Try perpendicular directions
        Vector2[] testDirections = new Vector2[]
        {
            direction,
            Quaternion.Euler(0, 0, 45) * direction,
            Quaternion.Euler(0, 0, -45) * direction,
            Quaternion.Euler(0, 0, 90) * direction,
            Quaternion.Euler(0, 0, -90) * direction
        };

        foreach (Vector2 testDir in testDirections)
        {
            // Cast ray to see if this direction is clear
            RaycastHit2D hit = Physics2D.Raycast(transform.position, testDir, chaseRange, obstacleLayers);

            if (hit.collider == null)
            {
                // This direction is clear, use it
                Vector2 intermediatePoint = (Vector2)transform.position + testDir * chaseRange * 0.5f;
                path.Add(intermediatePoint);
                path.Add(target);
                break;
            }
        }

        // If no path found, just go toward target (will get stuck on obstacles)
        if (path.Count == 0)
        {
            path.Add(target);
        }

        return path;
    }

    /// <summary>
    /// Apply obstacle avoidance to movement direction
    /// </summary>
    protected virtual Vector2 ApplyObstacleAvoidance(Vector2 movementDirection)
    {
        if (movementDirection == Vector2.zero) return movementDirection;

        // Check for obstacles in movement direction
        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            obstacleAvoidanceRadius,
            movementDirection,
            obstacleAvoidanceRadius * 2f,
            obstacleLayers
        );

        if (hit.collider != null)
        {
            // Calculate avoidance direction
            Vector2 avoidanceDirection = Vector2.Perpendicular(hit.normal);

            // Choose the direction that aligns better with original movement
            Vector2 dir1 = avoidanceDirection;
            Vector2 dir2 = -avoidanceDirection;

            float dot1 = Vector2.Dot(dir1.normalized, movementDirection.normalized);
            float dot2 = Vector2.Dot(dir2.normalized, movementDirection.normalized);

            Vector2 chosenDirection = (dot1 > dot2) ? dir1 : dir2;

            // Blend original direction with avoidance
            return Vector2.Lerp(movementDirection, chosenDirection, 0.7f).normalized;
        }

        return movementDirection;
    }

    /// <summary>
    /// Check if enemy has reached destination
    /// </summary>
    protected virtual bool HasReachedDestination()
    {
        return Vector2.Distance(transform.position, roamDestination) < waypointDistanceThreshold;
    }

    /// <summary>
    /// Flip sprite based on movement direction
    /// </summary>
    protected virtual void UpdateSpriteDirection()
    {
        if (spriteRenderer == null) return;

        // Determine direction based on velocity
        if (currentVelocity.x > 0.1f)
        {
            isFacingRight = true;
        }
        else if (currentVelocity.x < -0.1f)
        {
            isFacingRight = false;
        }

        // Apply flipping
        spriteRenderer.flipX = !isFacingRight;

        // Alternative: Use scale flipping
        // Vector3 scale = transform.localScale;
        // scale.x = Mathf.Abs(scale.x) * (isFacingRight ? 1 : -1);
        // transform.localScale = scale;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Initialize AI settings
    /// </summary>
    protected virtual void InitializeAI()
    {
        // Setup Rigidbody2D
        if (rb2D != null)
        {
            rb2D.gravityScale = 0; // Typically 0 for top-down 2D
            rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Start in idle state
        stateTimer = Random.Range(0.5f, 2f);
        TransitionToState(AIState.Idle);
    }

    /// <summary>
    /// Update player detection and line of sight
    /// </summary>
    protected virtual void UpdateDetection()
    {
        if (playerTarget == null) return;

        // Check distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
        isPlayerInRange = distanceToPlayer <= chaseRange;

        // Check line of sight using 2D raycast
        hasLineOfSight = false;
        if (isPlayerInRange)
        {
            Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, playerTarget.position);

            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                directionToPlayer,
                distance,
                detectionLayers
            );

            if (hit.collider != null)
            {
                // Check if we hit the player or something else
                if (hit.collider.transform == playerTarget || hit.collider.transform.IsChildOf(playerTarget))
                {
                    hasLineOfSight = true;
                }
            }
        }
    }

    /// <summary>
    /// Set a random destination within roam radius
    /// </summary>
    protected virtual void SetRandomRoamingDestination()
    {
        Vector2 randomDirection = Random.insideUnitCircle * roamRadius;
        roamDestination = startingPosition + randomDirection;

        // Ensure destination is valid (not inside obstacles)
        Collider2D hitCollider = Physics2D.OverlapCircle(roamDestination, 0.5f, obstacleLayers);
        if (hitCollider != null)
        {
            // Try to find a valid position near the random point
            for (int i = 0; i < 10; i++)
            {
                Vector2 newDirection = Random.insideUnitCircle * roamRadius;
                Vector2 newDestination = startingPosition + newDirection;

                if (Physics2D.OverlapCircle(newDestination, 0.5f, obstacleLayers) == null)
                {
                    roamDestination = newDestination;
                    break;
                }
            }
        }

        // Reset path
        currentPath = null;
        currentPathIndex = 0;
    }

    /// <summary>
    /// Update animations based on state
    /// </summary>
    protected virtual void UpdateAnimations()
    {
        // Base class doesn't implement animations
        // Override in derived classes with animation logic
    }

    #endregion

    #region Public API

    /// <summary>
    /// Manually trigger chase state
    /// </summary>
    public virtual void Alert(Vector2 alertPosition)
    {
        // Look toward alert position
        Vector2 direction = (alertPosition - (Vector2)transform.position).normalized;
        isFacingRight = direction.x > 0;

        // Transition to chase if player is target
        if (playerTarget != null)
        {
            TransitionToState(AIState.Chasing);
        }
    }

    /// <summary>
    /// Reset AI to starting state
    /// </summary>
    public virtual void ResetAI()
    {
        TransitionToState(AIState.Idle);
        transform.position = startingPosition;
        stateTimer = Random.Range(minRoamWaitTime, maxRoamWaitTime);
        currentVelocity = Vector2.zero;

        if (rb2D != null)
        {
            rb2D.linearVelocity = Vector2.zero;
        }
    }

    /// <summary>
    /// Set a new target for the AI
    /// </summary>
    public virtual void SetTarget(Transform newTarget)
    {
        playerTarget = newTarget;

        if (newTarget != null && isPlayerInRange && hasLineOfSight)
        {
            TransitionToState(AIState.Chasing);
        }
    }

    /// <summary>
    /// Force the AI into a specific state
    /// </summary>
    public virtual void ForceState(AIState state, object transitionData = null)
    {
        TransitionToState(state, transitionData);
    }

    #endregion
}