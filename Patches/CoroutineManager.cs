
using System.Collections;
using UnityEngine;

namespace liquidclient.Managers
{
    public class CoroutineManager : MonoBehaviour
    {
        public static CoroutineManager instance;

        private void Awake() =>
            instance = this;

        [System.Obsolete("RunCoroutine is obsolete. Use StartCoroutine directly on MonoBehaviour instances instead.")]
        public static Coroutine RunCoroutine(IEnumerator enumerator) =>
            instance.StartCoroutine(enumerator);

        [System.Obsolete("EndCoroutine is obsolete. Use StopCoroutine directly on MonoBehaviour instances instead.")]
        public static void EndCoroutine(Coroutine enumerator) =>
            instance.StopCoroutine(enumerator);
    }
}