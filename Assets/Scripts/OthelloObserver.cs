using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	public PlayerColor[,] OthelloBoard;
	
	private List<IObserver<ModelChange>> Observers;
	
	
	private OthelloModel() 
	{
		OthelloBoard = new PlayerColor[8,8];
		Observers = new List<IObserver<ModelChange>>();

		currentPlayer = PlayerColor.Black;
		
		ResetBoard();
	}

	private void ResetBoard()
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
		Debug.Log("\tTry " + x + ", " + y);

		if (!LegalMove(x, y)) return false;
		
		int updates = DoMove(x, y);

		Debug.Log(updates + " Tiles Changed");
		
		return true;
	}
	
	public bool MakeMove(Point p) {
		return MakeMove(p.X, p.Y);
	}

	private int DoMove(int x, int y)
	{
        OthelloBoard[x, y] = currentPlayer;
        Notify(new BoardUpdate(x, y, currentPlayer));

		int numUps = 0;
		List<BoardUpdate> ups = new List<BoardUpdate>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                Point check = new Point(x + i, y + j);
				List<BoardUpdate> tmpUps = new List<BoardUpdate>();

                while (check.X > 0 && check.Y > 0 && check.X < 8 && check.Y < 8 && OthelloBoard[check.X, check.Y] != PlayerColor.Empty)
                {
                    if (OthelloBoard[check.X, check.Y] == currentPlayer)
                    {
						ups.AddRange(tmpUps);
                        break;
                    }
                    else
                    {
						tmpUps.Add(new BoardUpdate(check.X, check.Y, currentPlayer));
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

        //flip current player
        currentPlayer = currentPlayer == PlayerColor.Black ? PlayerColor.White : PlayerColor.Black;

        return numUps;
    }
	
	public bool LegalMove(int x, int y)
	{
		if (OthelloBoard[x,y] != PlayerColor.Empty) return false;

		//check if it completes line
		bool[,] dirs = new bool[3, 3];

		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				Point check = new Point(x + i, y + j);
				bool nextTo = true;

				while (check.X > 0 && check.Y > 0 && check.X < 8 && check.Y < 8 && OthelloBoard[check.X, check.Y] != PlayerColor.Empty)
				{
					if (OthelloBoard[check.X, check.Y] == currentPlayer)
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

	public void SetupBoard()
	{
		DoMove(3, 3);
        DoMove(3, 4);
        DoMove(4, 4);
        DoMove(4, 3);
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
		
		
		public BoardUpdate()
		{
			Type = ChangeType.BoardUpdate;
		}
		
		public BoardUpdate(int x, int y, OthelloModel.PlayerColor c): this()
		{
			this.loc = new Point(x, y);
			this.c = c;
		}
	}
}