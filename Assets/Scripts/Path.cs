using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    public Block startBlock, endBlock;
    public List<Block> blockList;
    public float cost;

    public Path(Block _startBlock, Block _endBlock, float _cost, List<Block> _blockList)
    {
        startBlock = _startBlock;
        endBlock = _endBlock;
        cost = _cost;
        blockList = _blockList;
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
