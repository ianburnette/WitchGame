using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcDialogue : MonoBehaviour {
	[SerializeField] List<string> dialogues;
	Collider col;

	public List<string> Dialogues
	{
		get { return dialogues; }
		set { dialogues = value; }
	}

	void OnEnable() => col = GetComponent<Collider>();
	void OnTriggerEnter(Collider other) => ToggleDialogueBubble(true);
	void OnTriggerExit(Collider other) => ToggleDialogueBubble(false);

	void ToggleDialogueBubble(bool state) {
		if (state)
			DialogueBase.instance.ShowBubble(col.bounds.max, this);
		else
			DialogueBase.instance.HideBubble();
	}
}
