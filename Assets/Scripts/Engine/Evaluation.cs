using System;
using UnityEngine;

public static class Evaluation
{
    public static int checkmateEval = 99999;
    public static int maxDepth = EngineSettings.unlimitedMaxDepth;


    static Board board;
    static int eval;

    static int sign;

    public static readonly int pawnValue = 100;
    public static readonly int knightValue = 300;
    public static readonly int bishopValue = 320;
    public static readonly int rookValue = 500;
    public static readonly int queenValue = 900;

    static readonly int[] pawnSquareTable = {
        0,   0,   0,   0,   0,   0,   0,   0,
        50,  50,  50,  50,  50,  50,  50,  50,
        10,  10,  20,  30,  30,  20,  10,  10,
        5,   5,  10,  25,  25,  10,   5,   5,
        0,   0,   0,  30,  30,   0,   0,   0,
        5,  -5, -10,   0,   0, -10,  -5,   5,
        5,  10,  10, -25, -25,  10,  10,   5,
        0,   0,   0,   0,   0,   0,   0,   0
    };

    static readonly int[] knightSquareTable = {
        -50,-40,-30,-30,-30,-30,-40,-50,
        -40,-20,  0,  0,  0,  0,-20,-40,
        -30,  0, 10, 15, 15, 10,  0,-30,
        -30,  5, 15, 20, 20, 15,  5,-30,
        -30,  0, 15, 20, 20, 15,  0,-30,
        -30,  5, 20, 15, 15, 20,  5,-30,
        -40,-20,  0,  5,  5,  0,-20,-40,
        -50,-40,-30,-30,-30,-30,-40,-50,
    };

    static readonly int[] bishopSquareTable =  {
        -20,-10,-10,-10,-10,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5, 10, 10,  5,  0,-10,
        -10,  5,  5, 10, 10,  5,  5,-10,
        -10,  0, 10, 10, 10, 10,  0,-10,
        -10, 10, 10, 10, 10, 10, 10,-10,
        -10,  5,  0,  0,  0,  0,  5,-10,
        -20,-10,-10,-10,-10,-10,-10,-20,
    };

    static readonly int[] rookSquareTable =  {
        0,  0,  0,  0,  0,  0,  0,  0,
        5, 10, 10, 10, 10, 10, 10,  5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,-10,  0,  5,  5,  5,-10, -5
    };

    static readonly int[] queenSquareTable =  {
        -20,-10,-10, -5, -5,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5,  5,  5,  5,  0,-10,
        -5,   0,  5,  5,  5,  5,  0, -5,
         0,   0,  5,  5,  5,  5,  0, -5,
        -10,  5,  5,  5,  5,  5,  0,-10,
        -10,  0,  5,  0,  0,  0,  0,-10,
        -20,-10,-10, -5, -5,-10,-10,-20
    };

    static readonly int[] kingMidSquareTable = 
    {
        -80, -70, -70, -70, -70, -70, -70, -80, 
        -60, -60, -60, -60, -60, -60, -60, -60, 
        -40, -50, -50, -60, -60, -50, -50, -40, 
        -30, -40, -40, -50, -50, -40, -40, -30, 
        -20, -30, -30, -40, -40, -30, -30, -20, 
        -10, -20, -20, -20, -20, -20, -20, -10, 
        20,  20,  -5,  -5,  -5,  -5,  20,  20, 
        20,  30,  10,  -5,   0,  -5,  30,  20
    };

    public static readonly int[] kingEndSquareTable = 
    {
        -20, -10, -10, -10, -10, -10, -10, -20,
        -5,   0,   5,   5,   5,   5,   0,  -5,
        -10, -5,   20,  30,  30,  20,  -5, -10,
        -15, -10,  35,  45,  45,  35, -10, -15,
        -20, -15,  30,  40,  40,  30, -15, -20,
        -25, -20,  20,  25,  25,  20, -20, -25,
        -30, -25,   0,   0,   0,   0, -25, -30,
        -50, -30, -30, -30, -30, -30, -30, -50
    };
    
    static readonly int[][] pieceSquareTables = {pawnSquareTable, knightSquareTable, bishopSquareTable, rookSquareTable, queenSquareTable, kingMidSquareTable, kingEndSquareTable};

    static readonly int[] materialValue = {pawnValue, knightValue, bishopValue, rookValue, queenValue};

    public static int Evaluate(Board _board)
    {
        board = _board;
        eval = 0;
        sign = board.isWhiteTurn ? 1 : -1;

        CountMaterial();

        PieceSquareTable();

        CastleRight();

        return eval;
    }

    static void CountMaterial()
    {
        for (int i = 0; i < 5; i++)
        {
            eval += materialValue[i] * (board.pieceSquares[i].count - board.pieceSquares[i + 6].count) * sign;
        }
    }

    static void CastleRight()
    {
        // King is in the middle of the board (d, e file)
        // and king lost its right to castle
        // Then it's probably bad.

        if (!board.isWhiteKingsideCastle && !board.isWhiteQueensideCastle && 
        !(((board.pieceSquares[5].squares[0] % 8) < 3) || ((board.pieceSquares[5].squares[0] % 8) > 4)))
        {
            eval -= 10 * sign;
        }
        if (!board.isBlackKingsideCastle && !board.isBlackQueensideCastle && 
        !(((board.pieceSquares[11].squares[0] % 8) < 3) || ((board.pieceSquares[11].squares[0] % 8) > 4)))
        {
            eval += 10 * sign;
        }
    }

    static void PieceSquareTable()
    {
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < board.pieceSquares[i].count; j++)
            {
                eval += pieceSquareTables[i][board.pieceSquares[i].squares[j]] * sign;
            }
            for (int j = 0; j < board.pieceSquares[i + 6].count; j++)
            {
                eval -= pieceSquareTables[i][GetFlippedPieceSquareIndex(board.pieceSquares[i + 6].squares[j])] * sign;
            }
        }
    }

    static int GetFlippedPieceSquareIndex(int square)
    {
        return (square % 8) + (7 - square / 8) * 8;
    }

    public static int GetAbsPieceValue(int piece)
    {
        int pieceType = Piece.GetType(piece);

        if (pieceType == Piece.Queen)
        {
            return queenValue;
        }
        else if (pieceType == Piece.Rook)
        {
            return rookValue;
        }
        else if (pieceType == Piece.Knight)
        {
            return knightValue;
        }
        else if (pieceType == Piece.Bishop)
        {
            return bishopValue;
        }
        else if (pieceType == Piece.Pawn)
        {
            return pawnValue;
        }

        // FailSafe
        return 0;
    }




    public static bool IsMateScore(int score)
    {
        if (Math.Abs(score) >= checkmateEval - maxDepth)
        {
            return true;
        }

        return false;
    }

}