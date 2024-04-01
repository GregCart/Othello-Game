using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static OthelloModel;


public class OthelloBoard : MonoBehaviour, IObserver<ModelChange>
{
    public GameObject GamePiece;
    public GameObject TransparentGamePiece;
    public GameObject MoveFollowPiece;
    public GameObject LeftGamePieceStorage;
    public GameObject RightGamePieceStorage;
    public List<GameObject> GamePieces;
    public GameObject[,] GameBoard;
    public System.Random rnd = new System.Random();
    public int NextGamePiece = 0;
    public bool AnimationCurrent = false;
    public GameObject AnimationPiece;
    public Vector3 AnimationStart;
    public Vector3 AnimationEnd;
    public float AnimationTime;
    public Queue<(Vector3 largest, Vector3 source, GameObject piece)> PieceAnimationQueue;


    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public void OnNext(ModelChange value)
    {
        if (value.Type == ChangeType.BoardUpdate)
        {
            BoardUpdate boardUpdate = (BoardUpdate)value;

            ChangeSquare(boardUpdate.loc, boardUpdate.c);
        }
        else if (value.Type == ChangeType.BoardReset)
        {
            //clear board, send message that baord is clear
        }
    }

    public void ChangeSquare(Point loc, PlayerColor c)
    {

    }


    // Start is called before the first frame update
    void Start()
    {
        GameBoard = new GameObject[8, 8];
        GamePieces = new List<GameObject>();

        for (int i = 0; i < 64; i++)
        {
            float x = i % 2 == 1 ? RightGamePieceStorage.gameObject.transform.position.x : LeftGamePieceStorage.gameObject.transform.position.x;
            float z = i % 2 == 1 ? RightGamePieceStorage.gameObject.transform.position.z : LeftGamePieceStorage.gameObject.transform.position.z;

            GameObject piece = Instantiate(GamePiece, new Vector3(x, 64 + i * 2 + (float)(rnd.NextDouble() / 20), z), Quaternion.identity);

            GamePieces.Add(piece);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!AnimationCurrent && PieceAnimationQueue.Count > 0)
        {
            AnimationCurrent = true;
            (AnimationEnd, AnimationStart, AnimationPiece) = PieceAnimationQueue.Dequeue();
            AnimationPiece.GetComponent<Rigidbody>().isKinematic = true;
            AnimationTime = 0.0f;
        }

        if (!AnimationCurrent) return;

        AnimationTime += Time.deltaTime;
        if (AnimationTime > 1)
        {
            AnimationCurrent = false;
            AnimationPiece.GetComponent<Rigidbody>().isKinematic = false;

            return;
        }

        AnimationPiece.transform.localPosition = new Vector3(
            (AnimationEnd.x - AnimationStart.x) * AnimationTime + AnimationStart.x,
            -40 * AnimationTime * AnimationTime + 40 * AnimationTime + AnimationStart.y,
            (AnimationEnd.z - AnimationStart.z) * AnimationTime + AnimationStart.z);
    }
}
