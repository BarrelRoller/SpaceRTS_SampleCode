using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour 
{
	struct UnitInfo
	{
		public int m_price;
		public GameObject m_prefab;
		public int m_popCost;

		public UnitInfo(int price, GameObject prefab, int popCost)
		{
			m_price = price;
			m_prefab = prefab;
			m_popCost = popCost;
		}

	}

	List<Unit> m_allUnits = new List<Unit>();
//	List<Unit> m_team1Units = new List<Unit>();
//	List<Unit> m_team2Units = new List<Unit>();
	List<Unit> m_selectedUnits = new List<Unit>();
	List<string> m_deployableUnits = new List<string>();
	List<List<Unit>> m_controlGroups = new List<List<Unit>>();

	const float c_unitCap = 50f;
	int m_team1PopulationCount = 0;
	int m_team2PopulationCount = 0;

	Dictionary<string, UnitInfo> m_unitInfo = new Dictionary<string, UnitInfo>();

	static UnitManager m_instance;
	public static UnitManager Get() {return m_instance;}

	string m_team1ReadyUnit;
	string m_team2ReadyUnit;

	public GameObject m_bars;
	public GameObject m_fighterSquad;
	public GameObject m_bomberSquad;
	public GameObject m_cruiser;
	public GameObject m_frigate;
	public GameObject m_capitalShip;
	public GameObject m_enemyFighterSquad;
	public GameObject m_enemyBomberSquad;
	public GameObject m_enemyCruiser;
	public GameObject m_enemyFrigate;
	public GameObject m_enemyCapitalShip;
	public GameObject m_fighterSquadOutline;
	public GameObject m_bomberSquadOutline;
	public GameObject m_cruiserOutline;
	public GameObject m_frigateOutline;
	public GameObject m_capitalShipOutline;

	void Start () 
	{
		m_instance = this;

		Unit[] units = GameObject.FindObjectsOfType<Unit>();
		foreach(Unit unit in units)
		{
			if(!unit.GetComponent<Fighter>() && !unit.GetComponent<Base>())
			{
				if(unit.tag == "Team1")
					m_team1PopulationCount++;
				else
					m_team2PopulationCount++;
				m_allUnits.Add(unit);
			}
		}

		for(int i = 0; i < 10; ++i)
		{
			//makes 10 empty lists for the 10 control groups
			m_controlGroups.Add(new List<Unit>());
		}

		m_unitInfo.Add("Fighter Squad", new UnitInfo(100, m_fighterSquad, 1));
		m_unitInfo.Add("Bomber Squad", new UnitInfo(200, m_bomberSquad, 1));
		m_unitInfo.Add("Cruiser", new UnitInfo(1000, m_cruiser, 3));
		m_unitInfo.Add("Frigate", new UnitInfo(5000, m_frigate, 5));
		m_unitInfo.Add("Capital Ship", new UnitInfo(10000, m_capitalShip, 10));
		m_unitInfo.Add("Enemy Fighter Squad", new UnitInfo(100, m_enemyFighterSquad, 1));
		m_unitInfo.Add("Enemy Bomber Squad", new UnitInfo(200, m_enemyBomberSquad, 1));
		m_unitInfo.Add("Enemy Cruiser", new UnitInfo(1000, m_enemyCruiser, 3));
		m_unitInfo.Add("Enemy Frigate", new UnitInfo(5000, m_enemyFrigate, 5));
		m_unitInfo.Add("Enemy Capital Ship", new UnitInfo(10000, m_enemyCapitalShip, 10));
		m_unitInfo.Add("Fighter Squad Outline", new UnitInfo(100, m_fighterSquadOutline, 0));
		m_unitInfo.Add("Bomber Squad Outline", new UnitInfo(200, m_bomberSquadOutline, 0));
		m_unitInfo.Add("Cruiser Outline", new UnitInfo(1000, m_cruiserOutline, 0));
		m_unitInfo.Add("Frigate Outline", new UnitInfo(5000, m_frigateOutline, 0));
		m_unitInfo.Add("Capital Ship Outline", new UnitInfo(10000, m_capitalShipOutline, 0));
	}

	void Update () 
	{

	}

	//selection
	public void SelectUnit(Unit unit, bool ctrlSelect)
	{
		if(!ctrlSelect)
			UnselectAll();

		unit.Select();
		m_selectedUnits.Add(unit);
	}
	
	public void UnselectAll()
	{
		foreach(Unit unit in m_selectedUnits)
		{
			unit.Deselect();
		}
		m_selectedUnits.Clear();
	}


	public void CreateControlGroup(int num)
	{
		Debug.Log("Creating group");
		foreach(Unit unit in m_selectedUnits)
		{
			m_controlGroups[num].Add(unit);
		}
	}

	public void SelectControlGroup(int num)
	{
		Debug.Log("Selecting group");
		UnselectAll();

		foreach(Unit unit in m_controlGroups[num])
		{
			SelectUnit(unit, true);
		}
	}

	public void SelectUnitsInRect(Vector3 startPos, Vector3 endPos)
	{
		Ray startRay = Camera.main.ScreenPointToRay(startPos);
		Ray endRay = Camera.main.ScreenPointToRay(endPos);
		RaycastHit hit1;
		RaycastHit hit2;
		Physics.Raycast(startRay, out hit1, 1000, 256);
		Physics.Raycast(endRay, out hit2, 1000, 256);

		UnselectAll();

		//Rect selectionField = new Rect(hit1.point.x, hit1.point.z, hit2.point.x - hit1.point.x, hit2.point.z - hit1.point.z);

		foreach(Unit unit in m_allUnits)
		{
			if(unit.tag != "Team1")
				continue;

			if(unit.transform.position.x < hit1.point.x && unit.transform.position.x > hit2.point.x)
			{
				if(unit.transform.position.z < hit1.point.z && unit.transform.position.z > hit2.point.z)
					SelectUnit(unit, true);
				else if(unit.transform.position.z > hit1.point.z && unit.transform.position.z < hit2.point.z)
					SelectUnit(unit, true);
			}
			else if(unit.transform.position.x > hit1.point.x && unit.transform.position.x < hit2.point.x)
			{
				if(unit.transform.position.z < hit1.point.z && unit.transform.position.z > hit2.point.z)
					SelectUnit(unit, true);
				else if(unit.transform.position.z > hit1.point.z && unit.transform.position.z < hit2.point.z)
					SelectUnit(unit, true);
			}

//			if(selectionField.Contains(unit.transform.position, true))
//				SelectUnit(unit, true);
		}
	}

	//handle all units
	public void AddNewUnit(Unit unit)
	{
		Debug.Log("Adding Unit");
		m_allUnits.Add(unit);
	}

	public void RemoveUnitFromGame(Unit unit)
	{
		m_allUnits.Remove(unit);
		m_selectedUnits.Remove(unit);
		int popAmout = 0;

		if(unit.GetComponent<FighterSquad>())
		{
			popAmout += 1;
		}
		else if(unit.GetComponent<BomberSquad>())
		{
			popAmout += 1;
		}
		else if(unit.GetComponent<Cruiser>())
		{
			popAmout += 3;
		}
		else if(unit.GetComponent<Frigate>())
		{
			popAmout += 5;
		}
		else if(unit.GetComponent<CapitalShip>())
		{
			popAmout += 10;
		}

		if(unit.tag == "Team1")
			m_team1PopulationCount -= popAmout;
		else
			m_team2PopulationCount -= popAmout;

		foreach(List<Unit> lists in m_controlGroups)
		{
			lists.Remove(unit);
		}
	}

	public Unit FindEnemyUnitInRange(Vector3 pos, float range, Vector3 forward, float firingArc, string teamName)
	{
		Unit target = null;
		float shortestDist = range + 1;

		foreach(Unit unit in m_allUnits)
		{
			if(unit.tag == teamName)
				continue;

			float dist = (unit.gameObject.transform.position - pos).magnitude;

			if(dist <= range && dist < shortestDist)
			{
				//forward and fire arc calc
				//if not in firearc, continue
				shortestDist = dist;
				target = unit;
			}
		}

		return target;
	}

	public List<Unit> FindAllUnitsInRange(Vector3 pos, float range, string teamName)
	{
		List<Unit> units = new List<Unit>();
		
		foreach(Unit unit in m_allUnits)
		{
			if(unit.tag != teamName)
				continue;
			
			float dist = (unit.gameObject.transform.position - pos).magnitude;
			
			if(dist <= range)
				units.Add(unit);
		}
		
		return units;
	}

	public List<Unit> GetAllTeamsUnits(string teamName)
	{
		List<Unit> units = new List<Unit>();

		foreach(Unit unit in m_allUnits)
		{
			if(unit.tag == teamName)
				units.Add(unit);
		}

		return units;
	}

	public Unit FindClosestSelectedUnitToPoint(Vector3 point)
	{
		Unit target = null;
		float shortestDist = 1000;
		
		foreach(Unit unit in m_selectedUnits)
		{
			float dist = (unit.gameObject.transform.position - point).magnitude;
			
			if(dist < shortestDist)
			{
				shortestDist = dist;
				target = unit;
			}
		}
		
		return target;
	}

	//movement
	public void SetDestination(Vector3 dest)
	{
		Unit leader = FindClosestSelectedUnitToPoint(dest);
		Vector3 destOffset = Vector3.zero;
		List<Unit> outOfFormationUnits = new List<Unit>();

		foreach(Unit unit in m_selectedUnits)
		{
			if(unit == leader)
				unit.SetDestination(dest);
			else
			{
				destOffset = unit.transform.position - leader.transform.position;
				if(destOffset.magnitude <= 100)// && CalculateAngle(leader.transform.forward, unit.transform.forward) <= 30)
					unit.SetDestination(dest + destOffset);
				else
				{
					outOfFormationUnits.Add(unit);
					//unit.SetDestination(dest + destOffset);
					//do other calculations, not this ^
				}
			}
		}
		if(outOfFormationUnits.Count > 0)
			CalcultateDestination(dest, outOfFormationUnits);
	}

	void CalcultateDestination(Vector3 dest, List<Unit> outOfFormation)
	{
		List<Unit> inFormation = new List<Unit>();
		foreach(Unit selected in m_selectedUnits)
		{
			if(!outOfFormation.Contains(selected))
				inFormation.Add(selected);
		}

		foreach(Unit unit in outOfFormation)
		{
			foreach(Unit inForm in inFormation)
			{

			}
			inFormation.Add(unit);
		}
	}

	float CalculateAngle(Vector3 mainDir, Vector3 targetDir)
	{
		float angle = Vector3.Dot(mainDir, targetDir);
		angle = (angle - 1) * -90;
		return angle;
	}

	//building/deployment
	public void OrderUnit(string unitName, string teamName)
	{
		//print("Ordering " + unitName);
		UnitInfo info;
		m_unitInfo.TryGetValue(unitName, out info);

		if(ResourceManager.Get().SpendResources(info.m_price, teamName))
		{
			//print(unitName + " Ordered");
			m_deployableUnits.Add(unitName);
		}
	}

	public void PrepareUnitForDeployment(string unitName, string teamName)
	{
		if(teamName == "Team1")
		{
			if(GetNumDeployableUnits(unitName) > 0)
			{
				m_team1ReadyUnit = unitName;
				GameManager.Get().ToggleDeploymentMode();
				//spawn outline
				UnitInfo info;
				m_unitInfo.TryGetValue(m_team1ReadyUnit + " Outline", out info);
				
				GameObject unit = (GameObject)Instantiate(info.m_prefab, Vector3.zero, info.m_prefab.transform.rotation);// Quaternion.identity);
			}
		}
		else
			m_team2ReadyUnit = unitName;
	}

	public void DeployUnit(Vector3 location, string teamName)
	{
		if(teamName == "Team1")
		{
			if(GetNumDeployableUnits(m_team1ReadyUnit) > 0 && m_team1PopulationCount < c_unitCap)
			{
				UnitInfo info;
				m_unitInfo.TryGetValue(m_team1ReadyUnit, out info);

				location.y = 0;

				Vector3 tempPos = new Vector3(-300, 0, -300);

				GameObject unit = (GameObject)Instantiate(info.m_prefab, tempPos, info.m_prefab.transform.rotation);// Quaternion.identity);
				unit.GetComponent<Unit>().WarpIn(location);
				AddNewUnit(unit.GetComponent<Unit>());

				GameObject bars = (GameObject)Instantiate(m_bars, Vector3.zero, Quaternion.identity);
				bars.GetComponent<BarHandler>().SetBarUnit(unit.GetComponent<Unit>());

				m_deployableUnits.Remove(m_team1ReadyUnit);
				m_team1PopulationCount += info.m_popCost;

				if(GetNumDeployableUnits(m_team1ReadyUnit) == 0)
					GameManager.Get().ToggleDeploymentMode();
			}
		}
		else
		{
			if(GetNumDeployableUnits(m_team2ReadyUnit) > 0 && m_team2PopulationCount < c_unitCap)
			{
				UnitInfo info;
				m_unitInfo.TryGetValue(m_team2ReadyUnit, out info);
				
				location.y = 0;
				
				Vector3 tempPos = new Vector3(-300, 0, -300);
				
				GameObject unit = (GameObject)Instantiate(info.m_prefab, tempPos, info.m_prefab.transform.rotation);// Quaternion.identity);
				unit.GetComponent<Unit>().WarpIn(location);
				AddNewUnit(unit.GetComponent<Unit>());
				
				GameObject bars = (GameObject)Instantiate(m_bars, Vector3.zero, Quaternion.identity);
				bars.GetComponent<BarHandler>().SetBarUnit(unit.GetComponent<Unit>());
				
				m_deployableUnits.Remove(m_team2ReadyUnit);
				m_team2PopulationCount += info.m_popCost;
			}
		}
	}

	public int GetNumDeployableUnits(string unitName)
	{
		int num = 0;
		foreach(string name in m_deployableUnits)
		{
			if(name == unitName)
				num ++;
		}
		return num;
	}

	//combat
	public void SetTarget(Unit target)
	{
		foreach(Unit unit in m_selectedUnits)
		{
			unit.SetTarget(target.Get());
		}
	}
}