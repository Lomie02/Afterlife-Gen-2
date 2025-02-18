using System.Collections;
using UnityEngine;

public class HandSway : MonoBehaviour
{
    public float swayAmount = 0.02f;
    public float maxSwayAmount = 0.06f;
    public float smoothAmount = 6f;

    float amount = 10;
    float maxamount = 15;
    float smooth = 3;
    private Vector3 def;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
        def = transform.localEulerAngles;

        StartCoroutine(UpdateItemSway());
    }

    IEnumerator UpdateItemSway()
    {
        while (true)
        {
            float moveX = -Input.GetAxis("Mouse X") * swayAmount;
            float moveY = -Input.GetAxis("Mouse Y") * swayAmount;

            moveX = Mathf.Clamp(moveX, -maxSwayAmount, maxSwayAmount);
            moveY = Mathf.Clamp(moveY, -maxSwayAmount, maxSwayAmount);

            float factorX = (Input.GetAxis("Mouse Y")) * amount;
            float factorY = -(Input.GetAxis("Mouse X")) * amount;
            float factorZ = -Input.GetAxis("Vertical") * amount;

            if (factorX > maxamount)
                factorX = maxamount;

            if (factorX < -maxamount)
                factorX = -maxamount;

            if (factorY > maxamount)
                factorY = maxamount;

            if (factorY < -maxamount)
                factorY = -maxamount;

            if (factorZ > maxamount)
                factorZ = maxamount;

            if (factorZ < -maxamount)
                factorZ = -maxamount;

            Vector3 Final = new Vector3(def.x + factorX, def.y + factorY, def.z + factorZ);
            transform.localEulerAngles = Vector3.Slerp(transform.localEulerAngles, Final, (Time.deltaTime * smooth));

            Vector3 targetPosition = new Vector3(moveX, moveY, 0f);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition + initialPosition, Time.deltaTime * smoothAmount);

            yield return new WaitForSeconds(0.05f);
        }
    }
}


