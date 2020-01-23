using UnityEngine;
using System.Collections;
using System;

public class Heap<T> where T : IHeapItem<T>
{

    T[] items;
    int currentItemCount;

    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }

    public void Add(T item)
    {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;

        Debug.Log("***** AFTER ADD ITEM******");
        ConsistencyCheck();
    }

    public T RemoveFirst()
    {
        T firstItem = items[0];
        currentItemCount--;
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);

        Debug.Log("***** AFTER REMOVE FIRST ******");
        ConsistencyCheck();

        return firstItem;
    }

    public void PrintHeap()
    {
        if (currentItemCount > 25)
            return;
        for(int i = 0; i < currentItemCount; i++)
        {
            Debug.Log("Heap Index: " + i + " , fCost: " + (items[i].getFCost()));
        }
    }

    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    public int Count
    {
        get
        {
            return currentItemCount;
        }
    }

    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    void ConsistencyCheck()
    {
        for(int i = 0; i < currentItemCount / 2 + 5; i++)
        {
            if(GetChildIndexLeft(i) != -1)
            {
                if(items[i].getFCost() > items[GetChildIndexLeft(i)].getFCost())
                {
                    Debug.Log("Inconsistency at indexes: " + i + " , " + GetChildIndexLeft(i));
                    PrintHeap();
                }
            }

            if (GetChildIndexRight(i) != -1)
            {
                if (items[i].getFCost() > items[GetChildIndexRight(i)].getFCost())
                {
                    Debug.Log("Inconsistency at indexes: " + i + " , " + GetChildIndexRight(i));
                    PrintHeap();
                }
            }
        }
    }

    int GetChildIndexLeft(int index)
    {
        int childIndexLeft = items[index].HeapIndex * 2 + 1;
        if (childIndexLeft >= currentItemCount)
            return -1;
        return childIndexLeft;
    }

    int GetChildIndexRight(int index)
    {
        int childIndexRight = items[index].HeapIndex * 2 + 2;
        if (childIndexRight >= currentItemCount)
            return -1;
        return childIndexRight;
    }

    void SortDown(T item)
    {
        while (true)
        {
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            if (childIndexLeft < currentItemCount)
            {
                swapIndex = childIndexLeft;

                if (childIndexRight < currentItemCount)
                {
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                    {
                        swapIndex = childIndexRight;
                    }
                }

                if (item.CompareTo(items[swapIndex]) < 0)
                {
                    Swap(item, items[swapIndex]);
                }
                else
                {
                    return;
                }

            }
            else
            {
                return;
            }

        }
    }

    void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;

        while (true)
        {
            T parentItem = items[parentIndex];
            if (item.CompareTo(parentItem) > 0)
            {
                Swap(item, parentItem);
            }
            else
            {
                break;
            }

            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    void Swap(T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;
        int itemAIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }
}

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex
    {
        get;
        set;
    }

    float getFCost();

}