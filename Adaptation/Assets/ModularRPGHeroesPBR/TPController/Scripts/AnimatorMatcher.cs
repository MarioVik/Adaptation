using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DM
{
    public class AnimatorMatcher : MonoBehaviour
    {
        Animator anim;

        PlayerControlManager playerControl;
        EnemyControlManager enemyControl;
        bool isPlayer;

        Vector3 dPosition;
        Vector3 vPosition;


        void Start()
        {
            anim = GetComponent<Animator>();
            if (gameObject.tag == "Player")
            {
                isPlayer = true;
                playerControl = GetComponentInParent<PlayerControlManager>();
            }
            else if (gameObject.tag == "Enemy")
            {
                isPlayer = false;
                enemyControl = GetComponentInParent<EnemyControlManager>();
            }
        }


        private void OnAnimatorMove()   //It updates every frame when animator's animations in play.
        {

            if (isPlayer ? playerControl.canMove : enemyControl.canMove)
                return;

            if (isPlayer)
                playerControl.rigid.drag = 0;
            else
                enemyControl.rigid.drag = 0;

            float multiplier = 3f;

            dPosition = anim.deltaPosition;   //storing delta positin of active model's position.         

            dPosition.y = 0f;   //flatten the Y (height) value of root animations.

            vPosition = (dPosition * multiplier) / Time.fixedDeltaTime;     //defines the vector 3 value for the velocity.      

            if (isPlayer)
                playerControl.rigid.velocity = vPosition; //This will move the root gameObject for matching active model's position.
            else
                enemyControl.rigid.velocity = vPosition;
        }

    }
}