//
// Scroller script for Lattice
//
using UnityEngine;

namespace Kvant
{
    [RequireComponent(typeof(Lattice))]
    [AddComponentMenu("Kvant/Lattice Scroller")]
    public class LatticeScroller : MonoBehaviour
    {
        public float yawAngle;
        public float speed;

        void Update()
        {
            var r = yawAngle * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(r), Mathf.Sin(r));
            GetComponent<Lattice>().offset += dir * speed * Time.deltaTime;
        }
    }
}
