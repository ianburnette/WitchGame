using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcDialogue : MonoBehaviour {
	[SerializeField] DialogueBase dialogueBase;
	[SerializeField] string[] dialogues;

	Collider col;

	void OnEnable() {
		col = GetComponent<Collider>();
	}

	void OnTriggerEnter(Collider other) => ToggleDialogueBubble(true);
	void OnTriggerExit(Collider other) => ToggleDialogueBubble(false);

	void ToggleDialogueBubble(bool state) {
		if (state)
			DialogueBase.instance.ShowBubble(col.bounds.max, dialogues[0]);
		else
			DialogueBase.instance.HideBubble();
	}
}
