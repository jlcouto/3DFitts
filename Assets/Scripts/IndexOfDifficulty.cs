public class IndexOfDifficulty
{
    public float targetWidth { get; private set; }
    public float targetsDistance { get; private set; }
    public float indexOfDifficulty { get; private set; }

    public IndexOfDifficulty(float targetWidth, float targetsDistance)
    {
        this.targetWidth = targetWidth;
        this.targetsDistance = targetsDistance;
        this.indexOfDifficulty = ResultsMath.IndexOfDifficulty(targetWidth, targetsDistance);
    }
}
