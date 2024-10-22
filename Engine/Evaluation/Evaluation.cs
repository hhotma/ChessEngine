namespace Chess.Core;

public class Evaluation
{
    public const float PawnValue = 100;
    public const float KnightValue = 300;
    public const float BishopValue = 300;
    public const float RookValue = 500;
    public const float QueenValue = 900;

    public EvaluationData whiteEval;
    public EvaluationData blackEval;
    Board board;

    public float Evaluate(Board board)
    {
        this.board = board;

        float eval = AlgoEvaluation();

        eval = (float)System.Math.Round((decimal)eval, 2);

        return eval;
    }

    float AlgoEvaluation()
    {
        MaterialInfo whiteMaterial = GetMaterialInfo(Board.WhiteIndex);
        MaterialInfo blackMaterial = GetMaterialInfo(Board.BlackIndex);

        // evaluate how piece count
        whiteEval.materialScore = whiteMaterial.materialScore;
        blackEval.materialScore = blackMaterial.materialScore;

        // evaluate piece position
        whiteEval.pieceSquareScore = EvaluatePieceSquareTables(true, blackMaterial.endgameT);
        blackEval.pieceSquareScore = EvaluatePieceSquareTables(false, whiteMaterial.endgameT);

        // push king to the edge in endgames
        whiteEval.mopUpScore = MopUpEval(true, whiteMaterial, blackMaterial);
        blackEval.mopUpScore = MopUpEval(false, blackMaterial, whiteMaterial);

        float perspective = board.IsWhiteToMove ? 1 : -1;
        float eval = whiteEval.Sum() - blackEval.Sum();
        return eval * perspective;
    }

    float EvaluatePieceSquareTables(bool isWhite, float endgameT)
    {
        float value = 0;
        int colourIndex = isWhite ? Board.WhiteIndex : Board.BlackIndex;

        //value += EvaluatePieceSquare(PieceSquare.Pawns, board.pawns[colourIndex], isWhite);
        value += EvaluatePieceSquare(PieceSquare.Rooks, board.rooks[colourIndex], isWhite);
        value += EvaluatePieceSquare(PieceSquare.Knights, board.knights[colourIndex], isWhite);
        value += EvaluatePieceSquare(PieceSquare.Bishops, board.bishops[colourIndex], isWhite);
        value += EvaluatePieceSquare(PieceSquare.Queens, board.queens[colourIndex], isWhite);

        float pawnEarly = EvaluatePieceSquare(PieceSquare.Pawns, board.pawns[colourIndex], isWhite);
        float pawnLate = EvaluatePieceSquare(PieceSquare.PawnsEnd, board.pawns[colourIndex], isWhite);
        value += pawnEarly * (1 - endgameT);
        value += pawnLate * endgameT;

        float kingEarlyPhase = PieceSquare.Read(PieceSquare.KingStart, board.KingSquare[colourIndex], isWhite);
        value += kingEarlyPhase * (1 - endgameT);
        float kingLatePhase = PieceSquare.Read(PieceSquare.KingEnd, board.KingSquare[colourIndex], isWhite);
        value += kingLatePhase * endgameT;

        return value;
    }

    float MopUpEval(bool isWhite, MaterialInfo myMaterial, MaterialInfo enemyMaterial)
    {
        if (myMaterial.materialScore > enemyMaterial.materialScore + PawnValue * 2 && enemyMaterial.endgameT > 0)
        {
            float mopUpScore = 0;
            int friendlyIndex = isWhite ? Board.WhiteIndex : Board.BlackIndex;
            int opponentIndex = isWhite ? Board.BlackIndex : Board.WhiteIndex;

            int friendlyKingSquare = board.KingSquare[friendlyIndex];
            int opponentKingSquare = board.KingSquare[opponentIndex];

            mopUpScore += (14 - PrecomputedMoveData.OrthogonalDistance[friendlyKingSquare, opponentKingSquare]) * 4;
            mopUpScore += PrecomputedMoveData.CentreManhattanDistance[opponentKingSquare] * 10;
            return mopUpScore * enemyMaterial.endgameT;
        }
        return 0;
    }

    static float EvaluatePieceSquare(float[] table, PieceList pieceList, bool isWhite)
    {
        float value = 0;
        for (int i = 0; i < pieceList.Count; i++)
        {
            value += PieceSquare.Read(table, pieceList[i], isWhite);
        }
        return value;
    }

    MaterialInfo GetMaterialInfo(int colourIndex)
    {
        int numPawns = board.pawns[colourIndex].Count;
        int numKnights = board.knights[colourIndex].Count;
        int numBishops = board.bishops[colourIndex].Count;
        int numRooks = board.rooks[colourIndex].Count;
        int numQueens = board.queens[colourIndex].Count;

        bool isWhite = colourIndex == Board.WhiteIndex;
        ulong myPawns = board.pieceBitboards[Piece.MakePiece(Piece.Pawn, isWhite)];
        ulong enemyPawns = board.pieceBitboards[Piece.MakePiece(Piece.Pawn, !isWhite)];

        return new MaterialInfo(numPawns, numKnights, numBishops, numQueens, numRooks, myPawns, enemyPawns);
    }

    public struct EvaluationData
    {
        public float materialScore;
        public float pieceSquareScore;
        public float mopUpScore;

        public float Sum()
        {
            return materialScore + pieceSquareScore + mopUpScore;
        }
    }

    public readonly struct MaterialInfo
    {
        public readonly float materialScore;
        public readonly int numPawns;
        public readonly int numMajors;
        public readonly int numMinors;
        public readonly int numBishops;
        public readonly int numQueens;
        public readonly int numRooks;

        public readonly ulong pawns;
        public readonly ulong enemyPawns;

        public readonly float endgameT;

        public MaterialInfo(int numPawns, int numKnights, int numBishops, int numQueens, int numRooks, ulong pawns, ulong enemyPawns)
        {
            this.numPawns = numPawns;
            this.numBishops = numBishops;
            this.numQueens = numQueens;
            this.numRooks = numRooks;
            this.pawns = pawns;
            this.enemyPawns = enemyPawns;

            numMajors = numRooks + numQueens;
            numMinors = numBishops + numKnights;

            materialScore = 0;
            materialScore += numPawns * PawnValue;
            materialScore += numKnights * KnightValue;
            materialScore += numBishops * BishopValue;
            materialScore += numRooks * RookValue;
            materialScore += numQueens * QueenValue;

            // Endgame Transition (0->1)
            const int queenEndgameWeight = 45;
            const int rookEndgameWeight = 20;
            const int bishopEndgameWeight = 10;
            const int knightEndgameWeight = 10;

            const float endgameStartWeight = 2 * rookEndgameWeight + 2 * bishopEndgameWeight + 2 * knightEndgameWeight + queenEndgameWeight;
            float endgameWeightSum = numQueens * queenEndgameWeight + numRooks * rookEndgameWeight + numBishops * bishopEndgameWeight + numKnights * knightEndgameWeight;
            endgameT = 1 - System.Math.Min(1, endgameWeightSum / endgameStartWeight);
        }
    }
}