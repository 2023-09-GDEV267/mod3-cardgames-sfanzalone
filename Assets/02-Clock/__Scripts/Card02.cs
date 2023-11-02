using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card02 : MonoBehaviour
{

	public string    suit;
	public int       rank;
	public Color     color = Color.black;
	public string    colS = "Black";  // or "Red"
	
	public List<GameObject> decoGOs = new List<GameObject>();
	public List<GameObject> pipGOs = new List<GameObject>();
	
	public GameObject back;  // back of card;
	public CardDefinition def;  // from DeckXML.xml		

	//List of the SpriteRenderer Components of this GameObject and its children
	public SpriteRenderer[] spriteRenderers;

	[SerializeField]

	GameObject PrefabClockCard;
	GameObject PrefabSprite;

	[SerializeField]

	int numOfObjects = 12;

	[SerializeField]

	float radius = 5f;

	//List of the SpriteRenderer Components of this GameObject and its children
	//public SpriteRenderer[] spriteRenderers; //This must stay commented

	// Use this for initialization
	void Start()
	{
		//SetOrder(0); //Ensures that the card starts properly depth sorted
	}

	//If spriteRenderers isn't yet defined, this function defines it
	public void PopulateSpriteRenderers()
    {
		//If spriteRenderers is null or empty
		if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
			//Get SpriteRenderer Components of this GameObject and its children
			spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

	//Sets the sortingLayerName on all SpreiteRenderer Components
	public void SetSortingLayerName(string tSLN)
    {
		PopulateSpriteRenderers();

		foreach(SpriteRenderer tSR in spriteRenderers)
        {
			tSR.sortingLayerName = tSLN;
        }
    }

	//Sets the sortingOrder of all SpriteRenderer Components
	public void SetSortOrder(int sOrd)
    {
		PopulateSpriteRenderers();

		//Iterate through all the spriteRenderers as tSR
		foreach(SpriteRenderer tSR in spriteRenderers)
        {
			if(tSR.gameObject == this.gameObject)
            {
				//If the gameObject is this.gameObject, its the background
				tSR.sortingOrder = sOrd; //Set its order to sOrd
				continue; //And continue to the next iteration of the loop
            }

            //Each of the children of this GameObject are named switch based on the names
            switch (tSR.gameObject.name)
            {
				case "back": //If the name is "back"
					//Set it to the highest layer to cover the other sprites
					tSR.sortingOrder = sOrd + 2;

					break;

				case "face": //If the name is "face"
				default: //or if it's anything else
						 //Set it to the middle layer to be above the background
					tSR.sortingOrder = sOrd + 1;

					break;
            }
        }
    }

	public bool faceUp
	{
		get
		{
			return (!back.activeSelf);
		}

		set
		{
			back.SetActive(!value);
		}
	}
	
	//Virtual methods can be converted by subclass methods with the same name
	virtual public void OnMouseUpAsButton()
    {
		print(name); //When clicked, this outputs the card name
    }

	// Update is called once per frame
	void Update ()
	{
        if (Input.GetKeyDown(KeyCode.Space))
        {
			for(int i = 0; i < numOfObjects; i++)
			{
				float angle = i * Mathf.PI * 2 / numOfObjects;
				float x = Mathf.Cos(angle) * radius;
				float y = Mathf.Tan(angle) * radius;
				Vector3 pos = transform.position + new Vector3(x, y, 0);
				float angleDegrees = -angle * Mathf.Rad2Deg;
				Quaternion rot = Quaternion.Euler(0, 0, angleDegrees);
				Instantiate(PrefabClockCard, pos, rot);
			}
        }
	}
} // class Card

[System.Serializable]
public class Decorator02
{
	public string	type;			// For card pips, type = "pip"
	public Vector3	loc;			// location of sprite on the card
	public bool		flip = false;	//whether to flip vertically
	public float 	scale = 1.0f;
}

[System.Serializable]
public class CardDefinition02
{
	public string	face;	//sprite to use for face cart
	public int		rank;	// value from 1-13 (Ace-King)
	public List<Decorator>	
					pips = new List<Decorator>();  // Pips Used
}