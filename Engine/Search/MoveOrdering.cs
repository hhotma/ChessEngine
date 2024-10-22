namespace Chess.Core;

public class MoveOrdering
{
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    public System.Span<Move> OrderMoves(Board board, System.Span<Move> moves, ulong oppPawnAttacks, ulong oppAttacks)
    {
        int[] moveScores = new int[moves.Length];

        for (int x = 0; x < moves.Length; x++)
        {
            int moveScoreGuess = 0;
            int movePieceType = Piece.PieceType(board.Square[moves[x].StartSquare]);
            int capturePieceType = Piece.PieceType(board.Square[moves[x].TargetSquare]);

            if (capturePieceType != 0)
            {
                moveScoreGuess = 10 * pieceValues[capturePieceType] - pieceValues[movePieceType];
            }

            if (moves[x].IsPromotion)
            {
                moveScoreGuess += pieceValues[(int)moves[x].PromotionPieceType];
            }

            bool isAttacked = BitBoardUtility.ContainsSquare(oppPawnAttacks | oppAttacks, moves[x].TargetSquare);
            if (isAttacked)
            {
                moveScoreGuess -= pieceValues[movePieceType];
            }

            moveScores[x] = moveScoreGuess;
        }

        Sort(moves, moveScores);
        return moves;
    }

    // Sort the moves list based on scores
    void Sort(System.Span<Move> moves, int[] moveScores)
    {
        for (int i = 0; i < moves.Length - 1; i++)
        {
            for (int j = i + 1; j > 0; j--)
            {
                int swapIndex = j - 1;
                if (moveScores[swapIndex] < moveScores[j])
                {
                    (moves[j], moves[swapIndex]) = (moves[swapIndex], moves[j]);
                    (moveScores[j], moveScores[swapIndex]) = (moveScores[swapIndex], moveScores[j]);
                }
            }
        }
    }
}