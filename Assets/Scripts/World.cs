using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realtime.Messaging.Internal;
using Assets.Scripts;
using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using Debug = UnityEngine.Debug;

/// <summary>
/// The world MonoBehavior is in charge of creating, updating and destroying chunks based on the player's location.
/// These mechanisms are completed with the help of Coroutines (IEnumerator methods). https://docs.unity3d.com/Manual/Coroutines.html
/// Der Unterschied zwischen Koroutinen und Funktionen ist, dass Koroutinen ihren Ablauf unterbrechen und später wieder aufnehmen können, wobei sie ihren Status beibehalten
/// </summary>
public class World : MonoBehaviour
{


    public GameObject AIcam;

    public Material textureAtlas;
    public Material fluidTexture;
    public static int columnHeight = 16;
    public static int chunkSize = 4;
    public static int radius = 3;
    public static uint maxCoroutines = 1000;
    public static Dictionary<string, Chunk> chunks;
    List<Block> randomlySpawnedApples = new List<Block>();
    public static List<string> toRemove = new List<string>();
    public static bool firstbuild = true;
    public Vector3 lastbuildPos;
    public static CoroutineQueue queue;

    //Worldsize
    public int WorldX;
    public int WorldY;





    /// <summary>
    /// Creates a name for the chunk based on its position
    /// </summary>
    /// <param name="v">Position of the chunk</param>
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

        int blx = (int)Mathf.Abs((int)Mathf.Round(pos.x) - cx);
        int bly = (int)Mathf.Abs((int)Mathf.Round(pos.y) - cy);
        int blz = (int)Mathf.Abs((int)Mathf.Round(pos.z) - cz);

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





    /// <summary>
    /// Creates static world
    /// </summary>
    /// <param name="xMax"></param>
    /// <param name="zMax"></param>

    private void BuildWorld(int xMax, int zMax)   // xMax, yMax, zMax determine the size of the world
    {
        int startX = 2;
        int startY = 17;
        int startZ = 2;

        //Build first chunk


        BuildChunkAt(startX, startY, startZ);


        BuildChunkAt(startX + 1, startY, startZ); //?



        //Build the whole world from the position of the first chunk.


        for (int x = startX; x < xMax; x++)
        {

            for (int z = startZ; z < zMax; z++)
            {




                // World Height equals 6 Chunks
                BuildChunkAt(x, startY, z);
                BuildChunkAt(x, startY + 1, z);
                BuildChunkAt(x, startY, z);
                BuildChunkAt(x, startY - 1, z);
                BuildChunkAt(x, startY - 2, z);
                BuildChunkAt(x, startY - 3, z);



            }

        }


    }




    /// <summary>
    /// renders chunks
    /// </summary>

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





    /// <summary>
    /// Unity lifecycle start method. Initializes the world and its first chunk and triggers the building of further chunks.
    /// Player is disabled during Start() to avoid him falling through the floor. Chunks are built using coroutines.
    /// </summary>
    void Start()
    {



        /* 
         Vector3 ppos = player.transform.position;
         player.transform.position = new Vector3(ppos.x,
                                             Utils.GenerateHeight(ppos.x, ppos.z) + 1,
                                             ppos.z);
         lastbuildPos = player.transform.position;
         player.SetActive(false);*/

        firstbuild = true;
        chunks = new Dictionary<string, Chunk>();
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;


        BuildWorld(20, 20);        

        DrawChunks();

    

    }

    List<List<Block>> aStarListsOfVistedBlocks = new List<List<Block>>();

