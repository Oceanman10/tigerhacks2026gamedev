/// <summary>
/// Simple data holder that survives a scene load within the same play session.
/// The Difficulty Select screen writes to this; the Gameplay scene reads from it.
/// </summary>
public static class GameSession
{
    public static string SelectedLevelName = "Easy";
}
