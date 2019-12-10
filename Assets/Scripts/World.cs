using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realtime.Messaging.Internal;


/// <summary>
/// The world MonoBehavior is in charge of creating, updating and destroying chunks based on the player's location.
/// These mechanisms are completed with the help of Coroutines (IEnumerator methods). https://docs.unity3d.com/Manual/Coroutines.html
/// Der Unterschied zwischen Koroutinen und Funktionen ist, dass Koroutinen ihren Ablauf unterbrechen und später wieder aufnehmen können, wobei sie ihren Status beibehalten
/// </summary>
public class World : MonoBehaviour
{
    public GameObject player;
    public Material textureAtlas;
    public Material fluidTexture;
    public static int columnHeight = 16;
    public static int chunkSize = 4;
    public static int radius = 3;
    public static uint maxCoroutines = 1000;
    public static Dictionary<string, Chunk> chunks ;


    //Worldsize
    public int WorldX;  
    public int WorldY;




    List<Block> randomlySpawnedApples = new List<Block>();


    public static List<string> toRemove = new List<string>();        //TODO:delete

    public static bool firstbuild = true;                             //TODO:delete

    public static CoroutineQueue queue;                               //TODO:delete





    public Vector3 lastbuildPos;

    /// <summary>
    /// Creates a name for the chunk based on its position
    /// </summary>
    /// <param name="v">Position of tje chunk</param>
    /// <returns>Returns a string witht he chunk's name</returns>
	public static string BuildChunkName(Vector3 v)
    {
        return (int)v.x + "_" +
                     (int)v.y + "_" +
                     (int)v.z;
    }

    /// <summary>
    /// Creates a name for the column based on its position
    /// </summary>
    /// <param name="v">Position of the column</param>
    /// <returns>Returns a string witht he column's name</returns>
	public static string BuildColumnName(Vector3 v)
    {
        return (int)v.x + "_" + (int)v.z;
    }

    /// <summary>
    /// Get block based on world coordinates
    /// </summary>
    /// <param name="pos">Rough position of the block to be returned</param>
    /// <returns>Returns the block related to the input position</returns>
	public static Block GetWorldBlock(Vector3 pos)
    {
        int cx, cy, cz;

        if (pos.x < 0)
            cx = (int)((Mathf.Round(pos.x - chunkSize) + 1) / (float)chunkSize) * chunkSize;
        else
            cx = (int)(Mathf.Round(pos.x) / (float)chunkSize) * chunkSize;

        if (pos.y < 0)
            cy = (int)((Mathf.Round(pos.y - chunkSize) + 1) / (float)chunkSize) * chunkSize;
        else
            cy = (int)(Mathf.Round(pos.y) / (float)chunkSize) * chunkSize;

        if (pos.z < 0)
            cz = (int)((Mathf.Round(pos.z - chunkSize) + 1) / (float)chunkSize) * chunkSize;
        else
            cz = (int)(Mathf.Round(pos.z) / (float)chunkSize) * chunkSize;

        int blx = (int)Mathf.Abs((float)Mathf.Round(pos.x) - cx);
        int bly = (int)Mathf.Abs((float)Mathf.Round(pos.y) - cy);
        int blz = (int)Mathf.Abs((float)Mathf.Round(pos.z) - cz);

        string cn = BuildChunkName(new Vector3(cx, cy, cz));
        Chunk c;
        if (chunks.TryGetValue(cn, out c))
        {

            return c.chunkData[blx, bly, blz];
        }
        else
            return null;
    }

