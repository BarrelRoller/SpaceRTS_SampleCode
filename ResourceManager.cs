using UnityEngine;
using System.Collections;

public class ResourceManager : MonoBehaviour 
{
	struct ResourceInfo
	{
		public int m_resources;
		public int m_numMines;

		public ResourceInfo(int resources, int numMines)
		{
			m_resources = resources;
			m_numMines = numMines;
		}
	}

	static ResourceManager m_instance;
	public static ResourceManager Get(){return m_instance;}

	const int c_baseGatherRate = 10;
	const int c_mineGatherRate = 30;
	const float c_gainRate = 1.0f;

	ResourceInfo m_team1Info = new ResourceInfo(800,0);
	ResourceInfo m_team2Info = new ResourceInfo(800,0);

	float m_timer;

	void Start () 
	{
		m_instance = this;
		m_timer = 0;
	}

	void Update () 
	{
		m_timer += Time.deltaTime;

		if(m_timer >= c_gainRate)
		{
			m_timer = 0.0f;
			m_team1Info.m_resources += c_baseGatherRate + (m_team1Info.m_numMines * c_mineGatherRate);
			m_team2Info.m_resources += c_baseGatherRate + (m_team2Info.m_numMines * c_mineGatherRate);
		}
	}

	public bool SpendResources(int amount, string team)
	{
		if(team == "Team1")
		{
			if(amount <= m_team1Info.m_resources)
			{
				m_team1Info.m_resources -= amount;
				return true;
			}
		}
		else
		{
			if(amount <= m_team2Info.m_resources)
			{
				Debug.Log("Team 2 spent");
				m_team2Info.m_resources -= amount;
				return true;
			}
		}
		return false;
	}

	public void AddMine(string team)
	{
		if(team == "Team1")
			m_team1Info.m_numMines++;
		else
			m_team2Info.m_numMines++;
	}

	public void RemoveMine(string team)
	{
		if(team == "Team1")
			m_team1Info.m_numMines--;
		else
			m_team2Info.m_numMines--;
	}

	public int GetNumMines(string team)
	{
		if(team == "Team1")
			return m_team1Info.m_numMines;
		return m_team2Info.m_numMines;
	}

	public int GetResources(string team)
	{
		if(team == "Team1")
			return m_team1Info.m_resources;
		return m_team2Info.m_resources;
	}
}
