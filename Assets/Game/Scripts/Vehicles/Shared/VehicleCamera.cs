using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleCamera : MonoBehaviour {
	
	public enum CameraType { ThirdPerson, FirstPerson };
	[Header("Vehicle Camera General Settings")]
	public CameraType CurrentCameraType;

	[Header("Third Person Camera Settings")]
	public Transform SeperateObject;
	private Quaternion s_OriginalRotation;
	public GameObject TPCamera;
	public Transform CameraAnchor;
	public bool TpCamActive = false;
	public Vector2 rotationRangeY = new Vector3(70, 70);
	public Vector2 rotationRangeX = new Vector3(70, 70);
	public float rotationSpeed = 10;
	public float dampingTime = 0.2f;
	public bool autoZeroVerticalOnMobile = true;
	public bool autoZeroHorizontalOnMobile = false;
	public bool relative = true;
	private Vector3 m_TargetAngles;
	private Vector3 m_FollowAngles;
	private Vector3 m_FollowVelocity;
	private Quaternion m_OriginalRotation;

	[Header("First Person Camera Settings")]
	public GameObject FPCamera;
	public bool FPCameraStatic = true;

	void OnEnable()
	{
		m_OriginalRotation = Quaternion.identity;
		if (SeperateObject != null) {
			s_OriginalRotation = Quaternion.identity;
		}
		TPCamera.SetActive(true);
		TpCamActive = true;
	}

	void OnDisable()
	{
		if (GameManager.instance.MatchActive) {
			FPCamera.SetActive (false);
			TPCamera.SetActive(false);
		}
	}


	void Update()
	{
		CameraSetController ();
		ThirdPersonCamera ();
	}
		
	void CameraSetController ()
	{
		if (PlayerInputManager.instance.ChangeCamera) {
			if (CurrentCameraType == CameraType.ThirdPerson) {
				CurrentCameraType = CameraType.FirstPerson;
				TPCamera.SetActive (false);
				FPCamera.SetActive (true);
				TpCamActive = false;
			} else {
				CurrentCameraType = CameraType.ThirdPerson;
				TPCamera.SetActive (true);
				FPCamera.SetActive (false);
				TpCamActive = true;
			}
		}
	}



	void ThirdPersonCamera()
	{
		if (TpCamActive || !TpCamActive && !FPCameraStatic) {
			// we make initial calculations from the original local rotation
			CameraAnchor.localRotation = m_OriginalRotation;

			// read input from mouse or mobile controls
			float inputH;
			float inputV;
			if (relative) {
				inputH = PlayerInputManager.instance.MouseX;
				inputV = PlayerInputManager.instance.MouseY;
				// wrap values to avoid springing quickly the wrong way from positive to negative
				if (m_TargetAngles.y > 180) {
					m_TargetAngles.y -= 360;
					m_FollowAngles.y -= 360;
				}
				if (m_TargetAngles.x > 180) {
					m_TargetAngles.x -= 360;
					m_FollowAngles.x -= 360;
				}
				if (m_TargetAngles.y < -180) {
					m_TargetAngles.y += 360;
					m_FollowAngles.y += 360;
				}
				if (m_TargetAngles.x < -180) {
					m_TargetAngles.x += 360;
					m_FollowAngles.x += 360;
				}

				#if MOBILE_INPUT
				// on mobile, sometimes we want input mapped directly to tilt value,
				// so it springs back automatically when the look input is released.
				if (autoZeroHorizontalOnMobile) {
				m_TargetAngles.y = Mathf.Lerp (-rotationRange.y * 0.5f, rotationRange.y * 0.5f, inputH * .5f + .5f);
				} else {
				m_TargetAngles.y += inputH * rotationSpeed;
				}
				if (autoZeroVerticalOnMobile) {
				m_TargetAngles.x = Mathf.Lerp (-rotationRange.x * 0.5f, rotationRange.x * 0.5f, inputV * .5f + .5f);
				} else {
				m_TargetAngles.x += inputV * rotationSpeed;
				}
				#else
				// with mouse input, we have direct control with no springback required.
				m_TargetAngles.y += inputH * rotationSpeed;
				m_TargetAngles.x += inputV * rotationSpeed;
				#endif

				// clamp values to allowed range
				m_TargetAngles.y = Mathf.Clamp (m_TargetAngles.y, rotationRangeY.x * 0.5f, rotationRangeY.y * 0.5f);
				m_TargetAngles.x = Mathf.Clamp (m_TargetAngles.x, rotationRangeX.x * 0.5f, rotationRangeX.y * 0.5f);
			} else {
				inputH = Input.mousePosition.x;
				inputV = Input.mousePosition.y;

				// set values to allowed range
				m_TargetAngles.y = Mathf.Lerp (rotationRangeY.x * 0.5f, rotationRangeY.y * 0.5f, inputH / Screen.width);
				m_TargetAngles.x = Mathf.Lerp (rotationRangeX.x * 0.5f, rotationRangeX.y * 0.5f, inputV / Screen.height);
			}

			// smoothly interpolate current values to target angles
			m_FollowAngles = Vector3.SmoothDamp (m_FollowAngles, m_TargetAngles, ref m_FollowVelocity, dampingTime);

			if (SeperateObject == null) {
				// update the actual gameobject's rotation
				CameraAnchor.localRotation = m_OriginalRotation * Quaternion.Euler (-m_FollowAngles.x, m_FollowAngles.y, 0);
			} else {
				CameraAnchor.localRotation = m_OriginalRotation * Quaternion.Euler (0, m_FollowAngles.y, 0);
				SeperateObject.localRotation = s_OriginalRotation * Quaternion.Euler (-m_FollowAngles.x, 0, 0);
			}
		} else {
			CameraAnchor.localRotation = m_OriginalRotation;
			if(SeperateObject != null)
			{
				SeperateObject.localRotation = s_OriginalRotation;
			}
		}
	} 
}
