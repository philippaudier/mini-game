using UnityEngine;

public class MovingBlock : MonoBehaviour
{
    private float speed = 4f;
    private float moveRange = 3f;
    private bool movingPositive = true;
    private bool isActive = true;
    private int axis; // 0 = X, 1 = Z

    private Vector3 startPos;

    public void Init(int moveAxis, float blockSpeed, float range)
    {
        axis = moveAxis;
        speed = blockSpeed;
        moveRange = range;
        movingPositive = true;
        isActive = true;

        // Start from the edge
        startPos = transform.position;
        Vector3 pos = startPos;
        if (axis == 0)
            pos.x = moveRange;
        else
            pos.z = moveRange;
        transform.position = pos;
    }

    private void Update()
    {
        if (!isActive) return;

        float delta = speed * Time.deltaTime * (movingPositive ? 1f : -1f);
        Vector3 pos = transform.position;

        if (axis == 0)
        {
            pos.x += delta;
            if (pos.x > moveRange) { pos.x = moveRange; movingPositive = false; }
            else if (pos.x < -moveRange) { pos.x = -moveRange; movingPositive = true; }
        }
        else
        {
            pos.z += delta;
            if (pos.z > moveRange) { pos.z = moveRange; movingPositive = false; }
            else if (pos.z < -moveRange) { pos.z = -moveRange; movingPositive = true; }
        }

        transform.position = pos;
    }

    public void Stop()
    {
        isActive = false;
    }
}
