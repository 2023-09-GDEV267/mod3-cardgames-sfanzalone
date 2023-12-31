﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Clock : MonoBehaviour
{
	static public Clock S;

	[Header("Set in Inspector")]
	public TextAsset deckXML;
	public TextAsset layoutXML;
	public float xOffSet = 3;
	public float yOffSet = -2.5f;
	public Vector3 layoutCenter;
	public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
	public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
	public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
	public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
	public float reloadDelay = 1f; //The delay between rounds
	public Text gameOverText, roundResultText, highScoreText;


	[Header("Set Dynamically")]
	public Deck02 deck;
	public Layout02 layout;
	public List<CardClock> drawPile;
	public Transform layoutAnchor;
	public CardClock target;
	public List<CardClock> tableau;
	public List<CardClock> discardPile;
	public FloatingScore02 fsRun;

	void Awake()
	{
		S = this;
		SetUpUITexts();
	}

	void SetUpUITexts()
	{
		//Set up the HighScore UI Text
		GameObject go = GameObject.Find("Highscore");

		if (go != null)
		{
			highScoreText = go.GetComponent<Text>();
		}

		int highScore = ScoreManager02.HIGH_SCORE;
		string hScore = "High Score: " + Utils02.AddCommasToNumber(highScore);
		go.GetComponent<Text>().text = hScore;

		//Set up the UI Texts that show at the end of the round
		go = GameObject.Find("GameOver");

		if (go != null)
		{
			gameOverText = go.GetComponent<Text>();
		}

		go = GameObject.Find("RoundResult");

		if (go != null)
		{
			roundResultText = go.GetComponent<Text>();
		}

		//Make the end of round texts invisible
		ShowResultsUI(false);
	}

	//Make the end of round texts invisible
	void ShowResultsUI(bool show)
	{
		gameOverText.gameObject.SetActive(show);
		roundResultText.gameObject.SetActive(show);
	}

	void Start()
	{
		Scoreboard02.S.score = ScoreManager02.SCORE;

		deck = GetComponent<Deck02>(); //Get the Deck
		deck.InitDeck(deckXML.text); //Pass DeckXML to it
		Deck02.Shuffle(ref deck.cards); //This shuffles the deck
		drawPile = ConvertListCardsToListCardClocks(deck.cards);
		

		Card02 c;

		for(int cNum = 0; cNum < deck.cards.Count; cNum++) 
		{
			c = deck.cards[cNum];
			c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
		}

		layout = GetComponent<Layout02>(); //Get the Layout component
		layout.ReadLayout(layoutXML.text); //Pass LayoutXML to it
		drawPile = ConvertListCardsToListCardClocks(deck.cards);

		LayoutGame();
	}

	List<CardClock> ConvertListCardsToListCardClocks(List<Card02> lCD)
	{
		List<CardClock> lCP = new List<CardClock>();
		CardClock tCP;

		foreach (Card02 tCD in lCD)
		{
			tCP = tCD as CardClock; //Logic Error that needs to be fixed
			lCP.Add(tCP); //Logic Error must be fixed first
		}

		return (lCP);
	}

	//The Draw function will pull a single card from the drawPile and return it
	CardClock Draw()
	{
		CardClock cd = drawPile[0]; //Pull the 0'th CardClock
		drawPile.RemoveAt(0); //Then remove it from List<> drawPile

		return (cd); //And return it
	}

	//LayoutGame() positions the initial tableau of cards, a.k.a. "the mine"
	void LayoutGame()
	{
		//Create an empty GameObject to serve as an anchor for the tableau
		if (layoutAnchor == null)
		{
			GameObject tGO = new GameObject("_LayoutAnchor");
			//^Create an empty GameObject named _LayoutAnchor in the Hierarchy
			layoutAnchor = tGO.transform; //Grab its Transform
			layoutAnchor.transform.position = layoutCenter; //Position it
		}

		CardClock cp;

		//Follow the layout
		foreach (SlotDef tSD in layout.slotDefs)
		{
			//^Iterate through all the SlotDefs in the layout.slotDefs as tSD
			cp = Draw(); //Pull a card from the top (beginning) of the draw Pile
			cp.faceUp = tSD.faceUp; //Set its faceUp to the value in slotDef
			cp.transform.parent = layoutAnchor; //Make its parent layoutAnchor

			//This replaces the previous parent: deck.deckAnchor, which appears as _Deck
			//in the Hierarchy when the scene is playing.
			cp.transform.localPosition = new Vector3(layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y,
														-tSD.layerID);
			//^Set the localPosition of the card based on slotDef
			cp.layoutID = tSD.id;
			cp.slotDef = tSD;

			//CardClocks in the tableau have the state CardState.tableau
			cp.state = eCardState.tableau;
			cp.SetSortingLayerName(tSD.layerName); //Set the sorting layers

			tableau.Add(cp); //Add this CardClock to the List<> tableau
		}

		//Set which cards are hiding others
		foreach (CardClock tCP in tableau)
		{
			foreach (int hid in tCP.slotDef.hiddenBy)
			{
				cp = FindCardByLayoutID(hid);
				tCP.hiddenBy.Add(cp);
			}
		}

		//Set up the initial target card
		MoveToTarget(Draw());

		//Set up the Draw pile
		UpdateDrawPile();
	}

	//Moves the current target to the discardPile
	void MoveToDiscard(CardClock cd)
	{
		//Set the state of the card to discard
		cd.state = eCardState.discard;
		discardPile.Add(cd); //Add it to the discardPile List<>
		cd.transform.parent = layoutAnchor; //Update its transform parent

		//Position this card on the discardPile
		cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x,
													layout.multiplier.y * layout.discardPile.y,
													-layout.discardPile.layerID + 0.5f);
		cd.faceUp = true;

		//Place it on top of the pile for depth sorting
		cd.SetSortingLayerName(layout.discardPile.layerName);
		cd.SetSortOrder(-100 + discardPile.Count);
	}

	void MoveToTarget(CardClock cd)
	{
		//If there is currently a target card, move it to discardPile
		if (target != null)
		{
			MoveToDiscard(target);
		}

		target = cd; //cd is the new target

		cd.state = eCardState.target;
		cd.transform.parent = layoutAnchor;

		//Move to the target position
		cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x,
													layout.multiplier.y * layout.discardPile.y,
													-layout.discardPile.layerID);
		cd.faceUp = true; //Make it face-up

		//Set the depth sorting
		cd.SetSortingLayerName(layout.discardPile.layerName);
		cd.SetSortOrder(0);
	}

	void UpdateDrawPile()
	{
		CardClock cd;

		//Go through all the cards of the drawPile
		for (int i = 0; i < drawPile.Count; i++)
		{
			cd = drawPile[i];
			cd.transform.parent = layoutAnchor;

			//Position it correctly with the layout.drawPile.stagger
			Vector2 dpStagger = layout.drawPile.stagger;
			cd.transform.localPosition = new Vector3(layout.multiplier.x * (layout.discardPile.x +
													i * dpStagger.x), layout.multiplier.y *
													(layout.discardPile.y + i * dpStagger.y),
													-layout.drawPile.layerID + 0.1f * i);
			cd.faceUp = false; //Make them all face-down
			cd.state = eCardState.drawpile;

			//Set depth sorting 
			cd.SetSortingLayerName(layout.drawPile.layerName);
			cd.SetSortOrder(-10 * i);
		}
	}

	//Convert from the layoutID int to the CardClock with that ID
	CardClock FindCardByLayoutID(int layoutID)
	{
		foreach (CardClock tCP in tableau)
		{
			//If the card has the same ID, return it
			return (tCP);
		}

		//If it's not, return null
		return (null);
	}

	//This turns cards in the Mine face-up or face-down
	void SetTableauFaces()
	{
		foreach (CardClock cd in tableau)
		{
			bool faceUp = true; //Assume the card will be face-up

			foreach (CardClock cover in cd.hiddenBy)
			{
				//If either of the covering cards are in the tableau
				if (cover.state == eCardState.tableau)
				{
					faceUp = false; //Then this card is face-down
				}
			}

			cd.faceUp = faceUp; //Set the value on the card
		}
	}

	//CardClicked is called any time a card in the game is clicked
	public void CardClicked(CardClock cd)
	{
		//The reaction is determined by the state of the clicked card
		switch (cd.state)
		{
			case eCardState.target:
				//Clicking the target card does nothing
				break;

			case eCardState.drawpile:
				//Clicking any card in the drawPile will draw the next card
				MoveToDiscard(target); //Moves the target to the discardPile
				MoveToTarget(Draw()); //Moves the next drawn card to the target
				SetTableauFaces(); //Update tableau card face-ups
				UpdateDrawPile(); //Restacks the drawPile
				ScoreManager02.EVENT(eScoreEvent.draw);
				FloatingScoreHandler(eScoreEvent.draw);

				break;

			case eCardState.tableau:
				//Clicking a card in the tableau will check if it's a valid play
				bool validMatch = true;

				if (!cd.faceUp)
				{
					//If the card is face-down, it's not valid
					validMatch = false;
				}

				if (!AdjacentRank(cd, target))
				{
					//If it's not an adjacent rank, it's not valid
					validMatch = false;
				}

				if (!validMatch)
				{
					return; //Return if not valid
				}

				//If we got here, then: Yay!  It's a valid card.
				tableau.Remove(cd); //Remove it from the tableau List
				MoveToTarget(cd); //Make it the target card
				ScoreManager02.EVENT(eScoreEvent.mine);
				FloatingScoreHandler(eScoreEvent.mine);

				break;
		}

		//Check to see whether the game is over or not
		CheckForGameOver();
	}

	//Test whether the game is over
	void CheckForGameOver()
	{
		//If tableau is empty, the game is over
		if (tableau.Count == 0)
		{
			//Call GameOver() with a win
			GameOver(true);

			return;
		}

		//If there are still cards in the draw pile, the game's not over
		if (drawPile.Count > 0)
		{
			return;
		}

		//Check for remaining valid plays
		foreach (CardClock cd in tableau)
		{
			if (AdjacentRank(cd, target))
			{
				//If there's a valid play, the game's not over
				return;
			}

			//Since there's no valid plays, the game is over
			//Call GameOver with a loss
			GameOver(false);
		}
	}

	//Called when the game is over.  Simple for now, but expandable
	void GameOver(bool won)
	{
		int score = ScoreManager02.SCORE;

		if (fsRun != null)
		{
			score += fsRun.score;
		}

		if (won)
		{
			gameOverText.text = "Round Over";
			roundResultText.text = "You won this round! \nRound Score: " + score;
			ShowResultsUI(true);

			//print("Game Over.  You Won! :)"); //This is supposed to be commented out
			ScoreManager02.EVENT(eScoreEvent.gameWin);
			FloatingScoreHandler(eScoreEvent.gameWin);
		}

		else
		{
			gameOverText.text = "Game Over";

			if (ScoreManager02.HIGH_SCORE <= score)
			{
				string str = "You got the high score! \nHigh score: " + score;
				roundResultText.text = str;
			}

			else
			{
				roundResultText.text = "Yoyr final score was: " + score;
			}

			ShowResultsUI(true);

			//print("Game Over.  You Lost. :("); //This is supposed to be commented out
			ScoreManager02.EVENT(eScoreEvent.gameLoss);
			FloatingScoreHandler(eScoreEvent.gameLoss);
		}

		//Reload the scene, resetting the game
		SceneManager.LoadScene("__Clock");
	}

	void ReloadLevel()
	{
		//Reload the scene, resseting the game
		SceneManager.LoadScene("_Clock");
	}

	//Return true if the two cards are adjacent in rank (A & K wrap around)
	public bool AdjacentRank(CardClock c0, CardClock c1)
	{
		//If either card is face-down, it's not adjacent.
		if (!c0.faceUp || !c1.faceUp)
		{
			return (false);
		}

		//If they're 1 apart, they're adjacent
		if (Mathf.Abs(c0.rank - c1.rank) == 1)
		{
			return (true);
		}

		//If one is Ace and the other is King, they're adjacent
		if (c0.rank == 1 && c1.rank == 13)
		{
			return (true);
		}

		//If one is Ace and the other is King, they're adjacent
		if (c0.rank == 13 && c1.rank == 1)
		{
			return (true);
		}

		//Otherwise, return false
		return (false);
	}

	//Handle FloatingScore movement
	void FloatingScoreHandler(eScoreEvent evt)
	{
		List<Vector2> fsPts;

		switch (evt)
		{
			//Same things need to happen whether it's a draw, a win, or a loss
			case eScoreEvent.draw: //Drawing a card
			case eScoreEvent.gameWin: //Won the round
			case eScoreEvent.gameLoss: //Lost the round

				//Add fsRun to the Scoreboard score
				if (fsRun != null)
				{
					//Create points for the Bezier curve
					fsPts = new List<Vector2>();
					fsPts.Add(fsPosRun);
					fsPts.Add(fsPosMid2);
					fsPts.Add(fsPosEnd);
					fsRun.reportFinishTo = Scoreboard02.S.gameObject;

					fsRun.Init(fsPts, 0, 1);

					//Also adjust the fontSize
					fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
					fsRun = null; //Clear fsRun so it's created again
				}

				break;

			case eScoreEvent.mine: //Remove a mine card

				//Create a FloatingScore02 for this score
				FloatingScore02 fs;

				//Move it from the mousePosition to fsPosRun
				Vector2 p0 = Input.mousePosition;
				p0.x /= Screen.width;
				p0.y /= Screen.height;
				fsPts = new List<Vector2>();
				fsPts.Add(p0);
				fsPts.Add(fsPosMid);
				fsPts.Add(fsPosRun);
				fs = Scoreboard02.S.CreateFloatingScore(ScoreManager02.CHAIN, fsPts);
				fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });

				if (fsRun == null)
				{
					fsRun = fs;
					fsRun.reportFinishTo = null;
				}

				else
				{
					fs.reportFinishTo = fsRun.gameObject;
				}

				break;
		}
	}
}