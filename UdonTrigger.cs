using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System;
using UnityEditor;
using UdonSharpEditor;
using UnityEditorInternal;
using System.Linq;
#endif

public class UdonTrigger : UdonSharpBehaviour
{
	private int[] _eventType;
	private int[] _layerFlags;
	private bool[] _triggerIndividuals;
	private int[][] _actionTypes;
	//private GameObject[][] actionObjectTargets;

	void Start() {
		Initialize();
	}

	public void Initialize() {
		if(_eventType == null) _eventType = new int[0];
		if(_layerFlags == null) _layerFlags = new int[0];
		if(_triggerIndividuals == null) _triggerIndividuals = new bool[0];
		if(_actionTypes == null) _actionTypes = new int[0][];
	}

	public int GetEventCount() {
		return _eventType.Length;
	}

	public int GetEventType(int eventIndex) {
		return _eventType[eventIndex];
	}

	public void SetEventType(int eventIndex, int eventType) {
		_eventType[eventIndex] = eventType;
	}

	public int GetEventLayerFlags(int eventIndex) {
		return _layerFlags[eventIndex];
	}

	public void SetEventLayerFlags(int eventIndex, int eventLayerFlags) {
		_layerFlags[eventIndex] = eventLayerFlags;
	}

	public bool GetEventTriggerIndividuals(int eventIndex) {
		return _triggerIndividuals[eventIndex];
	}

	public void SetEventTriggerIndividuals(int eventIndex, bool eventTriggerIndividuals) {
		_triggerIndividuals[eventIndex] = eventTriggerIndividuals;
	}

	public int GetActionCount(int eventIndex) {
		return _actionTypes[eventIndex].Length;
	}

	public int GetActionType(int eventIndex, int actionIndex) {
		return _actionTypes[eventIndex][actionIndex];
	}
#if !COMPILER_UDONSHARP && UNITY_EDITOR 
	public void RemoveEvent(int eventIndex) {
		_eventType = _eventType.Where((o, i) => i != eventIndex).ToArray();
		_layerFlags = _layerFlags.Where((o, i) => i != eventIndex).ToArray();
		_triggerIndividuals = _triggerIndividuals.Where((o, i) => i != eventIndex).ToArray();
		_actionTypes = _actionTypes.Where((o, i) => i != eventIndex).ToArray();
	}

	public int AddEvent(int selectedEventType) {
		int newCount = GetEventCount() + 1;
		int eventIndex = newCount - 1;

		Array.Resize(ref _eventType, newCount);
		Array.Resize(ref _layerFlags, newCount);
		Array.Resize(ref _triggerIndividuals, newCount);
		Array.Resize(ref _actionTypes, newCount);

		_eventType[eventIndex] = selectedEventType;
		_layerFlags[eventIndex] = 0;
		_triggerIndividuals[eventIndex] = false;
		_actionTypes[eventIndex] = new int[0];

		return eventIndex;
	}

	public int CloneEvent(int eventIndex) {
		int newEventIndex = AddEvent(_eventType[eventIndex]);
		_eventType[newEventIndex] = _eventType[eventIndex];
		_layerFlags[newEventIndex] = _layerFlags[eventIndex];
		_triggerIndividuals[newEventIndex] = _triggerIndividuals[eventIndex];
		_actionTypes[newEventIndex] = _actionTypes[eventIndex].ToArray();

		return newEventIndex;
	}

	public int MoveUpEvent(int eventIndex) {
		if(GetEventCount() <= 1 || eventIndex <= 0) return eventIndex;
		return SwapEvent(eventIndex, eventIndex - 1);
	}

	public int MoveDownEvent(int eventIndex) {
		if(GetEventCount() <= 1 || (eventIndex + 1) <= 0 || (eventIndex + 1) >= GetEventCount()) return eventIndex;
		return SwapEvent(eventIndex, eventIndex + 1);
	}

	public int SwapEvent(int eventIndex, int newEventIndex) {
		int eventType = _eventType[eventIndex];
		_eventType[eventIndex] = _eventType[newEventIndex];
		_eventType[newEventIndex] = eventType;

		int layerFlags = _layerFlags[eventIndex];
		_layerFlags[eventIndex] = _layerFlags[newEventIndex];
		_layerFlags[newEventIndex] = layerFlags;

		bool triggerIndividuals = _triggerIndividuals[eventIndex];
		_triggerIndividuals[eventIndex] = _triggerIndividuals[newEventIndex];
		_triggerIndividuals[newEventIndex] = triggerIndividuals;

		int[] actionTypes = _actionTypes[eventIndex];
		_actionTypes[eventIndex] = _actionTypes[newEventIndex];
		_actionTypes[newEventIndex] = actionTypes;

		return newEventIndex;
	}
#endif
}

