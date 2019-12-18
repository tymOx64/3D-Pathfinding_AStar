using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{

    Blockpath[] bpArray;
    int indexBP;
    int indexBlock;
    Block nextBlock;
    float rotationSpeed = 22f;
    float moveSpeed = 8f;

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
        //Debug.Log("Distance: " + (transform.position - nextBlock.worldPosition).magnitude);
        if((transform.position - nextBlock.worldPosition).magnitude <= 0.15f)
        {
            IterateBlock();
        }            

        Vector3 lookDir = (nextBlock.worldPosition - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(lookDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        //  transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * RotationSpeed);

        /*float angle = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg;
        transform.eulerAngles = (new Vector3(0, angle + 180, 0));*/

        transform.position += transform.forward * Time.deltaTime * moveSpeed;
    }


    public void SetRoundtrip(Blockpath[] roundTrip)
    {
        bpArray = roundTrip;
        nextBlock = bpArray[0].blockList.ToArray()[0];
    }

    public void IterateBlock()
    {
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
}
