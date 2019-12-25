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
    float rotationSpeed = 15f; //16f smooth
    float moveSpeed = 2f; //4f smooth
    float fallAccelerationFactor = 1.0f; //1.0f means there is no acceleration 

    //when iterating to the next block, store movement information in bool variable
    bool stayOnSameLevel;
    bool fallDown;
    bool jump;

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
        float verticalDistance = float.NegativeInfinity;
        if(previousBlock != null)
        {
            verticalDistance = CalcVerticalDistance();
        }

        if((transform.position - nextBlock.worldPosition).magnitude <= 0.15f)
        {
            IterateBlock();
        }            

        Vector3 lookDir = (nextBlock.worldPosition - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(lookDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        //if we just started moving, there is no prev block and therefore we just move straight towards the next block        
        if (previousBlock == null || stayOnSameLevel)
        {
            transform.position += transform.forward * Time.deltaTime * moveSpeed;
        }
        else if(fallDown)
        {
            Debug.Log("Horizontal distance: " + CalcHorizontalDistanceAIPos());
            //pseudo code [TODO]: move horizontally until there is just air beneath us, then dont move along x or z achsis and fall until y coordinate is reached
            if(CalcHorizontalDistanceAIPos() > 0.1f)
            {
                Vector3 horizontalMovDir = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
                transform.position += horizontalMovDir * Time.deltaTime * moveSpeed;
            }
            else
            {
                transform.position += transform.forward * Time.deltaTime * moveSpeed * fallAccelerationFactor;
                //pro Sekunde um x % schneller fallen /// zurzeit mit 0% , also OHNE, sieht besser aus so glaube ich, ggf. loeschen
                fallAccelerationFactor += 0.0f * Time.deltaTime;
            }
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
        fallAccelerationFactor = 1.0f;

        SetMovementVariables();
    }

    private void SetMovementVariables()
    {
        if (Mathf.Abs(CalcVerticalDistance()) < 0.5f)
        {
            stayOnSameLevel = true;
            fallDown = false;
            jump = false;
        }
        else if (CalcVerticalDistance() < 0.0f)
        {
            stayOnSameLevel = false;
            fallDown = true;
            jump = false;
        }
        else if (CalcVerticalDistance() > 0.0f)
        {
            stayOnSameLevel = false;
            fallDown = false;
            jump = true;
        }
    }

    public float CalcVerticalDistance()
    {
        return nextBlock.worldPosition.y - previousBlock.worldPosition.y;
    }

    public float CalcVerticalDistanceAIPos()
    {
        return nextBlock.worldPosition.y - transform.position.y;
    }

    public float CalcHorizontalDistance()
    {
        return new Vector2(nextBlock.worldPosition.x - previousBlock.worldPosition.x, nextBlock.worldPosition.z - previousBlock.worldPosition.z).magnitude;
    }

    public float CalcHorizontalDistanceAIPos()
    {
        return new Vector2(nextBlock.worldPosition.x - transform.position.x, nextBlock.worldPosition.z - transform.position.z).magnitude;
    }
}
