﻿using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using rgbMod.Arduino;

public class arduinoModHoldable : MonoBehaviour
{

    public arduinoService Service;

    private KMSelectable mainHoldable;
    private KMSelectable DisconnectButton;
    private KMSelectable RefreshButton;
    private KMSelectable TestButton;
    private KMSelectable[] defaultChildren;

    private GameObject Button;

    private List<GameObject> connectBTNs = new List<GameObject>();
    private List<KMSelectable> childrenBTNs = new List<KMSelectable>();

    private GameObject Frame;
    private GameObject redOBJ;
    private GameObject greenOBJ;
    private GameObject yellowOBJ;

    private Material defaultMat;

    void Start()
    {
        Debug.Log("[Arduino Manager Holdable] Initialised");
        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        while (true)
        {
            yield return null;
            Service = FindObjectsOfType<arduinoService>()[0].GetComponent<arduinoService>();
            if (Service != null)
            {
                Debug.Log("[Arduino Manager Holdable] Got it");
                mainHoldable = this.GetComponent<KMSelectable>();
                defaultChildren = mainHoldable.Children;
                DisconnectButton = mainHoldable.Children[0];
                RefreshButton = mainHoldable.Children[1];
                TestButton = mainHoldable.Children[2];
                Button = mainHoldable.Children[3].gameObject;
                Frame = mainHoldable.gameObject.transform.Find("arduinoHoldableBacking").gameObject;
                redOBJ = mainHoldable.gameObject.transform.Find("arduinoHoldableRedOBJ").gameObject;
                greenOBJ = mainHoldable.gameObject.transform.Find("arduinoHoldableGreenOBJ").gameObject;
                yellowOBJ = mainHoldable.gameObject.transform.Find("arduinoHoldableYellowOBJ").gameObject;
                defaultMat = Frame.GetComponent<Renderer>().material;
                DisconnectButton.OnInteract += disconnectBTNAction;
                RefreshButton.OnInteract += refreshBTNAction;
                TestButton.OnInteract += testBTNAction;
                if (Service.arduinoConnection._connected)
                {
                    Frame.GetComponent<Renderer>().material = greenOBJ.GetComponent<Renderer>().material;
                }
                Refresh();
                yield break;
            }
        }
    }


    private bool ConnectBTNAction(string portName)
    {
        if (Service != null) { StartCoroutine(attemptConnection(portName)); }
        return false;
    }

    private IEnumerator attemptConnection(string portName)
    {
        yield return null;
        Debug.LogFormat("Connecting to {0}", portName);
        if (Service.arduinoConnection._connected)
        {
            Service.arduinoConnection.Disconnect();
        }
        Frame.GetComponent<Renderer>().material = yellowOBJ.GetComponent<Renderer>().material;
        Service.arduinoConnection.Connect(portName, 9600);
        yield return new WaitForSeconds(1.5f);
        if (Service.arduinoConnection._connected)
        {
            Frame.GetComponent<Renderer>().material = greenOBJ.GetComponent<Renderer>().material;
        }
        else
        {
            Frame.GetComponent<Renderer>().material = redOBJ.GetComponent<Renderer>().material;
        }
        yield break;
    }

    private bool disconnectBTNAction()
    {
        if (Service != null)
        {
            Service.arduinoConnection.Disconnect();
            Frame.GetComponent<Renderer>().material = defaultMat;
        }
        return false;
    }

    private bool testBTNAction()
    {
        if (Service != null) { StartCoroutine(Test()); }
        return false;
    }

    private IEnumerator Test()
    {
        yield return null;
        Service.arduinoConnection.sendMSG("5 3 4 255 255 255");
        yield return new WaitForSeconds(3f);
        Service.arduinoConnection.Stop();
        yield break;
    }

    private bool refreshBTNAction()
    {
        if (Service != null) { Refresh(); }
        return false;
    }

    void resizeText(GameObject disp)
    {
        int pluschars = 0;
        pluschars = disp.GetComponent<TextMesh>().text.Length - 10;
        while (pluschars > 0)
        {
            disp.GetComponent<TextMesh>().fontSize = disp.GetComponent<TextMesh>().fontSize - 15;
            if (disp.GetComponent<TextMesh>().fontSize < 45)
            {
                disp.GetComponent<TextMesh>().fontSize = disp.GetComponent<TextMesh>().fontSize + 20;
            }
            pluschars = pluschars - 1;
        }
        return;
    }