    /// <summary>
    /// finds the shortest path between two blocks
    /// </summary>
    public Blockpath findPath(Block startBlock, Block endBlock)
    {
        List<Block> visitedBlocks = new List<Block>();

        Heap<Block> openSet = new Heap<Block>(chunkSize * columnHeight * chunkSize * columnHeight);

        HashSet<Block> closedSet = new HashSet<Block>();

        openSet.Add(startBlock);

        Stopwatch swNeighbourblocks = new Stopwatch();
        Stopwatch swCalcdistance = new Stopwatch();
        Stopwatch swRemoveFirstFromHeap = new Stopwatch();
        Stopwatch swSetValues = new Stopwatch();

        while (openSet.Count > 0)
        {
            swRemoveFirstFromHeap.Start();
            Block currentBlock = openSet.RemoveFirst();
            swRemoveFirstFromHeap.Stop();

            closedSet.Add(currentBlock);
            //visitedBlocks.Add(currentBlock);

            swNeighbourblocks.Start();
            List<Block> neighbourBlocks = GetNeighbourBlocks(currentBlock);

            //Debug.Log("elapsed ms neighbours: " + swNeighbourblocks.ElapsedMilliseconds);
            swNeighbourblocks.Stop();

            foreach (Block neighbourBlock in neighbourBlocks)
            {
                //swCalcdistance.Start();
                swCalcdistance.Start();
                if (closedSet.Contains(neighbourBlock))
                    continue;
                swCalcdistance.Stop();


                float newMovementCostToNeighbour = (float)(currentBlock.gCost + calcDistance(currentBlock, neighbourBlock));

                

                swSetValues.Start();

                if (newMovementCostToNeighbour < neighbourBlock.gCost || !openSet.Contains(neighbourBlock))
                {
                    neighbourBlock.gCost = newMovementCostToNeighbour;

                    
                    neighbourBlock.hCost = calcDistance(neighbourBlock, endBlock);
                    

                    neighbourBlock.pathParent = currentBlock;

                    //reached the endblock, but due to how the neighbourblocks are found above the ground
                    //it happens to find the destination above the endblock, correcting this before invoking RetracePath
                    if (neighbourBlock.hCost <= 0.5f)
                    {
                        neighbourBlock.pathParent = null;
                        neighbourBlock.gCost = 0f;
                        neighbourBlock.hCost = 0f;

                        endBlock.pathParent = currentBlock;
                        endBlock.gCost = newMovementCostToNeighbour;
                        endBlock.hCost = 0f;


                        openSet.Add(endBlock);

                        UnityEngine.Debug.Log("Time elapsed whilst calculating the (up to 8) neighbourblocks: " + swNeighbourblocks.ElapsedMilliseconds + " ms"); //51ms  605ms
                        UnityEngine.Debug.Log("Time elapsed whilst checking if a neighbourblock is already in the closed set: " + swCalcdistance.ElapsedMilliseconds + " ms"); //23ms   742ms
                        UnityEngine.Debug.Log("Time elapsed whilst assigning the values: " + swSetValues.ElapsedMilliseconds + " ms"); //13ms  96ms
                        UnityEngine.Debug.Log("Time elapsed whilst grabbing the 1st block from the heap (and maintaining its structure!): " + swRemoveFirstFromHeap.ElapsedMilliseconds + " ms");
                        UnityEngine.Debug.Log("Total cost of the path: " + endBlock.gCost);


                        //aStarListsOfVistedBlocks.Add(visitedBlocks);
                        UnityEngine.Debug.Log("PATH COMPLETE");
                        return RetracePath(startBlock, endBlock, endBlock.getFCost(), openSet, closedSet);
                    }


                    if (!openSet.Contains(neighbourBlock))
                        openSet.Add(neighbourBlock);
                    else
                        openSet.UpdateItem(neighbourBlock);
                }

                swSetValues.Stop();
            }
        }
        UnityEngine.Debug.Log("Unable to find path");
        return null;
    }


    /// <summary>
    /// finds all REACHABLE(!) neighbourblocks around given argument block
    /// </summary>
    public List<Block> GetNeighbourBlocks(Block block)
    {
        List<Block> neighbourList = new List<Block>();
        for (int x = -1; x <= 1; x++)
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
        {
            return dstY * 1.41f + (dstX - dstY);
        }            
        return dstX * 1.41f + (dstY - dstX);
    }



    /// <summary>
    /// 
    /// s the path from end to start by going along the pathParents of each block
    /// </summary>
    /// <param name="cost"> the total cost of the path </param>
    public Blockpath RetracePath(Block startBlock, Block endBlock, float cost, Heap<Block> openSet, HashSet<Block> closedSet)
    {
        List<Block> blockList = new List<Block>();
        Block currentBlock = endBlock;
        //Debug.Log("START RETRACE");
        while (currentBlock != startBlock)
        {
            //Visualization for testing purposes
            //Instantiate(testCube, currentBlock.worldPosition, Quaternion.identity);
            //Debug.Log("WorldPos: " + currentBlock.worldPosition);
            blockList.Add(currentBlock);
            currentBlock = currentBlock.pathParent;
        }
        //Debug.Log("END RETRACE");
        //reset all pathfinding Val (gCost, hCost, pathParent) for potential upcoming pathfinding runs
        while (openSet.Count > 0)
        {
            openSet.RemoveFirst().ResetPathfindingVal();
        }
        foreach (Block block in closedSet)
            block.ResetPathfindingVal();
        
        Blockpath result = new Blockpath(startBlock, endBlock, (float)cost, blockList);

        if (startBlock.edges == null)
            startBlock.edges = new List<Blockpath>();
        if (endBlock.edges == null)
            endBlock.edges = new List<Blockpath>();

        startBlock.edges.Add(result);
        endBlock.edges.Add(result);

        return result;
    }

