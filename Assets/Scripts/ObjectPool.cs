using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : new()
{
    Stack<T> pool;

    int cur_capacity;
    public ObjectPool(int capacity)
    {
        pool = new Stack<T>(capacity);
        for(int i = 0; i < capacity; ++i)
        {
            pool.Push(new T());
        }
        cur_capacity = capacity;
    }

    public T GetOne()
    {
        if(pool.Count <= 0)
        {
            for (int i = 0; i < 1.5 * cur_capacity; ++i) {
                pool.Push(new T());
            }
            cur_capacity += pool.Count;
        }
        return pool.Pop();
    }

    public void ReturnOne(T o)
    {
        pool.Push(o);
    }
}
