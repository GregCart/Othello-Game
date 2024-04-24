using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;


public class BoardInput : MonoBehaviour, IObservable<Point>
{
    private static List<IObserver<Point>> Observers;


    public void Start()
    {
        Observers = new List<IObserver<Point>>();
    }

    private void OnMouseDown()
	{
		Debug.Log("Mouse Down At: " + Input.mousePosition.ToString());

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);

		Debug.Log("\tHit at: " + hit.point.ToString());

		int x = (int)(hit.point.x - 1);
		int y = (int)(hit.point.z - 1);

		Debug.Log("\tCast as: " + x + ", " + y);
		
		if (x < -4 || x >= 4 ||
			y < -4 || y >= 4) return;
			
		OthelloModel.Instance.MakeMove(x + 4, y + 4);
	}

    private void OnMouseOver()
    {
        //Debug.Log("Mouse Hover At: " + Input.mousePosition.ToString());

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);

        //Debug.Log("\tHit at: " + hit.point.ToString());

        int x = (int)(hit.point.x - 1);
        int y = (int)(hit.point.z - 1);

        //Debug.Log("\tCast as: " + x + ", " + y);

        if (x < -4 || x >= 4 ||
            y < -4 || y >= 4) return;

        foreach (IObserver<Point> ob in Observers)
        {
            ob.OnNext(new Point(x, y));
        }
        return;
    }

    public IDisposable Subscribe(IObserver<Point> observer)
    {
        Observers.Add(observer);

        return null;
    }
}