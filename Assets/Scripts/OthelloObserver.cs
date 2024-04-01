using System;
using System.Collections.Generic;
using System.Drawing;

using static OthelloModel;


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
		if (!LegalMove(x, y)) return false;
		
		OthelloBoard[x,y] = currentPlayer;
		Notify(new BoardUpdate(x, y, currentPlayer));
		
		//flip current player
		currentPlayer = currentPlayer == PlayerColor.Black ? PlayerColor.White : PlayerColor.Black;
		
		return true;
	}
	
	public bool MakeMove(Point p) {
		return MakeMove(p.X, p.Y);
	}
	
	public bool LegalMove(int x, int y)
	{
		if (OthelloBoard[x,y] != PlayerColor.Empty) return false;

		//check if it completes line

		return true;
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