﻿using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Text;

namespace NonStandard.Inputs {
	[Serializable]
	public class KBind : IComparable<KBind> {
		/// <summary>
		/// how to name this key binding in any user interface that pops up.
		/// </summary>
		public string name;
		/// <summary>
		/// smaller number is greater priority.
		/// </summary>
		public int priority = 1000;
    
		/// <summary>
		/// if true, can still be triggered after the key event is consumed
		/// </summary>
		public bool eventAlwaysTriggerable;

		public KCombination[] keyCombinations = new KCombination[1];

		[Serializable]
		public class EventSet {
			[SerializeField, ContextMenuItem("DoPress", "DoPress")] protected UnityEvent onPress;
			[SerializeField, ContextMenuItem("DoHold", "DoHold")] protected UnityEvent onHold;
			[SerializeField, ContextMenuItem("DoRelease", "DoRelease")] protected UnityEvent onRelease;

			/// <summary>
			/// return true if the action worked, false if it should be ignored
			/// </summary>
			private Func<bool> actionPress, actionHold, actionRelease;

			public int CountPress => (onPress?.GetPersistentEventCount() ?? 0) + (actionPress?.GetInvocationList().Length ?? 0);
			public int CountHold => (onHold?.GetPersistentEventCount() ?? 0) + (actionHold?.GetInvocationList().Length ?? 0);
			public int CountRelease => (onRelease?.GetPersistentEventCount() ?? 0) + (actionRelease?.GetInvocationList().Length ?? 0);
        
			public void AddPress(Func<bool> a) { if (actionPress != null) { actionPress += a; } else { actionPress = a; } }
			public void AddHold(Func<bool> a) { if (actionHold != null) { actionHold += a; } else { actionHold = a; } }
			public void AddRelease(Func<bool> a) { if (actionRelease != null) { actionRelease += a; } else { actionRelease = a; } }
			public bool DoPress() { onPress?.Invoke(); return (actionPress?.Invoke() ?? false); }
			public bool DoHold() { onHold?.Invoke(); return (actionHold?.Invoke() ?? false);  }
			public bool DoRelease() { onRelease?.Invoke(); return (actionRelease?.Invoke() ?? false);  }
			public void RemovePresses() { onPress.RemoveAllListeners(); actionPress = null;}
			public void RemoveHolds() { onHold.RemoveAllListeners(); actionHold = null;}
			public void RemoveReleases() { onRelease.RemoveAllListeners(); actionRelease = null;}
        
			private string FilterMethodName(string methodName) {
				if (methodName.StartsWith("set_") || methodName.StartsWith("get_")) { return methodName.Substring(4); }
				return methodName;
			}
        
			public string GetDelegateText(UnityEvent ue, Func<bool> a) {
				StringBuilder text = new StringBuilder();
				if (ue != null) {
					for (int i = 0; i < ue.GetPersistentEventCount(); ++i) {
						if (text.Length > 0) { text.Append("\n"); }
						string t = ue.GetPersistentTarget(i)?.name ?? "<???>";
						text.Append(t).Append(".").Append(FilterMethodName(ue.GetPersistentMethodName(i)));
					}
				}
				if(a != null) {
					Delegate[] delegates = a.GetInvocationList();
					for (int i = 0; i < delegates.Length; ++i) {
						if (text.Length > 0) { text.Append("\n"); }
						text.Append(delegates[i].Target).Append(".").Append(delegates[i].Method.Name);
					}
				}
				return text.ToString();
			}
        
			public string CalculateDescription() {
				StringBuilder text = new StringBuilder();
				string holdText = GetDelegateText(onPress, actionPress);
				string pressText = GetDelegateText(onHold, actionHold);
				string releaseText = GetDelegateText(onRelease, actionRelease);
				text.Append(holdText);
				if (!string.IsNullOrEmpty(pressText)) {
					if (text.Length > 0) { text.Append("\n\n"); }
					text.Append("Press:\n").Append(pressText);
				}
				if (!string.IsNullOrEmpty(releaseText)) {
					if (text.Length > 0) { text.Append("\n\n"); }
					text.Append("Release:\n").Append(releaseText);
				}
				return text.ToString();
			}
		}

		[ContextMenuItem("DoActivateTrigger", "DoActivateTrigger")]
		public EventSet keyEvent = new EventSet();