    /// <summary>
    /// Instantiates a new chunk at a specified location.
    /// </summary>
    /// <param name="x">y position of the chunk</param>
    /// <param name="y">y position of the chunk</param>
    /// <param name="z">z position of the chunk</param>
	private void BuildChunkAt(int x, int y, int z)
    {

        //Debug.Log("Methodenaufruf BuildChunkAt");
        Vector3 chunkPosition = new Vector3(x * chunkSize,
                                            y * chunkSize,
                                            z * chunkSize);

        string n = BuildChunkName(chunkPosition);
        Chunk c;

        if (!chunks.TryGetValue(n, out c))
        {
            c = new Chunk(chunkPosition, textureAtlas, fluidTexture);
            c.chunk.transform.parent = this.transform;
            c.fluid.transform.parent = this.transform;
            chunks.Add(c.chunk.name, c); // add chunk to the dictionary
        }
    }


    /*
    /// <summary>
    /// Coroutine to recursively build chunks of the world depending on some location and a radius.
    /// </summary>
    /// <param name="x">x position</param>
    /// <param name="y">y position</param>
    /// <param name="z">z position</param>
    /// <param name="startrad">Starting radius (is necessary for recursive calls of this function)</param>
    /// <param name="rad">Desired radius</param>
    /// <returns></returns>
	IEnumerator BuildRecursiveWorld(int x, int y, int z, int startrad, int rad)
    {
        int nextrad = rad - 1;
        if (rad <= 0 || y < 0 || y > columnHeight) yield break;
        // Build chunk front
        BuildChunkAt(x, y, z + 1);
        queue.Run(BuildRecursiveWorld(x, y, z + 1, rad, nextrad));
        yield return null;

        // Build chunk back
        BuildChunkAt(x, y, z - 1);
        queue.Run(BuildRecursiveWorld(x, y, z - 1, rad, nextrad));
        yield return null;

        // Build chunk left
        BuildChunkAt(x - 1, y, z);
        queue.Run(BuildRecursiveWorld(x - 1, y, z, rad, nextrad));
        yield return null;

        // Build chunk right
        BuildChunkAt(x + 1, y, z);
        queue.Run(BuildRecursiveWorld(x + 1, y, z, rad, nextrad));
        yield return null;

        // Build chunk up
        BuildChunkAt(x, y + 1, z);
        queue.Run(BuildRecursiveWorld(x, y + 1, z, rad, nextrad));
        yield return null;

        // Build chunk down
        BuildChunkAt(x, y - 1, z);
        queue.Run(BuildRecursiveWorld(x, y - 1, z, rad, nextrad));
        yield return null;
    }


        */
    /// <summary>
    /// Coroutine to render chunks that are in the DRAW state. Adds chunks to the toRemove list, which are outside the player's radius.
    /// </summary>
    /// <returns></returns>
/*	IEnumerator DrawChunks()
    {
        toRemove.Clear();
        foreach (KeyValuePair<string, Chunk> c in chunks)
        {
            if (c.Value.status == Chunk.ChunkStatus.DRAW)
            {
                c.Value.DrawChunk();
            }
            if (c.Value.chunk && Vector3.Distance(player.transform.position,
                                c.Value.chunk.transform.position) > radius * chunkSize)
                toRemove.Add(c.Key);

            yield return null;
        }
    }

    
    */




    //New methods


        //Creates static world, TODO caves,..
    private void BuildWorld(int xMax, int zMax)   // xMax, yMax, zMax determine the size of the world
    {
        int startX = (int)(player.transform.position.x / chunkSize);
        int startY = (int)(player.transform.position.y / chunkSize);
        int startZ = (int)(player.transform.position.z / chunkSize);

        //Build first chunk
        

            BuildChunkAt(startX,startY,startZ);


        BuildChunkAt(startX + 1, startY, startZ); //?
        
     

            //Build the whole world from the position of the first chunk.


            for (int x = startX; x < xMax ; x++)
            {
                 
                    for (int z = startZ; z < zMax ; z++)
                    {


                

                // World Height equals 6 Chunks
                  BuildChunkAt(x,startY,z);
                  BuildChunkAt(x, startY+1, z);
                  BuildChunkAt(x, startY, z);
                  BuildChunkAt(x, startY - 1, z);
                  BuildChunkAt(x, startY - 2, z);
                  BuildChunkAt(x, startY - 3, z);
                  


                    }

                }

  


    }



