using UnityEngine;
using static EnemyAI;
using static UnityEngine.RuleTile.TilingRuleOutput;

/// <summary>
/// 2D Melee Enemy with sprite animations
/// </summary>
public class MeleeEnemy //: EnemyAI
{
    //[Header("Melee Settings")]
    //[SerializeField] private float attackCooldown = 1.5f;
    //[SerializeField] private int attackDamage = 10;
    //[SerializeField] private float attackWindupTime = 0.3f;
    //[SerializeField] private float attackDuration = 0.5f;
    //[SerializeField] private Vector2 attackSize = new Vector2(2f, 1.5f);

    //[Header("2D Animations")]
    //[SerializeField] private Animator animator;
    //[SerializeField] private string idleAnimation = "Idle";
    //[SerializeField] private string moveAnimation = "Move";
    //[SerializeField] private string attackAnimation = "Attack";
    //[SerializeField] private string hitAnimation = "Hit";

    //[Header("Visual Feedback")]
    //[SerializeField] private GameObject attackIndicator;
    //[SerializeField] private Color attackWarningColor = new Color(1f, 0.5f, 0.5f, 0.3f);

    //private float attackTimer = 0f;
    //private bool isAttacking = false;
    //private Color originalSpriteColor;

    //protected override void Start()
    //{
    //    base.Start();

    //    if (animator == null)
    //        animator = GetComponent<Animator>();

    //    if (spriteRenderer != null)
    //        originalSpriteColor = spriteRenderer.color;

    //    if (attackIndicator != null)
    //        attackIndicator.SetActive(false);
    //}

    //protected override void Attack()
    //{
    //    //base.Attack();

    //    //attackTimer -= Time.deltaTime;

    //    //if (!isAttacking && attackTimer <= 0)
    //    //{
    //    //    // Start attack sequence
    //    //    StartAttack();
    //    //}
    //    //else if (isAttacking && attackTimer <= attackCooldown - attackWindupTime)
    //    //{
    //    //    // Attack animation completed, check hit
    //    //    PerformAttack();
    //    //}
    //    //else if (attackTimer <= 0)
    //    //{
    //    //    // Attack cooldown finished
    //    //    isAttacking = false;

    //    //    // Check if player is still in range
    //    //    if (playerTarget != null)
    //    //    {
    //    //        float distance = Vector2.Distance(transform.position, playerTarget.position);

    //    //        if (distance > attackRange)
    //    //        {
    //    //            TransitionToState(AIState.Chasing);
    //    //        }
    //    //        else if (distance > chaseRange || !hasLineOfSight)
    //    //        {
    //    //            TransitionToState(AIState.Returning);
    //    //        }
    //    //    }
    //    //}
    //}

    //private void StartAttack()
    //{
    //    //isAttacking = true;
    //    //attackTimer = attackCooldown;

    //    //// Trigger attack animation
    //    //if (animator != null)
    //    //{
    //    //    animator.SetTrigger("Attack");
    //    //}

    //    //// Show attack indicator
    //    //if (attackIndicator != null)
    //    //{
    //    //    attackIndicator.transform.localScale = new Vector3(
    //    //        isFacingRight ? attackSize.x : -attackSize.x,
    //    //        attackSize.y,
    //    //        1
    //    //    );
    //    //    attackIndicator.SetActive(true);

    //    //    // Flash warning color
    //    //    if (spriteRenderer != null)
    //    //    {
    //    //        spriteRenderer.color = attackWarningColor;
    //    //        Invoke(nameof(ResetSpriteColor), attackWindupTime);
    //    //    }
    //    //}
    //}

    //private void PerformAttack()
    //{
    //    // Calculate attack position
    //    Vector2 attackPos = (Vector2)transform.position +
    //        new Vector2(isFacingRight ? attackSize.x * 0.5f : -attackSize.x * 0.5f, 0);

    //    // Check for player in attack range
    //    Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
    //        attackPos,
    //        attackSize,
    //        0f,
    //        detectionLayers
    //    );

    //    foreach (Collider2D collider in hitColliders)
    //    {
    //        if (collider.CompareTag("Player") || collider.transform == playerTarget)
    //        {
    //            break;
    //        }
    //    }

    //    // Hide attack indicator after attack
    //    if (attackIndicator != null)
    //    {
    //        Invoke(nameof(HideAttackIndicator), attackDuration - attackWindupTime);
    //    }
    //}

    //private void HideAttackIndicator()
    //{
    //    //if (attackIndicator != null)
    //    //    attackIndicator.SetActive(false);
    //}

    //private void ResetSpriteColor()
    //{
    //    if (spriteRenderer != null)
    //        spriteRenderer.color = originalSpriteColor;
    //}

    //protected override void UpdateAnimations()
    //{
    //    if (animator == null) return;

    //    animator.SetBool("IsMoving", currentVelocity.magnitude > 0.1f);
    //    animator.SetBool("IsChasing", IsChasing);
    //    animator.SetFloat("MoveSpeed", currentVelocity.magnitude / movementSpeed);

    //    // Update sprite flip in animator if needed
    //    animator.SetBool("IsFacingRight", isFacingRight);
    //}

    //protected override void OnStateEnter(AIState newState, AIState previousState, object transitionData)
    //{
    //    //base.OnStateEnter(newState, previousState, transitionData);

    //    //if (previousState == AIState.Attacking && newState != AIState.Attacking)
    //    //{
    //    //    isAttacking = false;
    //    //    attackTimer = 0f;
    //    //    HideAttackIndicator();
    //    //    ResetSpriteColor();
    //    //}
    //}
}