		/// <summary>
		/// additional requirements for the input
		/// </summary>
		public Func<bool> additionalRequirement;

		public bool IsAllowed() => additionalRequirement == null || additionalRequirement.Invoke();

		/// <summary>
		/// describes a function to execute when a specific key-combination is pressed
		/// </summary>
		public KBind(KCode key, Func<bool> onPressEvent, string name = null):this(key, name, onPressEvent) { }

		/// <summary>
		/// describes functions to execute when a specific key is pressed/held/released
		/// </summary>
		public KBind(KCode key, string name = null, Func<bool> onPressEvent = null, Func<bool> onHoldEvent = null,
			Func<bool> onReleaseEvent = null, Func<bool> additionalRequirement = null, 
			bool eventAlwaysTriggerable = false)
			: this(new KCombination(key), name, onPressEvent, onHoldEvent, onReleaseEvent, additionalRequirement) {
			this.eventAlwaysTriggerable = eventAlwaysTriggerable;
		}

		/// <summary>
		/// describes functions to execute when a specific key-combination is pressed/held/released
		/// </summary>
		public KBind(KCombination kCombo, string name = null, Func<bool> onPressEvent = null, Func<bool> onHoldEvent = null,
			Func<bool> onReleaseEvent = null, Func<bool> additionalRequirement = null, 
			bool eventAlwaysTriggerable = false)
			: this(new[] {kCombo}, name, onPressEvent, onHoldEvent, onReleaseEvent, additionalRequirement) {
			this.eventAlwaysTriggerable = eventAlwaysTriggerable;
		}
    
		/// <summary>
		/// describes functions to execute when any of the specified key-combinations are pressed/held/released
		/// </summary>
		public KBind(KCombination[] kCombos, string name = null, Func<bool> onPressEvent = null, Func<bool> onHoldEvent = null,
			Func<bool> onReleaseEvent = null, Func<bool> additionalRequirement = null, 
			bool eventAlwaysTriggerable = false) {
			keyCombinations = kCombos;
			Normalize();
			Array.Sort(keyCombinations); Array.Reverse(keyCombinations); // put least complex key bind first, backwards from usual processing
			this.name = name;
			AddEvents(onPressEvent, onHoldEvent, onReleaseEvent);
			if (additionalRequirement != null) {
				this.additionalRequirement = additionalRequirement;
			}
			this.eventAlwaysTriggerable = eventAlwaysTriggerable;
		}

		public void Normalize() { Array.ForEach(keyCombinations, k => k.Normalize()); }

		public void AddEvents(Func<bool> onPressEvent = null, Func<bool> onHoldEvent = null, Func<bool> onReleaseEvent = null) {
			if (onPressEvent != null) { keyEvent.AddPress(onPressEvent);}
			if (onHoldEvent != null) { keyEvent.AddHold(onHoldEvent);}
			if (onReleaseEvent != null) { keyEvent.AddRelease(onReleaseEvent);}
		}

		public void AddComplexKeyPresses(KCombination[] keysToUse) {
			if (keyCombinations.Length == 0 || keyCombinations[0].key == KCode.None) { keyCombinations = keysToUse; } else {
				List<KCombination> currentKeys = new List<KCombination>(keyCombinations);
				currentKeys.AddRange(keysToUse);
				// remove duplicates
				for (int a = 0; a < currentKeys.Count; ++a) {
					for (int b = currentKeys.Count - 1; b > a; --b) {
						if (currentKeys[a].CompareTo(currentKeys[b]) == 0) {
							currentKeys.RemoveAt(b);
						}
					}
				}
				keyCombinations = currentKeys.ToArray();
			}
			Normalize();
			Array.Sort(keyCombinations); Array.Reverse(keyCombinations); // put least complex key bind first (reverse of usual processing)
		}
    
		public void AddKeyCombinations(KCombination[] keyCombo, string nameToUse, Func<bool> onPress = null, Func<bool> onHold = null, Func<bool> onRelease = null) {
			if (keyCombo != null) { AddComplexKeyPresses(keyCombo); }
			if (string.IsNullOrEmpty(name)) { name = nameToUse; }
			AddEvents(onPress, onHold, onRelease);
		}

