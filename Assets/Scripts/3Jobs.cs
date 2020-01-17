using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

class ApplyVelocitySample1 : MonoBehaviour
{




    struct Job1: IJob
    {


        public void Execute()
        {

            int x = 0;
            for (int i = 0; i < 10000; i++)
            {
                x++;

            }
        }
    }
    struct  Job3: IJob
    {


        public void Execute()
        {

            int x = 0;
            for (int i = 0; i < 10000; i++)
            {
                x++;

            }
        }
    }


    struct Job2: IJob
    {
       

        public void Execute()
        {

            int x = 0;
            for (int i=0; i<10000;i++)
            {
                x++;

            }
        }
    }

    public void Update()
    {

        var job1 = new Job1();
        JobHandle jh1= job1.Schedule();
        



        var job2 = new Job2();
        JobHandle jh2= job2.Schedule(); //Schedule second job
        


        var job3 = new Job3();
        JobHandle jh3 = job3.Schedule();


    }
}