using System;
using System.Collections.Generic;

namespace SCCAlgorithm
{
    public class StronglyConnected
    {
        
        static Stack<long> ve;
        public static bool[] mark;
        public static List<long>[] adj;
        public static List<long>[] back_adj;

        public static void dfs1(long s)
        {
            mark[s] = true;
            foreach (long v in adj[s])
            {
                if (!mark[v])
                {
                    dfs1(v);
                }
            }
            ve.Push(s);
        }
        public static void bfs(long s)
        {
            Queue<long> Q = new Queue<long>();
            Q.Enqueue(s);
            mark[s] = true;
            while (Q.Count > 0)
            {
                long u = Q.Peek();
                Q.Dequeue();
                for (int i = 0; i < back_adj[u].Count; i++)
                {
                    long v = back_adj[u][i];
                    if (!mark[v])
                    {
                        Q.Enqueue(v);
                        mark[v] = true;
                    }
                }
            }
        }
        public static long Solve(long nodeCount, long[][] edges)
        {
            mark = new bool[nodeCount + 1];
            adj = new List<long>[nodeCount + 1];
            back_adj = new List<long>[nodeCount + 1];

            ve = new Stack<long>();
            for (int i = 0; i < nodeCount + 1; i++)
            {
                adj[i] = new List<long>();
                back_adj[i] = new List<long>();

            }
            for (int i = 0; i < edges.Length; i++)
            {
                long u = edges[i][0];
                long v = edges[i][1];
                adj[u].Add(v);
                back_adj[v].Add(u);
            }
            for (int i = 1; i <= nodeCount; i++)
            {
                if (!mark[i])
                {
                    dfs1(i);
                }
            }
            mark = new bool[nodeCount + 1];
            long ans = 0;
            while (ve.Count > 0)
            {
                long v = ve.Peek();
                ve.Pop();
                if (!mark[v])
                {
                    bfs(v);
                    ans++;
                }
            }
            return ans;
        }
    }
}
