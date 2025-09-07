using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  public float speed = 5f;

  public bool gravityEnabled = false;
  public float gravityScaleWhenEnabled = 1f;


  private Rigidbody2D rb;
  private Vector2 movement;

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();

    rb.freezeRotation = true;
    rb.gravityScale = gravityEnabled ? gravityScaleWhenEnabled : 0f;
  }

  void Update()
  {
    movement.x = Input.GetAxisRaw("Horizontal");
    movement.y = Input.GetAxisRaw("Vertical");

    if (movement.sqrMagnitude > 1f)
      movement = movement.normalized;

    float targetGravity = gravityEnabled ? gravityScaleWhenEnabled : 0f;
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