		public void AddKeyBinding(KCode keyToUse, string nameToUse, Func<bool> onPress = null, Func<bool> onHold = null, Func<bool> onRelease = null) {
			AddKeyCombinations(new KCombination[]{new KCombination(keyToUse)}, nameToUse, onPress, onHold, onRelease);
		}
		public string ShortDescribe(string betweenKeyPresses = "\n") {
			if (keyCombinations == null || keyCombinations.Length == 0) return "";
			string text = "";
			for (int i = 0; i < keyCombinations.Length; ++i) {
				if (i > 0) text += betweenKeyPresses;
				text += keyCombinations[i].ToString();
			}
			return text;
		}

		public int CompareTo(KBind other) {
			if (other == null) return -1;
			// the simpler key binding, more likely to be pressed, should go first
			for (int i = 0; i < keyCombinations.Length; ++i) {
				if (other.keyCombinations.Length <= i) return 1;
				int cmp = keyCombinations[i].CompareTo(other.keyCombinations[i]);
				if (cmp == 0) { cmp = priority.CompareTo(other.priority); }
				if (cmp != 0) return cmp;
			}
			return 1;
		}

		public override string ToString() { return $"{ShortDescribe(" || ")} \"{name}\""; }

		/// <returns>if the action succeeded (which may remove other actions from queue, due to priority)</returns>
		public bool DoPress() { return keyEvent.DoPress(); }

		/// <returns>if the action succeeded (which may remove other actions from queue, due to priority)</returns>
		public bool DoHold() { return keyEvent.DoHold(); }

		/// <returns>if the action succeeded (which may remove other actions from queue, due to priority)</returns>
		public bool DoRelease() { return keyEvent.DoRelease(); }

		public KCombination GetDown() {
			bool allowedChecked = false;
			for (int i = 0; i < keyCombinations.Length; ++i) {
				if (keyCombinations[i].IsSatisfiedDown()) {
					if (!allowedChecked) { if (!IsAllowed()) { return null; } allowedChecked = true; }
					return keyCombinations[i];
				}
			}
			return null;
		}
		public bool IsDown() { return GetDown() != null; }

		public KCombination GetHeld() {
			bool allowedChecked = false;
			for (int i = 0; i < keyCombinations.Length; ++i) {
				if (keyCombinations[i].IsSatisfiedHeld()) {
					if (!allowedChecked) { if (!IsAllowed()) { return null; } allowedChecked = true; }
					return keyCombinations[i];
				}
			}
			return null;
		}
		public bool IsHeld() { return GetHeld() != null; }

		public KCombination GetUp() {
			bool allowedChecked = false;
			for (int i = 0; i < keyCombinations.Length; ++i) {
				if (keyCombinations[i].IsSatisfiedUp()) {
					if (!allowedChecked) { if (!IsAllowed()) { return null; } allowedChecked = true; }
					return keyCombinations[i];
				}
			}
			return null;
		}
		public bool IsUp() { return GetUp() != null; }

		public void DoActivateTrigger() {
			if (keyEvent.CountPress > 0) { DoPress(); }
			else if (keyEvent.CountHold > 0) { DoHold(); }
			else if (keyEvent.CountRelease > 0) { DoRelease(); }
		}
	}

	[Serializable]
	public class KCombination : IComparable<KCombination> {
		/// <summary>
		/// the key that triggers this complex keypress
		/// </summary>
		public KCode key;
		public Modifier[] modifiers;

		public KCombination() { }

		public bool IsNone() { return key == KCode.None && (modifiers == null || modifiers.Length == 0); }
    
		public KCombination(KCode key) {
			this.key = key;
			modifiers = null;
		}

		public void Normalize() {
			key = key.Normalized();
			if (modifiers == null) return;
			for (int i = 0; i < modifiers.Length; ++i) { modifiers[i] = modifiers[i].Normalize(); }
		}

		public KCombination(KCode key, KCode modifier) : this(key) {
			AddModifier(modifier);
		}

		public KCombination(KCode key, params KCode[] modifiers) : this(key) {
			Array.ForEach(modifiers, m => AddModifier(m));
		}

		public int GetComplexity() { return modifiers?.Length ?? 0; }
    
		public bool AddModifier(KCode kCode) {
			Modifier mod = new Modifier(kCode);
			if (HasModifier(mod)) { return false; }
			List<Modifier> mods = new List<Modifier>();
			if(modifiers != null) {mods.AddRange(modifiers);}
			mods.Add(mod);
			mods.Sort();
			modifiers = mods.ToArray();
			return true;
		}    

