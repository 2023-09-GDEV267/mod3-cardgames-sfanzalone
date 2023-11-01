using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//An enum defines a variable type with a few prenamed values
public enum eCardState02
{
    drawpile,
    tableau,
    target,
    discard
}

public class CardClock : Card02
{
    [Header("Set Dynamically: CardClock")]

    //This is how you use the enum eCardState
    public eCardState state = eCardState.drawpile;

    //The hiddenBy list stores which other cards will keep this one face down
    public List<CardClock> hiddenBy = new List<CardClock>();

    //The layout matches this card to the tableau XML if it's a tableau card
    public int layoutID;

    //The SlotDef class stores information pulled in from the LayoutXML <slot>
    public SlotDef slotDef;

    //This allows the card to react to being clicked
    override public void OnMouseUpAsButton()
    {
        //Call the CardClicked method on the Clock singleton
        Clock.S.CardClicked(this);
        
        //Also call the base class (Card.cs) version of this method
        base.OnMouseUpAsButton();
    }
}