using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;


namespace NonStandard.Inputs {
	[System.Serializable]
	public class AxBind : InputBind {
		public bool disable;
		[System.Serializable]
		public class UnityEventFloat : UnityEvent<float> { }

		/// <summary>
		/// name of the axis, e.g.: Horizontal, Vertical
		/// </summary>
		public Axis[] axis = new Axis[] { new Axis("Horizontal") };


		[ContextMenuItem("DoActivateTrigger", "DoActivateTrigger")]
		public EventSet axisEvent = new EventSet();

		[System.Serializable]
		public class EventSet {

			[SerializeField, ContextMenuItem("DoAxisChange", "DoAxisChangeEmpty")] protected UnityEventFloat onAxisChange;

			public Func<float, bool> actionAxisChange;

			public int CountAxisChangeEvents => (onAxisChange?.GetPersistentEventCount() ?? 0) + (actionAxisChange?.GetInvocationList().Length ?? 0);
			public void AddAxisChangeEvent(Func<float, bool> a) { if (actionAxisChange != null) { actionAxisChange += a; } else { actionAxisChange = a; } }
			public bool DoAxisChange(float value) { onAxisChange?.Invoke(value); return (actionAxisChange?.Invoke(value) ?? false); }
			public bool DoAxisChangeEmpty() => DoAxisChange(0);
			public void RemoveAxisChange() { onAxisChange.RemoveAllListeners(); actionAxisChange = null; }

			public string GetDelegateText(UnityEventFloat ue, Func<float, bool> a) {
				StringBuilder text = new StringBuilder();
				if (ue != null) {
					for (int i = 0; i < ue.GetPersistentEventCount(); ++i) {
						if (text.Length > 0) { text.Append("\n"); }
						string t = ue.GetPersistentTarget(i)?.name ?? "<???>";
						text.Append(t).Append(".").Append(KBind.EventSet.FilterMethodName(ue.GetPersistentMethodName(i)));
					}
				}
				if (a != null) {
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
				string desc = GetDelegateText(onAxisChange, actionAxisChange);
				text.Append(desc);
				return text.ToString();
			}
		}

		/// <summary>
		/// additional requirements for the input
		/// </summary>
		public Func<bool> additionalRequirement;

		public bool IsAllowed() => !disable && (additionalRequirement == null || additionalRequirement.Invoke());

		/// <summary>
		/// describes a function to execute when a specific key-combination is pressed
		/// </summary>
		public AxBind(string axis, Func<float, bool> onAxisEvent, string name = null) : this(axis, name, onAxisEvent) { }

		/// <summary>
		/// describes functions to execute when a specific key is pressed/held/released
		/// </summary>
		public AxBind(string axis, string name = null, Func<float, bool> onAxisEvent = null, Func<bool> additionalRequirement = null)
			: this(new Axis(axis), name, onAxisEvent, additionalRequirement) {
		}

		/// <summary>
		/// describes functions to execute when a specific key-combination is pressed/held/released
		/// </summary>
		public AxBind(Axis axis, string name = null, Func<float, bool> onAxisEvent = null, Func<bool> additionalRequirement = null)
			: this(new[] { axis }, name, onAxisEvent, additionalRequirement) {
		}

		/// <summary>
		/// describes functions to execute when any of the specified key-combinations are pressed/held/released
		/// </summary>
		public AxBind(Axis[] axis, string name = null, Func<float, bool> onAxisEvent = null, Func<bool> additionalRequirement = null) {
			this.axis = axis;
			Normalize();
			this.name = name;
			AddEvents(onAxisEvent);
			if (additionalRequirement != null) {
				this.additionalRequirement = additionalRequirement;
			}
		}

		public void Normalize() { Array.ForEach(axis, ax => ax.Normalize()); }

		public void AddEvents(Func<float,bool> onAxisEvent = null) {
			if (onAxisEvent != null) { axisEvent.AddAxisChangeEvent(onAxisEvent); }
		}

