using UnityEngine;

public class CollisionIgnore : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
void Start()
    {
        // You were using 'Collider' (for 3D) but 'Physics2D' (for 2D).
        // You need to get 'Collider2D' components.
        var colliders = GetComponentsInChildren<Collider2D>();
        
        // This loop correctly makes all child colliders ignore each other
        for (int i = 0; i < colliders.Length; i++)
        {
            for (int j = i + 1; j < colliders.Length; j++)
            {
                Physics2D.IgnoreCollision(colliders[i], colliders[j]);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "player") {
            Physics2D.IgnoreCollision(this.gameObject.GetComponent<Collider2D>(), collision.gameObject.GetComponent<Collider2D>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

