using UnityEngine;
using System.Collections.Generic;

public static class ListExtensions {

	/// <summary>
	/// If the element at index /index/ is not at the end of the list, 
	/// null is placed there so that indices do not get rearranged
	/// </summary>
	/// <typeparam name="T">The type /l/ is a list of</typeparam>
	/// <param name="l">The list to remove the item from</param>
	/// <param name="index">The index to remove the item</param>
	public static void NullRemoveAt<T>(this List<T> l, int index)
    {

    }
}
