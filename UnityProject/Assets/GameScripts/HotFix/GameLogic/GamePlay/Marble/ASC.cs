using TEngine;
using UnityEngine;

namespace GameLogic.Marble
{
    public abstract class ASC : MonoBehaviour
    {
        public Rigidbody2D Rigidbody { get; private set; }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
        }
    }
}
