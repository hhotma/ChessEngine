using static System.Math;

namespace Chess.Core;

public class Engine
{
	public event Action<string> OnMoveChosen;
	public bool IsThinking { get; private set; }
	public bool LatestMoveIsBookMove { get; private set; }

	// References
	Search search;
	Board board;
	CancellationTokenSource cancelSearchTimer;

	public Engine(EngineSettings settings)
	{
		board = new Board();
		board.LoadStartPosition();
		search = new Search(board, settings);
		search.onSearchComplete += OnSearchComplete;
	}

	public void NotifyNewGame()
	{
		search.ClearForNewGame();
	}

	public void SetPosition(string fen)
	{
		board.LoadPosition(fen);
	}

	public void MakeMove(string moveString)
	{
		Move move = MoveUtility.MoveFromName(moveString, board);
		board.MakeMove(move);
	}

	public int ChooseThinkTime(int timeRemainingWhiteMs, int timeRemainingBlackMs, int incrementWhiteMs, int incrementBlackMs)
	{
		int myTimeRemainingMs = board.IsWhiteToMove ? timeRemainingWhiteMs : timeRemainingBlackMs;
		int myIncrementMs = board.IsWhiteToMove ? incrementWhiteMs : incrementBlackMs;
		// Get a fraction of remaining time to use for current move
		double thinkTimeMs = myTimeRemainingMs / 40.0;

		// Add increment
		if (myTimeRemainingMs > myIncrementMs * 2)
		{
			thinkTimeMs += myIncrementMs * 0.8;
		}

		double minThinkTime = Min(50, myTimeRemainingMs * 0.25);

		int thinkTime = (int)Ceiling(Max(minThinkTime, thinkTimeMs) * 0.8);
		return thinkTime;
	}

	public void StartThreadedSearch(int timeMs)
	{
		IsThinking = true;
		Task.Factory.StartNew(() => search.InitSearch(), TaskCreationOptions.LongRunning);

		cancelSearchTimer = new CancellationTokenSource();
		Task.Delay(timeMs, cancelSearchTimer.Token).ContinueWith((t) => TimeOutThreadedSearch());
	}

	void TimeOutThreadedSearch()
	{
		if (cancelSearchTimer == null || !cancelSearchTimer.IsCancellationRequested)
		{
			EndSearch();
		}
	}

	public void StopThinking()
	{
		EndSearch();
	}

	public void Quit()
	{
		EndSearch();
	}

	void EndSearch()
	{
		if (IsThinking)
		{
			search.EndSearch();
		}
	}

	void OnSearchComplete(Move move)
	{
		cancelSearchTimer?.Cancel();
		IsThinking = false;

		string moveName = MoveUtility.NameFromMove(move).Replace("=", "");
		OnMoveChosen?.Invoke(moveName);
	}
}