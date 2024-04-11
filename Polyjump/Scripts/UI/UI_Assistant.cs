using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Assistant : MonoBehaviour
{
    public string[] messages;
    private Text messageText;
    private TextWriter.TextWriterSingle textWriterSingle;

    private void Awake()
    {
        messageText = transform.Find("Message").Find("MessageText").GetComponent<Text>();
        //transform.Find("Message").GetComponent<Button>().onClick.AddListener(DisplayText);
    }

    private void Start()
    {
        //TextWriter.AddWriter_Static(messageText, "Welcome to Polyjump!", .1f, true, true);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DisplayText();
        }
    }

    private void DisplayText()
    {
        if (textWriterSingle != null && textWriterSingle.IsActive())
        {
            textWriterSingle.WriteAllAndDestroy();
        }
        else
        {
            string[] messageArray = new string[]
        {
            "Hello World",
            "Test",
            "aNother test of text"
        };

            string message = messages[Random.Range(0, messages.Length)];
            textWriterSingle = TextWriter.AddWriter_Static(messageText, message, .05f, true, true);
        }
    }
}
