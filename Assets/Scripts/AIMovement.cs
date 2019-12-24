using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{

    Blockpath[] bpArray;
    int indexBP;
    int indexBlock;
    Block previousBlock = null;
    Block nextBlock;
    float rotationSpeed = 16f;
    float moveSpeed = 4f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public AIMovement()
    {
        indexBP = 0;
        indexBlock = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (nextBlock == null || moveSpeed == 0f)
            return;
        float heightDifference = float.NegativeInfinity;
        if(previousBlock != null)
        {
            heightDifference = CalcVerticalDistance();
        }

        if((transform.position - nextBlock.worldPosition).magnitude <= 0.15f)
        {
            IterateBlock();
        }            

        Vector3 lookDir = (nextBlock.worldPosition - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(lookDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        //if we just started moving, the heightdifference is neg inf and therefore we just move straight towards the next block
        //if heightdifference is smaller than 0.5 the next block should be on the very same level => move straight forward to nextBlock
        if (heightDifference == float.NegativeInfinity || Mathf.Abs(heightDifference) < 0.5f)
        {
            transform.position += transform.forward * Time.deltaTime * moveSpeed;
        }
        //AI needs to go down at least 1 blocktile
        else if(heightDifference < -0.4f)
        {
            //pseudo code [TODO]: move horizontally until there is just air beneath us, then dont move along x or z achsis and fall until y coordinate is reached
        }
        
    }


    public void SetRoundtrip(Blockpath[] roundTrip)
    {
        bpArray = roundTrip;
        nextBlock = bpArray[0].blockList.ToArray()[0];
    }

    public void IterateBlock()
    {
        if (nextBlock != null)
            previousBlock = nextBlock;
        indexBlock++;
        Debug.Log("indexBlock: " + indexBlock + " , indexBP: " + indexBP);
        if (indexBlock >= bpArray[indexBP].blockList.ToArray().Length)
        {
            indexBlock = 0;
            indexBP++;
        }
        if(indexBP >= bpArray.Length)
        {
            moveSpeed = 0f;
            return;
        }
        Blockpath currentBP = bpArray[indexBP];
        nextBlock = currentBP.blockList.ToArray()[indexBlock];
        Debug.Log("nextBlock worldPos: " + nextBlock.worldPosition);
    }

    public float CalcVerticalDistance()
    {
        return nextBlock.worldPosition.y - previousBlock.worldPosition.y;
    }

    public float CalcHorizontalDistance()
    {
        return new Vector2(nextBlock.worldPosition.y - previousBlock.worldPosition.y, nextBlock.worldPosition.z - previousBlock.worldPosition.z).magnitude;
    }
}
