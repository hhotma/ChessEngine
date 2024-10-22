using static System.Math;

namespace Chess.Core;

public class Search
{
    // constants
    const int ttSizeMB = 64;
    const int immediateMateScore = 100000;
    const int positiveInfinity = 9999999;
    const int negativeInfinity = -positiveInfinity;

    // events
    public event Action<Move> onSearchComplete;

    // references
    EngineSettings settings;
    Board board;
    Evaluation evaluation;
    MoveGenerator moveGenerator;
    MoveOrdering moveOrdering;
    TranspositionTable transpositionTable;
    OpeningBook openingBook;

    // variables
    Move bestMoveThisIteration;
    float bestEvalThisIteration;
    Move bestMove;
    float bestEval;
    bool hasSearchedAtLeastOneMove;
    bool searchCancelled;

    public Search(Board board, EngineSettings settings)
    {
        this.board = board;
        this.settings = settings;

        evaluation = new Evaluation();
        moveGenerator = new MoveGenerator();
        moveOrdering = new MoveOrdering();
        transpositionTable = new TranspositionTable(board, ttSizeMB);

        if (settings.UseOpeningBook)
            openingBook = new OpeningBook(settings.OpeningBookFileName);
    }

    public void InitSearch()
    {
        searchCancelled = false;

        bestEvalThisIteration = bestEval = 0;
        bestMoveThisIteration = bestMove = Move.NullMove;

        if (settings.UseOpeningBook)
        {
            bestMove = openingBook.TryGetMove(board);
        }

        if (bestMove.IsNull)
        {
            IterativeDeepeningSearch();

            if (bestMove.IsNull)
            {
                bestMove = GetRandomMove();
            }
        }
        onSearchComplete?.Invoke(bestMove);
    }

    public void EndSearch()
    {
        searchCancelled = true;
    }

    void IterativeDeepeningSearch()
    {
        for (int searchDepth = 1; searchDepth <= 256; searchDepth++)
        {
            hasSearchedAtLeastOneMove = false;
            MainSearch(searchDepth, 0, negativeInfinity, positiveInfinity);


            if (searchCancelled)
            {
                if (hasSearchedAtLeastOneMove)
                {
                    // temporary fix for the partial search result
                    // just made it so its not partial and it does a full search
                    searchCancelled = false;
                    MainSearch(searchDepth, 0, negativeInfinity, positiveInfinity);

                    bestMove = bestMoveThisIteration;
                    bestEval = bestEvalThisIteration;
                }

                break;
            }
            else
            {
                bestMove = bestMoveThisIteration;
                bestEval = bestEvalThisIteration;

                bestEvalThisIteration = int.MinValue;
                bestMoveThisIteration = Move.NullMove;
                
                // mate found within search depth
                if (IsMateScore(bestEval) && NumPlyToMateFromScore(bestEval) <= searchDepth)
                {
                    break;
                }
            }
        }
    }

    float MainSearch(int plyRemaining, int plyFromRoot, float alpha, float beta)
    {
        if (searchCancelled)
        {
            return 0;
        }

        if (plyFromRoot > 0)
        {
            // could make problems
            if (board.currentGameState.fiftyMoveCounter >= 100 || board.RepetitionPositionHistory.Contains(board.currentGameState.zobristKey))
            {
                return 0;
            }

            // check for mate found in searched ply
            alpha = Max(alpha, -immediateMateScore + plyFromRoot);
            beta = Min(beta, immediateMateScore - plyFromRoot);
            if (alpha >= beta)
            {
                return alpha;
            }
        }

        // check position in transposition table
        float ttVal = transpositionTable.LookupEvaluation(plyRemaining, plyFromRoot, alpha, beta);
        if (ttVal != TranspositionTable.LookupFailed)
        {
            if (plyFromRoot == 0)
            {
                bestMoveThisIteration = transpositionTable.GetStoredMove();
                bestEvalThisIteration = transpositionTable.entries[transpositionTable.Index].value;
            }
            return ttVal;
        }

        if (plyRemaining == 0)
        {
            return QuiescenceSearch(alpha, beta);
        }

        System.Span<Move> moves = stackalloc Move[MoveGenerator.MaxMoves];
        moves = moveGenerator.GenerateMoves(board, moves);
        moveOrdering.OrderMoves(board, moves, moveGenerator.opponentPawnAttackMap, moveGenerator.opponentAttackMap);

        if (moves.Length == 0)
        {
            if (moveGenerator.InCheck())
            {
                float mateScore = immediateMateScore - plyFromRoot;
                return -mateScore;
            }
            else
            {
                return 0;
            }
        }

        int evaluationBound = TranspositionTable.UpperBound;
        Move bestMoveInThisPosition = Move.NullMove;

        for (int i = 0; i < moves.Length; i++)
        {
            Move move = moves[i];

            board.MakeMove(moves[i], inSearch: true);
            float eval = -MainSearch(plyRemaining - 1, plyFromRoot + 1, -beta, -alpha);
            board.UnmakeMove(moves[i], inSearch: true);
            
            if (searchCancelled) { return 0; }

            if (eval >= beta)
            {
                transpositionTable.StoreEvaluation(plyRemaining, plyFromRoot, beta, TranspositionTable.LowerBound, moves[i]);
                return beta;
            }

            if (eval > alpha)
            {
                evaluationBound = TranspositionTable.Exact;
                bestMoveInThisPosition = moves[i];

                alpha = eval;
                if (plyFromRoot == 0)
                {
                    bestMoveThisIteration = moves[i];
                    bestEvalThisIteration = eval;
                    hasSearchedAtLeastOneMove = true;
                }
            }
        }
        transpositionTable.StoreEvaluation(plyRemaining, plyFromRoot, alpha, evaluationBound, bestMoveInThisPosition);
        return alpha;
    }

    float QuiescenceSearch(float alpha, float beta)
    {
        if (searchCancelled) { return 0; }

        float eval = evaluation.Evaluate(board);
        if (eval >= beta)
        {
            return beta;
        }
        if (eval > alpha)
        {
            alpha = eval;
        }

        System.Span<Move> moves = stackalloc Move[MoveGenerator.MaxMoves];
        moves = moveGenerator.GenerateMoves(board, moves, false);
        moveOrdering.OrderMoves(board, moves, moveGenerator.opponentPawnAttackMap, moveGenerator.opponentAttackMap);
        for (int i = 0; i < moves.Length; i++)
        {
            board.MakeMove(moves[i], true);
            eval = -QuiescenceSearch(-beta, -alpha);
            board.UnmakeMove(moves[i], true);

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

    public static bool IsMateScore(float score)
    {
        if (score == int.MinValue)
        {
            return false;
        }
        const float maxMateDepth = 1000;
        return Abs(score) > immediateMateScore - maxMateDepth;
    }

    float NumPlyToMateFromScore(float score)
    {
        return immediateMateScore - Abs(score);
    }

    Move GetRandomMove()
    {
        var moves = moveGenerator.GenerateMoves(board);
        if (moves.Length > 0)
        {
            return moves[new System.Random().Next(moves.Length)];
        }
        return Move.NullMove;
    }

    public void ClearForNewGame()
    {
        transpositionTable.Clear();
    }
}