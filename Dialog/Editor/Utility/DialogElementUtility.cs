using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public static class DialogElementUtility
{
    public static Button CreateButton(string text, Action onClick = null)
    {
        Button button = new Button(onClick)
        {
            text = text
        };
        button.AddToClassList("ds-node__button");

        return button;
    }

    public static Foldout CreateFoldout(string value, bool collapsed = false)
    {
        Foldout foldout = new Foldout()
        {
            text = value,
            value = !collapsed
        };

        return foldout;
    }

    public static DialogPort CreatePort(this DialogNode node, string portName = "", Orientation orientation = Orientation.Horizontal, Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single)
    {
        return new DialogPort(orientation, direction, capacity, typeof(bool)){
            portName = portName
        };
    }

    public static Label CreateLabel(string value)
    {
        Label label = new Label(value);
        label.AddToClassList("ds-node__label");

        return label;
    }

    public static TextField CreatePortField(string value = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
    {
        TextField textField = new TextField()
        {
            value = value
        };
        textField.AddToClassList("ds-node__choice-textfield");

        if (onValueChanged != null)
        {
            textField.RegisterValueChangedCallback(onValueChanged);
        }

        return textField;
    }

    public static TextField CreateQuoteField(string value = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
    {
        TextField textField = new TextField()
        {
            value = value
        };
        textField.AddClasses("ds-node__quote-textfield");

        if (onValueChanged != null)
        {
            textField.RegisterValueChangedCallback(onValueChanged);
        }

        return textField;
    }

    public static TextField CreateTextArea(string value = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
    {
        TextField textArea = CreateQuoteField(value, onValueChanged);
        textArea.multiline = true;

        return textArea;
    }
}