    //Draws all the chunks with status draw
    private void DrawChunks()
    {

        //Debug.Log("Methodenaufruf DrawChunks");
        foreach (KeyValuePair<string, Chunk> c in chunks)
        {
            if (c.Value.status == Chunk.ChunkStatus.DRAW)
            {
                c.Value.DrawChunk();
            }
        }
    }









/*
    /// <summary>
    /// Coroutine to save and then to unload unused chunks.
    /// </summary>
    /// <returns></returns>
    IEnumerator RemoveOldChunks()
    {
        for (int i = 0; i < toRemove.Count; i++)
        {
            string n = toRemove[i];
            Chunk c;
            if (chunks.TryGetValue(n, out c))
            {
                Destroy(c.chunk);
                c.Save();
                chunks.TryRemove(n, out c);
                yield return null;
            }
        }
    }


    */

        /*
    /// <summary>
    /// Builds chunks that are inside the player's radius.
    /// </summary>
	public void BuildNearPlayer()
    {
        // Stop the coroutine of building the world, because it is getting replaced
        StopCoroutine("BuildRecursiveWorld");
        queue.Run(BuildRecursiveWorld((int)(player.transform.position.x / chunkSize),
                                            (int)(player.transform.position.y / chunkSize),
                                            (int)(player.transform.position.z / chunkSize), radius, radius));
    }


    */
    /// <summary>
    /// Unity lifecycle start method. Initializes the world and its first chunk and triggers the building of further chunks.
    /// Player is disabled during Start() to avoid him falling through the floor. Chunks are built using coroutines.
    /// </summary>
    void Start()
    {
        
        Vector3 ppos = player.transform.position;
        player.transform.position = new Vector3(ppos.x,
                                            Utils.GenerateHeight(ppos.x, ppos.z) + 1,
                                            ppos.z);
        lastbuildPos = player.transform.position;
        player.SetActive(false);

        firstbuild = true;
        chunks = new Dictionary<string, Chunk>();
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;

        // queue = new CoroutineQueue(maxCoroutines, StartCoroutine);


        /*
        // Build starting chunk
        BuildChunkAt((int)(player.transform.position.x / chunkSize),
                                            (int)(player.transform.position.y / chunkSize),
                                            (int)(player.transform.position.z / chunkSize));
        // Draw starting chunk
        queue.Run(DrawChunks());

        // Create further chunks
        queue.Run(BuildRecursiveWorld((int)(player.transform.position.x / chunkSize),
                                            (int)(player.transform.position.y / chunkSize),
                                            (int)(player.transform.position.z / chunkSize), radius, radius));*/
        
    
       BuildWorld(20,20);


        RandomAppleSpawn();

        DrawChunks();
    }



























    //valid world coordinates for simple testing purposes
    //33 65 23
    //43 65 32