		public bool HasModifiers(Modifier[] mods) {
			if (modifiers == null || modifiers.Length != mods.Length) return false;
			for (int i = 0; i < mods.Length; ++i) {
				if (!HasModifier(mods[i])) return false;
			}
			return true;
		}

		public bool HasModifier(Modifier m) {
			if (modifiers == null || modifiers.Length == 0) return false;
			for (int i = 0; i < modifiers.Length; ++i) {
				if (modifiers[i].Equals(m)) return true;
			}
			return false;
		}
    
		[Serializable]
		public struct Modifier : IComparable<Modifier> {
			public KCode key;
			/// <param name="key"></param>
			public Modifier(KCode key) { this.key = key; }
        
			public override string ToString() { return key.NormalName(); }

			public int CompareTo(Modifier other) { return key.CompareTo(other.key); }

			public override bool Equals(object obj) {
				if (obj == null) return false;
				Modifier mod = (Modifier) obj;
				return mod.key == key;
			}

			public bool Equals(Modifier other) { return key == other.key; }

			public override int GetHashCode() { return (int) key; }

			public Modifier Normalize() { key = key.Normalized(); return this; }
		}
    
		public int CompareTo(KCombination other) {
			int comp = key.CompareTo(other.key);
			if (comp != 0) return comp;
			if (modifiers != null && other.modifiers != null) {
				for (int i = 0; i < modifiers.Length && i < other.modifiers.Length; ++i) {
					if (i >= other.modifiers.Length) return -1;
					if (i >= modifiers.Length) return 1;
					comp = modifiers[i].key.CompareTo(other.modifiers[i].key);
					if (comp != 0) return comp;
				}
			} else {
				int selfScore = modifiers?.Length ?? 0;
				int otherScore = other.modifiers?.Length ?? 0;
				return -selfScore.CompareTo(otherScore); // the more complex ComplexKeyPress should be first
			}
			return 0;
		}

		public override string ToString() {
			StringBuilder text = new StringBuilder();
			if (modifiers != null) {
				for (int i = 0; i < modifiers.Length; ++i) {
					text.Append(modifiers[i]).Append("+");
				}
			}
			text.Append(key.NormalName());
			return text.ToString();
		}

		public static KCombination FromString(string s) {
			string[] keys = s.Split('+');
			int numMods = keys.Length - 1;
			KCombination kp = new KCombination();
			if (numMods > 0) {
				kp.modifiers = new Modifier[numMods];
				for (int i = 0; i < numMods; ++i) {
					string k = keys[i].Trim();
					kp.modifiers[i].key = k.ToEnum<KCode>();
				}
				kp.key = keys[keys.Length-1].Trim().ToEnum<KCode>();
			}
			return kp;
		}

		public bool IsSatisfiedDown() {
			KState ks = key.GetState();
			bool anyKeyIsBeingPressed = ks == KState.KeyDown;
			if (ks == KState.KeyReleased) return false;
			if(modifiers != null) {
				for (int i = 0; i < modifiers.Length; ++i) {
					ks = modifiers[i].key.GetState();
					if (ks == KState.KeyReleased) {
						return false;
					} else if (ks == KState.KeyDown) {
						anyKeyIsBeingPressed = true;
					}
				}
			}
			return anyKeyIsBeingPressed;
		}
		public bool IsSatisfiedHeld() {
			if (!key.IsHeld()) return false;
			if (modifiers != null) {
				for (int i = 0; i < modifiers.Length; ++i) {
					if (!modifiers[i].key.IsHeld()) { return false; }
				}
			}
			return true;
		}
		public bool IsSatisfiedUp() {
			KState ks = key.GetState();
			bool anyKeyIsBeingReleased = ks == KState.KeyUp;
			if (ks == KState.KeyReleased) return false;
			if (modifiers != null) {
				for (int i = 0; i < modifiers.Length; ++i) {
					ks = modifiers[i].key.GetState();
					if (ks == KState.KeyReleased) {
						return false;
					} else if (ks == KState.KeyUp) {
						anyKeyIsBeingReleased = true;
					}
				}
			}
			return anyKeyIsBeingReleased;
		}
	}

	public static class StringEnumExtension {
		public static EnumType ToEnum<EnumType>(this string text) { return (EnumType)Enum.Parse(typeof(EnumType), text, true); }
	}
}