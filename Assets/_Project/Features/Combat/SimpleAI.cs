public class SimpleAI
{
    private System.Random _random = new();

    public ActionType ChooseAction()
    {
        int value = _random.Next(0, 3);

        return (ActionType)value;
    }
}