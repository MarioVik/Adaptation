﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    [SerializeField]
    GameObject effectPrefab;
    GameObject effectIntance;

    MonoBehaviour user;

    AudioSource explosionAudio;

    float rotationSpeed = 100;
    float movementSpeed, range;
    int damage;

    Vector3 startPos;

    public void Initialize(EnemyRangedAttacking user, float speed, float range, int damage)
    {
        this.user = user;

        Vector3 originPosition = user.ShootOrigin.position;
        Quaternion originRotation = user.ShootOrigin.rotation;

        Initialize(originPosition, originRotation, speed, range, damage);
    }

    public void Initialize(PlayerRangedAttacking player, float speed, float range, int damage)
    {
        user = player;

        Vector3 originPosition = player.ShootOrigin.position;
        Quaternion originRotation = player.ShootOrigin.rotation;

        Initialize(originPosition, originRotation, speed, range, damage);
    }

    private void Initialize(Vector3 originPosition, Quaternion originRotation, float speed, float range, int damage)
    {
        transform.SetPositionAndRotation(new Vector3(originPosition.x, originPosition.y + 1, originPosition.z), originRotation);
        transform.position += transform.forward;

        movementSpeed = speed;
        this.range = range;
        this.damage = damage;

        startPos = transform.position;

        explosionAudio = GetComponent<AudioSource>();
        effectIntance = Instantiate(effectPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);
    }

    void Update()
    {
        transform.position += transform.forward.normalized * movementSpeed * Time.deltaTime;
        transform.Rotate(Vector3.forward, movementSpeed * rotationSpeed * Time.deltaTime);

        effectIntance.transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);

        if (Vector3.Distance(startPos, transform.position) >= range)
        {
            Destroy(effectIntance.gameObject, 1.0f);
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Effect")
            return;

        if (user is PlayerRangedAttacking)
        {
            if (collision.collider.tag != "Player")
            {
                EnemyHealth enemyHealth = collision.collider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage, enemyHealth.transform.position);
                }

                effectIntance.GetComponentInChildren<RFX4_PhysicsMotion>().Detonate(collision);

                Destroy(effectIntance.gameObject, 1.0f);
                Destroy(gameObject);

                Debug.Log("Enemy hit");
            }
        }
        else if (user is EnemyRangedAttacking)
        {
            if (collision.collider.tag != "Enemy")
            {
                PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }

                effectIntance.GetComponentInChildren<RFX4_PhysicsMotion>().Detonate(collision);

                Destroy(effectIntance.gameObject, 1.0f);
                Destroy(gameObject);

                Debug.Log("Player hit");
            }
        }
    }
}