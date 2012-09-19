using System.Collections.Generic;

namespace File_System_ES.Tree
{
	public interface IStack<T> : IEnumerable<T>
	{
		IStack<T> Pop();
		IStack<T> Push(T element);
		T Peek();
		bool IsEmpty { get; }
	}
}