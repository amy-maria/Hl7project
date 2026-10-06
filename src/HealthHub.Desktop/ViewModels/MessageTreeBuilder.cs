using System.Collections.Generic;
using HealthHub.Core.Hl7;


namespace HealthHub.Desktop.ViewModels;

///Turns raw Hl7 message into tree nodes: segment -> field -> repetitions -> component

public static class MessageTreeBuilder
{
    public static List<MessageTreeNode> Build(string raw)
    {
        var message = Hl7Message.Parse(raw);
        var nodes = new List<MessageTreeNode>();

        foreach (var segment in message.Segments)
         {
            var segmentNode = new MessageTreeNode(segment.Name);

            for (int f = 1; f <= segment.FieldCount; f++)
            {
                string value = segment.GetField(f);
                if (value == "")
                    continue; // skips empty fields to keep tree readable
                var fieldNode = new MessageTreeNode($"{segment.Name}-{f}", value);
                bool isDelimiterField = segment.Name == "MSH" && f <= 2;
                if (!isDelimiterField)
                    AddRepetitionsAndComponents(fieldNode, segment.GetRepetitions(f), message.Delimiters);
                    segmentNode.Children.Add(fieldNode);
            }
            nodes.Add(segmentNode);
        }
        return nodes;
    }   
    private static void AddRepetitionsAndComponents(MessageTreeNode fieldNode, string []repetitions, Delimiters d)
    {
        if (repetitions.Length == 1)
        {
            //most fields don't repeat, put components directly under the field
            AddComponents(fieldNode, repetitions[0], d);
            return;
    
        }
        for (int r = 0; r< repetitions.Length; r++)
        {
            var repetitionNode = new MessageTreeNode($"[{r + 1}]", repetitions[r]);
            AddComponents(repetitionNode, repetitions[r], d);
            fieldNode.Children.Add(repetitionNode);
        }
    }
    private static void AddComponents(MessageTreeNode parent, string value, Delimiters d)
    {
        string[] components = value.Split(d.Component);
        if (components.Length == 1)
            return; 
        
        for (int c = 0; c < components.Length; c++)
        {
            if (components[c] == "")
            continue;

            parent.Children.Add(new MessageTreeNode($".{c+1}", Hl7Escaping.Unescape(components[c], d)));
        }
    }
}