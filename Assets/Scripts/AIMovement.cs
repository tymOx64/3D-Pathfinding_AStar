using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{

    Blockpath[] bpArray;
    int indexBP;
    int indexBlock;
    Block nextBlock;

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
        if (nextBlock == null)
            return;
        Debug.Log("Distance: " + (transform.position - nextBlock.worldPosition).magnitude);
        if((transform.position - nextBlock.position).magnitude <= 1f)
        {
            nextBlock = GetNextBlock();
        }            

        Vector3 lookDir = transform.position - nextBlock.worldPosition;

        float angle = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg;

        transform.eulerAngles = (new Vector3(0, angle + 180, 0));
        transform.position += transform.forward * Time.deltaTime;
    }


    public void SetRoundtrip(Blockpath[] roundTrip)
    {
        bpArray = roundTrip;
        nextBlock = bpArray[0].blockList.ToArray()[0];
    }

    public Block GetNextBlock()
    {
        indexBlock++;
        if (indexBlock >= bpArray.Length)
        {
            indexBlock = 0;
            indexBP++;
        }
        if(indexBP >= bpArray.Length)
        {
            return null;
        }
        Blockpath currentBP = bpArray[indexBP];
        Block nextBlock = currentBP.blockList.ToArray()[indexBlock];        
        return nextBlock;
    }
}
