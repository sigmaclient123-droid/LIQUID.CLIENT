using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Collections;

namespace liquidclient.Classes
{
    public class CoroutineManager2 : MonoBehaviour
    {
        public static CoroutineManager2 instance = null;

        private void Awake() =>
            instance = this;

        public static Coroutine RunCoroutine(IEnumerator enumerator) =>
            instance.StartCoroutine(enumerator);

        public static void EndCoroutine(Coroutine enumerator) =>
            instance.StopCoroutine(enumerator);
    }
}