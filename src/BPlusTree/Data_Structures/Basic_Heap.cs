using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace File_System_ES
{
    public class BasicHeap<TIndex>
    {
        private List<HeapNode<TIndex>> InnerList = new List<HeapNode<TIndex>>();

        public BasicHeap() { }

        public int Count
        {
            get
            {
                return InnerList.Count;
            }
        }

        public void Push(TIndex index, long weight)
        {
            int p = InnerList.Count, p2;
            InnerList.Add(new HeapNode<TIndex>(index, weight)); // E[p] = O
            do
            {
                if (p == 0)
                    break;

                p2 = (p - 1) >> 1;
                if (InnerList[p].weight < InnerList[p2].weight)
                {
                    var h = InnerList[p];
                    InnerList[p] = InnerList[p2];
                    InnerList[p2] = h;

                    p = p2;
                }
                else
                    break;
            } while (true);
        }

        public TIndex Pop()
        {
            var result = InnerList[0];
            int p = 0, p1, p2, pn;
            InnerList[0] = InnerList[InnerList.Count - 1];
            InnerList.RemoveAt(InnerList.Count - 1);
            do
            {
                pn = p;
                p1 = (p << 1) + 1;
                p2 = (p << 1) + 2;
                if (InnerList.Count > p1 && InnerList[p].weight > InnerList[p1].weight)
                    p = p1;
                if (InnerList.Count > p2 && InnerList[p].weight > InnerList[p2].weight)
                    p = p2;

                if (p == pn)
                    break;

                var h = InnerList[p];
                InnerList[p] = InnerList[pn];
                InnerList[pn] = h;

            } while (true);
            return result.index;
        }
    }

    public struct HeapNode<I>
    {
        public HeapNode(I i, long w)
        {
            index = i;
            weight = w;

        }
        public I index;
        public long weight;

        public override string ToString()
        {
            return string.Format("Idx : {0}, Weight : {1}", index, weight);
        }
    }

}
