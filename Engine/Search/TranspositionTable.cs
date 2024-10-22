namespace Chess.Core;

public class TranspositionTable
{

    public const int LookupFailed = -1;

    // exact eval
    public const int Exact = 0;
    
    // beta cutoff
    public const int LowerBound = 1;

    // as close as possible
    public const int UpperBound = 2;

    public Entry[] entries;

    public readonly ulong count;
    public bool enabled = true;
    Board board;

    public TranspositionTable(Board board, int sizeMB)
    {
        this.board = board;

        int ttEntrySizeBytes = System.Runtime.InteropServices.Marshal.SizeOf<TranspositionTable.Entry>();
        int desiredTableSizeInBytes = sizeMB * 1024 * 1024;
        int numEntries = desiredTableSizeInBytes / ttEntrySizeBytes;

        count = (ulong)numEntries;
        entries = new Entry[numEntries];
    }

    public void Clear()
    {
        for (int i = 0; i < entries.Length; i++)
        {
            entries[i] = new Entry();
        }
    }

    public ulong Index
    {
        get
        {
            return board.currentGameState.zobristKey % count;
        }
    }

    public Move GetStoredMove()
    {
        return entries[Index].move;
    }

    public float LookupEvaluation(int depth, int plyFromRoot, float alpha, float beta)
    {
        if (!enabled)
        {
            return LookupFailed;
        }
        Entry entry = entries[Index];

        if (entry.key == board.currentGameState.zobristKey)
        {
            if (entry.depth >= depth)
            {
                float correctedScore = CorrectRetrievedMateScore(entry.value, plyFromRoot);

                if (entry.nodeType == Exact)
                {
                    return correctedScore;
                }
                if (entry.nodeType == UpperBound && correctedScore <= alpha)
                {
                    return correctedScore;
                }
                if (entry.nodeType == LowerBound && correctedScore >= beta)
                {
                    return correctedScore;
                }
            }
        }
        return LookupFailed;
    }

    public void StoreEvaluation(int depth, int numPlySearched, float eval, int evalType, Move move)
    {
        if (!enabled)
        {
            return;
        }
        
        Entry entry = new Entry(board.currentGameState.zobristKey, CorrectMateScoreForStorage(eval, numPlySearched), (byte)depth, (byte)evalType, move);
        entries[Index] = entry;
    }

    float CorrectMateScoreForStorage(float score, int numPlySearched)
    {
        if (Search.IsMateScore(score))
        {
            float sign = System.Math.Sign(score);
            return (score * sign + numPlySearched) * sign;
        }
        return score;
    }

    float CorrectRetrievedMateScore(float score, int numPlySearched)
    {
        if (Search.IsMateScore(score))
        {
            float sign = System.Math.Sign(score);
            return (score * sign - numPlySearched) * sign;
        }
        return score;
    }

    public Entry GetEntry(ulong zobristKey)
    {
        return entries[zobristKey % (ulong)entries.Length];
    }

    public struct Entry
    {

        public readonly ulong key;
        public readonly float value;
        public readonly Move move;
        public readonly byte depth;
        public readonly byte nodeType;

        public Entry(ulong key, float value, byte depth, byte nodeType, Move move)
        {
            this.key = key;    // zobrist key
            this.value = value; // eval
            this.depth = depth; // how much ply ahead was searched in this position
            this.nodeType = nodeType; // exact / beta / close
            this.move = move; // move
        }

        public static int GetSize()
        {
            return System.Runtime.InteropServices.Marshal.SizeOf<Entry>();
        }
    }
}