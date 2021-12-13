using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace v1.Controller
{
	public class CameraController : MonoBehaviour {

		public GameObject player;

		// Update is called once per frame
		void Update () {
			Vector3 playerPos = player.transform.position;
			playerPos.z = -10;
			transform.position = playerPos;
		}
	}
}