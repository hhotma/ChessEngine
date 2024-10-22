namespace Chess.Core;

public class UCI
{
	Engine engine;

	static string[] positionLabels = new[] { "position", "fen", "moves" };
	static string[] goLabels = new[] { "go", "movetime", "wtime", "btime", "winc", "binc", "movestogo" };

	public UCI(EngineSettings settings)
	{
		engine = new Engine(settings);
		engine.OnMoveChosen += OnMoveChosen;
	}

	public void ReceiveCommand(string message)
	{
		message = message.Trim();
		string messageType = message.Split(' ')[0].ToLower();

		switch (messageType)
		{
			case "uci":
				Respond("uciok");
				break;
			case "isready":
				Respond("readyok");
				break;
			case "ucinewgame":
				engine.NotifyNewGame();
				break;
			case "position":
				ProcessPositionCommand(message);
				break;
			case "go":
				ProcessGoCommand(message);
				break;
			case "stop":
				if (engine.IsThinking) { engine.StopThinking(); }
				break;
			case "quit":
				engine.Quit();
				break;
			default:
				break;
		}
	}

	void OnMoveChosen(string move)
	{
		Respond("bestmove " + move);
	}

	void ProcessGoCommand(string message)
	{
		if (message.Contains("movetime"))
		{
			int moveTimeMs = TryGetLabelledValueInt(message, "movetime", goLabels, 0);
			engine.StartThreadedSearch(moveTimeMs);
		}
		else
		{
			int timeRemainingWhiteMs = TryGetLabelledValueInt(message, "wtime", goLabels, 0);
			int timeRemainingBlackMs = TryGetLabelledValueInt(message, "btime", goLabels, 0);
			int incrementWhiteMs = TryGetLabelledValueInt(message, "winc", goLabels, 0);
			int incrementBlackMs = TryGetLabelledValueInt(message, "binc", goLabels, 0);

			int thinkTime = engine.ChooseThinkTime(timeRemainingWhiteMs, timeRemainingBlackMs, incrementWhiteMs, incrementBlackMs);
			engine.StartThreadedSearch(thinkTime);
		}

	}

	// Format: 'position startpos moves e2e4 e7e5'
	// Or: 'position fen rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 moves e2e4 e7e5'
	// Note: 'moves' section is optional
	void ProcessPositionCommand(string message)
	{
		// FEN
		if (message.ToLower().Contains("startpos"))
		{
			engine.SetPosition(FenUtility.StartPositionFEN);
		}
		else if (message.ToLower().Contains("fen")) {
			string customFen = TryGetLabelledValue(message, "fen", positionLabels);
			engine.SetPosition(customFen);
		}

		// Moves
		string allMoves = TryGetLabelledValue(message, "moves", positionLabels);
		if (!string.IsNullOrEmpty(allMoves))
		{
			string[] moveList = allMoves.Split(' ');
			foreach (string move in moveList)
			{
				engine.MakeMove(move);
			}
		}
	}

	void Respond(string reponse)
	{
		Console.WriteLine(reponse);
	}

	int TryGetLabelledValueInt(string text, string label, string[] allLabels, int defaultValue = 0)
	{
		string valueString = TryGetLabelledValue(text, label, allLabels, defaultValue + "");
		if (int.TryParse(valueString.Split(' ')[0], out int result))
		{
			return result;
		}
		return defaultValue;
	}

	string TryGetLabelledValue(string text, string label, string[] allLabels, string defaultValue = "")
	{
		text = text.Trim();
		if (text.Contains(label))
		{
			int valueStart = text.IndexOf(label) + label.Length;
			int valueEnd = text.Length;
			foreach (string otherID in allLabels)
			{
				if (otherID != label && text.Contains(otherID))
				{
					int otherIDStartIndex = text.IndexOf(otherID);
					if (otherIDStartIndex > valueStart && otherIDStartIndex < valueEnd)
					{
						valueEnd = otherIDStartIndex;
					}
				}
			}

			return text.Substring(valueStart, valueEnd - valueStart).Trim();
		}
		return defaultValue;
	}

}