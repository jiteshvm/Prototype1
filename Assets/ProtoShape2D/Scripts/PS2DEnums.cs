[System.Serializable]
public enum PS2DPivotPositions{
	Disabled,
	Center,
	Top,
	Right,
	Bottom,
	Left
}

[System.Serializable]
public enum PS2DFillType{
	Color,
	Gradient,
	Texture,
	TextureWithColor,
	TextureWithGradient,
	CustomMaterial
}

[System.Serializable]
public enum PS2DColliderType{
	None,
	PolygonStatic,
	PolygonDynamic,
	Edge,
	TopEdge
}

[System.Serializable]
public enum PS2DDirection{
	Up,
	Right,
	Down,
	Left
}