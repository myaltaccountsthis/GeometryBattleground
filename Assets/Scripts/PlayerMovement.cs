using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private const float movementSpeed = 5f;

    void Awake() {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal"), vertical = Input.GetAxis("Vertical");
        Vector2 position = transform.position;
        Vector2 moveVector = new Vector2(horizontal, vertical);
        if (moveVector.magnitude > 1)
            moveVector.Normalize();
        moveVector *= movementSpeed * Time.deltaTime;
        position += moveVector;
        transform.position = position;
        Camera.main.transform.position = new Vector3(position.x, position.y, Camera.main.transform.position.z);
        Vector2 mousePosition = Input.mousePosition;
        transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2(mousePosition.y - Screen.height / 2, mousePosition.x - Screen.width / 2) - 90);
    }
}
