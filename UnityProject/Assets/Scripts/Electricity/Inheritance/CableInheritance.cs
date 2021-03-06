using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CableInheritance : InputTrigger, IDeviceControl
{
	public bool SelfDestruct = false;
	public WiringColor CableType;
	public Connection WireEndA { get { return wireConnect.WireEndA; } set { wireConnect.WireEndA = value; } }
	public Connection WireEndB { get { return wireConnect.WireEndB; } set { wireConnect.WireEndB = value; } }
	public WireConnect wireConnect;
	public PowerTypeCategory ApplianceType;
	public HashSet<PowerTypeCategory> CanConnectTo;

	public override bool Interact(GameObject originator, Vector3 position, string hand)
	{
		if (!CanUse(originator, hand, position, false))
		{
			return false;
		}
		if (!isServer)
		{
			InteractMessage.Send(gameObject, hand);
		}
		else {
			var slot = InventoryManager.GetSlotFromOriginatorHand(originator, hand);
			var Wirecutter = slot.Item?.GetComponentInChildren<WirecutterTrigger>();
			if (Wirecutter != null)
			{
				toDestroy();
			}
		}
		return true;
	}
	public void PotentialDestroyed()
	{
		if (SelfDestruct)
		{
			//Then you can destroy
		}
	}
	//[ContextMethod("Destroy cable", "x")]
	public void toDestroy()
	{
		GetComponent<CustomNetTransform>().DisappearFromWorldServer();
		//gameObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
		ElectricalSynchronisation.StructureChange = true;
		ElectricalSynchronisation.NUCableStructureChange.Add(this);
	}

	void Awake()
	{
		wireConnect = GetComponent<WireConnect>();
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		_OnStartServer();
		//wireConnect.InData.ControllingDevice = this;
	}
	public virtual void _OnStartServer()
	{
	}
	public virtual void PowerUpdateStructureChange()
	{
		wireConnect.FlushConnectionAndUp();
		wireConnect.registerTile.UnregisterServer();
		PoolManager.PoolNetworkDestroy(gameObject);
	}
	//FIXME: Objects at runtime do not get destroyed. Instead they are returned back to pool
	//FIXME: that also renderers IDevice useless. Please reassess
	public void OnDestroy()
	{
		//		ElectricalSynchronisation.StructureChangeReact = true;
		//		ElectricalSynchronisation.ResistanceChange = true;
		//		ElectricalSynchronisation.CurrentChange = true;
		SelfDestruct = true;
		//Making Invisible
	}
	public void TurnOffCleanup()
	{
	}
	/// <summary>
	///     If you have some tray goggles on then set this bool to true to get the right sprite.
	///     I guess you still need to faff about with display layers but that isn't my issue.
	/// </summary>
	public bool TRay;

	public void damEditor()
	{

		SetSprite();
	}
	// Use this for initialization
	private void Start()
	{
		//FIXME this breaks wires that were placed via unity editor:
		// need to address when we allow users to add wires at runtime
		ElectricalSynchronisation.StructureChange = true;
		SetDirection(WireEndB, WireEndA, CableType);
		//FindOverlapsAndCombine();
	}


	public void FindOverlapsAndCombine()
	{
		//Logger.Log("A");
		if (WireEndA == Connection.Overlap | WireEndB == Connection.Overlap)
		{
			//Logger.Log("B");
			bool isA;
			if (WireEndA == Connection.Overlap)
			{
				isA = true;
			}
			else {
				isA = false;
			}
			List<ElectricalOIinheritance> Econns = new List<ElectricalOIinheritance>();
			//Logger.Log(wireConnect.registerTile.Position.ToString());
			var IEnumerableEconns = wireConnect.matrix.GetElectricalConnections(wireConnect.registerTile.PositionServer);
			foreach (var T in IEnumerableEconns) {
				Econns.Add(T);
			}
			int i = 0;
			if (Econns != null)
			{
				while (!(i >= Econns.Count)){
				//Econns[i]
					if (ApplianceType == Econns[i].InData.Categorytype)
					{
						//Logger.Log("C");
						if (wireConnect != Econns[i])
						{
							//Logger.Log("D");
							if (Econns[i].WireEndA == Connection.Overlap)
							{
								if (isA)
								{
									//Logger.Log("B");
									WireEndA = Econns[i].WireEndB;
								}
								else {
									//Logger.Log("C");
									WireEndB = Econns[i].WireEndB;
								}
								SetDirection(WireEndB, WireEndA, CableType);
								ElectricalCableMessage.Send(gameObject, WireEndA, WireEndB, CableType);
								Econns[i].gameObject.GetComponent<CableInheritance>().toDestroy();
							}
							else if (Econns[i].WireEndB == Connection.Overlap)
							{
								if (isA)
								{
									//Logger.Log("E");
									WireEndA = Econns[i].WireEndA;
								}
								else {
									//Logger.Log("F");
									WireEndB = Econns[i].WireEndA;
								}
								SetDirection(WireEndB, WireEndA, CableType);
								ElectricalCableMessage.Send(gameObject, WireEndA, WireEndB, CableType);
								Econns[i].gameObject.GetComponent<CableInheritance>().toDestroy();
							}
						}
					}
					i++;
				}
			}
		}
	}


	public void SetDirection(int WireEndB)
	{
		//this.WireEndB = WireEndB;
		//WireEndA = 0;
		//SetSprite();
	}

	public void SetDirection(Connection REWireEndA, Connection REWireEndB, WiringColor RECableType = WiringColor.unknown)
	{

		if (REWireEndA == REWireEndB) {
			//Logger.LogError("whY!!!! Don't make it end and start in the same place!" + REWireEndA + " " + REWireEndB , Category.Electrical);
			Logger.LogWarning(" Catching Wire connection both at the same place " + REWireEndA + " " + REWireEndB , Category.Electrical);
			return;
		}
		if (!(RECableType == WiringColor.unknown))
		{
			CableType = RECableType;
		}
		WireEndA = REWireEndA;
		WireEndB = REWireEndB;
		//Logger.Log(WireEndB.ToString() + " <WireEndB and WireEndA> " + WireEndA.ToString(), Category.Electrical);
		SetSprite();
		if (isServer) {
			FindOverlapsAndCombine();
		}
	}


	[ContextMenu("FindConnections")]
	private void SetSprite()
	{
		//WireEndA;
		//WireEndB;

		Sprite[] Color = SpriteManager.WireSprites[CableType.ToString()];
		if (Color == null)
		{
			SpriteManager.Instance.InitWireSprites();
			Color = SpriteManager.WireSprites[CableType.ToString()];
		}
		SpriteRenderer SR = gameObject.GetComponentInChildren<SpriteRenderer>();
		//the red sprite is spliced differently than the rest for some reason :^(
		string Compound;
		if (WireEndA < WireEndB)
		{
			Compound = WireEndA + "_" + WireEndB;
		}
		else {
			Compound = WireEndB + "_" + WireEndA;
		}
		//Logger.Log(Compound + "?");
		int spriteIndex = WireDirections.GetSpriteIndex(Compound);
		if (TRay)
		{
			spriteIndex += 36;
		}

		SR.sprite = Color[spriteIndex];
		if (SR.sprite == null)
		{
			Logger.LogError("aww man, it didn't return anything SetSprite Is acting up", Category.Electrical);
		}
	}
}
