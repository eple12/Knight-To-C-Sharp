using UnityEngine;
using System;
using System.Collections.Generic;

public static class Debugger
{
    static Dictionary<int, string> PieceCharLookup = new Dictionary<int, string>()
    {
        {Piece.Black | Piece.Pawn, "[♙]"}, {Piece.Black | Piece.Knight, "[♘]"}, {Piece.Black | Piece.Bishop, "[♗]"}, 
        {Piece.Black | Piece.Rook, "[♖]"}, {Piece.Black | Piece.Queen, "[♕]"}, {Piece.Black | Piece.King, "[♔]"}, 

        {Piece.White | Piece.Pawn, "[♟︎]"}, {Piece.White | Piece.Knight, "[♞]"}, {Piece.White | Piece.Bishop, "[♝]"}, 
        {Piece.White | Piece.Rook, "[♜]"}, {Piece.White | Piece.Queen, "[♛]"}, {Piece.White | Piece.King, "[♚]"}, 

        {Piece.None, "[    ]"}
    };


    public static void PrintPosition(Board board)
    {
        int[] position = board.position;
        string str = "";

        for (int rank = 7; rank >= 0; rank--)
        {
            for (int file = 0; file < 8; file++)
            {
                str += PieceCharLookup[position[8 * rank + file]];
                // str += position[8 * rank + file];
            }

            str += "\n";
        }

        Debug.Log(str);
    }

    public static void PrintList<T>(List<T> list)
    {
        foreach (var item in list)
        {
            Debug.Log(item);
        }
    }
    
    public static void PrintBitboard(ulong bitboard)
    {
        ulong mask = (ulong)1 << 63;
        string bitsString = "";
        for (int i = 0; i < 8; i++)
        {
            string s = "";
            for(int j = 0; j < 8; j++)
            {
                s += (bitboard & mask) != 0 ? "1  " : "0  ";
                mask >>= 1;
            }
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            bitsString += new string(charArray);
            bitsString += '\n';
        }
        Debug.Log(bitsString);
    }

}