    HashSet<GameObject> visualizedBlocks = new HashSet<GameObject>();

    public void VisualizeBlockpath(Blockpath bp)
    {
        if (bp == null)
        {
            UnityEngine.Debug.Log("blockpath null");
            return;
        }
        foreach (Block block in bp.blockList)
        {
            visualizedBlocks.Add(Instantiate(testCube, block.worldPosition, Quaternion.identity));
        }
    }

    public void VisualizeBlocklist(List<Block> blocklist)
    {
        foreach (Block block in blocklist)
        {
            visualizedBlocks.Add(Instantiate(testCubeRed, block.worldPosition + new Vector3(0.2f, 0.2f, 0.2f), Quaternion.identity));
        }
    }

    public void ClearRecentVisualization()
    {
        foreach (GameObject whiteCube in visualizedBlocks)
        {
            Destroy(whiteCube);
        }
    }

    public void InstantiateWhiteCubeAtWorldPos(Vector3 worldPos)
    {
        visualizedBlocks.Add(Instantiate(testCube, worldPos, Quaternion.identity));
    }

    int amountOfVisualizedCubesForTestingPurposes = 300;

    /// <param name="columnPos"> the block-column where we want to get the block above the ground </param>
    public Block getFirstNonsolidBlockAboveGround(Vector3 columnPos)
    {
        //at yPos 65 there should a valid block at all valid x and z positions, thus we are outside of the map area if equals to null
        if (GetWorldBlock(new Vector3(columnPos.x, 65f, columnPos.z)) == null)
        {
            //For bugfixing, to be deleted lateron
            if (amountOfVisualizedCubesForTestingPurposes-- > 0)
            {
                //visualizing where the function is getting outside of the map area
                //Instantiate(testCube, new Vector3(columnPos.x, 65f, columnPos.z), Quaternion.identity);
            }
            //Debug.Log("tried getting block out of map-area at WorldPos: " + new Vector3(columnPos.x, 65f, columnPos.z).ToString());           
            return null;
        }

        //traverse to highest valid blockPos, to avoid finding a nonsolid block under the ground
        while (GetWorldBlock(columnPos) != null)
        {
            columnPos += Vector3.up;
        }

        /*if (GetWorldBlock(columnPos + Vector3.down) == null)
        {
            Debug.Log("tried getting block out of map-area");
            return null;
        }*/

        //mainly for debugging/testing purposes. probably not needed lateron
        int errorCheck = 0;
        while (GetWorldBlock(columnPos) == null)
        {
            columnPos += Vector3.down;
            errorCheck++;
            if (errorCheck >= 100)
            {
                UnityEngine.Debug.Log("couldnt get first nonsolid block above ground");
                return null;
            }
        }

        //traverse until reaching a solid block
        while (GetWorldBlock(columnPos) == null || !GetWorldBlock(columnPos).isSolid)
        {
            columnPos += Vector3.down;
            if (errorCheck++ >= 125)
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

    //will probably be not needed anymore since we have to calculate all paths in both directions

 


    float timer = 2f;
    public GameObject testCube;
    public GameObject testCubeRed;

    bool einmal = true;
    /// <summary>
    /// Unity lifecycle update method. Actviates the player's GameObject. Updates chunks based on the player's position.
    /// </summary>
    void Update()
    {
        if (einmal)
        {
            CreateStoneWalls();
            RandomAppleSpawn();
            Stopwatch swA = new Stopwatch();
            swA.Start();

            //player.active = false;
            //AIcam.active = true;
            foreach (Block appleA in randomlySpawnedApples)
            {
                foreach (Block appleB in randomlySpawnedApples)
                {
                    if (appleA == appleB)
                    {
                        continue;
                    }
                    if (TSP.GetBlockpathFromAToB(appleA, appleB) == null)
                        findPath(appleA, appleB);
                    //VisualizeBlockpath(tspTestObj.GetBlockpathFromAToB(appleA, appleB));
                }
            }
            swA.Stop();
            UnityEngine.Debug.Log("Time used to calculate all paths/edges: " + swA.ElapsedMilliseconds + " ms");

            Stopwatch swB = new Stopwatch();
            swB.Start();

            TSP tsp = new TSP(randomlySpawnedApples);
            tsp.SetWorldObj(this.gameObject);
            Block[] roundTrip = tsp.simulatedAnnealing();
            Blockpath[] bpArray = new Blockpath[roundTrip.Length];

            swB.Stop();
            UnityEngine.Debug.Log("Time used to calculate the roundtrip (TSP sim. annealing): " + swB.ElapsedMilliseconds + " ms");

            for (int i = 0; i < roundTrip.Length - 1; i++)
            {
                VisualizeBlockpath(TSP.GetBlockpathFromAToB(roundTrip[i], roundTrip[i + 1]));
                bpArray[i] = TSP.GetBlockpathFromAToB(roundTrip[i], roundTrip[i + 1]);
            }
            bpArray[bpArray.Length - 1] = TSP.GetBlockpathFromAToB(roundTrip[roundTrip.Length - 1], roundTrip[0]);
            VisualizeBlockpath(TSP.GetBlockpathFromAToB(roundTrip[roundTrip.Length - 1], roundTrip[0]));

            //CorrectBlockpathsDirectionForRoundtrip(bpArray);

            AIcam.GetComponent<AIMovement>().SetRoundtrip(bpArray);

            StartCoroutine(VisualizeAllConfigs(tsp.simulatedAnnealingIntermediateConfigs));

            //StartCoroutine(VisualizedAllAStarCalcs());
        }

        einmal = false;

      
    }

    public List<GameObject> redBlocks = new List<GameObject>();

    public IEnumerator VisualizeAStarList(List<Block> blocklist)
    {
        foreach (Block block in blocklist)
        {
            if (redBlocks.Count > 4)
            {
                Destroy(redBlocks[0]);
                redBlocks.RemoveAt(0);
            }
            redBlocks.Add(Instantiate(testCubeRed, block.worldPosition + new Vector3(0.2f, 0.2f, 0.2f), Quaternion.identity));
            yield return new WaitForSeconds(0.06f);
        }
        yield return new WaitForSeconds(2f);
    }

    public IEnumerator VisualizedAllAStarCalcs()
    {
        foreach (List<Block> blocklist in aStarListsOfVistedBlocks)
        {
            yield return VisualizeAStarList(blocklist);
        }
    }


    public IEnumerator VisualizeConfig(List<Block> config)
    {
        ClearRecentVisualization();
        Block[] configArr = config.ToArray();
        for (int i = 0; i < configArr.Length - 1; i++)
        {
            Blockpath bp = TSP.GetBlockpathFromAToB(configArr[i], configArr[i + 1]);
            if (bp == null)
                continue;
            foreach (Block block in bp.blockList)
            {
                InstantiateWhiteCubeAtWorldPos(block.worldPosition);                
            }
        }
        Blockpath bpLast = TSP.GetBlockpathFromAToB(configArr[configArr.Length - 1], configArr[0]);
        foreach (Block block in bpLast.blockList)
        {
            InstantiateWhiteCubeAtWorldPos(block.worldPosition);
        }
        //Debug.Log("end of visualizing a single config");
        yield return new WaitForSeconds(1f);
    }

    public IEnumerator VisualizeAllConfigs(List<List<Block>> allTSPConfigs)
    {
        foreach (List<Block> config in allTSPConfigs)
        {
            //Debug.Log("vis next config");
            yield return StartCoroutine(VisualizeConfig(config));
        }
    }


    public void CreateStoneWalls()
    {
        for(int x = 22; x < 47; x++)
        {
            if(x != 40 && x != 41)
            {
                createBlockAtWorldPos(new Vector3(x, 65, 33), Block.BlockType.STONE);
                createBlockAtWorldPos(new Vector3(x, 66, 33), Block.BlockType.STONE);
            }            

            createBlockAtWorldPos(new Vector3(x, 65, 30), Block.BlockType.STONE);
            createBlockAtWorldPos(new Vector3(x, 66, 30), Block.BlockType.STONE);
        }

        createBlockAtWorldPos(new Vector3(46, 65, 31), Block.BlockType.STONE);
        createBlockAtWorldPos(new Vector3(46, 66, 31), Block.BlockType.STONE);
        createBlockAtWorldPos(new Vector3(46, 65, 32), Block.BlockType.STONE);
        createBlockAtWorldPos(new Vector3(46, 66, 32), Block.BlockType.STONE);

        for (int z = 22; z < 47; z++)
        {
            if(z != 31 && z != 32)
            {
                createBlockAtWorldPos(new Vector3(38, 65, z), Block.BlockType.STONE);
                createBlockAtWorldPos(new Vector3(38, 66, z), Block.BlockType.STONE);
            }            
        }

        for (int z = 34; z < 42; z++)
        {
            if (true || z != 31 && z != 32)
            {
                createBlockAtWorldPos(new Vector3(42, 65, z), Block.BlockType.STONE);
                createBlockAtWorldPos(new Vector3(42, 66, z), Block.BlockType.STONE);
            }
        }

        createBlockAtWorldPos(new Vector3(43, 65, 41), Block.BlockType.STONE);
        createBlockAtWorldPos(new Vector3(43, 66, 41), Block.BlockType.STONE);
        createBlockAtWorldPos(new Vector3(44, 65, 41), Block.BlockType.STONE);
        createBlockAtWorldPos(new Vector3(44, 66, 41), Block.BlockType.STONE);
        createBlockAtWorldPos(new Vector3(40, 65, 41), Block.BlockType.STONE);
        createBlockAtWorldPos(new Vector3(40, 66, 41), Block.BlockType.STONE);
        createBlockAtWorldPos(new Vector3(41, 65, 41), Block.BlockType.STONE);
        createBlockAtWorldPos(new Vector3(41, 66, 41), Block.BlockType.STONE);
        createBlockAtWorldPos(new Vector3(39, 65, 41), Block.BlockType.STONE);
        createBlockAtWorldPos(new Vector3(39, 66, 41), Block.BlockType.STONE);
    }

    /// <summary>
    /// Method randomly distributes apples(currently redstones) on the surface of the world
    /// </summary>
    public void RandomAppleSpawn()
    {
        //hardcoded nonrandom spawn
        randomlySpawnedApples.Add(getFirstNonsolidBlockAboveGround(AIcam.transform.position));
        
        Block blockA = getFirstNonsolidBlockAboveGround(new Vector3(55f, 66f, 42f));
        blockA.BuildBlock(Block.BlockType.APPLE);
        randomlySpawnedApples.Add(blockA);

        Block blockB = getFirstNonsolidBlockAboveGround(new Vector3(50f, 65f, 29f));
        blockB.BuildBlock(Block.BlockType.APPLE);
        randomlySpawnedApples.Add(blockB);

        //dead end bait block
        Block blockC = getFirstNonsolidBlockAboveGround(new Vector3(30f, 65f, 56f));
        blockC.BuildBlock(Block.BlockType.APPLE);
        randomlySpawnedApples.Add(blockC);

        Block blockD = getFirstNonsolidBlockAboveGround(new Vector3(26f, 65f, 28f));
        blockD.BuildBlock(Block.BlockType.APPLE);
        randomlySpawnedApples.Add(blockD);

        Block blockE = getFirstNonsolidBlockAboveGround(new Vector3(36f, 65f, 39f));
        blockE.BuildBlock(Block.BlockType.APPLE);
        randomlySpawnedApples.Add(blockE);

        //random locations after return ----->
        return;

        int amount = 1;

        UnityEngine.Debug.Log("Amount to be spawned: " + amount.ToString());
        int testt = 0;
        Block block;
        HashSet<Vector3> spawnLocations = new HashSet<Vector3>();

        randomlySpawnedApples.Add(getFirstNonsolidBlockAboveGround(AIcam.transform.position));

        while (amount > 0 && testt < 500)
        {
        proceed:
            testt++;
            if (testt > 498)
            {
                //UnityEngine.Debug.Log("random apple spawn failed to spawn all apples");
                return;
            }
            float xOffset = Random.Range(5f, 60f);
            float zOffset = Random.Range(5f, 60f);

            Vector3 spawnPos = new Vector3(10f + (int)xOffset, 65f, 10f + (int)zOffset);

            block = getFirstNonsolidBlockAboveGround(spawnPos);

            if (block == null)
                continue;

            //coninue if newly spawned apple would be in a radius of 5 units within another apple
            foreach (Vector3 vec in spawnLocations)
            {
                if (Mathf.Abs(vec.x - spawnPos.x) <= 4 && Mathf.Abs(vec.z - spawnPos.z) <= 4)
                {
                    //UnityEngine.Debug.Log("Skipped spawnPos - too close to another apple");
                    goto proceed;
                }
            }

            spawnLocations.Add(spawnPos);
            amount--;
            // block.BuildBlock(Block.BlockType.WOOD);
            block.BuildBlock(Block.BlockType.APPLE);
            randomlySpawnedApples.Add(block);
        }
    }
}