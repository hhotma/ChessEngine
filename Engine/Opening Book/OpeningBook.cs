namespace Chess.Core;

public class OpeningBook
{
    public Dictionary<string, BookMove[]> movesFromPosition;


    public OpeningBook(string fileName)
    {
        string file = File.ReadAllText(Directory.GetCurrentDirectory()+fileName);

        movesFromPosition = new Dictionary<string, BookMove[]>();

        string[] data = file.Trim(new char[] {' ','\n'}).Split("pos");

        for (int x = 0; x < data.Length; x++)
        {
            string[] curData = data[x].Trim('\n').Split('\n');
            string pos = curData[0].Trim();

            BookMove[] bookMoves = new BookMove[curData.Length-1];

            for (int i = 0; i < curData.Length-1; i++)
            {
                string[] moveData = curData[i+1].Split(" ");
                bookMoves[i] = new BookMove(moveData[0], int.Parse(moveData[1]));
            }

            movesFromPosition.Add(pos, bookMoves);
        }
    }

    public Move TryGetMove(Board board)
    {
        string fen = FenUtility.CurrentFen(board);
        fen = FixFen(fen);

        Random rnd = new Random();
        Move bookMove = Move.NullMove;

        if (movesFromPosition.TryGetValue(fen, out BookMove[] moves))
        {
            int idx = rnd.Next(0, moves.Length);
            bookMove = MoveUtility.MoveFromName(moves[idx].move, board);

            return bookMove;
        }

        return bookMove;
    }

    string FixFen(string fen)
    {
        string tmp = fen.Substring(0, fen.LastIndexOf(" "));
        return tmp.Substring(0, tmp.LastIndexOf(" "));
    }

    public struct BookMove
    {
        public string move;
        public int timesPlayed;

        public BookMove(string moveString, int played)
        {
            move = moveString;
            timesPlayed = played;
        }
    }
}