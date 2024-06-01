using System;
using System.Collections.Generic;
using UnityEngine;

public class Engine
{
    Board board;
    public TranspositionTable tt;

    readonly int ttSize = 64000;

    public Move bestMove;

    public Engine()
    {
        board = Main.mainBoard;
        tt = new TranspositionTable(board, ttSize);
    }

    public Move StartSearch(int depth)
    {
        bestMove = Move.NullMove;

        // Return Null Move
        if (depth <= 0)
        {
            return bestMove;
        }
        
        Search(depth, Infinity.negativeInfinity, Infinity.positiveInfinity, 0);

        return bestMove;
    }

    public int Search(int depth, int alpha, int beta, int plyFromRoot)
    {
        // Try looking up the current position in the transposition table.
        // If the same position has already been searched to at least an equal depth
        // to the search we're doing now,we can just use the recorded evaluation.
        if (plyFromRoot != 0)
        {
            int ttVal = tt.LookupEvaluation (depth, plyFromRoot, alpha, beta);
            if (ttVal != TranspositionTable.lookupFailed)
            {
                // The Transposition Table cannot store the repetition data, 
                // so whenever a position is repeated, the engine ends up in a threefold draw.
                // To prevent that, check if it's threefold once again!
                // For simplicity, just check if previous position is already reached
                if (board.positionHistory[board.currentZobristKey] > 1)
                {
                    return 0;
                }

                Move move = tt.GetStoredMove ();

                if (plyFromRoot == 0)
                {
                    bestMove = move;
                }

                return ttVal;
            }
        }

        if (depth == 0)
        {
            return QuiescenceSearch(alpha, beta);
        }

        List<Move> legalMoves = MoveGen.GenerateMoves(board);

        MateChecker.MateState mateState = MateChecker.GetPositionState(board, legalMoves, true);
        if (mateState != MateChecker.MateState.None)
        {
            if (mateState == MateChecker.MateState.Checkmate)
            {
                return -Evaluation.checkmateEval + plyFromRoot;
            }

            return 0;
        }

        MoveOrder.GetOrderedList(legalMoves);

        if (plyFromRoot == 0)
        {
            bestMove = legalMoves[0];
        }

        int evalType = TranspositionTable.UpperBound;

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);

            int eval = -Search(depth - 1, -beta, -alpha, plyFromRoot + 1);

            board.UnmakeMove(move);

            if (eval >= beta)
            {
                tt.StoreEvaluation (depth, plyFromRoot, beta, TranspositionTable.LowerBound, move);
                return beta;
            }

            if (eval > alpha)
            {
                alpha = eval;
                evalType = TranspositionTable.Exact;
                
                if (plyFromRoot == 0)
                {
                    bestMove = move;
                }
            }

            tt.StoreEvaluation (depth, plyFromRoot, alpha, evalType, bestMove);
        }

        return alpha;
    }

    public int QuiescenceSearch(int alpha, int beta)
    {
        int ttVal = tt.LookupEvaluation (0, 0, alpha, beta);
        if (ttVal != TranspositionTable.lookupFailed)
        {
            return ttVal;
        }

        int standPat = Evaluation.Evaluate(board);

        if (standPat >= beta)
        {
            return beta;
        }
        if (alpha < standPat)
        {
            alpha = standPat;
        }

        List<Move> moves = MoveGen.GenerateMoves(board, true);
        MoveOrder.GetOrderedList(moves);

        foreach (Move move in moves)
        {
            board.MakeMove(move);

            int eval = -QuiescenceSearch(-beta, -alpha);

            board.UnmakeMove(move);

            if (eval >= beta)
            {
                return beta;
            }
            if (eval > alpha)
            {
                alpha = eval;
            }
        }

        return alpha;
    }

    public Move GetMove()
    {
        return bestMove;
    }

}