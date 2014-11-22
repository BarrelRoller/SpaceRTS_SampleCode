using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour 
{
	public Texture2D m_selectionBoxTexture;

	Rect m_selectionBox = new Rect(0,0,0,0);
	float m_dragTimer = 0.0f;
	Vector3 m_dragStartPoint = -Vector3.one;

	void Update () 
	{
		if(!GameManager.Get().InGameMode())
			return;

		m_dragTimer += Time.deltaTime;


		//mouse
		if(Input.GetMouseButtonDown(0))
		{
			m_dragTimer = 0.0f;
			m_dragStartPoint = Input.mousePosition;

			Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, 1000))
			{
				if(GameManager.Get().InDeploymentMode())
				{
					//deploy unit
					UnitManager.Get().DeployUnit(hit.point, "Team1");
				}
				else
				{
					//select unit
					if(hit.collider.tag == "Team1") // || hit.collider.tag == "Team2")
					{
						if(Input.GetKey(KeyCode.LeftShift))
							UnitManager.Get().SelectUnit(hit.collider.gameObject.GetComponent<Unit>(), true);
						else
							UnitManager.Get().SelectUnit(hit.collider.gameObject.GetComponent<Unit>(), false);
						//hit.collider.gameObject.GetComponent<Unit>().Select();
					}
					else if(!Input.GetKey(KeyCode.LeftShift))
						UnitManager.Get().UnselectAll();
				}
			}
		}
		else if(Input.GetMouseButtonDown(1))
		{
			//command selected units
			Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, 1000))
			{
				if(hit.collider.tag == "Grid")
				{
					UnitManager.Get().SetDestination(hit.point);
				}
				else if(hit.collider.tag == "Team2")
				{
					UnitManager.Get().SetTarget(hit.collider.gameObject.GetComponent<Unit>());
				}
			}
		}

		if(m_dragTimer >= 0.1f && Input.GetMouseButton(0))
		{
			m_selectionBox = new Rect(m_dragStartPoint.x, Screen.height - m_dragStartPoint.y,
		                          	Input.mousePosition.x - m_dragStartPoint.x,
		                          	(Screen.height - Input.mousePosition.y) - (Screen.height - m_dragStartPoint.y));
			UnitManager.Get().SelectUnitsInRect(m_dragStartPoint, Input.mousePosition);
		}

		if(Input.GetMouseButtonUp(0))
		{
			if(m_dragTimer >= 0.1f)
				UnitManager.Get().SelectUnitsInRect(m_dragStartPoint, Input.mousePosition);
			m_dragStartPoint = -Vector3.one;
		}

		//keyboard
		if(Input.GetKeyDown(KeyCode.Alpha1))
		{
			if(Input.GetKey(KeyCode.LeftShift))
				UnitManager.Get().CreateControlGroup(1);
			else
				UnitManager.Get().SelectControlGroup(1);
		}

		if(Input.GetKeyDown(KeyCode.Escape))
		{
			if(GameManager.Get().InDeploymentMode())
				GameManager.Get().ToggleDeploymentMode();
			else
			{
				//GameManager.Get().PauseGame();
			}
		}
	}

	void OnGUI()
	{
		if(m_dragStartPoint != -Vector3.one && m_dragTimer >= 0.1f)
		{
			GUI.color = new Color(1,1,1,0.5f);
			GUI.DrawTexture(m_selectionBox, m_selectionBoxTexture);
		}
	}
}
