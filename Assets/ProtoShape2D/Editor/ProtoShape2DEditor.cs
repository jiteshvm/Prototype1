using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using System;

[CustomEditor(typeof(ProtoShape2D))]
public class PS2DEditor:Editor{

	ProtoShape2D script;
	bool isDragging=false;

	//For snapping
	PS2DSnap snap=new PS2DSnap();

	double lastClickTime=0;

	SerializedProperty _color;

	Rect windowRect;

	[MenuItem("GameObject/2D Object/ProtoShape 2D")]
	static void Create(){
		GameObject go=new GameObject();
		go.AddComponent<ProtoShape2D>();
		go.GetComponent<ProtoShape2D>().SetSpriteMaterial(AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat"));
		go.name="ProtoShape 2D";
		SceneView sc=SceneView.lastActiveSceneView!=null?SceneView.lastActiveSceneView:SceneView.sceneViews[0] as SceneView;
		go.transform.position=new Vector3(sc.pivot.x,sc.pivot.y,0f);
		if(Selection.activeGameObject!=null) go.transform.parent=Selection.activeGameObject.transform;
		Selection.activeGameObject=go;
	}

	void Awake(){
		script=(ProtoShape2D)target;
		if(script.points.Count==0){
			SceneView sc=SceneView.lastActiveSceneView!=null?SceneView.lastActiveSceneView:SceneView.sceneViews[0] as SceneView;
			float size=sc.size/6f;
			AddPoint((new Vector2(-2f,1f)+(UnityEngine.Random.insideUnitCircle*0.1f))*size);
			AddPoint((new Vector2(2f,1f)+(UnityEngine.Random.insideUnitCircle*0.1f))*size);
			AddPoint((new Vector2(2f,-1f)+(UnityEngine.Random.insideUnitCircle*0.1f))*size);
			AddPoint((new Vector2(-2f,-1f)+(UnityEngine.Random.insideUnitCircle*0.1f))*size);
			script.color1=RandomColor();
			script.color2=RandomColor();
			DeselectAllPoints();
			UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(script.GetComponent<MeshFilter>(),false);
			UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(script.GetComponent<MeshRenderer>(),false);
		}
		//Get a reference to Unity's sprite material so we can use switch to it when needed
		if(script.spriteMaterial==null) script.spriteMaterial=AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
		SceneView.RepaintAll();
		script.UpdateMaterialSettings();
		script.UpdateMesh();
	}

	/*

		Inspector window
		
	*/

	public override void OnInspectorGUI(){
		//Force repaint on undo/redo
		bool forceRepaint=false;
		if(Event.current.type==EventType.ValidateCommand){
			if(Event.current.commandName=="UndoRedoPerformed") forceRepaint=true;
		}
		//Fill settings
		script.showFillSettings=EditorGUILayout.Foldout(script.showFillSettings,"Fill");
		if(script.showFillSettings){
			PS2DFillType fillType=(PS2DFillType)EditorGUILayout.EnumPopup(new GUIContent("Fill type","Which fill to use for this object. Single color is optimal for mobile games since it uses built-in sprite shader"),script.fillType);
			//If fill type changed
			if(fillType!=script.fillType){
				Undo.RecordObject(script,"Change fill type");
				//If setting changed to single color, we use Unity's built-in sprite material
				if(script.fillType!=PS2DFillType.Color && fillType==PS2DFillType.Color){
					script.SetSpriteMaterial();
				//If setting changed to custom material, we use a material provided by user
				}else if(script.fillType!=PS2DFillType.CustomMaterial && fillType==PS2DFillType.CustomMaterial){
					script.SetCustomMaterial();
				//Otherwise we use our own shader that supports gradient and texture
				}else if((script.fillType==PS2DFillType.CustomMaterial || script.fillType==PS2DFillType.Color) && fillType!=PS2DFillType.CustomMaterial){
					script.SetDefaultMaterial();
				}
				script.fillType=fillType;
				script.UpdateMaterialSettings();
				EditorUtility.SetDirty(script);
			}
			//Texture
			if(script.fillType==PS2DFillType.Texture || script.fillType==PS2DFillType.TextureWithColor || script.fillType==PS2DFillType.TextureWithGradient){
				Texture2D texture=(Texture2D)EditorGUILayout.ObjectField(new GUIContent("Texture","An image for tiling. Needs to have \"Wrap Mode\" property set to \"Repeat\""),script.texture,typeof(Texture2D),false);
				if(script.texture!=texture){
					Undo.RecordObject(script,"Change texture");
					script.texture=texture;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(texture);
				}
				float textureScale=EditorGUILayout.FloatField(new GUIContent("Texture scale","Change size of the texture"),script.textureScale);
				if(textureScale!=script.textureScale){
					Undo.RecordObject(script,"Change texture size");
					script.textureScale=textureScale;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
			}
			//Single color setup
			if(script.fillType==PS2DFillType.Color || script.fillType==PS2DFillType.TextureWithColor){
				Color color1=EditorGUILayout.ColorField(new GUIContent("Color","Color to fill the object with"),script.color1);
				if(script.color1!=color1){
					Undo.RecordObject(script,"Change color");
					script.color1=color1;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
			}
			//Two color setup
			if(script.fillType==PS2DFillType.Gradient || script.fillType==PS2DFillType.TextureWithGradient){
				Color gcolor1=EditorGUILayout.ColorField(new GUIContent("Color one","Top color for the gradient"),script.color1);
				if(script.color1!=gcolor1){
					Undo.RecordObject(script,"Change color one");
					script.color1=gcolor1;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
				Color gcolor2=EditorGUILayout.ColorField(new GUIContent("Color two","Bottom color for the gradient"),script.color2);
				if(script.color2!=gcolor2){
					Undo.RecordObject(script,"Change color two");
					script.color2=gcolor2;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
				float gradientScale=EditorGUILayout.Slider(new GUIContent("Gradient scale","Zoom gradient in and out relatively to height of the object."),script.gradientScale,0f,10f);
				if(gradientScale!=script.gradientScale){
					Undo.RecordObject(script,"Change gradient scale");
					script.gradientScale=gradientScale;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
				float gradientRotation=EditorGUILayout.Slider(new GUIContent("Gradient rotation","Set angle of rotation for gradient"),script.gradientRotation,-180f,180f);
				if(gradientRotation!=script.gradientRotation){
					Undo.RecordObject(script,"Change gradient rotation");
					script.gradientRotation=gradientRotation;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
				float gradientOffset=EditorGUILayout.Slider(new GUIContent("Gradient offset","Offset gradient up or down."),script.gradientOffset,-1f,1f);
				if(gradientOffset!=script.gradientOffset){
					Undo.RecordObject(script,"Change gradient offset");
					script.gradientOffset=gradientOffset;
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
			}
			//Custom material setup
			if(script.fillType==PS2DFillType.CustomMaterial){
				Material material=(Material)EditorGUILayout.ObjectField(new GUIContent("Custom material","If you provide same material for multiple objects, it will lower the number of DrawCalls therefor optimizing the rendering process."),script.material,typeof(Material),false);
				if(script.material!=material){
					Undo.RecordObject(script,"Change custom material");
					script.SetCustomMaterial(material);
					script.UpdateMaterialSettings();
					EditorUtility.SetDirty(script);
				}
			}
		}

		script.showMeshSetting=EditorGUILayout.Foldout(script.showMeshSetting,"Mesh");
		if(script.showMeshSetting){
			//Edit curve iterations
			int curveIterations=EditorGUILayout.IntSlider(new GUIContent("Curve iterations","How many points a curved line should have"),script.curveIterations,1,30);
			if(curveIterations!=script.curveIterations){
				Undo.RecordObject(script,"Change curve iterations");
				script.curveIterations=curveIterations;
				EditorUtility.SetDirty(script);
			}
			//"Anti-aliasing"
			bool antialias=EditorGUILayout.Toggle(new GUIContent("Anti-aliasing","Create an anti-aliasing effect by adding a thin transparent gradient outline to the mesh"),script.antialias);
			if(antialias!=script.antialias){
				Undo.RecordObject(script,"Change anti-aliasing");
				script.antialias=antialias;
				EditorUtility.SetDirty(script);
			}
			//Show triangle count
			GUILayout.Box(new GUIContent(script.triangleCount>1?"The mesh has "+script.triangleCount.ToString()+" triangles":"The mesh is just one triangle"),EditorStyles.helpBox);
		}

		script.showColliderSettings=EditorGUILayout.Foldout(script.showColliderSettings,"Collider");
		if(script.showColliderSettings){
			PS2DColliderType colliderType=(PS2DColliderType)EditorGUILayout.EnumPopup(new GUIContent("Auto collider 2D","Automatically create a collider. Set to \"None\" if you want to create your collider by hand"),script.colliderType);
			if(colliderType!=script.colliderType){
				if(RemoveCollider(colliderType)){
					Undo.RecordObject(script,"Change collider type");
					script.colliderType=colliderType;
					AddCollider();
					script.UpdateMesh();
					EditorUtility.SetDirty(script);
				}
				EditorGUIUtility.ExitGUI();
			}
			if(script.colliderType!=PS2DColliderType.None){
				float colliderTopAngle=EditorGUILayout.Slider(new GUIContent("Top edge arc","Decides which edges are considered to be facing up"),script.colliderTopAngle,1,180);
				if(colliderTopAngle!=script.colliderTopAngle){
					Undo.RecordObject(script,"Change top edge arc");
					script.colliderTopAngle=colliderTopAngle;
					EditorUtility.SetDirty(script);
				}
				float colliderOffsetTop=EditorGUILayout.Slider(new GUIContent("Offset top","Displace part of collider that is considered to be facing up"),script.colliderOffsetTop,-1,1);
				if(colliderOffsetTop!=script.colliderOffsetTop){
					Undo.RecordObject(script,"Change offset top");
					script.colliderOffsetTop=colliderOffsetTop;
					EditorUtility.SetDirty(script);
				}
				bool showNormals=EditorGUILayout.Toggle(new GUIContent("Show normals","Visually shows which edges are facing which side. Just to better understand how \"Top edge arc\" works"),script.showNormals);
				if(showNormals!=script.showNormals){
					Undo.RecordObject(script,"Show normals");
					script.showNormals=showNormals;
					EditorUtility.SetDirty(script);
				}
			}
		}

		script.showTools=EditorGUILayout.Foldout(script.showTools,"Tools");
		if(script.showTools){
			//Z-sorting
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(new GUIContent("Z-sorting","Adds and substracts 0.01 on Z axis for very basic sorting"));
			if(GUILayout.Button("Pull")){
				Undo.RecordObject(script,"Subtract 0.01 on Z axis");
				script.transform.position-=Vector3.forward*0.01f;
				EditorUtility.SetDirty(script);
			}
			if(GUILayout.Button("Push")){
				Undo.RecordObject(script,"Add 0.01 on Z axis");
				script.transform.position+=Vector3.forward*0.01f;
				EditorUtility.SetDirty(script);
			}
			EditorGUILayout.EndHorizontal();
			//Auto pivot
			PS2DPivotPositions pivotPosition=(PS2DPivotPositions)EditorGUILayout.EnumPopup(new GUIContent("Auto pivot","Automatically move center of the object. Achieved by rearranging everything aroung the new pivot."),script.PivotPosition);
			if(pivotPosition!=script.PivotPosition){
				script.PivotPosition=pivotPosition;
				MovePivot(script.PivotPosition);
			}
			//Manual pivot. Set pivot one time
			if(pivotPosition==PS2DPivotPositions.Disabled){
				GUIStyle pivotButtonStyle=new GUIStyle(GUI.skin.button);
				pivotButtonStyle.margin=new RectOffset(0,2,0,2);
				pivotButtonStyle.padding=new RectOffset(2,2,2,2);
				//Pivot adjusting
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(new GUIContent("Manual pivot","Move object's pivot to top, right, bottom, left or center of the object."));
				if(GUILayout.Button((Texture)Resources.Load("Icons/pivotTop"),pivotButtonStyle)){
					MovePivot(PS2DPivotPositions.Top);
				}
				if(GUILayout.Button((Texture)Resources.Load("Icons/pivotRight"),pivotButtonStyle)){
					MovePivot(PS2DPivotPositions.Right);
				}
				if(GUILayout.Button((Texture)Resources.Load("Icons/pivotBottom"),pivotButtonStyle)){
					MovePivot(PS2DPivotPositions.Bottom);
				}
				if(GUILayout.Button((Texture)Resources.Load("Icons/pivotLeft"),pivotButtonStyle)){
					MovePivot(PS2DPivotPositions.Left);
				}
				if(GUILayout.Button((Texture)Resources.Load("Icons/pivotCenter"),pivotButtonStyle)){
					MovePivot(PS2DPivotPositions.Center);
				}
				EditorGUILayout.EndHorizontal();
			}
			//Export
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(new GUIContent("Export","Export the shape to enother form of object"));
			if(GUILayout.Button("Mesh")){
				ExportMesh();
			}
			if(GUILayout.Button("PNG")){
				ExportPNG();
			}
			EditorGUILayout.EndHorizontal();
			//Sprite sorting
			GUILayout.Space(10);
			//Get sorting layers
			int[] layerIDs=GetSortingLayerUniqueIDs();
			string[] layerNames=GetSortingLayerNames();
			//Get selected sorting layer
			int selected=-1;
			for(int i=0;i<layerIDs.Length;i++){
				if(layerIDs[i]==script.sortingLayer){
					selected=i;
				}
			}
			//Select Default layer if no other is selected
			if(selected==-1){
				for(int i=0;i<layerIDs.Length;i++){
					if(layerIDs[i]==0){
						selected=i;
					}
				}
			}
			//Sorting layer dropdown
			EditorGUI.BeginChangeCheck();
			GUIContent[] dropdown=new GUIContent[layerNames.Length+2];
			for(int i=0;i<layerNames.Length;i++){
				dropdown[i]=new GUIContent(layerNames[i]);
			}
			dropdown[layerNames.Length]=new GUIContent();
			dropdown[layerNames.Length+1]=new GUIContent("Add Sorting Layer...");
			selected=EditorGUILayout.Popup(new GUIContent("Sorting Layer","Name of the Renderer's sorting layer"),selected,dropdown);
			if(EditorGUI.EndChangeCheck()){
				Undo.RecordObject(script,"Change sorting layer");
				if(selected==layerNames.Length+1){
					EditorApplication.ExecuteMenuItem("Edit/Project Settings/Tags and Layers");
				}else{
					script.sortingLayer=layerIDs[selected];
				}
				EditorUtility.SetDirty(script);
			}
			//Order in layer field
			EditorGUI.BeginChangeCheck();
			int order=EditorGUILayout.IntField(new GUIContent("Order in Layer","Renderer's order within a sorting layer"),script.orderInLayer);
			if(EditorGUI.EndChangeCheck()){
				Undo.RecordObject(script,"Change order in layer");
				script.orderInLayer=order;
				EditorUtility.SetDirty(script);
			}

		}

		//React to changes in GUI
		if(GUI.changed || forceRepaint){
			script.UpdateMesh();
			SceneView.RepaintAll();
		}
	}

	void ExportMesh(){
		script.UpdateMesh();
		Mesh mesh=script.GetMesh();
		if(System.IO.File.Exists("Assets/"+mesh.name.ToString()+".asset") && !EditorUtility.DisplayDialog("Warning","Asset with this name already exists in root of your project.","Overwrite","Cancel")){
			return;
		}
		AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(mesh),"Assets/"+mesh.name.ToString()+".asset");
		AssetDatabase.SaveAssets();
	}

	void ExportPNG(){
		script.UpdateMesh();
		//Move current object to the root of the scene
		Transform sparent=script.transform.parent;
		script.transform.parent=null;
		//Disable all root game objects except the current one and main camera
		GameObject[] rootList=UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
		List<GameObject> disableList=new List<GameObject>(50);
		for(int i=0;i<rootList.Length;i++){
			if(rootList[i].activeSelf && rootList[i]!=script.gameObject){
				disableList.Add(rootList[i]);
				rootList[i].SetActive(false);
			}
		}
		//Create the temporary camera
		GameObject scameraGO=new GameObject();
		scameraGO.name="Screenshot Camera";
		Camera scamera=scameraGO.AddComponent<Camera>();
		scamera.cameraType=CameraType.Game;
		scamera.orthographic=true;
		scamera.enabled=false;
		scamera.clearFlags=CameraClearFlags.Color;
		scamera.backgroundColor=Color.clear;
		//Center camera on the object and set the size
		Bounds meshBounds=script.GetComponent<MeshRenderer>().bounds;
		scameraGO.transform.position=meshBounds.center+Vector3.back;
		scamera.orthographicSize=Mathf.Max(meshBounds.size.x/2,meshBounds.size.y);
		//Crete render texture with antialiasing
		RenderTexture rt=new RenderTexture((int)(meshBounds.size.x*200),(int)(meshBounds.size.y*200),0);
		rt.antiAliasing=8;
		rt.autoGenerateMips=false;
		scamera.targetTexture=rt;
		RenderTexture.active=scamera.targetTexture;
		//Take a rendered shot from a newly set up othographic camera
		scamera.Render();
		Texture2D image=new Texture2D(scamera.targetTexture.width,scamera.targetTexture.height,TextureFormat.RGBA32,false);
		image.ReadPixels(new Rect(0,0,scamera.targetTexture.width,scamera.targetTexture.height),0,0);
		////////File.WriteAllBytes(Application.dataPath + "/_TEST.png",image.EncodeToPNG());
		Color pixel;
		int[] cropRect=new int[]{0,0,scamera.targetTexture.width,scamera.targetTexture.height};
		bool[] cropSet=new bool[]{false,false,false,false};
		//Find all the transparent area we can crop
		for(int x=0;x<image.width;x++){
			for(int y=0;y<image.height;y++){
				pixel=image.GetPixel(x,y);
				if(pixel.r!=0 && pixel.g!=0 && pixel.b!=0 && pixel.a!=0){
					if(!cropSet[0] || x<cropRect[0]){
						cropRect[0]=x;
						cropSet[0]=true;
					}
					if(!cropSet[1] || y<cropRect[1]){
						cropRect[1]=y;
						cropSet[1]=true;
					}
					if(cropSet[0] && (!cropSet[2] || x+1>cropRect[2])){
						cropRect[2]=x+1;
						cropSet[2]=true;
					}
					if(cropSet[1] && (!cropSet[3] || y+1>cropRect[3])){
						cropRect[3]=y+1;
						cropSet[3]=true;
					}
				}
			}
		}
		//Crop out all the transparent area
		Texture2D cropImage=new Texture2D(cropRect[2]-cropRect[0],cropRect[3]-cropRect[1],TextureFormat.RGBA32,false);
		cropImage.SetPixels(image.GetPixels(cropRect[0],cropRect[1],cropRect[2]-cropRect[0],cropRect[3]-cropRect[1]));
		//Come up with a unique name for an image
		string filename;
		int iterator=0;
		do{
			filename=Application.dataPath + "/"+script.name+(iterator>0?" ("+iterator.ToString()+")":"")+".png";
			iterator++;
		}while(File.Exists(filename));
		//Save the image to PNG
		File.WriteAllBytes(filename,cropImage.EncodeToPNG());
		//Return thing to their original state
		RenderTexture.active=null;
		DestroyImmediate(scameraGO);
		AssetDatabase.Refresh();
		//Enable the objects we disabled previously
		for(int i=0;i<disableList.Count;i++){
			disableList[i].SetActive(true);
		}
		//Return object to its original parent
		script.transform.parent=sparent;
	}

	bool RemoveCollider(PS2DColliderType nextType){
		bool ok=true;
		if(script.GetComponent<Collider2D>()!=null){
			if(nextType==PS2DColliderType.None){
				ok=EditorUtility.DisplayDialog("Warning","This will remove existing Collider2D with all its settings.","Remove","Keep existing collider");
			}else{
				ok=EditorUtility.DisplayDialog("Warning","This will remove existing Collider2D with all its settings and add new Collider2D in its place.","Overwrite","Keep existing collider");
			}
			if(ok){
				while(script.GetComponent<Collider2D>()!=null){
					DestroyImmediate(script.GetComponent<Collider2D>());
				}
				while(script.GetComponent<PlatformEffector2D>()!=null){
					DestroyImmediate(script.GetComponent<PlatformEffector2D>());
				}
				while(script.GetComponent<Rigidbody2D>()!=null){
					DestroyImmediate(script.GetComponent<Rigidbody2D>());
				}
			}
		}
		return ok;
	}

	void AddCollider(){
		if(script.colliderType==PS2DColliderType.PolygonStatic){
			script.gameObject.AddComponent<Rigidbody2D>();
			script.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;
			script.gameObject.AddComponent<PolygonCollider2D>();
		}else if(script.colliderType==PS2DColliderType.PolygonDynamic){
			script.gameObject.AddComponent<Rigidbody2D>();
			script.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Dynamic;
			script.gameObject.AddComponent<PolygonCollider2D>();
		}else if(script.colliderType==PS2DColliderType.Edge){
			script.gameObject.AddComponent<Rigidbody2D>();
			script.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;
			script.gameObject.AddComponent<EdgeCollider2D>();
		}else if(script.colliderType==PS2DColliderType.TopEdge){
			script.gameObject.AddComponent<Rigidbody2D>();
			script.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;
			script.gameObject.AddComponent<EdgeCollider2D>();
			script.GetComponent<EdgeCollider2D>().usedByEffector=true;
			script.gameObject.AddComponent<PlatformEffector2D>();
			script.GetComponent<PlatformEffector2D>().surfaceArc=90f;
		}
		UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(script.GetComponent<PolygonCollider2D>(),false);
		UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(script.GetComponent<EdgeCollider2D>(),false);
		UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(script.GetComponent<Rigidbody2D>(),false);
	}

	/*

		Scene GUI

	*/

	void OnSceneGUI(){
		Tools.pivotMode=PivotMode.Pivot;
		EventType et=Event.current.type; //Need to save this because it can be changed to Used by other functions
		//If current tool is none, we're probably in collider edit mode
		if(Tools.current!=Tool.None){
			//Draw outline
			DrawLines();
			//Deselect all points on ESC
			if(et==EventType.keyDown){
				if(Event.current.keyCode==KeyCode.Escape){
					DeselectAllPoints();
					SceneView.RepaintAll();
				}
			}
			//When CTRL is pressed, draw only deleteable points
			if(Event.current.control){
				if(script.points.Count>3){
					for(int i=0;i<script.points.Count;i++){
						DrawDeletePoint(i);
					}
				}
			}else{
				//Draw draggable points
				for(int i=0;i<script.points.Count;i++){
					DrawPoint(i);
				}
				//Draw a marker to add new points, 
				//but only if no points are being dragged already
				if(!isDragging) DrawAddPointMarker();
				//Draw collider lines
				DrawCollider();
				//Draw properties of selected points
				DrawPointsProperties();
			}
		}else{
			DeselectAllPoints();
		}
		SceneView.RepaintAll();
	}

	void DrawLines(){
		//Draw the final, curved lines
		Handles.color=new Color(1f,1f,1f,0.6f);
		for(int i=0;i<script.pointsFinal.Count;i++){
			Handles.DrawLine(
				script.transform.TransformPoint(script.pointsFinal[i]),
				script.transform.TransformPoint(script.pointsFinal.Loop(i+1))
			);
		}
		Handles.color=Color.white;
		/*
		//Draw the straight lines that connect the points
		Handles.color=new Color(1f,1f,1f,0.1f);
		for(int i=0;i<script.points.Count;i++){
			Handles.DrawLine(
				script.transform.TransformPoint(script.points[i].position),
				script.transform.TransformPoint(script.points.Loop(i+1).position)
			);
		}
		Handles.color=Color.white;
		*/
		/*
		//Vidualize the bezeier handles
		float size=HandleUtility.GetHandleSize(script.transform.position)*0.05f;
		for(int i=0;i<script.points.Count;i++){
			Handles.color=Color.red;
			Handles.DrawLine(script.transform.TransformPoint(script.points[i].position),script.transform.TransformPoint(script.points[i].handleP));
			Handles.DrawSolidDisc(script.transform.TransformPoint(script.points[i].handleP),Vector3.back,size);
			Handles.color=Color.green;
			Handles.DrawLine(script.transform.TransformPoint(script.points[i].position),script.transform.TransformPoint(script.points[i].handleN));
			Handles.DrawSolidDisc(script.transform.TransformPoint(script.points[i].handleN),Vector3.back,size);
			Handles.color=Color.white;
		}
		*/
	}

	void DrawDeletePoint(int i){
		Plane oPlane=new Plane(script.transform.TransformPoint(new Vector3(0,0,0)),script.transform.TransformPoint(new Vector3(0,1,0)),script.transform.TransformPoint(new Vector3(1,0,0)));
		float size=HandleUtility.GetHandleSize(script.points[i].position)*0.1f;
		Handles.DrawWireDisc(script.transform.TransformPoint(script.points[i].position),oPlane.normal,size);
		Handles.DrawLine(
			script.transform.TransformPoint(script.points[i].position+((Vector2.up+Vector2.left)*(size*0.5f))),
			script.transform.TransformPoint(script.points[i].position+((Vector2.down+Vector2.right)*(size*0.5f)))
		);
		Handles.DrawLine(
			script.transform.TransformPoint(script.points[i].position+((Vector2.up+Vector2.right)*(size*0.5f))),
			script.transform.TransformPoint(script.points[i].position+((Vector2.down+Vector2.left)*(size*0.5f)))
		);
		if(Handles.Button(script.transform.TransformPoint(script.points[i].position),Quaternion.identity,0,size,Handles.CircleHandleCap)){
			Undo.RecordObject(script,"Delete point");
			DeletePoint(i);
			script.UpdateMesh();
			EditorUtility.SetDirty(script);
			MovePivot(script.PivotPosition);
		}
	}

	void DrawPoint(int i){
		Handles.color=Color.white;
		EventType et=Event.current.type;
		float size=HandleUtility.GetHandleSize(script.points[i].position)*0.1f;
		//Plane of the object
		Plane oPlane=new Plane(script.transform.TransformPoint(new Vector3(0,0,0)),script.transform.TransformPoint(new Vector3(0,1,0)),script.transform.TransformPoint(new Vector3(1,0,0)));
		//Draw a curve indicator circle
		if(script.points[i].curve>0f){
			if(script.points[i].selected==true) Handles.color=new Color(1,1,0,0.7f);
			else Handles.color=new Color(1,1,0,0.3f);
			Handles.DrawWireDisc(script.transform.TransformPoint(script.points[i].position),oPlane.normal,size*1.2f+(size*2f)*script.points[i].curve);
			Handles.color=Color.white;
		}
		//Circle around drag point
		Handles.color=new Color(1,1,1,0.5f);
		Handles.DrawWireDisc(script.transform.TransformPoint(script.points[i].position),oPlane.normal,size);
		//Drag point
		EditorGUI.BeginChangeCheck();
		GUI.SetNextControlName(script.points[i].name);
		Handles.color=Color.clear;
		Vector3 point=Handles.FreeMoveHandle(
			script.transform.TransformPoint(script.points[i].position),
			script.transform.rotation,
			size,
			Vector3.zero,
			Handles.CircleHandleCap
		);
		Handles.color=Color.white;

		//Snapping
		if(script.points[i].selected && isDragging && Event.current.shift){
			snap.Reset(size);
			for(int j=0;j<script.points.Count;j++){
				if(j==i) continue; //Don't snap to itself
				snap.CheckPoint(j,point,script.transform.TransformPoint(script.points[j].position));
			}
			if(snap.GetClosestAxes()>0){
				point=snap.snapLocation;
				Vector3 ab;
				if(snap.snapPoint1>-1){
					ab=((Vector3)snap.snapLocation-script.transform.TransformPoint(script.points[snap.snapPoint1].position)).normalized*(size*8);
					Handles.DrawLine(
						(Vector3)snap.snapLocation+ab,
						script.transform.TransformPoint(script.points[snap.snapPoint1].position)-ab
					);
				}
				if(snap.snapPoint2>-1){
					ab=((Vector3)snap.snapLocation-script.transform.TransformPoint(script.points[snap.snapPoint2].position)).normalized*(size*8);
					Handles.DrawLine(
						(Vector3)script.transform.TransformPoint(script.points[i].position)+ab,
						script.transform.TransformPoint(script.points[snap.snapPoint2].position)-ab
					);
				}
			}
		}

		//Actual dragging
		if(EditorGUI.EndChangeCheck() || (isDragging && Event.current.shift)){
			Undo.RecordObject(script,"Move point");
			if(!isDragging) isDragging=true;
			script.points[i].position=script.transform.InverseTransformPoint(point);
			script.UpdateMesh();
		}
		//Detect double click on current point and invert the bezier handles
		if(et==EventType.mouseDown && GUI.GetNameOfFocusedControl()==script.points[i].name){
			if(EditorApplication.timeSinceStartup-lastClickTime<0.3f){
				script.points[i].median*=-1;
				script.UpdateMesh();
				lastClickTime=0f;
			}else{
				lastClickTime=EditorApplication.timeSinceStartup;
			}
		}
		//Select point on mouse down
		if(et==EventType.mouseDown && GUI.GetNameOfFocusedControl()==script.points[i].name){
			DeselectAllPoints();
			SelectPoint(i,true);
			GUI.FocusControl(null);
		//Deselect all points when clicking on shape
		}else if(et==EventType.mouseDown && GUI.GetNameOfFocusedControl()==""){
			DeselectAllPoints();
		}
		//If point is selected, draw a circle
		if(script.points[i].selected==true){
			Handles.DrawWireDisc(point,oPlane.normal,size);
			Handles.DrawSolidDisc(script.transform.TransformPoint(script.points[i].position),oPlane.normal,size*0.75f);
		}
		//React to drag stop
		if(isDragging && et==EventType.mouseUp){
			isDragging=false;
			EditorUtility.SetDirty(script);
			MovePivot(script.PivotPosition);
		}
	}


	void DrawAddPointMarker(){
		float size=HandleUtility.GetHandleSize(script.transform.position)*0.05f;
		//Get position of cursor in the world
		//The cursor is the point where mouse ray intersects with object's plane
		Plane oPlane=new Plane(script.transform.TransformPoint(new Vector3(0,0,0)),script.transform.TransformPoint(new Vector3(0,1,0)),script.transform.TransformPoint(new Vector3(1,0,0)));
		Ray mRay=HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		float mRayDist;
		if(oPlane.Raycast(mRay,out mRayDist)){
			Vector3 cursor=mRay.GetPoint(mRayDist);
			//Get closest line
			Vector2 cursorIn=script.transform.InverseTransformPoint(cursor);
			Vector3 marker=Vector3.zero;
			Vector3 newMarker=Vector3.zero;
			int markerPoint=-1;
			for(int i=0;i<script.pointsFinal.Count;i++){
				//Get point where perpendicular meets the line
				newMarker=GetBasePoint(
					script.pointsFinal[i],
					script.pointsFinal.Loop(i+1),
					cursorIn
				);
				//If perpendicular doesn't meet the line, take closest end of the line
				if(newMarker==Vector3.zero){
					if(Vector2.Distance(cursorIn,script.pointsFinal[i])<Vector2.Distance(cursorIn,script.pointsFinal.Loop(i+1))){
						newMarker=script.pointsFinal[i];
					}else{
						newMarker=script.pointsFinal.Loop(i+1);
					}
				}
				//Save shortest marker distance
				if(marker==Vector3.zero || Vector3.Distance(cursorIn,newMarker)<Vector3.Distance(cursorIn,marker)){
					markerPoint=i;
					marker=newMarker;
				}
			}
			//Check if cursor is not too close to the point handle
			bool tooclose=false;
			for(int i=0;i<script.points.Count;i++){
				if(Vector3.Distance(script.points[i].position,marker)<size*5){
					tooclose=true;
					break;
				}
			}
			if(!tooclose && Vector3.Distance(cursorIn,marker)<size*6){
				marker=script.transform.TransformPoint(marker);
				Handles.color=Color.green;
				Handles.DrawSolidDisc(marker,oPlane.normal,size);
				if(Handles.Button(marker,Quaternion.identity,0,size*2,Handles.CircleHandleCap)){
					DeselectAllPoints();
					Undo.RecordObject(script,"Add point");
					//Find after which point we should add a new one by iterating through them
					int pointSum=0;
					int pointAfter=-1;
					for(int i=0;i<script.points.Count;i++){
						if(script.points[i].curve>0f || script.points.Loop(i+1).curve>0f){
							pointSum+=script.curveIterations;
						}else{
							pointSum++;
						}
						if(markerPoint<pointSum){
							pointAfter=i;
							break;
						}
					}
					AddPoint(script.transform.InverseTransformPoint(marker),pointAfter);
					SelectPoint(pointAfter+1,true);
					EditorUtility.SetDirty(script);
					script.UpdateMesh();
				}
				Handles.color=Color.white;
			}
		}
	}

	void DrawCollider(){
		if(script.colliderType!=PS2DColliderType.None){
			float size=HandleUtility.GetHandleSize(script.transform.position)*0.05f;
			Handles.color=Color.green;
			for(int i=0;i<script.cpointsFinal.Length;i++){
				if(i==0 && script.colliderType==PS2DColliderType.TopEdge) continue;
				Handles.DrawLine(
					script.transform.TransformPoint(script.cpointsFinal.Loop(i-1)),
					script.transform.TransformPoint(script.cpointsFinal[i])
				);
			}
			Handles.color=Color.white;
			//Debug edge normals
			if(script.showNormals){
				for(int i=0;i<script.cpoints.Count;i++){
					if(script.cpoints[i].direction==PS2DDirection.Up) Handles.color=Color.green;
					if(script.cpoints[i].direction==PS2DDirection.Right) Handles.color=Color.magenta;
					if(script.cpoints[i].direction==PS2DDirection.Left) Handles.color=Color.blue;
					if(script.cpoints[i].direction==PS2DDirection.Down) Handles.color=Color.yellow;
					Handles.DrawLine(
						(Vector2)script.transform.TransformPoint(Vector2.Lerp(script.cpoints[i].position,script.cpoints.Loop(i+1).position,0.5f)+(Vector2)script.cpoints[i].normal*(size*1)),
						(Vector2)script.transform.TransformPoint(Vector2.Lerp(script.cpoints[i].position,script.cpoints.Loop(i+1).position,0.5f)+(Vector2)script.cpoints[i].normal*(size*7))
					);
				}
				Handles.color=Color.white;
			}
		}
	}

	void DrawPointsProperties(){
		EventType et=Event.current.type;
		string selected="";
		int selPoint=-1;
		for(int i=0;i<script.points.Count;i++){
			if(script.points[i].selected){
				if(selected.Length>0) selected+=",";
				selected+=" "+i.ToString();
				selPoint=i;
			}
		}
		//EditorWindow
		if(selPoint>-1 && et!=EventType.repaint){
			windowRect=new Rect(Screen.width-200,Screen.height-87,190,60);
			int cid=GUIUtility.GetControlID(FocusType.Passive);
			GUILayout.Window(
				cid,
				windowRect,
				(id)=>{
					//Working with temporary vars for undo
					Vector2 pos=script.points[selPoint].position;
					float curve=script.points[selPoint].curve;
					//Define the window
					EditorGUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth=20;
					pos.x=EditorGUILayout.FloatField("X",pos.x,GUILayout.Width(88));
					pos.y=EditorGUILayout.FloatField("Y",pos.y,GUILayout.Width(88));
					EditorGUILayout.EndHorizontal();
					curve=EditorGUILayout.Slider(curve,0f,1f);
					//React to change
					if(GUI.changed){
						Undo.RecordObject(script,"Add point");
						script.points[selPoint].position=pos;
						script.points[selPoint].curve=Mathf.Round(curve*100f)/100;
						script.UpdateMesh();
						EditorUtility.SetDirty(script);
					}
					//Make window dragable but don't actually allow to drag it
					//It's a hack so window wouldn't disappear on click
					if(Event.current.type!=EventType.MouseDrag) GUI.DragWindow();
				},
				"Point"+selected+" properties",
				GUILayout.MinWidth(windowRect.width),
				GUILayout.MaxWidth(windowRect.width),
				GUILayout.MinHeight(windowRect.height),
				GUILayout.MaxHeight(windowRect.height)
			);
			GUI.FocusWindow(cid);
		}
	}

	/*
		Add, delete, select, deselect points
	*/

	void AddPoint(Vector2 pos,int after=-1){
		if(after==-1 || after==script.points.Count-1){
			after=script.points.Count-1;
			script.points.Add(new PS2DPoint(pos));
		}else{
			script.points.Insert(after+1,new PS2DPoint(pos));
		}
		//Give all points new names
		for(int i=0;i<script.points.Count;i++){
			script.points[i].name="point"+script.uniqueName+"_"+i.ToString();
		}
		//Set curve value based on neghboring points
		script.points.Loop(after+1).curve=Mathf.Lerp(script.points.Loop(after).curve,script.points.Loop(after+2).curve,0.5f)*0.5f;
	}

	void DeletePoint(int at){
		script.points.RemoveAt(at);
	}

	void DeselectAllPoints(){
		for(int i=0;i<script.points.Count;i++){
			script.points[i].selected=false;
		}
		Repaint();
	}

	void SelectPoint(int i,bool state){
		if(script.points[i].selected!=state){
			script.points[i].selected=state;
			Repaint();
		}
	}

	//Move pivot of the object
	//To do this, we rearrange all points around the new pivot and then just move the object
	private void MovePivot(PS2DPivotPositions newPivotPosition){
		if(newPivotPosition!=PS2DPivotPositions.Disabled){
			//Get min and max positions
			Vector2 min=Vector2.one*9999f;
			Vector2 max=-Vector2.one*9999f;
			for(int i=0;i<script.pointsFinal.Count;i++){
				if(script.pointsFinal[i].x<min.x) min.x=script.pointsFinal[i].x;
				if(script.pointsFinal[i].y<min.y) min.y=script.pointsFinal[i].y;
				if(script.pointsFinal[i].x>max.x) max.x=script.pointsFinal[i].x;
				if(script.pointsFinal[i].y>max.y) max.y=script.pointsFinal[i].y;
			}
			//Calculate the difference
			Vector2 projectedPivot=new Vector2();
			if(newPivotPosition==PS2DPivotPositions.Center) projectedPivot=Vector2.Lerp(min,max,0.5f);
			if(newPivotPosition==PS2DPivotPositions.Top) projectedPivot=new Vector2(Mathf.Lerp(min.x,max.x,0.5f),max.y);
			if(newPivotPosition==PS2DPivotPositions.Right) projectedPivot=new Vector2(max.x,Mathf.Lerp(min.y,max.y,0.5f));
			if(newPivotPosition==PS2DPivotPositions.Bottom) projectedPivot=new Vector2(Mathf.Lerp(min.x,max.x,0.5f),min.y);
			if(newPivotPosition==PS2DPivotPositions.Left) projectedPivot=new Vector2(min.x,Mathf.Lerp(min.y,max.y,0.5f));
			//Difference between projected and real pivots converted to lcoal scale
			Vector2 diff=(Vector2)projectedPivot-(Vector2)script.transform.InverseTransformPoint((Vector2)script.transform.position);
			//To record full state we need to use RegisterFullObjectHierarchyUndo
			Undo.RegisterFullObjectHierarchyUndo(script,"Moving pivot");
			//Use it to move the points and children
			for(int i=0;i<script.points.Count;i++){
				script.points[i].position-=diff;
			}
			if(script.transform.childCount>0){
				for(int i=0;i<script.transform.childCount;i++){
					script.transform.GetChild(i).transform.localPosition-=(Vector3)diff;
				}
			}
			//Convert projected pivot to world coordinates and move object to it
			Vector2 projectedPivotWorld=script.transform.TransformPoint(projectedPivot);
			script.transform.position=new Vector3(projectedPivotWorld.x,projectedPivotWorld.y,script.transform.position.z);
			script.UpdateMesh();
			//For undo
			EditorUtility.SetDirty(script);
		}
	}

	//This gets position of that green cursor for creating points
	private Vector2 GetBasePoint(Vector2 b1,Vector2 b2, Vector2 t,float sizeCap=0f){
		float d1=Vector2.Distance(b1,t);
		float d2=Vector2.Distance(b2,t);
		float db=Vector2.Distance(b1,b2);
		//Find one of the angles
		float angle1=Mathf.Acos((Mathf.Pow(d1,2)+Mathf.Pow(db,2)-Mathf.Pow(d2,2))/(2*d1*db));
		//Find distance to point
		float dist=Mathf.Cos(angle1)*d1;
		//Make sure it's within the line
		if(dist<sizeCap || dist>db-sizeCap) return Vector2.zero;
		else return (b1+(dist*(b2-b1).normalized));
	}

	//Generate a random mild color
	private Color RandomColor(){
		float hue=UnityEngine.Random.Range(0f,1f);
		while(hue*360f>=236f && hue*360f<=246f){
			hue=UnityEngine.Random.Range(0f,1f);
		}
		return Color.HSVToRGB(hue,UnityEngine.Random.Range(0.2f,0.7f),UnityEngine.Random.Range(0.8f,1f));
	}

	//Get the sorting layer IDs
	public int[] GetSortingLayerUniqueIDs() {
		Type internalEditorUtilityType=typeof(InternalEditorUtility);
		PropertyInfo sortingLayerUniqueIDsProperty=internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs",BindingFlags.Static|BindingFlags.NonPublic);
		return (int[])sortingLayerUniqueIDsProperty.GetValue(null,new object[0]);
	}

	//Get the sorting layer names
	public string[] GetSortingLayerNames(){
		Type internalEditorUtilityType=typeof(InternalEditorUtility);
		PropertyInfo sortingLayersProperty=internalEditorUtilityType.GetProperty("sortingLayerNames",BindingFlags.Static|BindingFlags.NonPublic);
		return (string[])sortingLayersProperty.GetValue(null,new object[0]);
	}

	/*

		On destroy

	*/

	public void OnDestroy(){
		if(script==null){
			//Destroy material if it's created by the object
			if(script.fillType!=PS2DFillType.Color && script.fillType!=PS2DFillType.CustomMaterial){
				DestroyImmediate(script.material);
			}
		}
	}

}
