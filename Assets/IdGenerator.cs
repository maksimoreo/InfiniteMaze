// It generates... IDs
public class IdGenerator
{
    private int counter = 0;

    public int Next()
    {
        return counter++;
    }
}
