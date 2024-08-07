#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using TMPro;

namespace DitzelGames.FastIK
{
    /// <summary>
    /// Fabrik IK Solver
    /// </summary>
    public class FastIKFabric : MonoBehaviour
    {
        /// <summary>
        /// Chain length of bones
        /// </summary>
        public int ChainLength = 2;

        /// <summary>
        /// Target the chain should bent to
        /// </summary>
        public Transform Target;
        public Transform Pole;

        /// <summary>
        /// Solver iterations per update
        /// </summary>
        [Header("Solver Parameters")]
        public int Iterations = 10;

        /// <summary>
        /// Distance when the solver stops
        /// </summary>
        public float Delta = 0.001f;

        /// <summary>
        /// Strength of going back to the start position.
        /// </summary>
        [Range(0, 1)]
        public float SnapBackStrength = 1f;

        /// <summary>
        /// A boolean indicating that we are using limits on our joints
        /// </summary>
        [Header("Constraints")]
        public bool UsingLimits = false;

        /// <summary>
        /// The minimum angles for the joints
        /// with straight up being 0 and left being - and right being +
        /// </summary>
        [Range(-180, 0)]
        public float[] MinAngles;

        /// <summary>
        /// The maximum angles for the joints
        /// with straight up being 0 and left being - and right being +
        /// </summary>
        [Range(0, 180)]
        public float[] MaxAngles;

        protected float[] BonesLength; //Target to Origin
        protected float CompleteLength;
        protected Transform[] Bones;
        protected Vector3[] Positions;
        protected Vector3[] StartDirectionSucc;
        protected Quaternion[] StartRotationBone;
        protected Quaternion StartRotationTarget;
        protected Transform Root;

        private Vector3 targetProjectedToFloor;

        public Transform RobotArm;
        public bool sendingMessages = false;
        private bool firstTime = true;

        public TextMeshPro instructionText;

        //private float time = 0.0f; //seconds
        //public float interpolationPeriod = 0.01f; //sec

        private BraccioController braccioController;
        private string previousAnglesData = "";
        public GameObject digitalDouble;

        public GameObject desk;


        // Start is called before the first frame update
        void Awake()
        {
            braccioController = GetComponent<BraccioController>();
            Init();
        }

