using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blockpath
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

}
