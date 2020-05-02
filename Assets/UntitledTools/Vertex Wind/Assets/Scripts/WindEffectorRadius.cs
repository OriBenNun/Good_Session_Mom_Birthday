using UnityEngine;

namespace UntitledTools
{
    namespace VertexWind
    {

        //The radius wind effector object
        public class WindEffectorRadius : MonoBehaviour
        {

            //The variables for the wind effector
            [Tooltip("The radius of the effect multiplier")]
            public float radius = 10f;
            [Tooltip("The \"amount\" override used in the wind effector")]
            public Vector3 amount = Vector3.one * 0.5f;

            //Shows the gizmos for the wind effector
            private void OnDrawGizmosSelected()
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(transform.position, radius);
                }
#endif
            }

            private void OnDrawGizmos()
            {
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlaying)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(transform.position, radius);
                }
#endif
            }

        }

    }
}
