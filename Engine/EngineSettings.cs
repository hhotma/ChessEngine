namespace Chess.Core;

public readonly struct EngineSettings
{
    public readonly bool UseOpeningBook;
    public readonly string OpeningBookFileName;

    public EngineSettings(bool useOpeningBook, string openingBookFileName)
    {
        UseOpeningBook = useOpeningBook;
        OpeningBookFileName = openingBookFileName;
    }
}