    /// <summary>
    /// finds the shortest path between two blocks
    /// </summary>
    public Blockpath findPath(Block startBlock, Block endBlock)
    {
        Heap<Block> openSet = new Heap<Block>(chunkSize * columnHeight * chunkSize * columnHeight);

        HashSet<Block> closedSet = new HashSet<Block>();

        openSet.Add(startBlock);

        while(openSet.Count > 0)
        {
            Block currentBlock = openSet.RemoveFirst();
            //TESTING: visualize pathfinding
            //Instantiate(testCube, currentBlock.worldPosition, Quaternion.identity);
            closedSet.Add(currentBlock);

            foreach(Block neighbourBlock in GetNeighbourBlocks(currentBlock))
            {
                if (closedSet.Contains(neighbourBlock))
                    continue;
                float newMovementCostToNeighbour = currentBlock.gCost + calcDistance(currentBlock, neighbourBlock);
                if(newMovementCostToNeighbour < neighbourBlock.gCost || !openSet.Contains(neighbourBlock))
                {
                    neighbourBlock.gCost = newMovementCostToNeighbour;
                    neighbourBlock.hCost = calcDistance(neighbourBlock, endBlock);
                    neighbourBlock.pathParent = currentBlock;

                    //reached the endblock, but due to how the neighbourblocks are found above the ground
                    //it happens to find the destination above the endblock, correcting this before invoking RetracePath
                    if (neighbourBlock.hCost <= 0.5f)
                    {
                        neighbourBlock.pathParent = null;
                        endBlock.pathParent = currentBlock;

                        Debug.Log("PATH COMPLETE");
                        return RetracePath(startBlock, endBlock, endBlock.getFCost(), openSet, closedSet);
                    }


                    if (!openSet.Contains(neighbourBlock))
                        openSet.Add(neighbourBlock);
                    else
                        openSet.UpdateItem(neighbourBlock);
                }
            }
        }
        Debug.Log("Unable to find path");
        return null;
    }

