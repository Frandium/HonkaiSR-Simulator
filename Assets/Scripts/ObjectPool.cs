using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : new()
{
    Stack<T> pool;
    public ObjectPool(int capacity)
    {
        pool = new Stack<T>(capacity);
        for(int i = 0; i < capacity; ++i)
        {
            pool.Push(new T());
        }
    }

    public T GetOne()
    {
        return pool.Pop();
    }

    public void ReturnOne(T o)
    {
        pool.Push(o);
    }
}
