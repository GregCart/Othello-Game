using UnityEngine;


public class BoardInput : MonoBehaviour
{
	private void onMouseDown()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		Physics.Raycast(ray, out hit);
		int x = (int)hit.point.x;
		int y = (int)hit.point.z;
		
		if (x < 0.0f || x >=8.0f ||
			y < 0.0f || y >= 8.0f) return;
			
		OthelloModel.Instance.MakeMove(x, y);
	}
}