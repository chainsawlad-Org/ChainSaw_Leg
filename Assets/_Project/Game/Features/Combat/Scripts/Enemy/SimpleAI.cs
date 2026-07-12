public class SimpleAI
{
    private System.Random random = new();

    public ActionType ChooseAction()
    {
        int value = random.Next(0, 3);

        return (ActionType)value;
    }
}