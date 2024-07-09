using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

[RequireComponent(typeof(ObiActor))]
public class GrabBehaviourController : MonoBehaviour
{
    ObiActor actor;
    //public GameObject GrabPoints;

    private void Awake()
    {
        actor = GetComponent<ObiActor>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    // finds the closest particle in the rope and links it to your hands with position and rotation
    // will return closest particle index if successful (else -1)
    public int GrabParticle(Vector3 grabPosition)
    {
        if (actor.isLoaded)
        {
            int closestParticleIndex = GetClosestParticleIndex(grabPosition);
            FixParticle(closestParticleIndex);
            MoveParticle(closestParticleIndex, grabPosition);
            return closestParticleIndex;
        }
        return -1;
    }

    // returns the index of the closest particle to the grab position
    private int GetClosestParticleIndex(Vector3 grabPosition)
    {
        float maxDistance = float.MaxValue;
        int closestParticleIndex = -1;
        for (int i = 0; i < actor.solverIndices.Length; i++)
        {
            int currentParticleIndex = actor.solverIndices[i];
            float currentDistance = Vector3.Distance(actor.GetParticlePosition(currentParticleIndex), grabPosition);
            if (currentDistance < maxDistance)
            {
                maxDistance = currentDistance;
                closestParticleIndex = currentParticleIndex;
            }
        }

        return closestParticleIndex;
    }

    // fixes particle which means it will not be influenced by the rope physics
    public void FixParticle(int particleIndex)
    {
        actor.solver.velocities[particleIndex] = Vector3.zero;
        actor.solver.invMasses[particleIndex] = 0;
    }

    // unfixes particle which means it will again be influenced by the rope physics
    public void UnfixParticle(int particleIndex)
    {
        actor.solver.velocities[particleIndex] = Vector3.zero; // TODO: give velocity of hand?
        actor.solver.invMasses[particleIndex] = 1;
    }

    // moves the particle according to the grab
    public void MoveParticle(int particleIndex, Vector3 grabPosition)
    {
        actor.solver.positions[particleIndex] = grabPosition;
    }

    // moves and rotates the particles according to the grab
    public void MoveParticle(int particleIndex, Vector3 grabPosition, Quaternion grabRotation)
    {
        MoveParticle(particleIndex, grabPosition);
        actor.solver.orientations[particleIndex] = grabRotation;
    }

}
