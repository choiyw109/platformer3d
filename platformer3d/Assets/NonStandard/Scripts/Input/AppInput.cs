using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.Inputs {
	public class AppInput : MonoBehaviour {
		public static void Log(string text) => Debug.Log(text);
		public static void Log(object obj) => Log(obj.ToString());
		private static AppInput _instance;

		public static AppInput Instance {
			get {
				if (!Application.isPlaying) { throw new Exception("something is trying to create AppInput during edit time"); }
				if (_instance != null) return _instance;
				_instance = FindObjectOfType<AppInput>();
				if (_instance == null) {
					GameObject go = new GameObject($"<{nameof(AppInput)}>");
					_instance = go.AddComponent<AppInput>();
				}
				return _instance;
			}
		}

		public List<KBind> keyBinds = new List<KBind>();
    
		protected List<KBind> kBindPresses = new List<KBind>();
		protected List<KBind> kBindHolds = new List<KBind>();
		protected List<KBind> kBindReleases = new List<KBind>();

		[TextArea(1, 30), SerializeField]
		private string CurrentKeyBindings;
    
		private bool textInputHappening = false;

		public static bool RemoveListener(string name) => Instance.RemoveKeyBind(name);
		public static bool RemoveListener(KBind kBind) => Instance.RemoveKeyBind(kBind);
		public static bool AddListener(KBind kBind) => Instance.AddKeyBind(kBind);
		public static bool AddListener(KCode key, Func<bool> whatToDo, string name) => AddListener(new KBind(key, whatToDo, name));

		public static bool HasListener(string name) {
			int index = (!string.IsNullOrEmpty(name)) ? Instance.keyBinds.FindIndex(kb => kb.name == name) : -1;
			return index >= 0;
		}

		public bool RemoveKeyBind(string name) {
			int index = keyBinds.FindIndex(kb => kb.name == name);
			if (index < 0) return false;
			return RemoveListener(keyBinds[index], index);
		}

		public bool RemoveKeyBind(KBind kBind) {
			int index = keyBinds.IndexOf(kBind);
			if (index < 0) return false;
			return RemoveListener(kBind, index);
		}

		private bool RemoveListener(KBind kBind, int kBindIndex){
			keyBinds.RemoveAt(kBindIndex);
			UpdateKeyBindGroups(kBind, KBindChange.Remove);
			return true;
		}
		public bool AddKeyBind(KBind kBind) {
			int index = (!string.IsNullOrEmpty(kBind.name)) ? keyBinds.FindIndex(kb=> kb.name == kBind.name) : -1;
			KBindChange kindOfChange = KBindChange.Add;
			if (index >= 0) {
				kindOfChange = KBindChange.Update; // will cause lists to Remove then re-Add
				return false;
			} else {
				keyBinds.Add(kBind);
			}
			return UpdateKeyBindGroups(kBind, kindOfChange);
		}

		private static bool UpdateLists(KBind kBind, KBindChange change) {
			return Instance.UpdateKeyBindGroups(kBind, change);
		}

		public enum KBindChange { None = 0, Add = 1, Remove = 2, Update = 3 }

		/// <summary>
		/// used to add/remove/update a specific <see cref="KBind"/>
		/// </summary>
		/// <param name="kBind"></param>
		/// <param name="add"></param>
		private bool UpdateKeyBindGroups(KBind kBind, KBindChange change) {
			bool changeHappened = false;
			kBind.Normalize();
			if(kBind.keyCombinations != null && kBind.keyCombinations[0].modifiers != null)
				Log(kBind.keyCombinations[0].modifiers[0]);
			Log(kBind);
			EnsureInitializedKeyBindGroups();
			if(change == KBindChange.Update) {
				changeHappened |= UpdateKeyBindGroups(kBind, KBindChange.Remove);
				change = KBindChange.Add;
			}
			for (int k = 0; k < keyBindGroups.Length; ++k) {
				KBindGroup group = keyBindGroups[k];
				changeHappened |= group.UpdateKeyBinding(kBind, change);
			}
			if (changeHappened) {
				UpdateCurrentKeyBindText();
			}
			return changeHappened;
		}
    
		public string DebugBindings(IList<KBind> list) {
			StringBuilder output = new StringBuilder();
			for (int i = 0; i < list.Count; ++i) {
				if (i > 0) output.Append(", ");
				output.Append(list[i].name).Append("[");
				for (int k = 0; k < list[i].keyCombinations.Length; ++k) {
					if (k > 0) output.Append(", ");
					output.Append(list[i].keyCombinations[k].key);
				}
				output.Append("]");
			}
			return output.ToString();
		}

		public static void BeginIgnoreKeyBinding(string text) { Instance.textInputHappening = true; }
		public static void EndIgnoreKeyBinding(string text) { Instance.textInputHappening = false; }
		private static UnityAction<string> s_BeginIgnoreKeyBinding = BeginIgnoreKeyBinding;
		private static UnityAction<string> s_EndIgnoreKeyBinding = EndIgnoreKeyBinding;
    
		public void TextInputDisablesAppInput(TMP_InputField _inputField) {
			_inputField.onSelect.AddListener( s_BeginIgnoreKeyBinding );
			_inputField.onDeselect.AddListener( s_EndIgnoreKeyBinding );
		}
    
		public static Vector3 MousePosition => Input.mousePosition;
		public static Vector3 MousePositionDelta => new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

		private static readonly Dictionary<KCode, KState> _pressState = new Dictionary<KCode, KState>();
    
		public static bool IsOldKeyCode(KCode code) => Enum.IsDefined(typeof(KeyCode), (int)code);

		private static bool GetKey_internal(KCode key) {
			bool v = false;
			switch (key) {
				case KCode.AnyAlt: v = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt); break;
				case KCode.AnyControl: v = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl); break;
				case KCode.AnyShift: v = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); break;
				case KCode.MouseWheelUp: v = Input.GetAxis("Mouse ScrollWheel") > 0; break;
				case KCode.MouseWheelDown: v = Input.GetAxis("Mouse ScrollWheel") < 0; break;
				case KCode.MouseXUp: v = Input.GetAxis("Mouse X") > 0; break;
				case KCode.MouseXDown: v = Input.GetAxis("Mouse X") < 0; break;
				case KCode.MouseYUp: v = Input.GetAxis("Mouse Y") > 0; break;
				case KCode.MouseYDown: v = Input.GetAxis("Mouse Y") < 0; break;
				// default: throw new Exception($"can't handle {key}");
			}
			return v;
		}

		private List<KCode> advanceToPressed = new List<KCode>();
		private List<KCode> advanceToReleased = new List<KCode>();
		public void LateUpdate() {
			// figure out which keys have just been pressed or released, and adjust those states
			advanceToPressed.Clear();
			advanceToReleased.Clear();
			foreach (KeyValuePair<KCode,KState> kvp in _pressState) {
				switch (kvp.Value) {
					case KState.KeyDown: advanceToPressed.Add(kvp.Key); break;
					case KState.KeyUp:  advanceToReleased.Add(kvp.Key); break;
				}
			}
			// values must be set out of dictionary traversal because of collection rules
			for (int i = 0; i <  advanceToPressed.Count;++i) { _pressState[ advanceToPressed[i]]= KState.KeyHeld; }
			for (int i = 0; i < advanceToReleased.Count;++i) { _pressState[advanceToReleased[i]]= KState.KeyReleased;}
		}

		public static bool GetKey(KCode key) {
			if (IsOldKeyCode(key)) { return UnityEngine.Input.GetKey((KeyCode)key); }
			bool pressed = GetKey_internal(key);
			_pressState.TryGetValue(key, out KState ks);
			if (pressed && ks == KState.KeyReleased) { _pressState[key] = KState.KeyDown; }
			if (!pressed && ks == KState.KeyHeld) { _pressState[key] = KState.KeyUp;}
			return pressed;
		}
    
		public static bool GetKeyDown(KCode key) {
			if (IsOldKeyCode(key)) { return UnityEngine.Input.GetKeyDown((KeyCode)key); }
			_pressState.TryGetValue(key, out KState ks);
			if (ks == KState.KeyHeld || ks == KState.KeyUp) { return false; }
			bool pressed = GetKey_internal(key);
			if (pressed && ks == KState.KeyReleased) { _pressState[key] = KState.KeyDown; }
			return pressed;
		}

		public static bool GetKeyUp(KCode key) {
			if (IsOldKeyCode(key)) { return UnityEngine.Input.GetKeyUp((KeyCode)key); }
			_pressState.TryGetValue(key, out KState ks);
			if (ks == KState.KeyReleased || ks == KState.KeyDown) { return false; }
			bool pressed = GetKey_internal(key);
			if (!pressed && ks == KState.KeyHeld) { _pressState[key] = KState.KeyReleased; }
			return !pressed;
		}

		public void Update() => DoUpdate();

		bool IsKeyBindAmbiguousWithTextInput(KBind kBind) {
			for (int i = 0; i < kBind.keyCombinations.Length; ++i) {
				bool isSimpleKeyPress = kBind.keyCombinations[i].modifiers == null || kBind.keyCombinations[i].modifiers.Length == 0;
				bool shiftModified = !isSimpleKeyPress && kBind.keyCombinations[i].modifiers[0].key == KCode.LeftShift;
				bool isFunctionKey = (kBind.keyCombinations[i].key >= KCode.F1 && kBind.keyCombinations[i].key <= KCode.F15);
				bool couldInterfereWithKeyboardInput = (isSimpleKeyPress && !isFunctionKey) || (shiftModified && !isFunctionKey);
				if (couldInterfereWithKeyboardInput) return true;
			}
			return false;
		}

		[Serializable]
		public class KBindGroup {
			public string name;
			public List<KBind> keyBindList;
			public List<KeyTrigger> triggerList = new List<KeyTrigger>();
			/// <summary>
			/// could be <see cref="KBind.GetDown"/>, <see cref="KBind.GetHeld"/>, or <see cref="KBind.GetUp"/>
			/// </summary>
			public Func<KBind, KCombination> trigger;
			/// <summary>
			/// should return true if the action was successful (prevents other key events with the same keycode from triggering)
			/// </summary>
			public Func<KBind, bool> action;
			/// <summary>
			/// a filter that prevents key bindings from entering this list
			/// </summary>
			public Func<KBind, bool> putInList;
        
			/// <summary>
			/// we want to know what <see cref="KBind"/> was triggered, including which specific <see cref="KCombination"/> did the triggerings
			/// </summary>
			public struct KeyTrigger : IComparable<KeyTrigger> {
				public KBind kb;
				/// <summary>
				/// which (of the possibly many) keypress triggered the key mapping to activate
				/// </summary>
				public KCombination kp;
				/// sort by keypress
				public int CompareTo(KeyTrigger other) { return kp.CompareTo(other.kp); }
			}
        
			public void Update(Func<KBind, bool> additionalFilter = null) {
				for (int i = 0; i < keyBindList.Count; ++i) {
					KeyCheck(keyBindList[i], triggerList, additionalFilter);
				}
			}
        
			/// <param name="kBind"></param>
			/// <param name="kind"></param>
			/// <returns></returns>
			public bool UpdateKeyBinding(KBind kBind, KBindChange kind) {
				if (kind == KBindChange.Add && !putInList(kBind)) return false;
				bool changeHappened = false;
				int index = keyBindList.IndexOf(kBind);
				switch (kind) {
				case KBindChange.Add:
					if (index >= 0) { Log($"already added {name} {kBind.name}?"); }
					if (index < 0) {
						keyBindList.Add(kBind);
						keyBindList.Sort();
						changeHappened = true;
						//Log($"added {name} {kBind.name}");
					} else { if (index >= 0) { Log($"will not add duplicate {name} {kBind.name}"); } }
					break;
				case KBindChange.Remove:
					if (index >= 0) { keyBindList.RemoveAt(index); changeHappened = true; }
					break;
				case KBindChange.Update:
					throw new Exception("Update is composed of a Remove and Add, should never be called directly like this.");
				}
				return changeHappened;
			}
        
			/// <param name="kb">check if this key bind is being triggered</param>
			/// <param name="list">where to mark if this is indeed triggered</param>
			/// <param name="additionalFilter">an additional gate that might prevent this particluar keybind from triggering. possibly heavy method, so only checked if the key is triggered</param>
			bool KeyCheck(KBind kb, List<KeyTrigger> list, Func<KBind, bool> additionalFilter = null) {
				KCombination kp = trigger(kb);
				if (kp != null && (additionalFilter == null || !additionalFilter.Invoke(kb))) {
					list.Add(new KeyTrigger{kb=kb, kp=kp});
					return true;
				}
				return false;
			}
        
			/// <summary>
			/// keeps one event consumed for each key (pressing w and a should trigger both w and a, not just the first one)
			/// </summary>
			private static Dictionary<KCode, KBind> s_eventConsumed = new Dictionary<KCode, KBind>();
			/// <summary>
			/// resolves key conflicts (giving priority to more complex key presses first) before invoking all triggered keys
			/// </summary>
			public void Resolve(bool showConflict, bool logActivatedKeyBinds) {
				if (triggerList.Count <= 0) { return; } 
				// sort by KCode in the triggering ComplexKeyPress, with the most complex first
				triggerList.Sort();
				// if there are multiple keybinds with the same kcode
				for (int a = 0; a < triggerList.Count; ++a) {
					int conflictHere = -1;
					for (int b = a + 1; b < triggerList.Count; ++b) {
						if (triggerList[a].kp.key == triggerList[b].kp.key) {
							conflictHere = b;
						}
					}
					// go through all of those keybinds
					if (conflictHere != -1) {
						string debugOutput = showConflict?$"possible {name} conflict":"";
						int complexityToKeep = triggerList[a].kp.GetComplexity();
						// if a keybind's modifiers are all fulfilled by a more complex keybind, ignore this keybind (remove from list) 
						for (int i = a; i <= conflictHere; ++i) {
							KBind kb = triggerList[i].kb;
							if (showConflict) { debugOutput += "\n" + kb + kb.priority; }
							if (triggerList[i].kp.GetComplexity() < complexityToKeep) {
								triggerList.RemoveAt(i);
								if (showConflict) { debugOutput += "[REMOVED]"; }
							}
						}
						if (showConflict) { Log(debugOutput); }
					}
				}

				string debugText = null;
				if (logActivatedKeyBinds) { debugText = $"{name} activating: "; }
            
				// trigger everything left in the list
				for (int i = 0; i < triggerList.Count; ++i) {
					KBind kb = triggerList[i].kb;
					s_eventConsumed.TryGetValue(triggerList[i].kp.key, out KBind eventConsumed);
					if (logActivatedKeyBinds) {
						if (i > 0) debugText += ", ";
						debugText += kb.name;
					}
					bool activated = false;
					// invoke non-consumptive events, or consumptive events as long as the event is not consumed
					if (eventConsumed == null || kb.eventAlwaysTriggerable) {
						activated = action.Invoke(kb);
					}
					// high-priority key mappings that consume events should prevent future events that also consume.
					if (!kb.eventAlwaysTriggerable && activated) {
						//Log($"{kb.name} consumed {name}");
						s_eventConsumed[triggerList[i].kp.key] = kb;
					}
				}
				if (logActivatedKeyBinds) {
					Log(debugText);
				}
				s_eventConsumed.Clear();
				triggerList.Clear();
			}
		}

		[HideInInspector] public bool debugPrintPossibleKeyConflicts = false;
		[HideInInspector] public bool debugPrintActivatedEvents = false;
		KBindGroup[] keyBindGroups;
		public void Awake() {
			Application.quitting += () => {
				IsQuitting = true;
				string c = "color", a = "#aa88ff", b = "#4488ff";
				Log($"<{c}={a}>{nameof(AppInput)}</{c}>.{nameof(IsQuitting)} = <{c}={b}>true</{c}>;");
			};
			EnsureInitializedKeyBindGroups();
		}

		public void EnsureInitializedKeyBindGroups() {
			if (keyBindGroups != null) return;
			keyBindGroups = new KBindGroup[] {
				new KBindGroup{name="press",  keyBindList=kBindPresses, trigger=kb=>kb.GetDown(),action=kb=>kb.DoPress(),  putInList=kb=>kb.keyEvent.CountPress>0  },
				new KBindGroup{name="hold",   keyBindList=kBindHolds,   trigger=kb=>kb.GetHeld(),action=kb=>kb.DoHold(),   putInList=kb=>kb.keyEvent.CountHold>0   },
				new KBindGroup{name="release",keyBindList=kBindReleases,trigger=kb=>kb.GetUp(),  action=kb=>kb.DoRelease(),putInList=kb=>kb.keyEvent.CountRelease>0},
			};
		}

		public void DoUpdate() {
			if(!textInputHappening) {
				Array.ForEach(keyBindGroups, ks=>ks.Update());
			} else {
				Array.ForEach(keyBindGroups, ks=>ks.Update(IsKeyBindAmbiguousWithTextInput));
			}
			Array.ForEach(keyBindGroups, ks=>ks.Resolve(debugPrintPossibleKeyConflicts, debugPrintActivatedEvents));
		}

		public static bool IsQuitting { get; private set; }
    
		public void UpdateCurrentKeyBindText() {
			EnsureInitializedKeyBindGroups();
			StringBuilder sb = new StringBuilder();
			for (int s = 0; s < keyBindGroups.Length; ++s) {
				KBindGroup ks = keyBindGroups[s];
				sb.Append($"[{ks.name}]\n");
				for (int i = 0; i < ks.keyBindList.Count; ++i) {
					KBind kb = ks.keyBindList[i];
					bool needsPriority = true;
					bool hasKeys = true;
					if (kb.keyCombinations.Length != 0 && (kb.keyCombinations.Length != 1 || kb.keyCombinations[0].key != KCode.None)) {
						KCombination theseKeys = kb.keyCombinations[0];
						bool hasPrev = i > 0;
						bool hasNext = i < ks.keyBindList.Count - 1;
						KBind prev = (hasPrev) ? ks.keyBindList[i - 1] : null;
						KBind next = (hasNext) ? ks.keyBindList[i + 1] : null;
						KCombination prevKeys = hasPrev && prev.keyCombinations.Length > 0 ? prev.keyCombinations[0] : null;
						KCombination nextKeys = hasNext && next.keyCombinations.Length > 0 ? next.keyCombinations[0] : null;
						needsPriority = (prevKeys != null && prevKeys.CompareTo(theseKeys) == 0 ||
							nextKeys != null && nextKeys.CompareTo(theseKeys) == 0);
					} else {
						hasKeys = false;
					}
					if (hasKeys) {
						sb.Append(kb.ShortDescribe(" | "));
					} else {
						sb.Append("(no keys)");
					}
					sb.Append(" :");
					if (needsPriority) { sb.Append(kb.priority.ToString()); }
					sb.Append(": ");
					sb.Append(kb.name);
					sb.Append("\n");
				}
			}
			CurrentKeyBindings = sb.ToString();
		}
	}
}
