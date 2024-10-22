namespace Chess.Core;

public static class PieceSquare 
{
    public static float Read(float[] table, int square, bool isWhite)
    {
        if (isWhite)
        {
            int file = BoardHelper.FileIndex(square);
            int rank = BoardHelper.RankIndex(square);
            rank = 7 - rank;
            square = BoardHelper.IndexFromCoord(file, rank);
        }

        return table[square];
    }

    public static float Read(int piece, int square)
    {
        return Tables[piece][square];
    }

    public static readonly float[] Pawns = {
            0,   0,   0,   0,   0,   0,   0,   0,
        50,  50,  50,  50,  50,  50,  50,  50,
        10,  10,  20,  30,  30,  20,  10,  10,
            5,   5,  10,  25,  25,  10,   5,   5,
            0,   0,   0,  20,  20,   0,   0,   0,
            5,  -5, -10,   0,   0, -10,  -5,   5,
            5,  10,  10, -20, -20,  10,  10,   5,
            0,   0,   0,   0,   0,   0,   0,   0
    };

    public static readonly float[] PawnsEnd = {
            0,   0,   0,   0,   0,   0,   0,   0,
        80,  80,  80,  80,  80,  80,  80,  80,
        50,  50,  50,  50,  50,  50,  50,  50,
        30,  30,  30,  30,  30,  30,  30,  30,
        20,  20,  20,  20,  20,  20,  20,  20,
        10,  10,  10,  10,  10,  10,  10,  10,
        10,  10,  10,  10,  10,  10,  10,  10,
            0,   0,   0,   0,   0,   0,   0,   0
    };

    public static readonly float[] Rooks =  {
        0,  0,  0,  0,  0,  0,  0,  0,
        5, 10, 10, 10, 10, 10, 10,  5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        0,  0,  0,  5,  5,  0,  0,  0
    };

    public static readonly float[] Knights = {
        -50,-40,-30,-30,-30,-30,-40,-50,
        -40,-20,  0,  0,  0,  0,-20,-40,
        -30,  0, 10, 15, 15, 10,  0,-30,
        -30,  5, 15, 20, 20, 15,  5,-30,
        -30,  0, 15, 20, 20, 15,  0,-30,
        -30,  5, 10, 15, 15, 10,  5,-30,
        -40,-20,  0,  5,  5,  0,-20,-40,
        -50,-40,-30,-30,-30,-30,-40,-50,
    };

    public static readonly float[] Bishops =  {
        -20,-10,-10,-10,-10,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5, 10, 10,  5,  0,-10,
        -10,  5,  5, 10, 10,  5,  5,-10,
        -10,  0, 10, 10, 10, 10,  0,-10,
        -10, 10, 10, 10, 10, 10, 10,-10,
        -10,  5,  0,  0,  0,  0,  5,-10,
        -20,-10,-10,-10,-10,-10,-10,-20,
    };
    
    public static readonly float[] Queens =  {
        -20,-10,-10, -5, -5,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5,  5,  5,  5,  0,-10,
        -5,  0,  5,  5,  5,  5,  0, -5,
        0,  0,  5,  5,  5,  5,  0, -5,
        -10,  5,  5,  5,  5,  5,  0,-10,
        -10,  0,  5,  0,  0,  0,  0,-10,
        -20,-10,-10, -5, -5,-10,-10,-20
    };
    
    public static readonly float[] KingStart ={ -80, -70, -70, -70, -70, -70, -70, -80, -60, -60, -60, -60, -60, -60, -60, -60, -40, -50, -50, -60, -60, -50, -50, -40, -30, -40, -40, -50, -50, -40, -40, -30, -20, -30, -30, -40, -40, -30, -30, -20, -10, -20, -20, -20, -20, -20, -20, -10, 20, 20, -5, -5, -5, -5, 20, 20, 20, 30, 10, 0, 0, 10, 30, 20 };

    public static readonly float[] KingEnd = { -20, -10, -10, -10, -10, -10, -10, -20, -5, 0, 5, 5, 5, 5, 0, -5, -10, -5, 20, 30, 30, 20, -5, -10, -15, -10, 35, 45, 45, 35, -10, -15, -20, -15, 30, 40, 40, 30, -15, -20, -25, -20, 20, 25, 25, 20, -20, -25, -30, -25, 0, 0, 0, 0, -25, -30, -50, -30, -30, -30, -30, -30, -30, -50 };

    public static readonly float[][] Tables;

    static PieceSquare()
    {
        Tables = new float[Piece.MaxPieceIndex + 1][];
        Tables[Piece.MakePiece(Piece.Pawn, Piece.White)] = Pawns;
        Tables[Piece.MakePiece(Piece.Rook, Piece.White)] = Rooks;
        Tables[Piece.MakePiece(Piece.Knight, Piece.White)] = Knights;
        Tables[Piece.MakePiece(Piece.Bishop, Piece.White)] = Bishops;
        Tables[Piece.MakePiece(Piece.Queen, Piece.White)] = Queens;

        Tables[Piece.MakePiece(Piece.Pawn, Piece.Black)] = GetFlippedTable(Pawns);
        Tables[Piece.MakePiece(Piece.Rook, Piece.Black)] = GetFlippedTable(Rooks);
        Tables[Piece.MakePiece(Piece.Knight, Piece.Black)] = GetFlippedTable(Knights);
        Tables[Piece.MakePiece(Piece.Bishop, Piece.Black)] = GetFlippedTable(Bishops);
        Tables[Piece.MakePiece(Piece.Queen, Piece.Black)] = GetFlippedTable(Queens);
    }

    static float[] GetFlippedTable(float[] table)
    {
        float[] flippedTable = new float[table.Length];

        for (int i = 0; i < table.Length; i++)
        {
            Coord coord = new Coord(i);
            Coord flippedCoord = new Coord(coord.fileIndex, 7 - coord.rankIndex);
            flippedTable[flippedCoord.SquareIndex] = table[i];
        }
        return flippedTable;
    }
}