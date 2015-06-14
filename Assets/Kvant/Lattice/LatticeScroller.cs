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
        [SerializeField]
        float _yawAngle;

        public float yawAngle {
            get { return _yawAngle; }
            set { _yawAngle = value; }
        }

        [SerializeField]
        float _speed;

        public float speed {
            get { return _speed; }
            set { _speed = value; }
        }

        void Update()
        {
            var r = _yawAngle * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(r), Mathf.Sin(r));
            GetComponent<Lattice>().offset += dir * _speed * Time.deltaTime;
        }
    }
}
