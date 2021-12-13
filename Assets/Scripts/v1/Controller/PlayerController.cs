using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using v1.Pathfinding2D;

namespace v1.Controller
{
	public class PlayerController : MonoBehaviour
	{
		private Pathfinding2DBehavior _pathfinding2D;
		// Use this for initialization
		void Start () {
			_pathfinding2D = GetComponent<Pathfinding2DBehavior>();
		}
	
		// Update is called once per frame
		void Update () {
			if (Input.GetMouseButtonDown(0))
			{
				if (Camera.main)
				{
					Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
					_pathfinding2D.FindPath(mousePos);
					//_pathfinding2D.ToWorldPosSize(gameObject);
				}
			}
		}
	}
}