        void Init()
        {
            //initial array
            Bones = new Transform[ChainLength + 1];
            Positions = new Vector3[ChainLength + 1];
            BonesLength = new float[ChainLength];
            StartDirectionSucc = new Vector3[ChainLength + 1];
            StartRotationBone = new Quaternion[ChainLength + 1];

            //find root
            Root = transform;
            for (var i = 0; i <= ChainLength; i++)
            {
                if (Root == null)
                    throw new UnityException("The chain value is longer than the ancestor chain!");
                Root = Root.parent;
            }

            //init target
            if (Target == null)
            {
                Target = new GameObject(gameObject.name + " Target").transform;
                SetPositionRootSpace(Target, GetPositionRootSpace(transform));
            }
            StartRotationTarget = GetRotationRootSpace(Target);


            //init data
            var current = transform;
            CompleteLength = 0;
            for (var i = Bones.Length - 1; i >= 0; i--)
            {
                Bones[i] = current;
                StartRotationBone[i] = GetRotationRootSpace(current);

                if (i == Bones.Length - 1)
                {
                    //leaf
                    StartDirectionSucc[i] = GetPositionRootSpace(Target) - GetPositionRootSpace(current);
                }
                else
                {
                    //mid bone
                    StartDirectionSucc[i] = GetPositionRootSpace(Bones[i + 1]) - GetPositionRootSpace(current);
                    BonesLength[i] = StartDirectionSucc[i].magnitude;
                    CompleteLength += BonesLength[i];
                }

                current = current.parent;
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            ResolveIK();
        }

        private void ResolveIK()
        {
            if (Target == null)
                return;

            if (BonesLength.Length != ChainLength)
                Init();

            //init rotation axis for joints
            this.targetProjectedToFloor = Vector3.ProjectOnPlane(Target.position - RobotArm.position, Vector3.up).normalized;

            //Fabric
            //  root
            //  (bone0) (bonelen 0) (bone1) (bonelen 1) (bone2)...
            //   x--------------------x--------------------x---...

            //get position
            for (int i = 0; i < Bones.Length; i++)
                Positions[i] = GetPositionRootSpace(Bones[i]);

            Vector3 targetPosition;
            if (UsingLimits)
                targetPosition = Quaternion.Inverse(Root.rotation) * new Vector3(Mathf.Sqrt(Target.position.x * Target.position.x + Target.position.z * Target.position.z) - Mathf.Sqrt(Root.position.x * Root.position.x + Root.position.z * Root.position.z), Target.position.y - Root.position.y, 0);
            else
                targetPosition = GetPositionRootSpace(Target);
            var targetRotation = GetRotationRootSpace(Target);

            //1st is possible to reach?
            if ((targetPosition - GetPositionRootSpace(Bones[0])).sqrMagnitude >= CompleteLength * CompleteLength)
            {
                //it is unreachable so we just strech it
                var direction = (targetPosition - Positions[0]).normalized;
                //set everything after root
                for (int i = 1; i < Positions.Length; i++)
                    Positions[i] = Positions[i - 1] + direction * BonesLength[i - 1];
            }
            else
            {
                //it is reachable so we do IK
                for (int i = 0; i < Positions.Length - 1; i++)
                    Positions[i + 1] = Vector3.Lerp(Positions[i + 1], Positions[i] + StartDirectionSucc[i], SnapBackStrength);

                for (int iteration = 0; iteration < Iterations; iteration++)
                {
                    //https://www.youtube.com/watch?v=UNoX65PRehA
                    //back
                    for (int i = Positions.Length - 1; i > 0; i--)
                    {
                        if (i == Positions.Length - 1)
                            Positions[i] = targetPosition; //set it to target
                        else
                        {
                            Vector3 rotation = (Positions[i] - Positions[i + 1]).normalized;
                            Positions[i] = Positions[i + 1] + rotation * BonesLength[i]; //set in line on distance
                        }
                    }

                    //forward
                    for (int i = 1; i < Positions.Length; i++)
                    {
                        Vector3 rotation = (Positions[i] - Positions[i - 1]).normalized;
                        Positions[i] = Positions[i - 1] + rotation * BonesLength[i - 1];
                    }

                    //close enough?
                    if ((Positions[Positions.Length - 1] - targetPosition).sqrMagnitude < Delta * Delta)
                        break;
                }
            }

            //move towards pole
            if (Pole != null)
            {
                var polePosition = GetPositionRootSpace(Pole);
                for (int i = 1; i < Positions.Length - 1; i++)
                {
                    var plane = new Plane(Positions[i + 1] - Positions[i - 1], Positions[i - 1]);
                    var projectedPole = plane.ClosestPointOnPlane(polePosition);
                    var projectedBone = plane.ClosestPointOnPlane(Positions[i]);
                    var angle = Vector3.SignedAngle(projectedBone - Positions[i - 1], projectedPole - Positions[i - 1], plane.normal);
                    Positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (Positions[i] - Positions[i - 1]) + Positions[i - 1];
                }
            }

            //set position & rotation
            for (int i = 0; i < Positions.Length; i++)
            {
                if (i == Positions.Length - 1)
                    SetRotationRootSpace(Bones[i], Quaternion.Inverse(targetRotation) * StartRotationTarget * Quaternion.Inverse(StartRotationBone[i]));
                else
                    SetRotationRootSpace(Bones[i], Quaternion.FromToRotation(StartDirectionSucc[i], Positions[i + 1] - Positions[i]) * Quaternion.Inverse(StartRotationBone[i]));
                SetPositionRootSpace(Bones[i], Positions[i]);

            }

            if (firstTime)
            {
                firstTime = false;
                SendRotations();
                SendWithLimiter("T-180");
            }

            //time += Time.deltaTime;
            //if (time > interpolationPeriod)
            //{
                //time = time - interpolationPeriod;
            SendRotations();
            //}
        }

        private void SendRotations()
        {
            Vector3 ShoulderSegmentVector = Bones[1].position - desk.transform.position; // - Vector3.zero
            Vector3 ElbowSegmentVector = Bones[2].position - Bones[1].position;
            Vector3 WristSegmentVector = Bones[3].position - Bones[2].position;

            // find rotations
            float BaseRotation = Mathf.Atan2(this.targetProjectedToFloor.x, this.targetProjectedToFloor.z) * 180 / Mathf.PI;
            float BaseRotationFlipped = ((BaseRotation + 360.0f) % 360.0f) - 180.0f; // so that the singularity is at the back and not in the front
            float ShoulderRotation = Vector3.Angle(ShoulderSegmentVector, desk.transform.position);// + 5.0f;
            float ElbowRotation = Vector3.Angle(ElbowSegmentVector, ShoulderSegmentVector);// + 5.0f;
            float WristRotation = Vector3.Angle(WristSegmentVector, ElbowSegmentVector);

            // make negative angles if behind bottom segment
            if (UsingLimits && Bones[1].position.x < 0) // generic because using Vector3.up
                ShoulderRotation = -ShoulderRotation; // make angle negative

            float angleBetweenElbowAndShoulder = Vector3.Cross(ElbowSegmentVector, ShoulderSegmentVector).y;
            if (UsingLimits && angleBetweenElbowAndShoulder > 0 && ShoulderRotation < 4 && ShoulderRotation > -4) // see if angle is negative
                ElbowRotation = -ElbowRotation;
            //if (UsingLimits && Vector3.Cross(WristSegmentVector, ElbowSegmentVector).y >= 0)
            WristRotation = -WristRotation;

            // create data string & send
            //Debug.Log(BaseRotation + " " + BaseRotationFlipped);
            string anglesData = "B" + BaseRotation + " S" + ShoulderRotation + " E" + ElbowRotation + " W" + WristRotation;
            SendWithLimiter(anglesData);
            
            digitalDouble.transform.rotation = Quaternion.Euler(digitalDouble.transform.rotation.x, BaseRotation - 90, digitalDouble.transform.rotation.z);
        }

        private float timePassed = 0;
        private float maxLimit = 0.1f; //1command/100ms

        private void SendWithLimiter(string anglesData)
        {
            timePassed += Time.deltaTime;

            if (previousAnglesData != anglesData && timePassed >= maxLimit && sendingMessages)
            {
                timePassed -= maxLimit;
                //Debug.Log("elbow: " + ElbowRotation + " = " + (Vector3.Cross(ElbowSegmentVector, ShoulderSegmentVector).y > 0) + " " + Vector3.Cross(ElbowSegmentVector, ShoulderSegmentVector).y);
                //Debug.Log("shoulder: " + ShoulderRotation);
                braccioController.SendSerialMessage(anglesData);
                previousAnglesData = anglesData;
            }
        }

        private Vector3 GetPositionRootSpace(Transform current)
        {
            if (Root == null)
                return current.position;
            else
                return Quaternion.Inverse(Root.rotation) * (current.position - Root.position);
        }

        private void SetPositionRootSpace(Transform current, Vector3 position)
        {
            if (Root == null)
                current.position = position;
            else
                current.position = Root.rotation * position + Root.position;
        }

        private Quaternion GetRotationRootSpace(Transform current)
        {
            //inverse(after) * before => rot: before -> after
            if (Root == null)
                return current.rotation;
            else
                return Quaternion.Inverse(current.rotation) * Root.rotation;
        }

        private void SetRotationRootSpace(Transform current, Quaternion rotation)
        {
            if (Root == null)
                current.rotation = rotation;
            else
                current.rotation = Root.rotation * rotation;
        }

        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            var current = this.transform;
            var scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
            Handles.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(0, Vector3.up), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
            Handles.color = Color.green;
            Handles.DrawSolidArc(Vector3.zero, Vector3.up, Vector3.right, 315, 0.02f);

            Handles.DrawLine(Vector3.zero, targetProjectedToFloor * 10);
            for (int i = 0; i < ChainLength && current != null && current.parent != null; i++)
            {
                scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
                Handles.color = Color.green;
                Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
                Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
                Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(1, 1, 1));

                // Draw Min Angle
                Handles.color = Color.yellow;
                float currentParentDistance = Vector3.Distance(current.parent.position, current.position);
                Handles.DrawSolidArc(new Vector3(0, currentParentDistance, 0), Vector3.Cross(Vector3.up, Vector3.left), Vector3.down, MinAngles[i],  0.02f);

                // Draw Max Angle
                Handles.color = Color.magenta;
                Handles.DrawSolidArc(new Vector3(0, currentParentDistance, 0), Vector3.Cross(Vector3.down, Vector3.right), Vector3.down, MaxAngles[i], 0.02f);
                current = current.parent;
            }
#endif
        }

    }
}