    /// <summary>
    /// finds all REACHABLE(!) neighbourblocks around given argument block
    /// </summary>
    public List<Block> GetNeighbourBlocks(Block block)
    {
        List<Block> neighbourList = new List<Block>();
        for(int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0)
                    continue;
                Block neighbour = getNeighbourBlockAboveGround(block.worldPosition, new Vector3(x, 0f, z));
                if (neighbour != null)
                {
                    neighbourList.Add(neighbour);
                }
            }
        }
        //Debug.Log("NeighbourSize: " + neighbourList.Count);
        return neighbourList;
    }

    /// <summary>
    /// calculates the distance between two blocks along the x- and z-axis, where only block-to-direct-neighbourblock-movement is allowed
    /// </summary>
    public float calcDistance(Block blockA, Block blockB)
    {
        float dstX = Mathf.Abs(blockA.worldPosition.x - blockB.worldPosition.x);
        float dstY = Mathf.Abs(blockA.worldPosition.z - blockB.worldPosition.z);

        //moving diagonally to a neighbourblock results in a distance of roughly 1.41 worldunits
        if (dstX > dstY)
            return dstY * 1.41f + (dstX - dstY);
        return dstX * 1.41f + (dstY - dstX);
    }

    /// <summary>
    /// retraces the path from end to start by going along the pathParents of each block
    /// </summary>
    /// <param name="cost"> the total cost of the path </param>
    public Blockpath RetracePath(Block startBlock, Block endBlock, float cost, Heap<Block> openSet, HashSet<Block> closedSet)
    {
        List<Block> blockList = new List<Block>();
        Block currentBlock = endBlock;

        while(currentBlock != startBlock)
        {
            //Visualization for testing purposes
            Instantiate(testCube, currentBlock.worldPosition, Quaternion.identity);

            blockList.Add(currentBlock);
            currentBlock = currentBlock.pathParent;
        }
        //reset all pathfinding Val (gCost, hCost, pathParent) for potential upcoming pathfinding runs
        while(openSet.Count > 0)
        {
            openSet.RemoveFirst().ResetPathfindingVal();
        }
        foreach(Block block in closedSet)
            block.ResetPathfindingVal();

        return new Blockpath(startBlock, endBlock, cost, blockList);
    }

    public Vector3 roundVector3(Vector3 vec)
    {
        return new Vector3((float)Mathf.RoundToInt(vec.x),
            (float)Mathf.RoundToInt(vec.y),
            (float)Mathf.RoundToInt(vec.z));
    }


    /// <param name="columnPos"> the block-column where we want to get the block above the ground </param>
    public Block getFirstNonsolidBlockAboveGround(Vector3 columnPos)
    {
        //traverse to highest valid blockPos, to avoid finding a nonsolid block under the ground
        while(GetWorldBlock(columnPos) != null)
        {         
            columnPos += Vector3.up;
        }
        if (GetWorldBlock(columnPos + Vector3.down) == null)
        {
            Debug.Log("tried getting block out of map-area");
            return null;
        }         
        //mainly for debugging/testing purposes. probably not needed lateron
        int errorCheck = 0;
        while (GetWorldBlock(columnPos) == null)
        {
            columnPos += Vector3.down;
            errorCheck++;
            if (errorCheck >= 100)
            {
                Debug.Log("couldnt get first nonsolid block above ground");
                return null;
            }                
        }
        //traverse until reaching a solid block
        while (GetWorldBlock(columnPos) == null || !GetWorldBlock(columnPos).isSolid)
        {
            columnPos += Vector3.down;
            if(errorCheck++ >= 125)
            {
                return null;
            }
        }
        columnPos += Vector3.up;
        Block resultBlock = GetWorldBlock(columnPos);
        if (resultBlock == null)
            return null;
        resultBlock.worldPosition = columnPos;
        return resultBlock;
    }

    /// <summary>
    /// E.g. relativePos of (-1,1,0) would return the neighbourblock which is -1 in x direction, +1 in y direction (or whatever y is needed to get above the ground)
    /// </summary>
    public Block getNeighbourBlockAboveGround(Vector3 blockPos, Vector3 relativePos)
    {
        Vector3 worldPosOfNeighbourBlock = blockPos + relativePos;
        Block resultBlock = getFirstNonsolidBlockAboveGround(worldPosOfNeighbourBlock);
        
        //can not jump more than 1f in y direction (i think, needs to be confirmed)
        if (resultBlock == null || resultBlock.worldPosition.y > blockPos.y + 1f)
            return null;
        return resultBlock;
    }

    /// <summary>
    /// returns null if worldPos is not a valid position in a chunk; returns the created block on success
    /// </summary>
    public Block createBlockAtWorldPos(Vector3 worldPos, Block.BlockType blockType)
    {
        Block block = GetWorldBlock(worldPos);
        if (block == null)
            return null;
        Chunk chunk = block.owner;
        chunk.chunkData[(int)block.position.x, (int)block.position.y, (int)block.position.z] =
                    new Block(blockType, new Vector3(block.position.x, block.position.y, block.position.z), chunk.chunk.gameObject, chunk);
        chunk.UpdateChunk();
        chunk.Redraw();
        Block createdBlock = chunk.chunkData[(int)block.position.x, (int)block.position.y, (int)block.position.z];
        createdBlock.worldPosition = worldPos;
        return createdBlock;
    }


    float timer = 2f;
    public GameObject testCube;

    bool einmal = true;
    /// <summary>
    /// Unity lifecycle update method. Actviates the player's GameObject. Updates chunks based on the player's position.
    /// </summary>
    void Update()
    {
        //beim aufruf von findPath muss der zurückgegebene blockpath im start- und zielblock gespeichert werden (im attribut edges) 
        if (einmal)
        {
            foreach(Block appleA in randomlySpawnedApples)
            {
                foreach (Block appleB in randomlySpawnedApples)
                {
                    findPath(appleA, appleB);
                }
            } 
            
        }
        

        einmal = false;
        /*//für testzwecke; wird alle 'timer' sek ausgeführt
        if(timer < Time.timeSinceLevelLoad)
        {
            timer += 60f;

            createBlockAtWorldPos(new Vector3(33, 65, 28), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(33, 66, 28), Block.BlockType.STONE);

            createBlockAtWorldPos(new Vector3(36, 65, 28), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(36, 66, 28), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(37, 65, 28), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(37, 66, 28), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(38, 65, 28), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(38, 66, 28), Block.BlockType.STONE);

            createBlockAtWorldPos(new Vector3(32, 65, 28), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(32, 66, 28), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(34, 65, 28), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(34, 66, 28), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(35, 65, 28), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(35, 66, 28), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(31, 65, 28), Block.BlockType.STONE);

            //block vor wand
            //createBlockAtWorldPos(new Vector3(31, 65, 29), Block.BlockType.GOLD);

            createBlockAtWorldPos(new Vector3(31, 66, 28), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(30, 65, 28), Block.BlockType.STONE);
            //createBlockAtWorldPos(new Vector3(30, 66, 28), Block.BlockType.STONE);            

            createBlockAtWorldPos(new Vector3(29, 65, 28), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(29, 66, 28), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(28, 65, 28), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(28, 66, 28), Block.BlockType.STONE);


            Block testBlockA = createBlockAtWorldPos(new Vector3(34, 65, 24), Block.BlockType.REDSTONE);
            Block testBlockB = createBlockAtWorldPos(new Vector3(33, 65, 45), Block.BlockType.REDSTONE);
            findPath(testBlockB, testBlockA);

            /*foreach(Block randomApple in randomlySpawnedApples)
            {
                findPath(testBlockA, randomApple);
            }
            
        }
        */




        // Activate the player's GameObject
        if (!player.activeSelf)
        {
            player.SetActive(true);
            
        }
    }

    //List<Block> _blockList


    /*
    /// <summary>
    /// Method randomly distributes apples(currently redstones) on the surface of the world
    /// </summary>
    public void RandomAppleSpawn( ) 
    {
        int amount = (int) Random.Range(4.0f, 6.0f);  //Amount of apples greater than or equal to 4 and less than or equal to 6

        Debug.Log("Amount" + amount.ToString());
        int testt = 0;
        while (amount > 0 && testt < 1000)
        {
            testt++;
            if (testt > 900)
                Debug.Log("testt failed");
            foreach (KeyValuePair<string, Chunk> c in chunks)
            {
                if( amount <= 0)
                {

                    break;
                }

                float createApple= Random.Range(0f, 1f); //random decision whether an apple is spawned on the current chunk or not

                if (createApple >0.5f)
                {
                    
                    Block block = c.Value.chunkData[3, 3, 3];

                    int i = 0;
                    //ensures that there is at least one air block above the apple and a solid block below it and that it only replaces air blocks
                    //and that 
                    while (!(block.HasSolidNeighbour((int)block.position.x, (int)block.position.y-1-i, (int)block.position.z) 
                        && !block.HasSolidNeighbour((int)block.position.x, (int)block.position.y-i , (int)block.position.z) 
                        && !block.HasSolidNeighbour((int)block.position.x, (int)block.position.y + 1-i, (int)block.position.z)) && i < 3)
                        
                    {
                        block = c.Value.chunkData[3, 3-i, 3];
                        i++;
                        

                        amount--;
                        Debug.Log(amount);
                    }
                    block.BuildBlock(Block.BlockType.REDSTONE);
                    randomlySpawnedApples.Add(c.Value.chunkData[3, 3-i, 3]);
                }
            }

        }
    }
    */

    /// <summary>
    /// Method randomly distributes apples(currently redstones) on the surface of the world
    /// </summary>
    public void RandomAppleSpawn()
    {
        int amount = (int)Random.Range(1.0f, 1.0f);  //Amount of apples greater than or equal to 4 and less than or equal to 6

        Debug.Log("Amount to be spawned" + amount.ToString());
        int testt = 0;
        Block block;

        while (amount > 0 && testt < 1000)
        {
            testt++;
            if (testt > 900)
                Debug.Log("testt failed");
            float xOffset = Random.Range(0f, 50f);
            float zOffset = Random.Range(0f, 50f);

            block = getFirstNonsolidBlockAboveGround(new Vector3(10f + xOffset, 65f, 10f + zOffset));

            if (block == null)
                continue;

            amount--;
            block.BuildBlock(Block.BlockType.REDSTONE);
            randomlySpawnedApples.Add(block);
        }
    }



}

