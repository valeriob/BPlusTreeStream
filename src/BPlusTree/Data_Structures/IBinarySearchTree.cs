using System;
using System.Collections.Generic;

namespace File_System_ES.Tree
{
	public interface IBinarySearchTree<TKey, TValue>
	{
		IComparer<TKey> Comparer { get; }
		int Count { get; }
		// IBinaryTree
		bool IsEmpty { get; }
		TValue Value { get; }

		IBinarySearchTree<TKey, TValue> LeftMost { get; }
		IBinarySearchTree<TKey, TValue> RightMost { get; }

		// IBinarySearchTree
		IBinarySearchTree<TKey, TValue> Left { get; }
		IBinarySearchTree<TKey, TValue> Right { get; }
		TKey Key { get; }
		IEnumerable<TKey> KeysInOrder { get; }
		IEnumerable<TKey> KeysInReverseOrder { get; }
		IEnumerable<TValue> ValuesInOrder { get; }
		IEnumerable<TValue> ValuesInReverseOrder { get; }
		IEnumerable<KeyValuePair<TKey, TValue>> Pairs { get; }
		IBinarySearchTree<TKey, TValue> Search(TKey key);
		IEnumerable<TValue> GreaterThan(TKey gtKey);
		IEnumerable<TValue> LessThan(TKey ltKey);
		IEnumerable<TValue> LessThanOrEqual(TKey ltKey);
		IEnumerable<TValue> GreaterThanOrEqual(TKey gteKey);
		IBinarySearchTree<TKey, TValue> Add(TKey key, TValue value);
		IBinarySearchTree<TKey, TValue> AddOrUpdate(TKey key, TValue value, Func<TKey, TValue, TValue> updateValueFactory);
		IBinarySearchTree<TKey, TValue> TryRemove(TKey key, out bool removed, out TValue value);

		// IMap
		bool Contains(TKey key);
		bool TryGetValue(TKey key, out TValue value);
		IBinarySearchTree<TKey, TValue> LocateNearest(TKey key, Predicate<TValue> isMatch);
	}
}