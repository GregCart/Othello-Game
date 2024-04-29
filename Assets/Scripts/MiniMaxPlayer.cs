using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using UnityEngine;
using static OthelloModel;
using Debug = UnityEngine.Debug;


public class MiniMaxPlayer : IObserver<ModelChange>
{
    public int AIlevel;
    
    private OthelloModel model;
    private PlayerColor me;


    public MiniMaxPlayer(int level, PlayerColor me)
    {
        Debug.Log(me + " Init, Level " + level);
        this.AIlevel = level;
        this.model = OthelloModel.Instance;

        this.model.Subscribe(this);
        Debug.Log("Subscribed");
        this.me = me;
    }

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
            BoardUpdate b = (BoardUpdate)value;
            if ((PlayerColor)(((int)b.c + 1) % 2) == me)
            {
                CalculateMiniMaxMove();
            }
        }
    }

    private void CalculateMiniMaxMove()
    {
        (int, Point, int) ret = RecursiveMiniMax(model.OthelloBoard, new Point(0, 0), 1);
        Debug.Log(ret.Item2);
    }

    //need level, point, and value
    private (int, Point, int) RecursiveMiniMax(PlayerColor[,] board, Point p, int level)
    {
        if (IsBaseCase(board) || level > AIlevel + 1)
        {
            return (level, p, EvaluateBoard(board));
        }

        List<(int, Point, int)> boardStates = new List<(int, Point, int)>();

        for (int y = 0; y < model.boardSize; y++)
        {
            for (int x = 0; x < model.boardSize; x++)
            {
                if (model.LegalMove(x, y))
                {
                    PlayerColor[,] state = board;
                    state[x, y] = (PlayerColor)(me + level % 2);
                    boardStates.Add(RecursiveMiniMax(state, new(x, y), level + 1));
                }
            }
        }

        (Point, int)  best = GetBestBoard(boardStates);
        return (level, best.Item1, best.Item2);
    }

    private int EvaluateBoard(PlayerColor[,] board)
    {
        return board.CountValues(me) - board.CountValues(me + 1 % 2);
    }

    private bool IsBaseCase(PlayerColor[,] board)
    {
        return board.CountValues(PlayerColor.White) + board.CountValues(PlayerColor.Black) == board.Length;
    }

    private (Point, int) GetBestBoard(List<(int, Point, int)> states)
    {
        (int, Point, int) bestState = states.FirstOrDefault();

        foreach (var state in states)
        {
            if (state.Item1 < bestState.Item1)
            {
                bestState = state;
            } else if (state.Item3 > bestState.Item3)
            {
                bestState = state;
            }
        }

        return (bestState.Item2, bestState.Item3);
    }
}