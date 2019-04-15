using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    float speed, range;
    int damage;

    Vector3 startPos;

    public void Initialize(Transform originTransform, float speed, float range, int damage)
    {
        transform.SetPositionAndRotation(originTransform.position, originTransform.rotation);
        this.speed = speed;
        this.range = range;
        this.damage = damage;

        startPos = originTransform.position;
    }

    void Awake()
    {
        //hitEnemies = new List<EnemyHealth>();
    }

    void Update()
    {
        transform.position += transform.forward.normalized * speed * Time.deltaTime;

        if (Vector3.Distance(startPos, transform.position) >= range)
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag != "Player")
        {
            EnemyHealth enemyHealth = collision.collider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage, enemyHealth.transform.position);
            }
            Destroy(gameObject);
            Debug.Log("Enemy hit");
        }
    }
}