    private void Refresh()
    {
        Button.GetComponent<Renderer>().enabled = false;
        for (int i = 0; i < connectBTNs.Count; i++)
        {
            if (i == 0) { continue; }
            Destroy(connectBTNs[i]);
        }
        connectBTNs.Clear();
        connectBTNs.Add(Button);
        childrenBTNs.Clear();
        childrenBTNs.Add(DisconnectButton);
        childrenBTNs.Add(RefreshButton);
        string[] ports = Service.arduinoConnection.getAvailablePorts();
        if (ports.Length > 0) { 
            connectBTNs[0].transform.Find("buttonText").gameObject.GetComponent<TextMesh>().text = ports[0];
            connectBTNs[0].GetComponent<Renderer>().enabled = true;
        }
        else
        {
            connectBTNs[0].transform.Find("buttonText").gameObject.GetComponent<TextMesh>().text = "";
            connectBTNs[0].GetComponent<Renderer>().enabled = false;
        }
        for (int i = 0; i < ports.Length; i++)
        {
            connectBTNs.Add(Instantiate(connectBTNs[0], new Vector3(connectBTNs[0].transform.position.x, connectBTNs[0].transform.position.y, connectBTNs[0].transform.position.z - (0.05f * (i + 1))), connectBTNs[0].transform.rotation));
        }
        for(int i = 0; i < ports.Length; i++)
        {
            connectBTNs[i].transform.Find("buttonText").gameObject.GetComponent<TextMesh>().text = ports[i];
            connectBTNs[i].transform.parent = mainHoldable.gameObject.GetComponent<Transform>();
            resizeText(connectBTNs[i].transform.Find("buttonText").gameObject);
            if (i == 0) { continue; }
            connectBTNs[i].transform.localPosition = new Vector3(connectBTNs[0].transform.localPosition.x, connectBTNs[0].transform.localPosition.y, connectBTNs[0].transform.localPosition.z - (0.05f * i));
        }
        connectBTNs[connectBTNs.Count-1].transform.parent = mainHoldable.gameObject.GetComponent<Transform>();
        GameObject btnToRemove = connectBTNs[connectBTNs.Count - 1];
        connectBTNs.RemoveAt(connectBTNs.Count - 1);
        Destroy(btnToRemove);
        foreach (GameObject item in connectBTNs)
        {
            item.GetComponent<KMSelectable>().OnInteract += () => ConnectBTNAction(item.transform.Find("buttonText").gameObject.GetComponent<TextMesh>().text);
            item.GetComponent<Renderer>().enabled = true;
            childrenBTNs.Add(item.GetComponent<KMSelectable>());
        }
        Debug.Log("Got there!");
        mainHoldable.Children = childrenBTNs.ToArray();
    }

    #pragma warning disable 414
    string TwitchHelpMessage = "Commands are: '!{0} disconnect'; '!{0} refresh'; '!{0} test'; '!{0} connect x' where x is the position of the correct connect button from top to bottom. Only the streamer can run these commands.";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        if (command == "disconnect")
        {
            yield return null;
            yield return new object[] { "streamer", (Action)(() => DisconnectButton.OnInteract()), "This command is for the streamer only." };
        }
        if (command == "refresh")
        {
            yield return null;
            yield return new object[] { "streamer", (Action)(() => RefreshButton.OnInteract()), "This command is for the streamer only." };
        }
        if (command == "test")
        {
            yield return null;
            yield return new object[] { "streamer", (Action)(() => TestButton.OnInteract()), "This command is for the streamer only." };
        }
        if (command.StartsWith("connect", StringComparison.InvariantCulture))
        {
            command = command.Replace("connect ", "");
            yield return null;
            if(!int.TryParse(command, out int num))
            {
                yield return "sendtochaterror Number not valid!";
                yield break;
            }
            if(num<1 || num > connectBTNs.Count)
            {
                yield return "sendtochaterror Number out of range!";
                yield break;
            }
            yield return new object[] { "streamer", (Action)(() => connectBTNs[num-1].GetComponent<KMSelectable>().OnInteract()), "This command is for the streamer only." };
            yield return String.Format("sendtochat Connecting to {0}", connectBTNs[num-1].transform.Find("buttonText").gameObject.GetComponent<TextMesh>().text);
            yield return new WaitForSeconds(1.51f);
            if (Service.arduinoConnection._connected)
            {
                yield return "sendtochat PraiseIt Connection succesful! PraiseIt";
            }
            else
            {
                yield return "sendtochat VoteNay Connection failed! VoteNay";
            }
        }
    }
}