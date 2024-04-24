using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static OthelloModel;
using Color = UnityEngine.Color;


public class OthelloBoard : MonoBehaviour, IObserver<ModelChange>, IObserver<Point>
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
    bool isClose = false;
    bool needsAdjust = false;


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

            PrepNextPiece();
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
        } else if (value.Type == ChangeType.PlayerWin)
        {
            
        }
    }

    public void OnNext(Point value)
    {
        //Debug.Log(this.MoveFollowPiece.transform.localPosition + " --> " + value);

        MoveFollowPiece.transform.localPosition = new Vector3(
            value.X,
            2f,
            value.Y
            );
    }


    public void ChangeSquare(Point loc, PlayerColor c)
    {
        GameObject go = this.GameBoard[loc.X, loc.Y];

        if (go != null)
        {
            if (c == PlayerColor.White)
            {
                //go.GetComponent<AnimatedRotation>().SetDirection(Vector3.up);
                PieceAnimationQueue.Enqueue((Vector3.up, AnimationType.Rotation, go));
            } else if (c == PlayerColor.Black)
            {
                //go.GetComponent<AnimatedRotation>().SetDirection(Vector3.down);
                PieceAnimationQueue.Enqueue((Vector3.down, AnimationType.Rotation, go));
            } else if (c == PlayerColor.Empty)
            {
                Destroy(go);
            }
        } else
        {
            this.GameBoard[loc.X, loc.Y] = GamePieces[NextGamePiece];
            PieceAnimationQueue.Enqueue((new Vector3((float)(loc.X - (BoardSize / 2) + .5f), 
                                        Board.transform.position.y + 2f, 
                                        (float)(loc.Y - (BoardSize / 2) + .5f))
                                        , AnimationType.Arc
                                        , GamePieces[NextGamePiece]));
            ChangeSquare(loc, c);
            NextGamePiece++;

            if (NextGamePiece > 3 && NextGamePiece % 4 == 3)
            {
                needsAdjust = true;
            }
        }
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
            Vector3 CorrectPos = new Vector3((float)(pos.X - (BoardSize / 2) + .5),
                                                (float)(go.transform.position.y + 2f),
                                                (float)(pos.Y - (BoardSize / 2) + .5));

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

        OthelloModel.Instance.Subscribe(this);

        yield return new WaitForSeconds(6);

        PrepNextPiece();

        OthelloModel.Instance.SetupBoard();

        this.gameObject.GetComponent<BoardInput>().Subscribe(this);
    }

    private void PrepNextPiece()
    {
        if (NextGamePiece % 2 == 1)
        {
            Vector3 location = new Vector3(6.75f, Board.transform.position.y + 1f, 0);

            PieceAnimationQueue.Enqueue((location, AnimationType.Direct, GamePieces[NextGamePiece]));
        } else
        {
            Vector3 location = new Vector3(-6.75f, Board.transform.position.y + 1f, 0);

            PieceAnimationQueue.Enqueue((location, AnimationType.Direct, GamePieces[NextGamePiece]));
        }
    }

    private IEnumerator AnimateNext()
    {
        AnimationStarted = true;

        yield return new WaitForSeconds(.05f);

        AnimationStart = AnimationEnd;
        (AnimationEnd, AType, AnimationPiece) = PieceAnimationQueue.Dequeue();
        AnimationPiece.GetComponent<Rigidbody>().isKinematic = true;
        AnimationStart = AnimationPiece.transform.localPosition;
        AnimationTime = -0.0f;

        AnimationCurrent = true;
    }

    private IEnumerator SetupAdjustment()
    {
        while (PieceAnimationQueue.Count > 0)
        {
            yield return new WaitForSeconds(1);
        }

        AdjustEntireBoard();
    }




    // Start is called before the first frame update
    void Start()
    {
        GameBoard = new GameObject[BoardSize, BoardSize];
        GamePieces = new List<GameObject>();
        PieceAnimationQueue = new Queue<(Vector3 target, AnimationType type, GameObject piece)>();
        isClose = false;

        StartCoroutine(SpawnBoardAfterTimer());

        this.TransparentGamePiece = Instantiate(MoveFollowPiece);
        Color c = this.TransparentGamePiece.GetComponentInChildren<Renderer>().material.color;
        //this.TransparentGamePiece.GetComponent<Renderer>().material.color = new Color(c.r, c.g, c.b, .5f);
        this.TransparentGamePiece.GetComponent<Rigidbody>().isKinematic = true;
        this.TransparentGamePiece.GetComponent <Collider>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!AnimationCurrent && !AnimationStarted && PieceAnimationQueue.Count > 0)
        {
            StartCoroutine(AnimateNext());

            if (needsAdjust)
            {
                StartCoroutine(SetupAdjustment());
                needsAdjust = false;
            }
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
                        AnimationPiece.transform.localPosition.LocationNearZ(AnimationEnd, .01f)) 
                        || AnimationTime > 5f)
                        //)
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
                        -5 * AnimationTime * AnimationTime + 5 * AnimationTime + AnimationStart.y + AnimationEnd.y,
                        (AnimationEnd.z - AnimationStart.z) * AnimationTime + AnimationStart.z);

                    //Debug.Log(AnimationTime + " @ " + AnimationPiece.transform.localPosition + " --> " + AnimationEnd);

                    AnimationTime += Time.deltaTime;
                    if (isClose || AnimationPiece.transform.localPosition.LocationBallPark(AnimationEnd, 1f) 
                        //|| AnimationTime > 1f)
                        )
                    {
                        AnimationPiece.GetComponent<Rigidbody>().isKinematic = false;
                        AnimationPiece.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        AnimationPiece.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                        isClose = true;

                        if (AnimationPiece.transform.localPosition.LocationNear(AnimationEnd, .25f) 
                            || isClose && AnimationTime > 1.2f)
                        //)
                        {
                            AnimationPiece.transform.localPosition = AnimationEnd;
                            AnimationCurrent = false;
                            AnimationStarted = false;
                        }

                        return;
                    }
                    break;
                case AnimationType.Rotation:
                    if (AnimationStarted)
                    {
                        AnimationStarted = false;

                        if (!AnimationPiece.GetComponent<AnimatedRotation>().SetDirection(AnimationEnd))
                        {
                            AnimationTime = 999f;
                        }
                    }

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

                    AnimationPiece.transform.localPosition = new Vector3(
                        AnimationStart.x,
                        AnimationStart.y,
                        AnimationStart.z);

                    break;

            }
        } 
        else
        {
            return;
        }
    }
}
