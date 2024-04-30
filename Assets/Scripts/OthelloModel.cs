using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static OthelloModel;
using Debug = UnityEngine.Debug;


public class OthelloModel: IObservable<ModelChange>
{
	private static OthelloModel instance;

	public enum PlayerColor {White, Black, Empty}
	
	public static OthelloModel Instance 
	{
		get 
		{
			if (instance == null) {
                instance = new OthelloModel();
			}
			
			return instance;
		}
	}
	
	public PlayerColor currentPlayer {private set; get;}
	public int boardSize { private set; get;}

	public PlayerColor[,] OthelloBoard;
    public MiniMaxPlayer[] players;

    private List<IObserver<ModelChange>> Observers;
	private int moves;
	[SerializeField] private int AI1Level;
	[SerializeField] private int AI2Level;
	
	
	private OthelloModel() 
	{
		boardSize = 8;

		OthelloBoard = new PlayerColor[boardSize, boardSize];
		Observers = new List<IObserver<ModelChange>>();
		moves = 0;

		currentPlayer = PlayerColor.Black;
	}

	public void ResetBoard()
	{
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
			{
				OthelloBoard[x, y] = PlayerColor.Empty;
				Notify(new BoardUpdate(x, y, PlayerColor.Empty));
			}
		}
	}
	
	public IDisposable Subscribe(IObserver<ModelChange> observer) 
	{
		Observers.Add(observer);
		
		return null;
	}
	
	private void Notify(ModelChange change)
	{
		foreach (IObserver<ModelChange> observer in Observers)
		{
			observer.OnNext(change);
		}
	}

    public bool MakeMove(int x, int y) 
	{
		//Debug.Log("\tTry " + x + ", " + y);

		if (!LegalMove(x, y)) return false;
		
		DoMove(x, y);

		int updates = UpdateBoard(x, y);

		Debug.Log(updates + " Tiles Changed");

        //flip current player
        currentPlayer = currentPlayer == PlayerColor.Black ? PlayerColor.White : PlayerColor.Black;

        if (moves >= 64)
		{
            PlayerColor w = DetermineWinner();
			GameUpdate gu = new GameUpdate();
			gu.c = w;

			Notify(gu);
		}
		
		return true;
	}
	
	public bool MakeMove(Point p) {
		return MakeMove(p.X, p.Y);
	}

	private int DoMove(int x, int y)
	{
        OthelloBoard[x, y] = currentPlayer;
        Notify(new BoardUpdate(x, y, currentPlayer));
		return moves++;
    }
	
	public bool LegalMoveForOn(int x, int y, PlayerColor color, PlayerColor[,] board)
	{
		if (board[x,y] != PlayerColor.Empty) return false;

		//check if it completes line
		bool[,] dirs = new bool[3, 3];

		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				Point check = new Point(x + i, y + j);
				bool nextTo = true;

				while (check.X >= 0 && check.Y >= 0 && check.X < 8 && check.Y < 8 && board[check.X, check.Y] != PlayerColor.Empty)
				{
					if (board[check.X, check.Y] == color)
					{
						dirs[i + 1, j + 1] = !nextTo;
						break;
					}
					else
                    {
                        nextTo = false;
                        check = check.Add(new Point(i, j));
                    }
				}
            }
		}

		return dirs.HasValue<bool>(true, (x, y) => { return x == y; }) ;
	}

	public bool LegalMoveFor(int x, int y,  PlayerColor color)
	{
		return LegalMoveForOn(x, y, color, this.OthelloBoard);
	}

	public bool LegalMove(int x, int y)
	{
		return LegalMoveFor(x, y, currentPlayer);
	}

	private int UpdateBoard(int x, int y)
	{
        int numUps = 0;
        List<BoardUpdate> ups = new List<BoardUpdate>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                Point check = new Point(x + i, y + j);
                List<BoardUpdate> tmpUps = new List<BoardUpdate>();

                while (check.X >= 0 && check.Y >= 0 && check.X < 8 && check.Y < 8 && OthelloBoard[check.X, check.Y] != PlayerColor.Empty)
                {
                    if (OthelloBoard[check.X, check.Y] == currentPlayer)
                    {
                        ups.AddRange(tmpUps);
                        break;
                    }
                    else
                    {
                        tmpUps.Add(new BoardUpdate(check.X, check.Y, currentPlayer, true));
                        check = check.Add(new Point(i, j));
                    }
                }
            }
        }

        foreach (BoardUpdate up in ups)
        {
            OthelloBoard[up.loc.X, up.loc.Y] = currentPlayer;
            Notify(up);
            numUps++;
        }

		return numUps;
    }

	public void SetupBoard()
	{
		DoMove(3, 3);
        currentPlayer = currentPlayer == PlayerColor.Black ? PlayerColor.White : PlayerColor.Black;
        DoMove(3, 4);
        currentPlayer = currentPlayer == PlayerColor.Black ? PlayerColor.White : PlayerColor.Black;
        DoMove(4, 4);
        currentPlayer = currentPlayer == PlayerColor.Black ? PlayerColor.White : PlayerColor.Black;
        DoMove(4, 3);
        currentPlayer = currentPlayer == PlayerColor.Black ? PlayerColor.White : PlayerColor.Black;

        players = new MiniMaxPlayer[2];
        if (AI1Level > 0)
        {
            players[0] = new MiniMaxPlayer(AI1Level, PlayerColor.Black);
        }
        if (AI2Level > 0)
        {
            players[1] = new MiniMaxPlayer(AI2Level, PlayerColor.White);
        }
    }

	public void SetAiLevels(int AI1Level, int AI2Level)
	{
		this.AI1Level = AI1Level;
		this.AI2Level = AI2Level;
	}

	public PlayerColor DetermineWinner()
	{
		int total = 0;

		foreach (PlayerColor c in OthelloBoard)
		{
			if (c == PlayerColor.White)
			{
				total++;
			} else
			{
				total--;
			}
		}

		if (total > 0)
		{
			return PlayerColor.White;
		} else if (total < 0)
		{
			return PlayerColor.Black;
		} else
		{
			return PlayerColor.Empty;
		}
	}
	
	//other helpful fields
	public enum ChangeType {BoardUpdate, BoardReset, PlayerWin}

	public class ModelChange
	{
		public ChangeType Type { get; set; }
	}

	public class BoardUpdate: ModelChange
	{
		public Point loc { get; private set;  }
		public PlayerColor c { get; private set; }
		public bool isFlip { get; private set; }


        public BoardUpdate()
		{
			Type = ChangeType.BoardUpdate;
		}
		
		public BoardUpdate(int x, int y, OthelloModel.PlayerColor c): this()
		{
			this.loc = new Point(x, y);
			this.c = c;
		}

        public BoardUpdate(int x, int y, OthelloModel.PlayerColor c, bool flip) : this()
        {
            this.loc = new Point(x, y);
            this.c = c;
			this.isFlip = flip;
        }
    }

	public class GameUpdate : ModelChange
	{
		public PlayerColor c;


		public GameUpdate()
		{
			this.Type = ChangeType.PlayerWin;
		}
	}
}