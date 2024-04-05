using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Unity.VisualScripting;
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
    public int BoardSize = 8;
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
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    Destroy(GameBoard[x, y]);
                }
            }

            StartCoroutine(SpawnBoardAfterTimer());
        }
    }

    public void ChangeSquare(Point loc, PlayerColor c)
    {
        GameObject go = this.GameBoard[loc.X, loc.Y];

        if (go != null)
        {
            //go = go.transform.GetChild(0).gameObject;
            if (c == PlayerColor.White)
            {
                go.GetComponent<AnimatedRotation>().SetDirection(Vector3.up);
            } else if (c == PlayerColor.Black)
            {
                go.GetComponent<AnimatedRotation>().SetDirection(Vector3.down);
            } else if (c == PlayerColor.Empty)
            {
                Destroy(go);
            }
        } else
        {
            this.GameBoard[loc.X, loc.Y] = GamePieces[NextGamePiece];
            PieceAnimationQueue.Enqueue((new Vector3((float)(loc.X - (BoardSize / 2) - .5), 2f, (float)(loc.Y - (BoardSize / 2) - .5)), GamePieces[NextGamePiece].transform.position, GamePieces[NextGamePiece]));
            ChangeSquare(loc, c);
            NextGamePiece++;
        }
    }

    private IEnumerator SpawnBoardAfterTimer()
    {
        yield return new WaitForSeconds(2);

        for (int i = 0; i < 64; i++)
        {
            float x = i % 2 == 1 ? RightGamePieceStorage.gameObject.transform.position.x : LeftGamePieceStorage.gameObject.transform.position.x;
            float z = i % 2 == 1 ? RightGamePieceStorage.gameObject.transform.position.z : LeftGamePieceStorage.gameObject.transform.position.z;
            Quaternion rotation = Quaternion.AngleAxis((float)((Math.PI * i) % (2 * Math.PI)), Vector3.right);

            GameObject piece = Instantiate(GamePiece, new Vector3(x, 64 + i * 2 + (float)(rnd.NextDouble() / 20), z), rotation);

            GamePieces.Add(piece);
        }

        yield return new WaitForSeconds(3);

        PrepNextPiece();
        ChangeSquare(new Point(4, 4), PlayerColor.Black);
        PrepNextPiece();
        ChangeSquare(new Point(4, 5), PlayerColor.White);
        PrepNextPiece();
        ChangeSquare(new Point(5, 5), PlayerColor.Black);
        PrepNextPiece();
        ChangeSquare(new Point(5, 4), PlayerColor.White);
    }

    private void PrepNextPiece()
    {
        if (NextGamePiece % 2 == 1)
        {
            Vector3 location = new Vector3(8.5f, 1.5f, 0);

            PieceAnimationQueue.Enqueue((location, GamePieces[NextGamePiece].transform.position, GamePieces[NextGamePiece]));
        } else
        {
            Vector3 location = new Vector3(-8.5f, 1.5f, 0);

            PieceAnimationQueue.Enqueue((location, GamePieces[NextGamePiece].transform.position, GamePieces[NextGamePiece]));
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        GameBoard = new GameObject[BoardSize, BoardSize];
        GamePieces = new List<GameObject>();
        PieceAnimationQueue = new Queue<(Vector3 largest, Vector3 source, GameObject piece)>();

        StartCoroutine(SpawnBoardAfterTimer());
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
