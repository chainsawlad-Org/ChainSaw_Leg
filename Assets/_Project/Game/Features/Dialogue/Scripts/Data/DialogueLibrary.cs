using System.Collections.Generic;

public static class DialogueLibrary
{
    public static List<IDialogueEvent> TestDialogue()
    {
        // var endNode = new DialogueNode
        // {
        //     text = "Понятно."
        // };

        var goodNode = new DialogueNode
        {
            text = "Рад это слышать!",
            // nextNode = endNode
        };

        var badNode = new DialogueNode
        {
            text = "Бывает...",
            // nextNode = endNode
        };

        var secondQuestion = new DialogueNode
        {
            text = "Как у тебя дела?",
            choices = new List<DialogueChoice>
            {
                new DialogueChoice { text = "Хорошо", nextNode = goodNode },
                new DialogueChoice { text = "Плохо", nextNode = badNode }
            }
        };

        var nameResponse = new DialogueNode
        {
            text = "Приятно познакомиться!",
            nextNode = secondQuestion
        };

        var rudeNode = new DialogueNode
        {
            text = "Ну и ладно...",
            nextNode = secondQuestion
        };

        var askName = new DialogueNode
        {
            text = "Как тебя зовут?",
            choices = new List<DialogueChoice>
            {
                new DialogueChoice { text = "Не твоё дело", nextNode = rudeNode },
                new DialogueChoice { text = "Меня зовут Игрок", nextNode = nameResponse }
            }
        };

        var startNode = new DialogueNode
        {
            text = "Привет! Меня зовут NPC.",
            nextNode = askName
        };

        return ConvertToEvents(startNode);
    }

    private static List<IDialogueEvent> ConvertToEvents(DialogueNode start)
    {
        var events = new List<IDialogueEvent>();

        DialogueNode current = start;

        while (current != null)
        {
            events.Add(new TypewriterEvent { text = current.text, speed = 0.05f });

            if (current.choices != null && current.choices.Count > 0)
            {
                events.Add(new ChoiceEvent { choices = current.choices });
                break;
            }

            current = current.nextNode;
        }

        return events;
    }
}