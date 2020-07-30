using UnityEngine;

public class MapSpriteSelector : MonoBehaviour {
	
	public Sprite 	spU, spD, spR, spL,
			spUD, spRL, spUR, spUL, spDR, spDL,
			spULD, spRUL, spDRU, spLDR, spUDRL;
	
	public bool up, down, left, right;
	
	public int type; 
	
	public Color normalColor, enterColor;
	
	private Color mainColor;
	private SpriteRenderer rend;
	
	void Start () 
	{
		rend = GetComponent<SpriteRenderer>();
		mainColor = normalColor;
		PickSprite();
		PickColor();
	}
	
	//Pick correct sprite based on the four door booleans
	void PickSprite()
	{ 
		if (up)
		{
			if (down)
			{
				if (right)
					rend.sprite = left ? spUDRL : spDRU;
				else if (left)
					rend.sprite = spULD;
				else
					rend.sprite = spUD;
			}
			else
			{
				if (right)
					rend.sprite = left ? spRUL : spUR;
				else if (left)
					rend.sprite = spUL;
				else
					rend.sprite = spU;
			}
			return;
		}
		
		if (down)
		{
			if (right)
				rend.sprite = left ? spLDR : spDR;
			else if (left)
				rend.sprite = spDL;
			else
				rend.sprite = spD;
			return;
		}
		
		if (right)
			rend.sprite = left ? spRL : spR;
		else
			rend.sprite = spL;
	}

	void PickColor()
	{ 
        if (type == 0) 
	        mainColor = normalColor;
        
        else if (type == 1)
	        mainColor = enterColor;
        
        else if (type == 2)
            mainColor = Color.red;
        
		rend.color = mainColor;
	}
}