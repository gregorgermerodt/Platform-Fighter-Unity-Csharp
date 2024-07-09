using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public Fighter owner { get; private set; }
    public string summoningState;
    public int startFrame { get; private set; }
    public int endFrame { get; private set; }
    public float damage { get; private set; }
    public float baseKnockback { get; private set; }
    public float knockback { get; private set; }
    public Vector2 direction { get; private set; }
    public Transform bone { get; private set; }
    public SphereCollider sphere { get; private set; }

    public HashSet<Collider> alreadyHit;

    public void Initialize(Fighter owner, int startFrame, int endFrame, float damage, float baseKnockback, float knockback, Vector2 direction, Transform bone, float radius, Vector3 offset)
    {
        this.owner = owner;
        this.summoningState = owner.fighterMoveset.currentState;
        this.startFrame = startFrame;
        this.endFrame = endFrame;
        this.damage = damage;
        this.baseKnockback = baseKnockback;
        this.knockback = knockback;
        this.direction = direction;
        this.bone = bone;
        gameObject.transform.SetParent(bone);
        sphere = bone.gameObject.AddComponent<SphereCollider>();
        sphere.radius = radius;
        sphere.center = offset;
        sphere.isTrigger = true;
        sphere.includeLayers = 1 << LayerMask.NameToLayer("Player");

        alreadyHit = new HashSet<Collider>();
    }

    void OnDestroy()
    {
        Destroy(sphere);
        owner = null;
        bone = null;
    }

    void OnTriggerEnter(Collider other)
    {
        Fighter hitFighter;
        other.gameObject.TryGetComponent<Fighter>(out hitFighter);
        if (hitFighter != null)
        {
            if (hitFighter.playerNumber != owner.playerNumber && !alreadyHit.Contains(other))
            {
                alreadyHit.Add(other);
                hitFighter.percentDamage += damage;
                hitFighter.fighterMoveset.fighterController.AddVelocity(direction * (hitFighter.percentDamage * knockback + baseKnockback));
                if (direction.x < 0)
                    hitFighter.fighterMoveset.SetFaceDirection(FighterMoveset.FaceDirection.RIGHT);
                else if (direction.x > 0)
                    hitFighter.fighterMoveset.SetFaceDirection(FighterMoveset.FaceDirection.LEFT);
            }
        }
    }
}