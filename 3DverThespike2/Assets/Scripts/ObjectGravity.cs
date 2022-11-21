
using UnityEngine;
using static Constants;

public class ObjectGravity : MonoBehaviour
{
    [SerializeField]public float gravityScale = 0.05f;
    [SerializeField]private LayerMask groundLayer;
    [SerializeField]private float elasticity;

    private BoxCollider2D boxCollider;      
    public float verticalSpeed = 0f;
    public float horizontalSpeed = 0f;
    void Start()
    {
         boxCollider = GetComponent<BoxCollider2D>();
    }
    void FixedUpdate()
    {
        
        if (isGrounded()){
            if (verticalSpeed > 0) verticalSpeed = 0;
        } 
        else {// Gravity
            verticalSpeed += gravityScale * Constants.playSpeed ; // Gravity increase
        }
        
        transform.position += Vector3.down * verticalSpeed * Constants.playSpeed; // 음수가 위 , 양수가 아래.
        transform.position += Vector3.right * horizontalSpeed * Constants.playSpeed; // 양수가 오른 쪽 , 음수가 왼 쪽
    }
    private void OnTriggerEnter2D(Collider2D collision) // 충돌 후 바닥에 붙이는 작업.
    {
        if (elasticity > 0) { // 충돌이잖아
            verticalSpeed = -verticalSpeed / elasticity;
        }
        Transform tr = collision.gameObject.GetComponent<Transform>();
        BoxCollider2D bx = collision.gameObject.GetComponent<BoxCollider2D>();
        transform.position = new Vector3 (transform.position.x , bx.bounds.center.y + ( bx.bounds.size.y + boxCollider.bounds.size.y )  / 2, transform.position.z );

    }
    public bool isGrounded() {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center , boxCollider.bounds.size ,  0, Vector2.down , 0f , groundLayer );
        return raycastHit.collider != null;
    }    
}
