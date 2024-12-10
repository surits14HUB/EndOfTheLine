using System;
using System.Collections;
using Fusion;
using RacingOnline.Networking;
using RacingOnline.Race;
using UnityEngine;

namespace RacingOnline.Race
{
    public enum Axis
    {
        X,
        Y,
        Z
    }
    /// <summary>
    /// This entire script is referenced from the Fusion SDK's KartController
    /// Only the required functionality is added for simplicity purposes and minor modifications are made to suit this racing project
    /// </summary>
    public class NetworkCar : NetworkBehaviour
    {
        #region Variables & Properties

        [SerializeField] Transform camHolder;
        [Networked] internal string username { get; set; }
        private Camera carCam;
        [Networked] private CarInput.NetworkInputData Inputs { get; set; }
        [SerializeField] Rigidbody Rigidbody;
        public Transform tireFL, tireFR, tireBL, tireBR;
        [Networked] public float AppliedSpeed { get; set; }
        [SerializeField] internal NetworkRaceTime networkRaceTime;
        public bool HasFinishedRace => networkRaceTime.EndRaceTick != 0;
        public bool HasStartedRace => networkRaceTime.StartRaceTick != 0;
        public bool CanDrive => HasStartedRace && !HasFinishedRace;
        [Networked] public float MaxSpeed { get; set; }
        public float maxSpeedNormal;
        public float reverseSpeed;
        public float acceleration;
        public float deceleration;
        [Networked] private float SteerAmount { get; set; }
        public float maxSteerStrength = 35;
        public float steerAcceleration;
        public float steerDeceleration;
        public Transform model;
        [Tooltip("X-Axis: steering\nY-Axis: velocity\nCoordinate space is normalized")]
        public AnimationCurve steeringCurve = AnimationCurve.Linear(0, 0, 1, 1);
        private float RealSpeed => transform.InverseTransformDirection(Rigidbody.linearVelocity).z;
        [Networked] public float TireYaw { get; set; }
        private bool hasSpawned;

        #endregion

        #region Monobehaviour Methods

        private void Update()
        {
            if (hasSpawned)
            {
                UpdateTireRotation();
            }
        }

        #endregion

        #region Custom Methods

        /// <summary>
        /// As soon as the car is spawned, the main camera is assigned and added as a child to the network car with input authority
        /// </summary>
        public override void Spawned()
        {
            base.Spawned();

            if (Object.HasInputAuthority)
            {
                SetCamera();
            }
            networkRaceTime.OnRaceCompleted = RaceTrackManager.Instance.OnCarCompletedRace;
            MaxSpeed = maxSpeedNormal;

            hasSpawned = true;
        }
        private void SetCamera()
        {
            carCam = Camera.main;
            carCam.transform.SetParent(camHolder);
            carCam.transform.localPosition = Vector3.zero;
            carCam.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
        /// <summary>
        /// Updates the tire movement when ever the car is accelerated or reversed
        /// </summary>
        private void UpdateTireRotation()
        {
            if (CanDrive)
            {
                tireFL.Rotate(90 * Time.deltaTime * AppliedSpeed * 0.5f, 0, 0);
                tireFR.Rotate(90 * Time.deltaTime * AppliedSpeed * 0.5f, 0, 0);
                tireBL.Rotate(90 * Time.deltaTime * AppliedSpeed * 0.5f, 0, 0);
                tireBR.Rotate(90 * Time.deltaTime * AppliedSpeed * 0.5f, 0, 0);
            }
        }
        /// <summary>
        /// Carried out in the network fixed update
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            if (GetInput(out CarInput.NetworkInputData input))
            {
                Inputs = input;
            }

            if (CanDrive)
                Move(Inputs);
            else
                RefreshAppliedSpeed();

            HandleStartRace();
            Steer(Inputs);
            UpdateTireYaw(Inputs);
        }
        private void Move(CarInput.NetworkInputData input)
        {
            if (input.IsAccelerate)
            {
                AppliedSpeed = Mathf.Lerp(AppliedSpeed, MaxSpeed, acceleration * Runner.DeltaTime);
            }
            else if (input.IsReverse)
            {
                AppliedSpeed = Mathf.Lerp(AppliedSpeed, -reverseSpeed, acceleration * Runner.DeltaTime);
            }
            else
            {
                AppliedSpeed = Mathf.Lerp(AppliedSpeed, 0, deceleration * Runner.DeltaTime);
            }

            var vel = (Rigidbody.rotation * Vector3.forward) * AppliedSpeed;
            vel.y = Rigidbody.linearVelocity.y;
            Rigidbody.linearVelocity = vel;
        }
        /// <summary>
        /// Modifications are made to slow down the car as soon as it reaches the finish line
        /// </summary>
        private void RefreshAppliedSpeed()
        {
            if (HasFinishedRace && Rigidbody.linearVelocity.z > 0.0f && AppliedSpeed > 0.0f)
            {
                AppliedSpeed = transform.InverseTransformDirection(Rigidbody.linearVelocity).z - 0.25f;

                if (AppliedSpeed < 0.0f)
                {
                    AppliedSpeed = 0.0f;
                }
                var vel = (Rigidbody.rotation * Vector3.forward) * AppliedSpeed;
                vel.y = Rigidbody.linearVelocity.y;
                Rigidbody.linearVelocity = vel;
            }
        }
        private void HandleStartRace()
        {
            if (!HasStartedRace && RaceTrackManager.Instance != null && RaceTrackManager.Instance.StartRaceTimer.Expired(Runner))
            {
                networkRaceTime.OnRaceStart();
            }
        }
        private void Steer(CarInput.NetworkInputData input)
        {
            var steerTarget = GetSteerTarget(input);

            if (SteerAmount != steerTarget)
            {
                var steerLerp = Mathf.Abs(SteerAmount) < Mathf.Abs(steerTarget) ? steerAcceleration : steerDeceleration;
                SteerAmount = Mathf.Lerp(SteerAmount, steerTarget, Runner.DeltaTime * steerLerp);
            }

            model.localEulerAngles = LerpAxis(Axis.Y, model.localEulerAngles, 0, 6 * Runner.DeltaTime);

            if (CanDrive)
            {
                var rot = Quaternion.Euler(
                    Vector3.Lerp(
                        Rigidbody.rotation.eulerAngles,
                        Rigidbody.rotation.eulerAngles + Vector3.up * SteerAmount,
                        3 * Runner.DeltaTime)
                );

                Rigidbody.MoveRotation(rot);
            }
        }
        private float GetSteerTarget(CarInput.NetworkInputData input)
        {
            var steerFactor = steeringCurve.Evaluate(Mathf.Abs(RealSpeed) / maxSpeedNormal) * maxSteerStrength *
                              Mathf.Sign(RealSpeed);

            return input.Steer * steerFactor;
        }
        private static Vector3 LerpAxis(Axis axis, Vector3 euler, float tgtVal, float t)
        {
            if (axis == Axis.X) return new Vector3(Mathf.LerpAngle(euler.x, tgtVal, t), euler.y, euler.z);
            if (axis == Axis.Y) return new Vector3(euler.x, Mathf.LerpAngle(euler.y, tgtVal, t), euler.z);
            return new Vector3(euler.x, euler.y, Mathf.LerpAngle(euler.z, tgtVal, t));
        }
        private void UpdateTireYaw(CarInput.NetworkInputData input)
        {
            TireYaw = input.Steer * maxSteerStrength;
        }

        #endregion
    }
}
