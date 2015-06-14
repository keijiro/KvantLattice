//
// Scroller script for Lattice
//
using UnityEngine;

namespace Kvant
{
    [RequireComponent(typeof(Lattice)), AddComponentMenu("Kvant/Lattice Scroller")]
    public class LatticeScroller : MonoBehaviour
    {
        public Vector2 speed;

        Vector3 _origin;
        Vector2 _position;

        void Start()
        {
            _origin = transform.position;
        }

        void Update()
        {
            var lattice = GetComponent<Lattice>();

            _position += speed * Time.deltaTime;

            var lsize = lattice.extent;
            var dx = lsize.x / lattice.columns;
            var dy = lsize.y / (lattice.rows / 2);

            var ox = _position.x % dx;
            var oy = _position.y % dy;

            transform.position = _origin - new Vector3(ox, 0, oy);
            lattice.noiseOffset = new Vector2(_position.x - ox, _position.y - oy);
        }
    }
}
