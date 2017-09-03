using UnityEngine;

[System.Serializable]
public class PS2DPoint:System.Object{
	public Vector2 position=Vector2.zero;
	public float curve=0f;
	public string name="";
	public bool selected=false;
	public bool clockwise=true;
	public Vector2 median=Vector2.zero;
	public Vector2 handleP=Vector2.zero;
	public Vector2 handleN=Vector2.zero;
	public PS2DPoint(Vector2 position,string name=""){
		this.position=position;
		this.name=name;
	}
}
