using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BPlusTree.Core;

namespace BPlusTree.Test
{
    [TestClass]
    public class leafs_right_enumerator_should
    {
        string name = "leafs_right_enumerator_should";

        [TestMethod]
        public void be_able_enumerate_all_values()
        {
            var cfg = Config.Configuration<int>.Default_For(name);
            cfg.BPTree_Order = 3;
            cfg.Stream_Factory.Clear();
            var tree = new Core.BPlusTree<int>(cfg);

            for (int i = 0; i < 10; i++)
                tree.Put(i, BitConverter.GetBytes(i));

            tree.Commit();
            var reader = tree as IData_Reader<int>;

            var leaf = tree.Find_Leaf_Node(0);
            var enumerator = new Leafs_Right_Enumerator<int>(leaf, 0, reader);

            while (enumerator.MoveNext())
            {
                var data = enumerator.Current;
                var id = BitConverter.ToInt32(data.Payload, 0);
            }
        }
    }
}
