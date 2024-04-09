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
    private enum AnimationType { Direct, Arc, Rotation }

    public GameObject GamePiece;
    public GameObject TransparentGamePiece;
    public GameObject MoveFollowPiece;
    public GameObject LeftGamePieceStorage;
    public GameObject RightGamePieceStorage;
    public List<GameObject> GamePieces;
    public GameObject[,] GameBoard;
    public GameObject Board;
    public System.Random rnd = new System.Random();
    public int NextGamePiece = 0;
    public bool AnimationCurrent = false, AnimationStarted = false;
    public GameObject AnimationPiece;
    public Vector3 AnimationStart;
    public Vector3 AnimationEnd;
    public float AnimationTime;
    public int BoardSize = 8;

    [SerializeField]
    private Queue<(Vector3 target, AnimationType type, GameObject piece)> PieceAnimationQueue;
    [SerializeField]
    private AnimationType AType;


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
            PieceAnimationQueue.Enqueue((new Vector3((float)(loc.X - (BoardSize / 2) - .5f), 
                                        Board.transform.position.y + 500f, 
                                        (float)(loc.Y - (BoardSize / 2) - .5f))
                                        , AnimationType.Arc
                                        , GamePieces[NextGamePiece]));
            ChangeSquare(loc, c);
            NextGamePiece++;
        }

        AdjustEntireBoard();
    }

    private void AdjustEntireBoard()
    {
        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                AdjustPieceAt(new Point(x, y));
            }
        }
    }

    private void AdjustPieceAt(Point pos)
    {
        GameObject go = GameBoard[pos.X, pos.Y];

        if (go != null)
        {
            Vector3 CorrectPos = new Vector3((float)(pos.X - (BoardSize / 2) - .5),
                                                (float)(go.transform.position.y + 2f),
                                                (float)(pos.Y - (BoardSize / 2) - .5));

            PieceAnimationQueue.Enqueue((CorrectPos, AnimationType.Direct, go));

            AnimatedRotation rot = go.GetComponent<AnimatedRotation>();
            rot.Duration = .5f;

            PieceAnimationQueue.Enqueue((rot.LastDirection, AnimationType.Rotation, go));
        }
    }

    private IEnumerator SpawnBoardAfterTimer()
    {
        yield return new WaitForSeconds(1);

        for (int i = 0; i < 64; i++)
        {
            float x = i % 2 == 1 ? RightGamePieceStorage.gameObject.transform.position.x : LeftGamePieceStorage.gameObject.transform.position.x;
            float z = i % 2 == 1 ? RightGamePieceStorage.gameObject.transform.position.z : LeftGamePieceStorage.gameObject.transform.position.z;
            Quaternion rotation = Quaternion.AngleAxis((float)((Math.PI * i) % (2 * Math.PI)), Vector3.right);

            GameObject piece = Instantiate(GamePiece, new Vector3(x, 10 + i * 2 + (float)(rnd.NextDouble() / 20), z), rotation);

            GamePieces.Add(piece);
        }

        yield return new WaitForSeconds(7);

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
            Vector3 location = new Vector3(6.75f, Board.transform.position.y + .75f, 0);

            PieceAnimationQueue.Enqueue((location, AnimationType.Direct, GamePieces[NextGamePiece]));
        } else
        {
            Vector3 location = new Vector3(-6.75f, Board.transform.position.y + .75f, 0);

            PieceAnimationQueue.Enqueue((location, AnimationType.Direct, GamePieces[NextGamePiece]));
        }
    }

    private IEnumerator AnimateNext()
    {
        AnimationStarted = true;

        yield return new WaitForSeconds(.35f);

        (AnimationEnd, AType, AnimationPiece) = PieceAnimationQueue.Dequeue();
        AnimationPiece.GetComponent<Rigidbody>().isKinematic = true;
        AnimationStart = AnimationPiece.transform.localPosition;
        AnimationTime = -0.0f;

        AnimationCurrent = true;
    }


    // Start is called before the first frame update
    void Start()
    {
        GameBoard = new GameObject[BoardSize, BoardSize];
        GamePieces = new List<GameObject>();
        PieceAnimationQueue = new Queue<(Vector3 target, AnimationType type, GameObject piece)>();

        StartCoroutine(SpawnBoardAfterTimer());
    }

    // Update is called once per frame
    void Update()
    {
        if (!AnimationCurrent && !AnimationStarted && PieceAnimationQueue.Count > 0)
        {
            StartCoroutine(AnimateNext());
        }

        if (AnimationCurrent)
        {
            switch (AType)
            {
                case AnimationType.Direct:
                    AnimationPiece.transform.localPosition = new Vector3(
                        (AnimationEnd.x - AnimationStart.x) * AnimationTime + AnimationStart.x,
                        (AnimationEnd.y - AnimationStart.y) * AnimationTime + AnimationStart.y,
                        (AnimationEnd.z - AnimationStart.z) * AnimationTime + AnimationStart.z);

                    AnimationTime += Time.deltaTime;
                    if ((AnimationPiece.transform.localPosition.LocationNearY(AnimationEnd, .25f) &&
                        AnimationPiece.transform.localPosition.LocationNearX(AnimationEnd, .01f) &&
                        AnimationPiece.transform.localPosition.LocationNearZ(AnimationEnd, .01f)) || AnimationTime > .75f)
                    {
                        AnimationPiece.GetComponent<Rigidbody>().isKinematic = false;
                        AnimationPiece.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        AnimationPiece.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                        AnimationCurrent = false;
                        AnimationStarted = false;

                        return;
                    }
                    break;
                case AnimationType.Arc:
                    AnimationPiece.transform.localPosition = new Vector3(
                        (AnimationEnd.x - AnimationStart.x) * AnimationTime + AnimationStart.x,
                        -30 * AnimationTime * AnimationTime + 30 * AnimationTime + AnimationStart.y + .25f,
                        (AnimationEnd.z - AnimationStart.z) * AnimationTime + AnimationStart.z);

                    AnimationTime += Time.deltaTime;
                    if (AnimationPiece.transform.localPosition.LocationNear(AnimationEnd, .5f) || AnimationTime > .50f)
                    {
                        AnimationPiece.GetComponent<Rigidbody>().isKinematic = false;
                        AnimationPiece.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        AnimationPiece.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

                        if (AnimationPiece.transform.localPosition.LocationNear(AnimationEnd, .1f) || AnimationTime > 1f)
                        {
                            AnimationCurrent = false;
                            AnimationStarted = false;
                        }

                        return;
                    }
                    break;
                case AnimationType.Rotation:
                    if (AnimationStarted)
                    {
                        AnimationPiece.GetComponent<AnimatedRotation>().SetDirection(AnimationEnd);
                        AnimationStarted = false;
                    }

                    AnimationPiece.transform.localPosition = new Vector3(
                        AnimationStart.x,
                        AnimationStart.y,
                        AnimationStart.z);

                    AnimationTime += Time.deltaTime;
                    if (AnimationTime > AnimationPiece.GetComponent<AnimatedRotation>().Duration
                        || AnimationTime > .9f)
                    {
                        AnimationPiece.GetComponent<Rigidbody>().isKinematic = false;
                        AnimationPiece.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        AnimationPiece.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                        AnimationCurrent = false;

                        return;
                    }
                    break;

            }
        } 
        else
        {
            return;
        }
    }
}