		public void AddAxis(Axis[] axisToUse) {
			if (axis.Length == 0) { axis = axisToUse; } else {
				List<Axis> currentAxis = new List<Axis>(axis);
				currentAxis.AddRange(axisToUse);
				// remove duplicates
				for (int a = 0; a < currentAxis.Count; ++a) {
					for (int b = currentAxis.Count - 1; b > a; --b) {
						if (currentAxis[a].CompareTo(currentAxis[b]) == 0) {
							currentAxis.RemoveAt(b);
						}
					}
				}
				axis = currentAxis.ToArray();
			}
			Normalize();
			//Array.Sort(axis); Array.Reverse(axis); // put least complex key bind first (reverse of usual processing)
		}

		public void AddAxis(Axis[] axisToAdd, string nameToUse, Func<float,bool> onAxis = null) {
			if (axisToAdd != null) { AddAxis(axisToAdd); }
			if (string.IsNullOrEmpty(name)) { name = nameToUse; }
			AddEvents(onAxis);
		}

		public void AddAxis(string axisName, string nameToUse, Func<float, bool> onAxis = null) {
			AddAxis(new Axis[] { new Axis(axisName) }, nameToUse, onAxis);
		}
		public string ShortDescribe(string betweenKeyPresses = "\n") {
			if (axis == null || axis.Length == 0) return "";
			string text = "";
			for (int i = 0; i < axis.Length; ++i) {
				if (i > 0) text += betweenKeyPresses;
				text += axis[i].ToString();
			}
			return text;
		}

		//public int CompareTo(AxBind other) {
		//	if (other == null) return -1;
		//	// the simpler key binding, more likely to be pressed, should go first
		//	for (int i = 0; i < axis.Length; ++i) {
		//		if (other.axis.Length <= i) return 1;
		//		int cmp = axis[i].CompareTo(other.axis[i]);
		//		if (cmp == 0) { cmp = priority.CompareTo(other.priority); }
		//		if (cmp != 0) return cmp;
		//	}
		//	return 1;
		//}

		public override string ToString() { return $"{ShortDescribe(" || ")} \"{name}\""; }

		/// <returns>if the action succeeded (which may remove other actions from queue, due to priority)</returns>
		public bool DoAxis(float value) { return axisEvent.DoAxisChange(value); }

		public Axis GetActiveAxis() {
			bool allowedChecked = false;
			for (int i = 0; i < axis.Length; ++i) {
				if (axis[i].IsValueChanged()) {
					if (!allowedChecked) { if (!IsAllowed()) { return null; } allowedChecked = true; }
					axis[i].MarkValueAsKnown();
					return axis[i];
				}
			}
			return null;
		}
		public bool IsActive() { return GetActiveAxis() != null; }

		public void DoActivateTrigger() {
			if (axisEvent.CountAxisChangeEvents > 0) { DoAxis(0); }
		}

		//public void Sort() {
		//	Array.Sort(axis, (a, b) => a.priority.CompareTo(b.priority));
		//}

		//public void Start() {
		//	Sort();
		//}

		public void Update() {
			if (!IsAllowed()) return;
			for(int i = 0; i < axis.Length; ++i) {
				Axis ax = axis[i];
				if (ax.IsValueChanged()) {
					ax.MarkValueAsKnown();
					DoAxis(ax.cachedValue * ax.multiplier);
					break;
				}
			}
		}
	}

	[System.Serializable]
	public class Axis : IComparable<Axis> {
		public string name;
		public float multiplier = 1;
		//public int priority = 1000;
		private float knownValue = -1;
		public bool useRawValue = true;
		public bool useCachedValueIfMissingModifier = false;
		[HideInInspector] public float cachedValue;

		public KCombination.Modifier[] modifiers;

		public Axis(string name) { this.name = name; }

		public bool IsValueChanged() {
			bool isAllowed = modifiers == null || modifiers.Length == 0 || KCombination.IsSatisfiedHeld(modifiers);
			if (!isAllowed) {
				if (!useCachedValueIfMissingModifier) { cachedValue = 0; }
			} else {
				cachedValue = useRawValue ? GetValueRaw() : GetValue();
			}
			return (cachedValue != knownValue);
		}

		public void MarkValueAsKnown() { knownValue = cachedValue; }

		public float GetValue() { return Input.GetAxis(name); }
		public float GetValueRaw() { return Input.GetAxisRaw(name); }

		public int CompareTo(Axis other) {
			return name.CompareTo(other.name);
		}

		public void Normalize() {
			
		}

		public override string ToString() { return KCombination.ToString(modifiers)+name; }
	}
}