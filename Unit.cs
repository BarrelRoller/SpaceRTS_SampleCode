using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour 
{
	//protected
	protected float m_health;
	protected float m_shields;
	protected float m_shieldRegen;
	protected float m_shieldRegenRate;
	protected float m_damage;
	protected float m_speed;

	protected float m_fireRate;
	protected float m_fireRange;

	protected float m_avoidanceRange;

	protected float m_maxRot;
	protected bool m_moveAndRoatate;

	protected bool m_hasHardpoints;
	//protected float m_hardPointHealth;

	//public 
	public List<HardPoint> m_hardpoints = new List<HardPoint>();

	public List<ParticleSystem> m_engineParticles;
	//public LayerMask m_mask;
	public GameObject m_selectionCircle;
	public GameObject m_laser;

	//private
	bool m_rotationNeeded;
	bool m_selected;
	bool m_canMove;
	
	float m_fireTimer = 0.0f;
	float m_shieldTimer = 0.0f;
	float m_maxHealth, m_maxShields;

	protected Unit m_target;
	protected Unit m_tempTarget;
	protected HardPoint m_hardpointTarget;
	protected HardPoint m_tempHardpointTarget;

	protected Vector3 m_destination;

	const float c_warpInTime = 1f;
	const float c_warpDistance = 400f;
	float m_warpInTimer = 0.0f;
	Vector3 m_warpDest;
	bool m_warpingIn = false;

	protected void Start () 
	{
		m_selectionCircle.renderer.enabled = false;
		m_destination = transform.position;
		m_rotationNeeded = true;
		m_maxHealth = m_health;
		m_maxShields = m_shields;
		m_canMove = true;
		//m_destination = null;
	}

	protected void Update () 
	{
		if(m_warpingIn)
		{
			HandleWarpIn();
			return;
		}

		CheckHealth();

		UpdateShields();

		UpdateMovement();

		UpdateCombat();
	}

	void UpdateMovement()
	{
		Vector3 dist = m_destination - transform.position;
		
		if(dist.magnitude > 0.25f)
		{
			float enginePower = 1f;
			if(m_hasHardpoints)
				enginePower = EnginePowerCheck();

			if(m_rotationNeeded)
			{
				Vector3 direction = m_destination - transform.position;
				direction.Normalize();
				UpdateRotation(direction);

				if(m_canMove || m_moveAndRoatate)
					transform.position += transform.forward * Time.deltaTime * (m_speed * enginePower);
			}
			else
				transform.position += transform.forward * Time.deltaTime * (m_speed * enginePower);

			PlayEngineParticles();
		}
		else
			StopEngineParticles();
	}

	float EnginePowerCheck()
	{
		foreach(HardPoint hp in m_hardpoints)
		{
			if(hp.m_type == HardPoint.Type.ENGINE)
				if(!hp.DesroyedCheck())
					return 1f;
		}
		return 0.2f;
	}

	void UpdateRotation(Vector3 targetDir)
	{
		targetDir.Normalize();

		float rot = Vector3.Angle(transform.forward, targetDir);

		if(rot <= 45)
			m_canMove = true;
		else
			m_canMove = false;
		
		if(Vector3.Cross(targetDir, transform.forward).y > 0)
		{
			if(rot < m_maxRot)
			{
				transform.Rotate(Vector3.up, -rot);
				m_rotationNeeded = false;
			}
			else
			{
				transform.Rotate(Vector3.up, -m_maxRot);
			}
		}
		else
		{
			if(rot < m_maxRot)
			{
				transform.Rotate(Vector3.up, rot);
				m_rotationNeeded = false;
			}
			else
			{
				transform.Rotate(Vector3.up, m_maxRot);
			}
		}
	}

	protected virtual void UpdateCombat()
	{
		m_fireTimer += Time.deltaTime;

		if(m_target != null)
		{
			if(m_hasHardpoints)
			{
				if(!CheckHarpointsInRange())
					MoveIntoRange();
				return;
			}

			Vector3 targetDestination = Vector3.zero;

			if(m_target.HasHardpoints())
			{
				if(m_hardpointTarget == null || m_hardpointTarget.DesroyedCheck())
					m_hardpointTarget = m_target.GetClosestHardpoint(transform.position);
				
				if(m_hardpointTarget != null)
					targetDestination = CalculateInterceptionCourse(m_hardpointTarget.gameObject.transform.position, 
				                                                m_target.GetVelocity(), transform.position, 
				                                                m_laser.GetComponent<Laser>().GetSpeed());
			}
			else
				targetDestination = CalculateInterceptionCourse(m_target.gameObject.transform.position, 
				                                                m_target.GetVelocity(), transform.position, 
				                                                m_laser.GetComponent<Laser>().GetSpeed());
			
			Vector3 distance = targetDestination - transform.position;
			
			if(m_fireTimer >= m_fireRate && distance.magnitude <= m_fireRange)
			{
				m_fireTimer = 0.0f;
				Vector3 t_dir = distance;
				t_dir.Normalize();
				
				GameObject t_laser = (GameObject)Instantiate(m_laser, transform.position, Quaternion.LookRotation(t_dir));
				t_laser.tag = tag + " Laser";
			}
			else if(distance.magnitude > m_fireRange)
			{
				MoveIntoRange();
//				Vector3 dir =  transform.position - m_target.transform.position;
//				dir.Normalize();
//				SetDestination(m_target.transform.position + ((m_fireRange * .9f) * dir), true); //get withing 90% of fire distance
			}
		}
		else
		{
			SearchForTarget();
			if(m_tempTarget != null)
			{
				Vector3 targetDestination = Vector3.zero;
				
				if(m_tempTarget.HasHardpoints())
				{
					if(m_tempHardpointTarget == null || m_tempHardpointTarget.DesroyedCheck())
						m_tempHardpointTarget = m_tempTarget.GetClosestHardpoint(transform.position);//m_owner.FindNewHardpoint();
					
					//HardPoint hardpointTarget = m_owner.GetHardpointTarget();
					
					targetDestination = CalculateInterceptionCourse(m_tempHardpointTarget.gameObject.transform.position, 
					                                                m_tempTarget.GetVelocity(), transform.position, 
					                                                m_laser.GetComponent<Laser>().GetSpeed());
				}
				else
					targetDestination = CalculateInterceptionCourse(m_tempTarget.gameObject.transform.position, 
					                                                m_tempTarget.GetVelocity(), transform.position, 
					                                                m_laser.GetComponent<Laser>().GetSpeed());
				
				Vector3 distance = targetDestination - transform.position;
				
				if(m_fireTimer >= m_fireRate && distance.magnitude <= m_fireRange)
				{
					m_fireTimer = 0.0f;
					Vector3 t_dir = distance;
					t_dir.Normalize();
					
					GameObject t_laser = (GameObject)Instantiate(m_laser, transform.position, Quaternion.LookRotation(t_dir));
					t_laser.tag = tag + " Laser";
				}
			}
		}
	}

	void MoveIntoRange()
	{
		Vector3 dir;
//		if(m_target.HasHardpoints())
//			dir = transform.position - m_target.GetClosestHardpoint(transform.position).transform.position;
//		else
			dir = transform.position - m_target.transform.position;
		float percent = dir.magnitude / m_fireRange;
		percent = Mathf.Clamp(percent, 0, .9f);
		dir.Normalize();
		SetDestination(m_target.transform.position + ((m_fireRange * percent) * dir), true); //get withing 90% of fire distance
	}

	protected virtual void CheckHealth()
	{
		if(m_hasHardpoints)
		{
			m_health = CalculateHarpointHealth();
		}
		if(m_health <= 0.0f)
		{
			UnitManager.Get().RemoveUnitFromGame(this);
			Destroy(this.gameObject);
		}
	}

	void UpdateShields()
	{
		if(ShieldCheck())
		{
			m_shieldTimer += Time.deltaTime;

			if(m_shieldTimer >= m_shieldRegenRate)
			{
				m_shieldTimer = 0.0f;
				m_shields += m_shieldRegen;
				if(m_shields > m_maxShields)
					m_shields = m_maxShields;
			}
		}
		else
			m_shields = 0.0f;
	}

	bool ShieldCheck()
	{
		foreach(HardPoint hp in m_hardpoints)
		{
			if(hp.m_type == HardPoint.Type.SHIELD_GENERATOR)
				if(hp.DesroyedCheck())
					return false;
		}
		return true;
	}

	protected float CalculateHarpointHealth()
	{
		float health = 0;
		foreach(HardPoint hp in m_hardpoints)
		{
			health += hp.GetHealth();
		}
		return health;
	}

	void PlayEngineParticles()
	{
		foreach(ParticleSystem ps in m_engineParticles)
		{
			if(!ps.isPlaying)
				ps.Play();
		}
	}

	void StopEngineParticles()
	{
		foreach(ParticleSystem ps in m_engineParticles)
		{
			if(ps.isPlaying)
				ps.Stop();
		}
	}

	protected Vector3 CalculateInterceptionCourse(Vector3 targetPos, Vector3 targetVel, Vector3 shotPos, float shotSpeed)
	{
		float ox = targetPos.x - shotPos.x;
		float oz = targetPos.z - shotPos.z;
		
		float h1 = Mathf.Pow(targetVel.x, 2) +  Mathf.Pow(targetVel.z, 2) - Mathf.Pow(shotSpeed, 2);
		float h2 = ox * targetVel.x + oz * targetVel.z;
		float t;
		
		if(h1 == 0)
			t = -(Mathf.Pow(ox, 2) + Mathf.Pow(oz, 2)) / 2*h2;
		else
		{
			float minusPHalf = -h2 / h1;
			float discrim = minusPHalf * minusPHalf - (ox * ox + oz * oz) / h1;
			
			if(discrim < 0)
			{
				return Vector3.zero;
			}
			
			float root = Mathf.Sqrt(discrim);
			
			float t1 = minusPHalf + root;
			float t2 = minusPHalf - root;
			
			float t_min = Mathf.Min(t1, t2);
			float t_max = Mathf.Max(t1, t2);
			
			t = t_min > 0 ? t_min : t_max;
			if( t < 0)
			{
				return Vector3.zero;
			}
		}
		return new Vector3(targetPos.x + t * targetVel.x, targetPos.y, targetPos.z + t * targetVel.z);
	}
	
	protected void TakeDamage(float damage, bool shieldPen)
	{
		//print (damage);
		if(shieldPen)
		{
			m_health -= damage;
		}
		else
		{

			if(m_shields >= damage)
				m_shields -= damage;
			else
			{
				m_health -= damage - m_shields;
				m_shields = 0;
			}
		}
	}

	void SearchForTarget()
	{
		if(m_tempTarget == null)
		{
			m_tempTarget = UnitManager.Get().FindEnemyUnitInRange(transform.position, m_fireRange, Vector2.one, 360, tag);
		}
	}
	
	float ShieldCheck(float damage)
	{
		if(m_shields > damage)
		{
			m_shields -= damage;
			return 0;
		}
		m_shields = 0;
		return damage - m_shields;
	}

	bool CheckHarpointsInRange()
	{
		for(int i = 0; i < m_hardpoints.Count; ++i)
		{
			if(m_hardpoints[i].OutOfRange())
				return false;
		}
		return true;
	}

	//public
	public virtual void SetDestination(Vector3 dest)
	{
		m_destination = dest;
		m_rotationNeeded = true;
		m_target = null;
	}

	public virtual void SetDestination(Vector3 dest, bool keepTarget)
	{
		m_destination = dest;
		m_rotationNeeded = true;
		if(!keepTarget)
			m_target = null;
	}
	
	public Vector3 GetDestination()
	{
		return m_destination;
	}

	public virtual void Select()
	{
		m_selected = true;
		m_selectionCircle.renderer.enabled = true;
	}

	public virtual void Deselect()
	{
		m_selected = false;
		m_selectionCircle.renderer.enabled = false;
	}

	public virtual void SetTarget(Unit target)
	{
		m_target = target;
		m_destination = transform.position;
	}

	public void SetTempTarget(Unit target)
	{
		m_tempTarget = target;
	}

	public void SetHardpointTarget(HardPoint hardpoint)
	{
		m_target = hardpoint.GetOwner();
		m_hardpointTarget = hardpoint;
	}

	public bool HasHardpoints()
	{
		return m_hasHardpoints;
	}

	//Get functions
	public float GetHealth()
	{
		return m_health;
	}

	public float GetMaxHealth()
	{
		return m_maxHealth;
	}

	
	public float GetShields()
	{
		return m_shields;
	}
	
	public float GetMaxShields()
	{
		return m_maxShields;
	}

	public float GetHardpointHealth()
	{
		return m_maxHealth / m_hardpoints.Count;
	}

	public float GetFireRange()
	{
		return m_fireRange;
	}

	public float GetFireRate()
	{
		return m_fireRate;
	}

	public virtual Vector3 GetVelocity()
	{
		Vector3 dist = m_destination - transform.position;

		float enginePower = 1f;
		if(m_hasHardpoints)
			enginePower = EnginePowerCheck();
		
		if(dist.magnitude > 0.25f)
			return transform.forward * (m_speed * enginePower);
		
		return Vector3.zero;
	}

	public Unit Get()
	{
		return this;
	}

	public Unit GetTarget()
	{
		return m_target;
	}

	public HardPoint GetHardpointTarget()
	{
		return m_hardpointTarget;
	}
	
	public HardPoint GetRandomHardpoint()
	{
		for(int i = 0; i < m_hardpoints.Count; ++i)
		{
			if(!m_hardpoints[i].DesroyedCheck())
			{
				return m_hardpoints[i]; //not random, come back and change
			}
		}
		return null;
	}

	public HardPoint GetClosestHardpoint(Vector3 pos)
	{
		HardPoint closest = null;
		float shortestDist = Mathf.Infinity;

		for(int i = 0; i < m_hardpoints.Count; ++i)
		{
			if(!m_hardpoints[i].DesroyedCheck())
			{
				float dist = (m_hardpoints[i].transform.position - pos).magnitude;
				if(dist < shortestDist)
				{
					shortestDist = dist;
					closest = m_hardpoints[i];
				}
			}
		}
		return closest;
	}

	public List<HardPoint> GetHardpointList()
	{
		return m_hardpoints;
	}

	public GameObject GetLaserPrefab()
	{
		return m_laser;
	}

	public void FindNewHardpoint()
	{
		if(m_target != null)
			if(m_target.HasHardpoints())
				if(m_hardpointTarget == null || m_hardpointTarget.DesroyedCheck())
					m_hardpointTarget = m_target.GetClosestHardpoint(transform.position);
	}

	public float GetAvoidanceRange()
	{
		return m_avoidanceRange;
	}

	public virtual void WarpIn(Vector3 warpInLocation)
	{
		m_warpDest = warpInLocation;
		transform.position = m_warpDest - (transform.forward * c_warpDistance);
		m_warpingIn = true;
		m_warpInTimer = 0.0f;
	}

	void HandleWarpIn()
	{
		m_warpInTimer += Time.deltaTime;
		Vector3 start = m_warpDest - (transform.forward * c_warpDistance);
		transform.position = Vector3.Lerp(start, m_warpDest, m_warpInTimer / c_warpInTime);

		if(m_warpInTimer / c_warpInTime >= 1)
		{
			m_warpingIn = false;
			m_destination = m_warpDest;
		}
	}

	//collision
	protected virtual void OnTriggerEnter(Collider i_col)
	{
		if(tag == "Team1")
		{
			if(i_col.tag == "Team2 Laser")
			{
				if(m_hasHardpoints)
				{
					float damage = ShieldCheck(i_col.gameObject.GetComponent<Laser>().GetDamage());
					if(damage > 0)
						i_col.gameObject.GetComponent<Laser>().SetDamage(damage);
					else
						Destroy(i_col.gameObject);
				}
				else
				{
					TakeDamage(i_col.gameObject.GetComponent<Laser>().GetDamage(), false);
					Destroy(i_col.gameObject);
				}
			}
			else if(i_col.tag == "Team2 Missle")
			{
				if(!m_hasHardpoints)
				{
					TakeDamage(i_col.gameObject.GetComponent<Torpedo>().GetDamage(), true);
					Destroy(i_col.gameObject);
				}
			}
		}
		else
		{
			if(i_col.tag == "Team1 Laser")
			{
				if(m_hasHardpoints)
				{
					float damage = ShieldCheck(i_col.gameObject.GetComponent<Laser>().GetDamage());
					if(damage > 0)
						i_col.gameObject.GetComponent<Laser>().SetDamage(damage);
					else
						Destroy(i_col.gameObject);
				}
				else
				{
					TakeDamage(i_col.gameObject.GetComponent<Laser>().GetDamage(), false);
					Destroy(i_col.gameObject);
				}
			}
			else if(i_col.tag == "Team1 Missle")
			{
				if(!m_hasHardpoints)
				{
					TakeDamage(i_col.gameObject.GetComponent<Torpedo>().GetDamage(), true);
					Destroy(i_col.gameObject);
				}
			}
		}
	}
}