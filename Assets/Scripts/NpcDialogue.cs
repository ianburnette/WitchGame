using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcDialogue : Promptable  {
	[SerializeField] List<string> dialogues;
	Collider myCollider;

	public List<string> Dialogues { get { return dialogues; } set { dialogues = value; }}
	
	
	public Collider MyCollider
	{
		get { return myCollider; }
		set { myCollider = value; }
	}

	void OnEnable() => MyCollider = GetComponent<Collider>();
	void OnTriggerEnter(Collider other) => ToggleInputPrompt(true);//ToggleDialogueBubble(true);
	void OnTriggerExit(Collider other) => ToggleInputPrompt(false);

	void Start()
	{
		promptableCollider = GetComponent<Collider>();
	}

	void ToggleInputPrompt(bool state)
	{
		InputPrompt.instance.Prompt = state ? this : null;
		DialogueBase.instance.InRange = state;
		DialogueBase.instance.CurrentDialogue = state ? this : null;
	}
}
