using System.Collections.Generic;
using System.Reflection.Emit;

namespace HealthHub.Desktop.ViewModels;

///One line in the message tree: segment, field, repitition, or component.

public sealed class MessageTreeNode
{
    public MessageTreeNode(string label, string value = "")
    {
        Label = label;
        Value = value;
    }
    
    public string Label {get;}
    public string Value {get;}
    public List<MessageTreeNode> Children {get;} = new();
}   
