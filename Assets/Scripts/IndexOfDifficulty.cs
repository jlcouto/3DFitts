public class IndexOfDifficulty
{
    private float _targetWidth;
    private float _targetsDistance;
    private float _indexOfDifficulty;

    public float targetWidth { get; }
    public float targetsDistance { get; }
    public float indexOfDifficulty { get; }

    public IndexOfDifficulty(float targetWidth, float targetsDistance)
    {
        _targetWidth = targetWidth;
        _targetsDistance = targetsDistance;
        _indexOfDifficulty = ResultsMath.IndexOfDifficulty(targetWidth, targetsDistance);
    }
}
