using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverseLegsController : MonoBehaviour
{
    Animator m_Animator;

    public LayerMask m_LayerMask; // Select all layers that foot placement applies to.

    [Range(0, 1f)]
    public float m_DistanceToTheGround; // Distance from where the foot transform is to the lowest possible position of the foot.

    private void Start()
    {

        m_Animator = GetComponent<Animator>();

    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
            m_Animator.SetBool("Walk", !m_Animator.GetBool("Walk"));

    }

    private void OnAnimatorIK(int layerIndex)
    {

        if (m_Animator)
        { // Only carry out the following code if there is an Animator set.

            // Set the weights of left and right feet to the current value defined by the curve in our animations.
            m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, m_Animator.GetFloat("IKLeftFootWeight"));
            m_Animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, m_Animator.GetFloat("IKLeftFootWeight"));
            m_Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, m_Animator.GetFloat("IKRightFootWeight"));
            m_Animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, m_Animator.GetFloat("IKRightFootWeight"));

            // Left Foot
            RaycastHit hit;
            // We cast our ray from above the foot in case the current terrain/floor is above the foot position.
            Ray ray = new Ray(m_Animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hit, m_DistanceToTheGround + 1f, m_LayerMask))
            {

                // We're only concerned with objects that are tagged as "Walkable"
                if (hit.transform.tag == "Walkable")
                {

                    Vector3 footPosition = hit.point; // The target foot position is where the raycast hit a walkable object...
                    footPosition.y += m_DistanceToTheGround; // ... taking account the distance to the ground we added above.
                    m_Animator.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
                    m_Animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));

                }

            }

            // Right Foot
            ray = new Ray(m_Animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hit, m_DistanceToTheGround + 1f, m_LayerMask))
            {

                if (hit.transform.tag == "Walkable")
                {

                    Vector3 footPosition = hit.point;
                    footPosition.y += m_DistanceToTheGround;
                    m_Animator.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);
                    m_Animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));

                }

            }


        }

    }
}
