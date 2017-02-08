﻿using UnityEngine;
using System.Collections;

public class Quadtree2<T> : QuadTree2Node<T>, IQuadtree<T>
{
    private SearchData<T> m_searchData = new SearchData<T>();

    public Quadtree2 (float p_bottomLeftX, float p_bottomLeftY, float p_topRightX, float p_topRightY) : base (p_bottomLeftX, p_bottomLeftY, p_topRightX, p_topRightY)
    {
    }
    

    public void Add(float p_keyx, float p_keyy, T p_value)
    {
        QuadNodeData<T> data = new QuadNodeData<T> (p_keyx, p_keyy, p_value);

        this.Add (data);
    }

    public SearchData<T> ClosestTo(float p_keyx, float p_keyy, DQuadtreeFilter<T> p_filter = null)
    {
        m_searchData.SetData (p_keyx, p_keyy, p_filter);

        Search (m_searchData);

        m_searchData.m_currentDistance = Mathf.Sqrt (m_searchData.m_currentDistance);

        return m_searchData;
    }
}

public class QuadTree2Node<T>
{
    private const int K_BUCKET_SIZE = 4;
    private const int K_RIGHT = 1;
    private const int K_TOP = 2;

    private float m_bottomLeftX;
    private float m_bottomLeftY;
    private float m_topRightX;
    private float m_topRightY;

    private float m_centerX;
    private float m_centerY;

    private QuadTree2Node<T>[] m_nodes;
    private QuadNodeData<T>[] m_bucket;
    private int m_bucketCount;

    bool m_bucketMode = true;

    public QuadTree2Node (float p_bottomLeftX, float p_bottomLeftY, float p_topRightX, float p_topRightY)
    {
        this.m_bottomLeftX = p_bottomLeftX;
        this.m_bottomLeftY = p_bottomLeftY;
        this.m_topRightX = p_topRightX;
        this.m_topRightY = p_topRightY;

        m_centerX = (m_bottomLeftX + m_topRightX) * 0.5f;
        m_centerY = (m_bottomLeftY + m_topRightY) * 0.5f;

        m_bucket = new QuadNodeData<T>[K_BUCKET_SIZE];
        m_bucketCount = 0;
    }

    public void Clear()
    {
        m_bucketMode = true;

        m_bucketCount = 0;
    }

    public void Add(QuadNodeData<T> p_data)
    {
        if (m_bucketMode) // Bucket mode
        {
            if (m_bucketCount == K_BUCKET_SIZE) // Spill
            {
                m_bucketMode = false;

                if (m_nodes == null)
                    CreateChildNodes ();
                else
                {
                    for (int i = 0; i < m_nodes.Length; i++)
                    {
                        m_nodes [i].Clear ();
                    }
                }

                QuadNodeData<T>[] temp = m_bucket;

                for (int i = 0; i < K_BUCKET_SIZE; i++)
                {
                    Add (temp [i]);
                }

                Add (p_data);
            }
            else
            {
                m_bucket [m_bucketCount++] = p_data;
            }
        }
        else // Tree mode
        {
            int quadrant = GetQuadrant (p_data.m_keyx, p_data.m_keyy);

            m_nodes [quadrant].Add (p_data);
        }
    }

    public void Search(SearchData<T> p_searchData)
    {
        if (m_bucketMode) // Bucket mode
        {
            for (int i = 0; i < m_bucketCount; i++)
                p_searchData.Feed (m_bucket [i]);
        }
        else // Tree mode
        {
            QuadTree2Node<T>[] nodes = m_nodes;
            int quadrant = GetQuadrant (p_searchData.m_keyx, p_searchData.m_keyy);

            nodes [quadrant].Search (p_searchData);

            float temp = p_searchData.m_keyx - m_centerX;

            int both = 0;

            if ((temp * temp) < p_searchData.m_currentDistance)
            {
                int index = quadrant ^ K_RIGHT;

                nodes [index].Search (p_searchData);

                both++;
            }

            temp = p_searchData.m_keyy - m_centerY;
            if ((temp * temp) < p_searchData.m_currentDistance)
            {
                int index = quadrant ^ K_TOP;

                nodes [index].Search (p_searchData);

                both++;
            }    

            if (both == 2)
            {
                int index = quadrant ^ (K_TOP | K_RIGHT);

                nodes [index].Search (p_searchData);
            }
        }
    }

    private void CreateChildNodes()
    {
        m_nodes = new QuadTree2Node<T>[4];

        m_nodes [0]                 = new QuadTree2Node<T> (m_bottomLeftX, m_bottomLeftY, m_centerX, m_centerY);
        m_nodes [K_RIGHT]           = new QuadTree2Node<T> (m_centerX, m_bottomLeftY, m_topRightX, m_centerY);
        m_nodes [K_TOP]             = new QuadTree2Node<T> (m_bottomLeftX, m_centerY, m_centerX, m_topRightY);
        m_nodes [K_RIGHT + K_TOP]   = new QuadTree2Node<T> (m_centerX, m_centerY, m_topRightX, m_topRightY);
    }

    private int GetQuadrant(float p_keyx, float p_keyy)
    {
        int ret = 0;

        if (p_keyx > m_centerX)
            ret += K_RIGHT;

        if (p_keyy > m_centerY) 
            ret += K_TOP;

        return ret;
    }
}
