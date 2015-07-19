using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace BPlusTree.Test
{
    [TestClass]
    public class enumerator_should
    {
        string name = "leafs_right_enumerator_should";

        [TestMethod]
        public void be_able_enumerate_all_values()
        {
            var dictionary = new Persistent_Dictionary<int, int>(name);
            dictionary.Clear();

            int count = 10;
            for (int i = 0; i < count; i++)
                dictionary[i] = i;

            var allValues = dictionary.Values;

            Assert.AreEqual(allValues.Count, count);
            for (int i = 0; i < count; i++)
                Assert.IsTrue(allValues.Contains(i));
        }

        [TestMethod]
        public void be_able_enumerate_all_keys()
        {
            var dictionary = new Persistent_Dictionary<int, int>(name);
            dictionary.Clear();

            int count = 10;
            for (int i = 0; i < count; i++)
                dictionary[i] = i;

            var allKeys = dictionary.Keys;

            Assert.AreEqual(allKeys.Count, count);
            for (int i = 0; i < count; i++)
                Assert.IsTrue(allKeys.Contains(i));
        }

    }

}
