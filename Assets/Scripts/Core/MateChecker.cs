using System;
using System.Collections.Generic;
using UnityEngine;

public static class MateChecker
{
    public enum MateState {None, Checkmate, Stalemate, FiftyDraw, Threefold, Material};

    public static MateState GetPositionState(Board board, List<Move> moves, bool simplifiedThreefold = false)
    {
        if (moves.Count == 0)
        {
            if (MoveGen.InCheck())
            {
                // Checkmate
                return MateState.Checkmate;
            }
            else
            {
                // Stalemate
                return MateState.Stalemate;
            }
        }
        
        if (board.fiftyRuleHalfClock == 100)
        {
            // Fifty-move rule
            return MateState.FiftyDraw;
        }

        if (IsInsufficientMaterial(board))
        {
            // Insufficient Material
            return MateState.Material;
        }

        if (!simplifiedThreefold)
        {
            if (board.positionHistory[board.currentZobristKey] >= 3)
            {
                // Threefold repetition
                return MateState.Threefold;
            }
        }
        else
        {
            if (board.positionHistory[board.currentZobristKey] > 1)
            {
                // Simplified repetition
                return MateState.Threefold;
            }
        }

        return MateState.None;
    }

    // public static int StorePositionHistory(Board board)
    // {
    //     if (board.positionHistory.ContainsKey(board.currentZobristKey))
    //     {
    //         board.positionHistory[board.currentZobristKey] += 1;
    //         return board.positionHistory[board.currentZobristKey];
    //     }
    //     else
    //     {
    //         board.positionHistory.Add(board.currentZobristKey, 1);
    //         return -1;
    //     }
    // }

    public static bool IsInsufficientMaterial(Board board)
    {
        int wq = board.pieceSquares[BitboardIndex.WhiteQueen].count;
        int wr = board.pieceSquares[BitboardIndex.WhiteRook].count;
        PieceList wb = board.pieceSquares[BitboardIndex.WhiteBishop];
        int wn = board.pieceSquares[BitboardIndex.WhiteKnight].count;
        int wp = board.pieceSquares[BitboardIndex.WhitePawn].count;

        int bq = board.pieceSquares[BitboardIndex.BlackQueen].count;
        int br = board.pieceSquares[BitboardIndex.BlackRook].count;
        PieceList bb = board.pieceSquares[BitboardIndex.BlackBishop];
        int bn = board.pieceSquares[BitboardIndex.BlackKnight].count;
        int bp = board.pieceSquares[BitboardIndex.BlackPawn].count;

        if (wp != 0 || wq != 0 || wr != 0 || bp != 0 || bq != 0 || br != 0)
        {
            return false;
        }
        if (wn <= 1 && bn <= 1 && wb.count <= 1 && bb.count <= 1)
        {
            if (!(wn == 1 && bn == 1) && wb.count == 0 && bb.count == 0)
            {
                return true;
            }
            if (!(wb.count == 1 && bb.count == 1) && wn == 0 && bn == 0)
            {
                return true;
            }
            if (wn == 0 && bn == 0 && wb.count == 1 && bb.count == 1)
            {
                if (((wb.squares[0] % 8) + (wb.squares[0] / 8)) % 2 == ((bb.squares[0] % 8) + (bb.squares[0] / 8)) % 2)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static void PrintMateState(MateState state)
    {
        switch (state)
        {
            case MateState.Checkmate:
                Debug.Log("Checkmate!");
                break;
            
            case MateState.Stalemate:
                Debug.Log("StaleMate!");
                break;

            case MateState.FiftyDraw:
                Debug.Log("Draw! (50-move rule)");
                break;

            case MateState.Threefold:
                Debug.Log("Draw! (Threefold)");
                break;

            case MateState.Material:
                Debug.Log("Draw! (Insufficient material)");
                break;
            
            default:
                break;
        }
    }


}