#if !COMPILER_UDONSHARP && UNITY_EDITOR 
[CustomEditor(typeof(UdonTrigger))]
public class UdonTriggerInspectorEditor : Editor {
	private string[] triggerEventTypes = { "OnEnterTrigger", "OnExitTrigger" };
	private int selectedTriggerEventType = 0;
	private int layerFlags = 0;
	private bool triggerIndividuals = false;
	private int expandedEvent = -1;
	private bool expandAllEvents = false;

	public override void OnInspectorGUI() {
		if(UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

		UdonTrigger trigger = (UdonTrigger)target;

		trigger.Initialize();

		// Add Event Panel
		Rect r1 = (Rect)EditorGUILayout.BeginVertical();
		GUI.Box(r1, "");
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		GUILayout.Space(10);


			selectedTriggerEventType = EditorGUILayout.Popup(selectedTriggerEventType, triggerEventTypes);
			if(GUILayout.Button("+")) {
				trigger.AddEvent(selectedTriggerEventType);
				expandedEvent = trigger.GetEventCount() - 1;
			}


		GUILayout.Space(10);
		GUILayout.EndHorizontal();
		GUILayout.Space(10);
		EditorGUILayout.EndVertical();



		// Seperator
		GUILayout.Space(10);



		// Event List Panel
		if(trigger.GetEventCount() > 0) {
			Rect r2 = (Rect)EditorGUILayout.BeginVertical();
			GUI.Box(r2, "");
			//GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			GUILayout.BeginVertical();

				for(int e = 0; e < trigger.GetEventCount(); ++e) {
					bool isEventExpanded = (e == expandedEvent);

					// Seperator
					GUILayout.Space(10);

					var colorModifier = 1.0f;
					if(isEventExpanded) colorModifier = 0.8f;
					else if(expandAllEvents) colorModifier = (e % 2 == 0) ? 0.9f : 1.0f;

					var defaultBackgroundColor = GUI.backgroundColor;
					var color = colorModifier == 1.0f ? defaultBackgroundColor : new Color(defaultBackgroundColor.r * colorModifier, defaultBackgroundColor.g * colorModifier, defaultBackgroundColor.b * colorModifier, defaultBackgroundColor.a);

					Rect r3 = (Rect)EditorGUILayout.BeginVertical();
					GUI.backgroundColor = color;
					GUI.Box(r3, "");
					GUI.backgroundColor = defaultBackgroundColor;
					GUILayout.Space(13);
					GUILayout.BeginHorizontal();
					GUILayout.Space(20);
					GUILayout.BeginVertical();


						// Event Controls
						GUILayout.BeginHorizontal();

						// Display Event Properties
						if(!expandAllEvents && !isEventExpanded) {
							GUIStyle style = new GUIStyle(EditorStyles.foldout);
							style.stretchWidth = false;
							if(EditorGUILayout.Foldout(e == expandedEvent, (e == expandedEvent) ? "Hide" : "Show", true, style)) {
								expandedEvent = e;
								isEventExpanded = true;
							} else if(e == expandedEvent) {
								expandedEvent = -1;
							}
						}

						// Event Properties
						trigger.SetEventType(e, EditorGUILayout.Popup(trigger.GetEventType(e), triggerEventTypes));
						if(GUILayout.Button("Clone")) {
							expandedEvent = trigger.CloneEvent(e);
							return;
						}

						if(GUILayout.Button("✖")) {
							trigger.RemoveEvent(e);
							if(e == expandedEvent) expandedEvent = -1;
							return;
						}

						if(GUILayout.Button("▲")) {
							expandedEvent = trigger.MoveUpEvent(e);
							return;
						}

						if(GUILayout.Button("▼")) {
							expandedEvent = trigger.MoveDownEvent(e);
							return;
						}
						GUILayout.EndHorizontal();

						// Event Properties
						if((isEventExpanded && e == expandedEvent) || expandAllEvents) {
							// Seperator
							GUILayout.Space(10);

							// Trigger Individuals
							trigger.SetEventTriggerIndividuals(e, EditorGUILayout.Toggle("Trigger Individuals", trigger.GetEventTriggerIndividuals(e)));

							// Layers
							trigger.SetEventLayerFlags(e, InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(EditorGUILayout.MaskField("Layers", InternalEditorUtility.LayerMaskToConcatenatedLayersMask(trigger.GetEventLayerFlags(e)), InternalEditorUtility.layers)));

							// Seperator
							GUILayout.Space(10);

							// Action List
							EditorGUILayout.LabelField("Actions");

							// Seperator
							GUILayout.Space(10);

							// Action Properties
							if(EditorGUILayout.Foldout(true, "Action Properties", true)) {
								// Action Properties
							}
						}


					GUILayout.EndVertical();
					GUILayout.Space(10);
					GUILayout.EndHorizontal();
					GUILayout.Space(13);
					EditorGUILayout.EndVertical();
				}


			GUILayout.EndVertical();
			GUILayout.Space(10);
			GUILayout.EndHorizontal();
			GUILayout.Space(10);
			EditorGUILayout.EndVertical();
		}
	}
}
#endif