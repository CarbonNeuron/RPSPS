namespace RPSPS.Models;

public enum GameMode
{
    Classic,
    Spock
}

public static class GameModeExtensions
{
    public static int MoveCount(this GameMode mode) => mode switch
    {
        GameMode.Classic => 3,
        GameMode.Spock => 5,
        _ => 3
    };

    public static string DisplayName(this GameMode mode) => mode switch
    {
        GameMode.Classic => "Classic",
        GameMode.Spock => "Spock",
        _ => "Classic"
    };

    public static string Emoji(this GameMode mode) => mode switch
    {
        GameMode.Classic => "\U0001faa8\U0001f4c4\u2702\ufe0f",
        GameMode.Spock => "\U0001faa8\U0001f4c4\u2702\ufe0f\U0001f98e\U0001f596",
        _ => "\U0001faa8\U0001f4c4\u2702\ufe0f"
    };
}
