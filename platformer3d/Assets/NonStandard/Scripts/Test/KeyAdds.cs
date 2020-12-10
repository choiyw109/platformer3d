using NonStandard.Inputs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyAdds : MonoBehaviour
{
	public KBind keyMap;

	void Start()
    {
		KBind keyMap = new KBind(KCode.F, () => { Debug.Log("RESPECT"); return true; }, "pay respects");
		AppInput.AddListener(keyMap);
		AppInput.AddListener(this.keyMap);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
