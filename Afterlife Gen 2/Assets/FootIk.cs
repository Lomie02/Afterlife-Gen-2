using UnityEngine;
using UnityEngine.Animations.Rigging;

public class FootIk : MonoBehaviour
{
    public Transform leftFootTarget;
    public Transform rightFootTarget;

    public LayerMask m_GroundLayer;

    float footOffset = 0.1f;
    float RaycastDistance = 1.5f;
    float footPlacementSpeed = 5f;

    void Update()
    {
        AdjustFootPlacement(leftFootTarget);
        AdjustFootPlacement(leftFootTarget);
    }

    void AdjustFootPlacement(Transform foot)
    {
        RaycastHit hit;

        Vector3 startRay = foot.position + Vector3.up * 0.5f;

        if (Physics.Raycast(startRay, Vector3.down, out hit, RaycastDistance, m_GroundLayer))
        {
            Vector3 newPoint = hit.point + Vector3.up * footOffset;
            leftFootTarget.position = Vector3.Lerp(foot.position, newPoint, Time.deltaTime * footPlacementSpeed);
        }
    }
}
