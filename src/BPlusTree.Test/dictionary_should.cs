using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BPlusTree.Core;


namespace BPlusTree.Test
{
    [TestClass]
    public class dictionary_should
    {
        string name = "test_dictionary";

        [TestMethod]
        public void be_created_with_Int_Key_and_string_Value()
        {
            var dictionary = new Persistent_Dictionary<int, string>(name);
        }

        [TestMethod]
        public void persist_an_insered_Value()
        {
            var dictionary = new Persistent_Dictionary<int, string>(name);
            dictionary.Clear();
            dictionary[1] = "ciao2";

            var ciao = dictionary[1];
        }

        [TestMethod]
        public void persist_two_insered_Value()
        {
            var dictionary = new Persistent_Dictionary<int, string>(name);
            dictionary.Clear();

            dictionary[1] = "ciao 1";
            dictionary[2] = "ciao 2";

            var ciao_1 = dictionary[1];
            var ciao_2 = dictionary[2];

            Assert.AreEqual("ciao 1", ciao_1);
            Assert.AreEqual("ciao 2", ciao_2);
        }

        [TestMethod]
        [ExpectedExceptionAttribute(typeof(Key_Not_Found))]
        public void remove_inserd_value_when_cleaning()
        {
            var dictionary = new Persistent_Dictionary<int, string>(name);
            dictionary.Clear();
            dictionary[1] = "ciao2";
            dictionary.Clear();

            var _ = dictionary[1];
        }

        [TestMethod]
        public void insert_1000_items_and_remember_them_all_backward()
        {
            var dictionary = new Persistent_Dictionary<int, string>(name);
            dictionary.Clear();

            string body ="new item with body ";
            for (int i = 0; i < 1000; i++)
                dictionary[i] = body + i;

            for (int i = 999; i >= 0; i--)
                Assert.AreEqual(dictionary[i], body + i);
        }

        [TestMethod]
        public void return_record_count()
        {
            var dictionary = new Persistent_Dictionary<int, string>(name);
            dictionary.Clear();

            string body = "new item with body ";
            int count = 1000;
            for (int i = 0; i < count; i++)
                dictionary[i] = body + i;

            var verify_count = dictionary.Count;

            Assert.AreEqual(count, verify_count);
        }


    }
}
