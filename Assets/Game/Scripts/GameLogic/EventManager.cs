using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class EventManager : MonoBehaviour {
	/*
	Used to communicate directly to the current gametype monobehaviour as setting a hard reference to gametype is not a very clean way to access it, can easily be used for other purposses.
	*/
	private Dictionary <string, UnityEvent> eventDictionary;

	private static EventManager eventManager;

	public static EventManager instance
	{
		get
		{
			if (!eventManager)	//If there is not an eventmanager yet
			{
				eventManager = FindObjectOfType (typeof (EventManager)) as EventManager;	//Find one in the scene

				if (!eventManager)	//If no EventMananger could be found
				{
					Debug.LogError ("There needs to be one active EventManger script on a GameObject in your scene.");	//Display an error
				}
				else
				{
					eventManager.Init (); //If there is one found, init the found EventManager
				}
			}

			return eventManager;	//Return an EventManager as instance
		}
	}

	void Init ()
	{
		if (eventDictionary == null)	//If the is not yet an event dictionary to be found
		{
			eventDictionary = new Dictionary<string, UnityEvent>();	//Create a new dictionary
		}
	}

	public static void StartListening (string eventName, UnityAction listener)	//Start listening to events
	{
		UnityEvent thisEvent = null;
		if (instance.eventDictionary.TryGetValue (eventName, out thisEvent))
		{
			thisEvent.AddListener (listener);
		} 
		else
		{
			thisEvent = new UnityEvent ();
			thisEvent.AddListener (listener);
			instance.eventDictionary.Add (eventName, thisEvent);
		}
	}

	public static void StopListening (string eventName, UnityAction listener)	//Stop listening to events
	{
		if (eventManager == null) return;
		UnityEvent thisEvent = null;
		if (instance.eventDictionary.TryGetValue (eventName, out thisEvent))
		{
			thisEvent.RemoveListener (listener);
		}
	}

	public static void TriggerEvent (string eventName)						//Trigger an certain event
	{
		UnityEvent thisEvent = null;
		if (instance.eventDictionary.TryGetValue (eventName, out thisEvent))
		{
			thisEvent.Invoke ();
		}
	}
}