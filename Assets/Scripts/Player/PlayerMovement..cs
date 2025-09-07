using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  public float speed = 5f;

  private Rigidbody2D rb;
  private Vector2 movement;

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
  }

  void Update()
  {

    movement.x = Input.GetAxisRaw("Horizontal");
    movement.y = Input.GetAxisRaw("Vertical");
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
