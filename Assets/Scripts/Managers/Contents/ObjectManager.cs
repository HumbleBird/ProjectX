using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ObjectManager
{
	#region Id를 용한 Dic
	// 추후에 서버 붙으면 자주 이용할 오브젝트 매니저
	//Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
	//Dictionary<int, Item> _items = new Dictionary<int, Item>();

	//public void Add(int id, GameObject go)
	//{
	//	var r = _objects.ContainsKey(id);
	//	if (r == true)
	//		return;

	//	_objects.Add(id, go);


	//}

	//public void Remove(int id)
	//{
	//	_objects.Remove(id);
	//}

	//public GameObject Find(Func<GameObject, bool> condition)
	//{
	//	foreach (GameObject obj in _objects.Values)
	//	{
	//		if (condition.Invoke(obj))
	//			return obj;
	//	}

	//	return null;
	//}

	//public GameObject Find(int id)
	//{
	//	GameObject obj = null;
	//	_objects.TryGetValue(id, out obj);
	//	if (obj == null)
	//		return null;

	//	return obj;
	//}
	#endregion

	public List<GameObject> _objects = new List<GameObject>();

	//public PlayerManager m_MyPlayer { get; set; }

	public void Add(GameObject go)
	{
		_objects.Add(go);
	}

	public void Remove(GameObject go)
	{
		_objects.Remove(go);
	}

	public GameObject Find(Func<GameObject, bool> condition)
	{
		foreach (GameObject obj in _objects)
		{
			if (condition.Invoke(obj))
				return obj;
		}

		return null;
	}

	public List<GameObject> FindList(Func<GameObject, bool> condition)
	{
		List<GameObject> list = new List<GameObject>();

		foreach (GameObject obj in _objects)
		{
			if (condition.Invoke(obj))
				list.Add(obj);
		}

		return list;
	}

	public void Clear()
	{
		_objects.Clear();
		//m_MyPlayer = null;
	}



}
