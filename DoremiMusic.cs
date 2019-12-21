using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Doremi Multichannel Loop", menuName = "Doremi Audio Engine/Multichannel Loop")]
public class DoremiMusic : ScriptableObject {
	public List<AudioClip> audio;
	public int loopEndSamples;
	public int loopStartSamples;
}