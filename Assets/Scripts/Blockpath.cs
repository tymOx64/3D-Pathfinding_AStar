using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blockpath : MonoBehaviour
{
    public Block startBlock, endBlock;
    public List<Block> blockList;
    public float cost;

    public Blockpath(Block _startBlock, Block _endBlock, float _cost, List<Block> _blockList)
    {
        startBlock = _startBlock;
        endBlock = _endBlock;
        cost = _cost;
        blockList = _blockList;
        //because of the way retracing paths works, we have to reserve it in order to get the path from startblock to endblock
        blockList.Reverse();
    }







    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
