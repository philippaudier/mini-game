using UnityEngine;

public static class BlockCutter
{
    public struct CutResult
    {
        public bool perfect;
        public bool missed;
        public GameObject placedBlock;
        public GameObject fallingPiece;
        public Vector3 newSize;
        public Vector3 newPosition;
    }

    private const float PerfectTolerance = 0.05f;

    /// <summary>
    /// Cuts a moving block against the previous block.
    /// axis: 0 = X, 1 = Z
    /// </summary>
    public static CutResult Cut(GameObject movingBlock, Vector3 prevPosition, Vector3 prevSize, int axis)
    {
        CutResult result = new CutResult();

        Vector3 movingPos = movingBlock.transform.position;
        Vector3 movingSize = movingBlock.transform.localScale;

        float movingMin, movingMax, prevMin, prevMax;

        if (axis == 0)
        {
            movingMin = movingPos.x - movingSize.x / 2f;
            movingMax = movingPos.x + movingSize.x / 2f;
            prevMin = prevPosition.x - prevSize.x / 2f;
            prevMax = prevPosition.x + prevSize.x / 2f;
        }
        else
        {
            movingMin = movingPos.z - movingSize.z / 2f;
            movingMax = movingPos.z + movingSize.z / 2f;
            prevMin = prevPosition.z - prevSize.z / 2f;
            prevMax = prevPosition.z + prevSize.z / 2f;
        }

        // Overlap region
        float overlapMin = Mathf.Max(movingMin, prevMin);
        float overlapMax = Mathf.Min(movingMax, prevMax);
        float overlapSize = overlapMax - overlapMin;

        // Complete miss
        if (overlapSize <= 0f)
        {
            result.missed = true;
            // Let the whole block fall
            if (movingBlock.TryGetComponent<MovingBlock>(out var mb))
                Object.Destroy(mb);
            Rigidbody rb = movingBlock.AddComponent<Rigidbody>();
            rb.mass = 1f;
            result.fallingPiece = movingBlock;
            return result;
        }

        float currentSize = (axis == 0) ? movingSize.x : movingSize.z;
        float hangover = currentSize - overlapSize;

        // Perfect placement
        if (hangover < PerfectTolerance)
        {
            result.perfect = true;
            result.placedBlock = movingBlock;
            result.newSize = movingSize;

            // Snap to previous position on the axis
            Vector3 snappedPos = movingPos;
            if (axis == 0)
                snappedPos.x = prevPosition.x;
            else
                snappedPos.z = prevPosition.z;
            movingBlock.transform.position = snappedPos;
            result.newPosition = snappedPos;

            if (movingBlock.TryGetComponent<MovingBlock>(out var mb))
                Object.Destroy(mb);

            return result;
        }

        // Normal cut: create placed block + falling piece
        float overlapCenter = (overlapMin + overlapMax) / 2f;

        // Placed block
        Vector3 placedSize = movingSize;
        Vector3 placedPos = movingPos;
        if (axis == 0)
        {
            placedSize.x = overlapSize;
            placedPos.x = overlapCenter;
        }
        else
        {
            placedSize.z = overlapSize;
            placedPos.z = overlapCenter;
        }

        movingBlock.transform.localScale = placedSize;
        movingBlock.transform.position = placedPos;

        if (movingBlock.TryGetComponent<MovingBlock>(out var movingComp))
            Object.Destroy(movingComp);

        result.placedBlock = movingBlock;
        result.newSize = placedSize;
        result.newPosition = placedPos;

        // Falling piece
        float fallingSize = hangover;
        float fallingCenter;

        // Determine which side is hanging
        if (axis == 0)
        {
            if (movingPos.x > prevPosition.x)
                fallingCenter = overlapMax + fallingSize / 2f;
            else
                fallingCenter = overlapMin - fallingSize / 2f;
        }
        else
        {
            if (movingPos.z > prevPosition.z)
                fallingCenter = overlapMax + fallingSize / 2f;
            else
                fallingCenter = overlapMin - fallingSize / 2f;
        }

        GameObject fallingPiece = Object.Instantiate(movingBlock);
        Vector3 fallSize = movingSize;
        Vector3 fallPos = movingPos;
        if (axis == 0)
        {
            fallSize.x = fallingSize;
            fallPos.x = fallingCenter;
        }
        else
        {
            fallSize.z = fallingSize;
            fallPos.z = fallingCenter;
        }
        fallingPiece.transform.localScale = fallSize;
        fallingPiece.transform.position = fallPos;

        Rigidbody fallRb = fallingPiece.AddComponent<Rigidbody>();
        fallRb.mass = 0.5f;
        result.fallingPiece = fallingPiece;

        // Destroy falling piece after a delay
        Object.Destroy(fallingPiece, 3f);

        return result;
    }
}
