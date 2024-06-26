using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public static class PreComputedData // Pre-Computed Data to speed move generation up
{
    public static int[,] numSquaresToEdge = new int[64, 8];

    // Pre-Computed Bitboard (ulong)
    public static ulong[] knightMap = new ulong[64];
    public static ulong[] kingMap = new ulong[64];
    public static ulong[] whitePawnAttackMap = new ulong[64];
    public static ulong[] blackPawnAttackMap = new ulong[64];

    // Pre-Computed Squares (Index)
    public static List<int>[] knightSquares = new List<int>[64];
    public static List<int>[] kingSquares = new List<int>[64];

    // Pre-Computed Direction Lookup Table
    // Index : directionLookup[targetSquare - startSquare + 63]
    // Values with index of impossible pin ray such as 126 are invalid (targetSquare = 63, startSquare = 0 -> THIS CANNOT BE A PIN RAY)
    public static int[] directionLookup = new int[127];


    public static void Initialize()
    {
        GenerateNumSquaresToEdge();
        GenerateMaps();
        GenerateDirectionLookup();
    }

    public static void GenerateNumSquaresToEdge()
    {
        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                int numR = 7 - file;
                int numU = 7 - rank;
                int numL = file;
                int numD = rank;

                int squareIndex = 8 * rank + file;

                numSquaresToEdge[squareIndex, 0] = numR;
                numSquaresToEdge[squareIndex, 1] = numU;
                numSquaresToEdge[squareIndex, 2] = numL;
                numSquaresToEdge[squareIndex, 3] = numD;

                numSquaresToEdge[squareIndex, 4] = Math.Min(numR, numU);
                
                numSquaresToEdge[squareIndex, 5] = Math.Min(numL, numU);
                
                numSquaresToEdge[squareIndex, 6] = Math.Min(numL, numD);
                
                numSquaresToEdge[squareIndex, 7] = Math.Min(numD, numR);
            }
        }
    }

    public static void GenerateMaps()
    {
        GenerateKnightMap();
        GenerateKingMap();
        GeneratePawnAttackMap();
    }

    static void GenerateKnightMap()
    {
        for (int square = 0; square < 64; square++)
        {
            knightSquares[square] = new List<int>();

            int file = square % 8;
            int rank = square / 8;

            if (file < 6 && rank < 7)
            {
                knightMap[square] |= (ulong) 1 << square + 10;
                knightSquares[square].Add(square + 10);
            }
            if (file < 7 && rank < 6)
            {
                knightMap[square] |= (ulong) 1 << square + 17;
                knightSquares[square].Add(square + 17);
            }
            if (file > 0 && rank < 6)
            {
                knightMap[square] |= (ulong) 1 << square + 15;
                knightSquares[square].Add(square + 15);
            }
            if (file > 1 && rank < 7)
            {
                knightMap[square] |= (ulong) 1 << square + 6;
                knightSquares[square].Add(square + 6);
            }

            if (file > 1 && rank > 0)
            {
                knightMap[square] |= (ulong) 1 << square - 10;
                knightSquares[square].Add(square - 10);
            }
            if (file > 0 && rank > 1)
            {
                knightMap[square] |= (ulong) 1 << square - 17;
                knightSquares[square].Add(square - 17);
            }
            if (file < 7 && rank > 1)
            {
                knightMap[square] |= (ulong) 1 << square - 15;
                knightSquares[square].Add(square - 15);
            }
            if (file < 6 && rank > 0)
            {
                knightMap[square] |= (ulong) 1 << square - 6;
                knightSquares[square].Add(square - 6);
            }
        }
    }

    static void GenerateKingMap()
    {
        for (int square = 0; square < 64; square++)
        {
            kingSquares[square] = new List<int>();
            
            int file = square % 8;
            int rank = square / 8;

            if (file < 7)
            {
                kingMap[square] |= (ulong) 1 << square + 1;
                kingSquares[square].Add(square + 1);
            }
            if (file > 0)
            {
                kingMap[square] |= (ulong) 1 << square - 1;
                kingSquares[square].Add(square - 1);
            }
            if (rank < 7)
            {
                if (file < 7)
                {
                    kingMap[square] |= (ulong) 1 << square + 9;
                    kingSquares[square].Add(square + 9);
                }
                
                kingMap[square] |= (ulong) 1 << square + 8;
                kingSquares[square].Add(square + 8);
                
                if (file > 0)
                {
                    kingMap[square] |= (ulong) 1 << square + 7;
                    kingSquares[square].Add(square + 7);
                }
            }
            if (rank > 0)
            {
                if (file < 7)
                {
                    kingMap[square] |= (ulong) 1 << square - 7;
                    kingSquares[square].Add(square - 7);
                }

                kingMap[square] |= (ulong) 1 << square - 8;
                kingSquares[square].Add(square - 8);

                if (file > 0)
                {
                    kingMap[square] |= (ulong) 1 << square - 9;
                    kingSquares[square].Add(square - 9);
                }
            }
        }
    }

    static void GeneratePawnAttackMap()
    {
        for (int square = 0; square < 64; square++)
        {
            int file = square % 8;
            int rank = square / 8;

            if (rank < 7)
            {
                if (file < 7)
                {
                    whitePawnAttackMap[square] |= (ulong) 1 << (square + 9);
                }
                if (file > 0)
                {
                    whitePawnAttackMap[square] |= (ulong) 1 << (square + 7);
                }
            }
            if (rank > 0)
            {
                if (file < 7)
                {
                    blackPawnAttackMap[square] |= (ulong) 1 << (square - 7);
                }
                if (file > 0)
                {
                    blackPawnAttackMap[square] |= (ulong) 1 << (square - 9);
                }
            }
        }
    }

    static void GenerateDirectionLookup()
    {
        for (int i = 0; i < 127; i++)
        {
            int offset = i - 63;
            int absOffset = Math.Abs(offset);
            int absDir = 1;

            if (absOffset % 9 == 0)
            {
                absDir = 9;
            }
            else if (absOffset % 8 == 0)
            {
                absDir = 8;
            }
            else if (absOffset % 7 == 0)
            {
                absDir = 7;
            }

            directionLookup[i] = absDir * Math.Sign(offset);
        }
    }


}
