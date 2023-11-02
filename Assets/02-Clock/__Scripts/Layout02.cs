using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//The SlotDef class isn't a subclass of Monobehaviour, so it doesn't need
//a separate C# file
[System.Serializable] //This makes SlotDefs visible in the Unity Inspecotr pane

public class SlotDef02
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public string type = "slot";
    public Vector2 stagger;
}

public class Layout02 : MonoBehaviour
{
    public PT_XMLReader02 xmlr; //Just like the Deck, this has a PT_XMLReader
    public PT_XMLHashtable xml; //This variable is for faster xml access
    public Vector2 multiplier; //The offset of the tableau's center

    //SlotDef references
    public List<SlotDef> slotDefs; //All the SlotDefs(02) for Row0 - Row3
    public SlotDef drawPile;
    public SlotDef discardPile;

    //This holds all of the possible names for the layers set by layerID
    public string[] sortingLayerNames = new string[] { "Row0", "Row1", "Row2", "Row3", "Discard", "Draw" };

    //This function is called to read in the LayoutXML.xml file
    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader02();
        xmlr.Parse(xmlText); //The XML is parsed
        xml = xmlr.xml["xml"][0]; //And xml is set as a shortcut to the XML

        //Read in the multiplier, which sets card spacing
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

        //Read in the slots
        SlotDef tSD;

        //slotX is used as a shortcut to all the <slot>s
        PT_XMLHashList slotsX = xml["slot"];

        for(int i = 0; i < slotsX.Count; i++)
        {
            tSD = new SlotDef(); //Create a new SlotDef instance

            if (slotsX[i].HasAtt("type"))
            {
                //If this <slot> has a type attribute, parse it
                tSD.type = slotsX[i].att("type");
            }

            else
            {
                //If not, set its type to "slot"; in a card in the rows
                tSD.type = "slot";
            }

            //Various attributes are parsed into numerical values
            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.layerID = int.Parse(slotsX[i].att("layer"));

            //This converts the number of the layerID into a text layerName
            tSD.layerName = sortingLayerNames[tSD.layerID];

            switch (tSD.type)
            {
                //Pull additional attributes based on the type of this <slot>
                case "slot":
                    tSD.faceUp = (slotsX[i].att("faceup") == "1");
                    tSD.id = int.Parse(slotsX[i].att("id"));

                    if (slotsX[i].HasAtt("hiddenby"))
                    {
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');

                        foreach(string s in hiding)
                        {
                            tSD.hiddenBy.Add(int.Parse(s));
                        }
                    }

                    slotDefs.Add(tSD);

                    break;

                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tSD;

                    break;

                case "discardpile":
                    discardPile = tSD;

                    break;
            }
        }
    }
}