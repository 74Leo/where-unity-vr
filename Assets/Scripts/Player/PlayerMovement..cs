using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  [Header("Déplacements")]

  [Tooltip("Vitesse de déplacement")]
  public float speed = 5f;

  [Tooltip("Gravity scale appliquée")]
  public float gravityScale = 0f;

  [Tooltip("Bloquer la rotation due à la physique")]
  public bool freezeRotation = true;

  private Rigidbody2D rb;
  private Vector2 movement;

  [Header("Animations")]
  public Animator animator;

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    if (rb == null)
    {
      Debug.LogError("[PlayerMovement] Rigidbody2D introuvable — script désactivé.");
      enabled = false;
      return;
    }

    rb.constraints = freezeRotation ? RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.None;
    rb.gravityScale = gravityScale;
  }

  void Update()
  {

    movement.x = Input.GetAxisRaw("Horizontal");
    movement.y = Input.GetAxisRaw("Vertical");

    animator.SetFloat("Horizontal", movement.x);
    animator.SetFloat("Vertical", movement.y);
    animator.SetFloat("Speed", movement.sqrMagnitude);

    if (movement.sqrMagnitude > 1f)
      movement = movement.normalized;

    float targetGravity = gravityScale;
    if (!Mathf.Approximately(rb.gravityScale, targetGravity))
      rb.gravityScale = targetGravity;
  }

  void FixedUpdate()
  {
    rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    Debug.Log("Touché : " + collision.gameObject.name);
  }
}
