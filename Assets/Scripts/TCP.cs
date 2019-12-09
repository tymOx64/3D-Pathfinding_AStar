using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MathNet.Numerics;
namespace Assets.Scripts
{



    /// <summary>
    /// open traveling salesman problem
    /// </summary>
    class TSP : World
    {

        public List<Block> nodes;
        public float[,] distance;

        /// <summary>
        /// adds startblock and apples to nodes
        /// </summary>
        /// <param name="startBlock"></param>
        /// <param name="apples"></param>

        public TSP(Block startBlock, List<Block> apples)
        {
            int minCost;
            List<Block> cheapestPath;

            nodes.Add(startBlock);


            foreach (Block b in apples)
            {

                nodes.Add(b);

            }


        }




        /// <summary>
        /// classic TSP distance matrix
        /// </summary>
        /// <returns></returns>
        public float[,] distanceMatrix()
        {

            distance = new float[nodes.Count, nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < nodes.Count; j++)
                {


                    distance[i, j] = findPath(nodes.ElementAt<Block>(i), nodes.ElementAt<Block>(j)).cost;
                    distance[j, i] = distance[i, j]; //the same price for the way there and back
                }



            }
            return distance;
        }


        /// <summary>
        /// returns the cheapest path 
        /// </summary>
        /// <returns></returns>
        public List<Block> BruteForce()
        {



            for (int i = 1; i <= faculty(nodes.Count - 1); i++)
            {


                List<Block> sublist = new List<Block>();
                sublist.Add(nodes.ElementAt<Block>(0));//all paths start at the player's starting position

            }

            List<List<Block>> paths = new List<List<Block>>(); //all possible paths

            //hier sollen später aller permutationen berechnet werden


            return null;
        }




        /// <summary>
        /// faculty
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public int faculty(int n)
        {
            int f = 1;
            for (int i = 1; i <= n; i++)
            {

                f *= i;

            }

            return f;

        }

    }
}