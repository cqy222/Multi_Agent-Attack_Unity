using UnityEngine;
public class Player : MonoBehaviour {
    public KeyCode move, turnLeft, turnRight, shoot;
    public static float moveSpeed = 1, turnSpeed = 5;
    int hp = 2;
    Animator animator;
    public GameObject bullet;
    public GameObject onDie;
    private void Start() {
        animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter(Collider other) {
        if (other.tag != tag) {
            hp--;
            Destroy(other.gameObject);
            if (hp == 0)
                Destroy(gameObject);
        }
        
    }

    private void OnDestroy() {
        if (onDie != null)
            Instantiate(onDie,transform.position,Quaternion.identity);
    }
    //}
    void Update() {
        Debug.DrawRay(transform.position, transform.forward* moveSpeed * 1.3f,Color.red);
        if (Input.GetKeyDown(move)) {
            if (animator != null)
                animator.Play("Run");
        }
        if (Input.GetKeyDown(move) &&!Physics.Raycast(transform.position,transform.forward,moveSpeed*1.5f)) {
            transform.Translate(transform.forward * moveSpeed, Space.World);
        }
        if (Input.GetKeyDown(turnLeft))
            transform.Rotate(new Vector3(0, -turnSpeed, 0), Space.Self);
        if (Input.GetKeyDown(turnRight))
            transform.Rotate(new Vector3(0, turnSpeed, 0), Space.Self);
        if (Input.GetKeyDown(shoot)) {
            if (animator != null)
                animator.Play("Shoot");
            GameObject bulletInst = Instantiate(bullet, transform.position + new Vector3(0,10f,0), transform.rotation);
            bulletInst.GetComponent<Bullet>().direction = transform.forward;
            bulletInst.tag = tag;
        }
    }
}
